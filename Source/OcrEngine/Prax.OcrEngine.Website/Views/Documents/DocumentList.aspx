<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<DocumentListModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Prax
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeadContent" runat="server">
	<style type="text/css">
		p#noDocumentsMessage {
			text-align: center;
			margin-top: 1em;
		}
		.SiteIntro {
			width: 50%;
			margin: 1.5em auto;
			padding: 5px;
			border: 2px solid darkgray;
			background: #DD9;
		}
		form.Upload {
			text-align: center;
		}
		form.Upload fieldset {
			width: 50%;
			padding: 5px 10px 12px;
			text-align: center;
			border: 2px groove darkgray;
			margin: 0 auto;
		}
		form.Upload .Submit {
			font-size: 2.5em;
			padding: 4px;
			margin-top: .2em;
		}
	</style>
	<%if (false) {%>
	<%-- Add <script> and <link> tags here to convince Visual Studio that the 
		 files are referenced for IntelliSense. 
		 The files should actually be included by the ResourceSet architecture. --%>
	<link href="../../Content/CSS/DocumentTable.css" rel="stylesheet" type="text/css" />

	<script src="../../Content/Javascript/jQuery/jquery-1.4.1.js" type="text/javascript"></script>

	<script src="../../Content/Javascript/jQuery/swfobject.js" type="text/javascript"></script>

	<script src="../../Content/Javascript/jQuery/jquery.uploadify.v2.1.0.js" type="text/javascript"></script>

	<script src="../../Content/Javascript/ProgressBar.js" type="text/javascript"></script>

	<script src="../../Content/Javascript/Utilities.js" type="text/javascript"></script>

	<script src="../../Content/Javascript/DocumentTable.js" type="text/javascript"></script>

	<script src="../../Content/Javascript/DocumentUpload.js" type="text/javascript"></script>

	<%} %>
	<%=Html.Stylesheets(ResourceSet.DocumentsCss) %>
	<%=Html.Scripts(ResourceSet.DocumentsJavascript) %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<%if (Model.ShowSiteIntro)
	   Html.RenderAction("SiteIntro", "Content"); %>
	<%
		using (Html.BeginForm("Upload", "Documents", FormMethod.Post, new { enctype = "multipart/form-data", @class = "Upload" })) {%>
	<fieldset id="uploadForm">
		<legend>Upload Document</legend>
		<input type="file" name="file" /><br />
		<input type="submit" name="UploadFile" value="Upload" class="Submit" />
	</fieldset>
	<%  } %>
	<%if (Model.Documents.Count == 0) { %>
	<p id="noDocumentsMessage">
		Would you like to upload a document?</p>
	<%} %>
	<%using (Html.BeginForm("Delete", "Documents")) { %>
	<table id="documents" style="<%: Model.Documents.Count == 0?"display: none;" : "" %>">
		<thead>
			<tr>
				<th>Name</th>
				<th class="Right" style="width: 75px;">Size</th>
				<th class="Right">Date</th>
				<th class="Center" style="width: 135px">Progress</th>
				<th>
					<%-- Delete buttons --%></th>
			</tr>
		</thead>
		<tbody>
			<%foreach (var doc in Model.Documents) { %>
			<tr id="document-<%:doc.Id.DocumentId %>">
				<td class="NameCell">
					<%:Html.ActionLink(doc.Name, "View", new { id = doc.Id.DocumentId,  doc.Name }, new { target = "DocumentPreview" })%>
				</td>
				<td class="SizeCell Right" title="<%:doc.Length.ToString()%> bytes">
					<%:doc.Length.ToSizeString()%></td>
				<td class="DateCell Right">
					<%:doc.DateUploaded.ToShortDateString()%></td>
				<td class="StatusCell Center">
					<%:Html.Action("ProgressBar", doc)%>
					<%if (doc.State == DocumentState.Scanned) { %>
					<%:Html.ResultsLink(doc, ResultFormat.PlainText) %>
					<%:Html.ResultsLink(doc, ResultFormat.Pdf)%>
					<%} %>
				</td>
				<td class="DeleteCell">
					<%-- The submit button passes the ID parameter to the form.
					 Since the value attribute also controls the caption, I
					 hide the caption using CSS.  For accessibility reasons,
					 I include the word delete, which the action removes.--%>
					<input type="submit" name="id" title="Delete <%:doc.Name%>" value="Delete <%:doc.Id.DocumentId	%>" class="Sprite16" />
				</td>
			</tr>
			<%} %>
		</tbody>
	</table>
	<%} %>

	<script type="text/javascript">
		var documents = new Prax.DocumentTable($('#documents'));
		var uploader = new Prax.DocumentUploader(documents, '#uploadForm');
		var refreshDelay = parseInt('<%:(Model.Documents.Min(d => d.GetRefreshTime()) ?? TimeSpan.Zero).TotalMilliseconds %>', 10); //Using a string fixes VS's syntax parser
		if (refreshDelay > 0)
			setTimeout(function () { documents.updateData(); }, refreshDelay);
	</script>

</asp:Content>
