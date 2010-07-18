<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Prax.OcrEngine.Website.Models.DocumentListModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Prax
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeadContent" runat="server">
	<style type="text/css">
		table#documents {
			margin-top: 1em;
			width: 100%;
			border: 1px solid black;
		}
		table#documents a {
			color: Blue;
		}
		table#documents thead th {
			background: LightGray;
			border-bottom: 2px solid black;
			font-weight: bold;
			padding: 2px 5px;
		}
		table#documents tbody td {
			padding: 2px 5px;
			border-top: gray solid 1px;
		}
		.Right {
			text-align: right;
		}
		.Center {
			text-align: center;
		}
		.ProgressContainer {
			width: 100%;
			position: relative;
			background-color: #eee;
			border: 1px solid black;
			padding: 1px;
			margin-right: -1px;
			text-align: left;
		}
		.ProgressContainer .ProgressBar {
			background-color: Blue;
			position: relative;
			overflow: hidden;
			height: 1em;
			padding-bottom: 2px;
		}
		.ProgressContainer span {
			color: Black;
			text-align: center;
			width: 100%;
			position: absolute;
		}
		.ProgressContainer .ProgressBar span {
			color: White;
		}
		.SiteIntro {
			width: 50%;
			margin: 1.5em auto;
			padding: 5px;
			border: 2px solid darkgray;
			background: #DD9;
		}
		form.Upload fieldset {
			width: 50%;
			padding: 5px 10px 12px;
			text-align: center;
			border: 2px groove darkgray;
			margin: 0 auto;
		}
		form.Upload fieldset {
			text-align: center;
		}
		form.Upload .Submit {
			font-size: 2.5em;
			padding: 4px;
			margin-top: .2em;
		}
	</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<%if (Model.ShowSiteIntro)
	   Html.RenderAction("SiteIntro", "Content"); %>
	<%
		using (Html.BeginForm("Upload", "Documents", FormMethod.Post, new { enctype = "multipart/form-data", @class = "Upload" })) {%>
	<fieldset>
		<legend>Upload Document</legend>
		<input type="file" name="file" /><br />
		<input type="submit" name="UploadFile" value="Upload" class="Submit" />
	</fieldset>
	<%  } %>
	<%if (Model.Documents.Count == 0) { %>
	<p>
		You have no documents.<br />
		Would you like to upload one?</p>
	<%} else { %>
	<table id="documents">
		<thead>
			<tr>
				<th>Name</th>
				<th class="Right">Size</th>
				<th class="Right">Date</th>
				<th class="Center">Progress</th>
			</tr>
		</thead>
		<tbody>
			<%foreach (var doc in Model.Documents) { %>
			<tr>
				<td>
					<%:Html.ActionLink(doc.Name, "View", new { doc.Id, doc.Name })%></td>
				<td class="Right">
					<%:doc.Length.ToSizeString()%></td>
				<td class="Right">
					<%:doc.DateUploaded.ToShortDateString()%></td>
				<td class="Center">
					<%:Html.Action("ProgressBar", doc)%>
				</td>
			</tr>
			<%} %></tbody></table>
	<%} %>
</asp:Content>
