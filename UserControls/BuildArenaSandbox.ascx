<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BuildArenaSandbox.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.MiscModules.BuildArenaSandbox" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<asp:Panel ID="pnlPassword" runat="server" Visible="false">
  <center><div style="background-color: #ffbb33; border: solid 2px black; width: 450px; padding: 8px; margin-bottom: 20px;">
  Unable to authenticate you with the password provided. Please try your password again.
  </span></div>
</asp:Panel>

<asp:Panel ID="pnlReady" runat="server" Visible="false">
  <center><div style="background-color: #66bb33; border: solid 2px black; width: 450px; padding: 8px; margin-bottom: 20px;">
  <asp:Literal ID="ltReady" runat="server"></asp:Literal>
  </span></div>
</asp:Panel>

Enter your password: <asp:TextBox ID="txtPassword" TextMode="Password" runat="server"></asp:TextBox><br />
Select Arena version: <asp:DropDownList ID="ddlVersion" runat="server"></asp:DropDownList> <br />
<asp:Button ID="btnBuild" runat="server" OnClick="btnBuild_Click" Text="Build" /><br />

<small>(Only click Build once, it can take up to a minute to build your sandbox.)</small>
