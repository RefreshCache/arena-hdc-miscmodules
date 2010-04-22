<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BuildArenaSandbox.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.MiscModules.BuildArenaSandbox" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<pre>
<asp:Literal ID="lbStatus" runat="server"></asp:Literal>
</pre>

Please enter your password: <asp:TextBox ID="txtPassword" TextMode="Password" runat="server"></asp:TextBox><br />
<asp:Button ID="btnBuild" runat="server" OnClick="btnBuild_Click" />
