<%@ Control Language="c#" Inherits="ArenaWeb.UserControls.Custom.HDC.Misc.MyAttributes" Codefile="MyAttributes.ascx.cs" CodeBehind="MyAttributes.ascx.cs" %>

<input type="hidden" id="iRedirect" runat="server" name="iRedirect" />

<asp:Panel ID="pnlSelectTags" Runat="server" Visible="True">
<div class="textWrap">
	<table border="0">
	    <tr>
	        <td style="width:20px;">&nbsp;</td>
	        <td><asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder></td>
	    </tr>
	</table>
	<p>
    <asp:Button ID="btnSubmit" Runat="server" CssClass="smallText" Text="Submit"></asp:Button>
    </p>
    <asp:Label ID="lbDone" runat="server"></asp:Label>
</div>
</asp:Panel>
