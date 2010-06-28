<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	FAQ
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="HeadContent">
	<style type="text/css">
		dt {
			font-weight: bold;
		}
		dd {
			margin: 1px 0 1em 20px;
		}
	</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<dl>
		<dt>What is this site?</dt>
		<dd>
			This website performs OCR</dd>
		<dt>What is OCR?</dt>
		<dd>
			<b>O</b>ptical <b>C</b>haracter <b>R</b>ecognition</dd>
	</dl>
</asp:Content>
