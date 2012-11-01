<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddressEditor.ascx.cs" CodeBehind="AddressEditor.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.MiscModules.AddressEditor" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<style type="text/css">
    table.peopleAddresses { font-size: small; }
    table.peopleAddresses > tbody > tr > th { font-weight: bold; text-align: left; }
    table.peopleAddresses > tbody > tr > th:not(.button) { min-width: 125px; }
    table.peopleAddresses > tbody > tr.pending > td > span.personName { color: Red; }
    table.peopleAddresses > tbody > td > span.personName { font-size: small; padding-left: 8px; }
    table.peopleAddresses > tbody > tr.buttons > td { padding-top: 5px; }
    table.peopleAddresses > tbody > tr > td { padding-left: 7px; padding-right: 7px; }
    table.peopleAddresses > tbody > tr > td.button { text-align: center; }
    table.peopleAddresses > tbody > tr.addressError { background-color: #ffff40; }
    table.peopleAddresses > tbody > tr.spacer { height: 10px; }

    div.editAddress > div.header { margin-bottom: 6px; padding: 3px; background-color: #e0e0f0; text-align: center; }
    span.editAddressTitle { font-size: small; display: inline-block; width: 120px; }
    div.editAddress > div > input { margin: 2px; }
</style>

<script type="text/javascript"">
    function selectAllPeople(obj) {
        $('span.personName > input').each(function () {
            $(this).attr('checked', $(obj).attr('checked'));
            setBulkUpdateState();
        });
    }

    function setBulkUpdateState() {
        if ($("input[id*='_cbPerson_']:checked").length == 0) {
            $("#<%= btnUpdateSeleted.ClientID %>").attr("disabled", "disabled");
        }
        else {
            $("#<%= btnUpdateSeleted.ClientID %>").removeAttr("disabled");
        }
    }

    //
    // Make sure there are no invalid addresses before saving.
    //
    function validateAddresses() {
        var addresses = $("select[id*='_ddlType_']");
        var count = 0;

        $(addresses).each(function () { if ($(this).next().is(':visible')) count++; });
        if (count > 0) {
            alert('One or more addresses share the same address type for a person, this is not allowed and must be fixed.');
            return false;
        }
    }

    $(document).ready(function () {
        setBulkUpdateState();

        //
        // If a user clicks on a individual person-checkbox, unselect the select all.
        //
        $("input[id*='_cbPerson_']").click(function () {
            $("input[id$='_cbSelectAll']").attr('checked', false);
            setBulkUpdateState();
        });

        //
        // Make sure only one primary checkbox is set per-person.
        //
        $("input[id*='_cbPrimary_']").click(function () {
            var id = $(this).attr('id');
            var personID = id.split("_cbPrimary_")[1].split("_")[0];
            if ($(this).attr('checked') === true) {
                $("input[id*='_cbPrimary_" + personID + "_']").each(function () {
                    if ($(this).attr('id') !== id)
                        $(this).attr('checked', false);
                });
            }
        });

        //
        // Make sure there are no duplicate address types per person.
        //
        $("select[id*='_ddlType_']").change(function () {
            var id = $(this).attr('id');
            var personID = id.split("_ddlType_")[1].split("_")[0];
            var addresses = $("select[id*='_ddlType_" + personID + "_']");

            //
            // Hide all error messages and then show then as needed.
            //
            $(addresses).next().each(function () { $(this).hide(); });
            $(addresses).closest('tr').each(function () { $(this).removeClass('addressError'); });
            $(addresses).each(function () {
                var value = $(this).val();
                var count = 0;

                $(addresses).each(function () { if ($(this).val() == value) count++; });
                if (count > 1)
                    $(addresses).each(function () {
                        $('#' + $(this).attr('id') + '_error').show();
                        $(this).closest('tr').addClass('addressError');
                    });
            });
        });
    });
</script>

<p>
    <Arena:ArenaCheckBox ID="cbSelectAll" runat="server" Text="Select All" onclick="selectAllPeople(this)" />
</p>

<div>
    <asp:Table ID="tblPeople" runat="server" class="peopleAddresses" CellSpacing="0">
        <asp:TableHeaderRow>
            <asp:TableHeaderCell>Name</asp:TableHeaderCell>
            <asp:TableHeaderCell>Type</asp:TableHeaderCell>
            <asp:TableHeaderCell>Address</asp:TableHeaderCell>
            <asp:TableHeaderCell CssClass="button">Primary</asp:TableHeaderCell>
            <asp:TableHeaderCell CssClass="button">Edit</asp:TableHeaderCell>
            <asp:TableHeaderCell CssClass="button">Delete</asp:TableHeaderCell>
        </asp:TableHeaderRow>

        <asp:TableRow CssClass="buttons">
            <asp:TableCell ColumnSpan="3" HorizontalAlign="Left">
                <asp:DropDownList ID="ddlBulkType" runat="server"></asp:DropDownList><br />
                <Arena:ArenaButton ID="btnUpdateSeleted" runat="server" Enabled="false" OnClick="btnBulkUpdate_Click" Text="Bulk Update Selected" />
            </asp:TableCell>
            <asp:TableCell ColumnSpan="3" HorizontalAlign="Right">
                <Arena:ArenaButton ID="btnFinished" runat="server" OnClick="btnFinished_Click" Text="Finished" />
                <Arena:ArenaButton ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" OnClientClick="return validateAddresses();" />
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
</div>

<div style="display: none;">
    <asp:Panel ID="pnlEditAddress" runat="server" class="editAddress" DefaultButton="btnEditSave">
        <div class="header"><asp:Label ID="lbEditHeader" runat="server"></asp:Label></div>
        <div><span class="editAddressTitle">Street Address:</span><asp:TextBox ID="tbEditStreetAddress1" runat="server" Width="240"></asp:TextBox></div>
        <div><span class="editAddressTitle">&nbsp;</span><asp:TextBox ID="tbEditStreetAddress2" runat="server" Width="240"></asp:TextBox></div>
        <div><span class="editAddressTitle">City:</span><asp:TextBox ID="tbEditCity" runat="server" Width="120"></asp:TextBox></div>
        <div><span class="editAddressTitle">State/Postal:</span><asp:TextBox ID="tbEditState" runat="server" Width="30"></asp:TextBox><asp:TextBox ID="tbEditPostal" runat="server" Width="100"></asp:TextBox></div>
        <div><span class="editAddressTitle">Country:</span><asp:DropDownList ID="ddlEditCountry" runat="server" Width="240"></asp:DropDownList></div>
        <div><span class="editAddressTitle">Primary/Mailing:</span><Arena:ArenaCheckBox ID="cbEditPrimary" runat="server" /></div>
        <div style="margin-top: 20px; text-align: right;">
            <asp:HiddenField ID="hfEditPerson" runat="server" Value="-1" />
            <asp:HiddenField ID="hfEditType" runat="server" Value="-1" />
            <Arena:ArenaButton ID="btnEditSave" runat="server" OnClick="btnEditSave_Click" Text="Save" />
        </div>
    </asp:Panel>
</div>

<div style="display: none;">
    <asp:Panel ID="pnlBulkUpdate" runat="server" class="editAddress" DefaultButton="btnBulkSave">
        <div class="header"><asp:Label ID="lbBulkHeader" runat="server"></asp:Label></div>
        <div><span class="editAddressTitle">Street Address:</span><asp:TextBox ID="tbBulkStreetAddress1" runat="server" Width="240"></asp:TextBox></div>
        <div><span class="editAddressTitle">&nbsp;</span><asp:TextBox ID="tbBulkStreetAddress2" runat="server" Width="240"></asp:TextBox></div>
        <div><span class="editAddressTitle">City:</span><asp:TextBox ID="tbBulkCity" runat="server" Width="120"></asp:TextBox></div>
        <div><span class="editAddressTitle">State/Postal:</span><asp:TextBox ID="tbBulkState" runat="server" Width="30"></asp:TextBox><asp:TextBox ID="tbBulkPostal" runat="server" Width="100"></asp:TextBox></div>
        <div><span class="editAddressTitle">Country:</span><asp:DropDownList ID="ddlBulkCountry" runat="server" Width="240"></asp:DropDownList></div>
        <div><span class="editAddressTitle">Primary/Mailing:</span><Arena:ArenaCheckBox ID="cbBulkPrimary" runat="server" /></div>
        <div style="margin-top: 20px; text-align: right;">
            <asp:HiddenField ID="hfBulkIDs" runat="server" />
            <asp:HiddenField ID="hfBulkType" runat="server" />
            <Arena:ArenaButton ID="btnBulkSave" runat="server" OnClick="btnBulkSave_Click" Text="Update" />
        </div>
    </asp:Panel>
</div>
