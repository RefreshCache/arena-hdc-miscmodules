<%@ Control Language="c#" Inherits="ArenaWeb.UserControls.Custom.HDC.Misc.SelfJoinTags" CodeFile="SelfJoinTags.ascx.cs" CodeBehind="SelfJoinTags.ascx.cs" %>

<input type="hidden" id="iRedirect" runat="server" name="iRedirect" />

<asp:Panel ID="pnlSelectTags" Runat="server" Visible="True">
<div class="textWrap">
	<table border="0">
	    <tr>
	        <td style="width=20px;">&nbsp;</td>
	        <td><asp:PlaceHolder ID="phProfiles" runat="server"></asp:PlaceHolder></td>
	    </tr>
	</table>
	<p>
    <asp:Button ID="btnSubmit" Runat="server" CssClass="smallText" Text="Submit"></asp:Button>
    </p>
</div>
</asp:Panel>
