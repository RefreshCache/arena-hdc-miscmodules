<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DisplaySignEntry.ascx.cs" CodeBehind="DisplaySignEntry.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.MiscModules.DisplaySignEntry" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>


<style type="text/css">
/* css for timepicker */
.ui-timepicker-div .ui-widget-header { margin-bottom: 8px; }
.ui-timepicker-div dl { text-align: left; }
.ui-timepicker-div dl dt { height: 25px; margin-bottom: -25px; }
.ui-timepicker-div dl dd { margin: 0 10px 10px 65px; }
.ui-timepicker-div td { font-size: 90%; }
.ui-tpicker-grid-label { background: none; border: none; margin: 0; padding: 0; }

span.formTitle { display: inline-block; min-width: 100px; }
</style>

<script type="text/javascript">
    function selectDocument(documentId, documentTitle, state, modalID)
    {
        document.getElementById('<%= ihBlobID.ClientID %>').value = documentId;
            $find(modalID).hide();
            //Call Async Postback
            <%= Page.ClientScript.GetPostBackEventReference(btnRefreshDocument, null) %>
    }
</script>


<asp:Label ID="lbMessage" runat="server" Visible="false" Text="" />

<div>
    <span class="formTitle">Title:</span>
    <span class="formItem"><asp:TextBox ID="tbTitle" runat="server"></asp:TextBox></span>
</div>
<div>
    <span class="formTitle">Start Date:</span>
    <span class="formItem"><asp:TextBox ID="tbStartDate" runat="server"></asp:TextBox></span>
</div>
<div>
    <span class="formTitle">End Date:</span>
    <span class="formItem"><asp:TextBox ID="tbEndDate" runat="server"></asp:TextBox></span>
</div>
<div>
    <span class="formTitle">Topic Area:</span>
    <span class="formItem"><asp:DropDownList ID="ddlTopicArea" runat="server" /></span>
</div>
<div>
    <span class="formTitle">Weekly:</span>
    <span class="formItem"><asp:CheckBox ID="cbWeekly" runat="server" /></span>
</div>

<div style="margin-top: 30px;">
    <input type="hidden" id="ihBlobID" runat="server" />
    <asp:Button ID="btnRefreshDocument" runat="server" Style="display: none" />
    
    <asp:HiddenField ID="hfDocumentOrder" runat="server" />
    <asp:Button ID="btnUpdateDocumentOrder" runat="server" style="display: none;" OnClick="btnUpdateDocumentOrder_Click" />
    <Arena:DataGrid ID="dgDocuments" runat="server" Width="100%" DataKeyField="GUID">
        <Columns>
            <asp:TemplateColumn HeaderText="Preview" HeaderStyle-Width="200px">
                <ItemTemplate>
                    <img src="CachedBlob.aspx?guid=<%# Eval("GUID") %>&width=160" alt="Image" />
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn HeaderText="Title" DataField="Title" />
            <asp:BoundColumn HeaderText="OriginalFileName" DataField="OriginalFileName" />
        </Columns>
    </Arena:DataGrid>
    
    <asp:LinkButton Text="Add" runat="server" ID="lbAddItem" />
    <asp:Panel ID="pnlWrongSize" Visible="false" runat="server" style="color: Red;">
        <asp:Literal ID="ltWrongSize" runat="server"></asp:Literal>
    </asp:Panel>
    <Arena:ModalPopupIFrame ID="mdlDocuments" runat="server" />
</div>

<div style="margin-top: 30px;">
    <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
</div>
