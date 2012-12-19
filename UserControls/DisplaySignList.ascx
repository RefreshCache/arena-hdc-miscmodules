<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DisplaySignList.ascx.cs" CodeBehind="DisplaySignList.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.MiscModules.DisplaySignList" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<Arena:DataGrid ID="dgPromotions" runat="server" AllowSorting="true">
    <Columns>
        <asp:BoundColumn DataField="promotion_request_id" Visible="false"></asp:BoundColumn>
        <asp:TemplateColumn HeaderText="Title" SortExpression="title" ItemStyle-Wrap="false" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left">
            <ItemTemplate><%# GetFormattedTitle(DataBinder.Eval(Container.DataItem, "promotion_request_id"), DataBinder.Eval(Container.DataItem, "title")) %></ItemTemplate>
        </asp:TemplateColumn>
        <asp:TemplateColumn HeaderText="Start Date" SortExpression="web_start_date" ItemStyle-Wrap="false" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left">
            <ItemTemplate><%# GetFormattedDateLong(DataBinder.Eval(Container.DataItem, "web_start_date")) %></ItemTemplate>
        </asp:TemplateColumn>
        <asp:TemplateColumn HeaderText="End Date" SortExpression="web_end_date" ItemStyle-Wrap="false" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left">
            <ItemTemplate><%# GetFormattedDateLong(DataBinder.Eval(Container.DataItem, "web_end_date")) %></ItemTemplate>
        </asp:TemplateColumn>
        <asp:TemplateColumn HeaderText="Weekly" ItemStyle-Wrap="false" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left">
            <ItemTemplate><asp:Image ID="imgWeekly" runat="server" ImageUrl="~/images/check.gif" Visible='<%# (DataBinder.Eval(Container.DataItem, "web_summary").ToString() == "Weekly") %>' /></asp:Image></ItemTemplate>
        </asp:TemplateColumn>
    </Columns>
</Arena:DataGrid>
