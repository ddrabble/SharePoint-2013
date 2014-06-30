<%@ Page language="C#"   Inherits="Microsoft.SharePoint.Publishing.PublishingLayoutPage,Microsoft.SharePoint.Publishing,Version=15.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c" meta:webpartpageexpansion="full" meta:progid="SharePoint.WebPartPage.Document" %>
<%@ Register Tagprefix="SharePointWebControls" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="PublishingWebControls" Namespace="Microsoft.SharePoint.Publishing.WebControls" Assembly="Microsoft.SharePoint.Publishing, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="PublishingNavigation" Namespace="Microsoft.SharePoint.Publishing.Navigation" Assembly="Microsoft.SharePoint.Publishing, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<asp:Content ContentPlaceholderID="PlaceHolderPageTitle" runat="server">
	<SharePointWebControls:FieldValue id="PageTitle" FieldName="Title" runat="server"/>
</asp:Content>
<asp:Content ContentPlaceholderID="PlaceHolderMain" runat="server">
V1
<WebPartPages:SPProxyWebPartManager runat="server" id="spproxywebpartmanager"></WebPartPages:SPProxyWebPartManager>
<p></p>
<WebPartPages:WebPartZone id="g_0733831ECEE74254A1D8E0342C6444B9" runat="server" title="Zone 1">


<WebPartPages:ContentEditorWebPart webpart="true" runat="server" __WebPartId="{4905D7E8-8C9F-4635-853F-D45A7B27D58B}"><WebPart xmlns="http://schemas.microsoft.com/WebPart/v2">
	<Title>$Resources:core,ContentEditorWebPartTitle;</Title>
	<Description>$Resources:core,ContentEditorWebPartDescription;</Description>
	<PartImageLarge>/_layouts/15/images/mscontl.gif</PartImageLarge>
<PartOrder>1</PartOrder>
<ID>g_4905d7e8_8c9f_4635_853f_d45a7b27d58b</ID>
</WebPart></WebPartPages:ContentEditorWebPart>


</WebPartPages:WebPartZone>

</asp:Content>
