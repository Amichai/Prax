<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Prax.OcrEngine.Website.Models.ProgressBarModel>" %>
<%
	int progress = Model.Progress;
	string caption = Model.Caption;

	int inverseProgress;
	if (progress == 0)
		inverseProgress = 0;
	else
		inverseProgress = 100 * 100 / progress;
%>
<%-- The CSS styles are defined in DocumentList.aspx--%>
<div class="ProgressContainer"><span class="OuterText">
	<%:caption%></span>
	<div class="ProgressBar" style="width: <%:progress %>%"><span style="width: <%:inverseProgress %>%">
		<%:caption%></span></div>
</div>
