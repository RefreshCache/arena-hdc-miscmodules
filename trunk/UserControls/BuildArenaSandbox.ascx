<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BuildArenaSandbox.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.MiscModules.BuildArenaSandbox" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<asp:Panel ID="pnlError" runat="server" Visible="false">
  <center><div style="background-color: #ffbb33; border: solid 2px black; width: 450px; padding: 8px; margin-bottom: 20px;">
  Error occurred trying to authenticate and build your sandbox. Please try again or contact Daniel at HDC.
  </span></div>
</asp:Panel>

<asp:Panel ID="pnlReady" runat="server" Visible="false">
  <center><div style="background-color: #66bb33; border: solid 2px black; width: 450px; padding: 8px; margin-bottom: 20px;">
  <asp:Literal ID="ltReady" runat="server"></asp:Literal>
  </span></div>
</asp:Panel>

Select Arena version: <asp:DropDownList ID="ddlVersion" runat="server"></asp:DropDownList> <br />
<asp:Button ID="btnBuild" runat="server" OnClick="btnBuild_Click" Text="Build" /><br />

<small>(Only click Build once, it can take up to a minute to build your sandbox.)</small>
