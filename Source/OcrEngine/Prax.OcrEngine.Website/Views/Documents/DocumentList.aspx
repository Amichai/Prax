<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Prax.OcrEngine.Website.Models.DocumentListModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Prax
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<%if (Model.ShowSiteIntro)
	   Html.RenderAction("SiteIntro", "Content"); %>
	<%
		using (Html.BeginForm("Upload", "Documents", FormMethod.Post, new { enctype = "multipart/form-data" })) {%>
	<input type="file" name="file" />
	<input type="submit" name="UploadFile" value="Upload" />
	<%  } %>
</asp:Content>
