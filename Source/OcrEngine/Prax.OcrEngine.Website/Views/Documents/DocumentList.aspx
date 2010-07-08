<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Prax.OcrEngine.Website.Models.DocumentListModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Prax
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeadContent" runat="server">
	<style type="text/css">
		td.Size {
			text-align: right;
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
	<table>
		<thead>
			<tr>
				<th>Name</th>
				<th>Size</th>
				<th>Date</th>
				<th>State</th>
				<th>Progress</th>
			</tr>
		</thead>
		<tbody>
			<%foreach (var doc in Model.Documents) { %>
			<tr>
				<td>
					<%:doc.Name %></td>
				<td class="Size">
					<%:doc.Length.ToSizeString() %></td>
				<td>
					<%:doc.DateUploaded.ToShortDateString() %></td>
				<td>
					<%:doc.State %></td>
				<td>
					<%:doc.ScanProgress %></td>
			</tr>
			<%} %></tbody></table>
</asp:Content>
