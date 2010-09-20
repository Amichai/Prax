using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;

namespace Prax.OcrEngine.Services.Azure {
	///<summary>Maintains cached copies of Azure blobs.</summary>
	public class AzureDataCache : IDataCache {
		CloudBlobContainer container;

		///<summary>Creates an AzureDataCache service.</summary>
		///<param name="container">The CloudBlobContainer to read data from.</param>
		///<param name="destination">The path to save the data.  (on the local disk)</param>
		public AzureDataCache(CloudBlobContainer container, string destination) {
			if (container == null) throw new ArgumentNullException("container");
			if (destination == null) throw new ArgumentNullException("destination");

			this.container = container;
			LocalPath = destination;
		}

		public Version LocalVersion { get; private set; }
		public string LocalPath { get; private set; }

		static readonly BlobRequestOptions Options = new BlobRequestOptions { BlobListingDetails = BlobListingDetails.Metadata, UseFlatBlobListing = true };
		public bool Update() {
			container.FetchAttributes();
			var newVersion = Version.Parse(container.Metadata["Version"]);
			if (LocalVersion == newVersion)
				return false;

			var oldFiles = new HashSet<string>(
				Directory.EnumerateFiles(LocalPath, "*", SearchOption.AllDirectories),
				StringComparer.OrdinalIgnoreCase
			);

			foreach (CloudBlob remoteFile in container.ListBlobs(Options)) {
				var relativePath = container.Uri.MakeRelativeUri(remoteFile.Uri).ToString().Replace('/', '\\');

				var localFile = new FileInfo(Path.Combine(LocalPath, relativePath));
				oldFiles.Remove(localFile.FullName);

				if (localFile.Exists
				 && localFile.Length == remoteFile.Properties.Length
				 && Convert.FromBase64String(remoteFile.Metadata["SHA512"]).SequenceEqual(localFile.SHA512Hash()))
					continue;

				remoteFile.DownloadToFile(localFile.FullName);
				localFile.LastWriteTimeUtc = remoteFile.Properties.LastModifiedUtc;
			}

			//Delete any files that aren't in blob storage.
			foreach (var path in oldFiles)
				File.Delete(path);

			LocalVersion = newVersion;
			return true;
		}
	}
	///<summary>Uploads files to Azure blob storage for an AzureDataCache.</summary>
	public static class AzureDataUploader {
		static readonly BlobRequestOptions Options = new BlobRequestOptions { BlobListingDetails = BlobListingDetails.Metadata, UseFlatBlobListing = true };
		///<summary>Updates the blobs in a storage container.</summary>
		///<param name="container">The container to update.</param>
		///<param name="newVersion">The new version being uploaded.</param>
		///<param name="sourcePath">The directory on the local disk containing the new files.</param>
		public static void UpdateStorage(CloudBlobContainer container, Version newVersion, string sourcePath) {
			container.FetchAttributes();

			var remoteFiles = container.ListBlobs(Options).Cast<CloudBlob>().ToArray();
			var oldBlobs = remoteFiles.ToDictionary(
				blob => Path.Combine(sourcePath,
					container.Uri.MakeRelativeUri(blob.Uri).ToString().Replace('/', '\\')
				)
			);

			var baseUri = new Uri(sourcePath, UriKind.Absolute);
			foreach (var file in new DirectoryInfo(sourcePath).EnumerateFiles("*", SearchOption.AllDirectories)) {
				CloudBlob blob;
				byte[] hash = file.SHA512Hash();

				//If there already is a blob for this file, use it.
				if (!oldBlobs.TryGetValue(file.FullName, out blob)) {
					blob = container.GetBlobReference(baseUri.MakeRelativeUri(new Uri(file.FullName, UriKind.Absolute)).ToString());
				} else {
					oldBlobs.Remove(file.FullName);	//Remove the blob from the dictionary; all blobs left in the dictionary will be deleted.

					if (file.Length == blob.Properties.Length
					 && Convert.FromBase64String(blob.Metadata["SHA512"]).SequenceEqual(hash))
						continue;	//If the blob is identical, don't re-upload it.
				}
				blob.Metadata["SHA512"] = Convert.ToBase64String(hash);
				blob.UploadFile(file.FullName);
			}

			foreach (var blob in oldBlobs.Values)
				blob.Delete();

			container.Metadata["Version"] = newVersion.ToString();
			container.SetMetadata();
		}
	}
}
