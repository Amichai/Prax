<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Prax.OcrEngine.Website.Models.DocumentListModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Prax
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeadContent" runat="server">
	<style type="text/css">
		table#documents {
			margin-top: 2em;
			width: 100%;
			border: 1px solid black;
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
		.ProgressContainer {
			width: 100%;
			position: relative;
			background-color: #eee;
			border: 1px solid black;
			padding: 1px;
			margin-right: -1px;
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
	</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<%if (Model.ShowSiteIntro)
	   Html.RenderAction("SiteIntro", "Content"); %>
	<%
		using (Html.BeginForm("Upload", "Documents", FormMethod.Post, new { enctype = "multipart/form-data" })) {%>
	<input type="file" name="file" />
	<input type="submit" name="UploadFile" value="Upload" />
	<%  } %>
	<table id="documents">
		<thead>
			<tr>
				<th>Name</th>
				<th class="Right">Size</th>
				<th class="Right">Date</th>
				<th>Progress</th>
			</tr>
		</thead>
		<tbody>
			<%foreach (var doc in Model.Documents) { %>
			<tr>
				<td>
					<%:doc.Name %></td>
				<td class="Right">
					<%:doc.Length.ToSizeString() %></td>
				<td class="Right">
					<%:doc.DateUploaded.ToShortDateString() %></td>
				<td>
					<%:Html.Action("ProgressBar", doc) %>
				</td>
			</tr>
			<%} %></tbody></table>
</asp:Content>
