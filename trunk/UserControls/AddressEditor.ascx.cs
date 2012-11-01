using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
using Arena.Portal;
using Arena.Portal.UI;

namespace ArenaWeb.UserControls.Custom.HDC.MiscModules
{
    public partial class AddressEditor : PortalControl
    {
        private int family_id = -1;


        #region Module Settings

        [LookupSetting("Main Address", "The Main/Home address. When this address is edited via bulk update the old address will be stored in the Previous Address as defined below.", false, "9B4BE12C-C105-4F80-8254-8639B27D7640")]
        public string MainAddressSetting { get { return Setting("MainAddress", null, false); } }

        [LookupSetting("Previous Address", "The Previous Address to use for storing a previous address during a bulk update operation.", false, "9B4BE12C-C105-4F80-8254-8639B27D7640")]
        public string PreviousAddressSetting { get { return Setting("PreviousAddress", null, false); } }

        #endregion


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                btnEditSave.OnClientClick = "$(document).bind('cbox_closed', function() { " + Page.ClientScript.GetPostBackEventReference(btnEditSave, "false") + "; }); $.colorbox.close(); return false;";
                btnBulkSave.OnClientClick = "$(document).bind('cbox_closed', function() { " + Page.ClientScript.GetPostBackEventReference(btnBulkSave, "false") + "; }); $.colorbox.close(); return false;";
                btnFinished.Visible = (!String.IsNullOrEmpty(Request.QueryString["REDIRECT"]));
                ArenaWeb.Utilities.LoadCountries(ddlEditCountry);
                ArenaWeb.Utilities.LoadCountries(ddlBulkCountry);

                LookupType lktype = new LookupType(new Guid("9B4BE12C-C105-4F80-8254-8639B27D7640"));
                lktype.Values.LoadDropDownList(ddlBulkType);
            }
        }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //
            // Add in all the scripts we need.
            //
            BasePage.AddJavascriptInclude(Page, BasePage.JQUERY_INCLUDE);
            BasePage.AddJavascriptInclude(Page, "UserControls/Custom/HDC/Misc/Includes/jquery.colorbox-min.js");
            BasePage.AddCssLink(Page, "UserControls/Custom/HDC/Misc/Includes/colorbox.css");
            
            //
            // Determine the family_id and load the controls for viewstate.
            //
            family_id = Convert.ToInt32(Request.QueryString["FAMILY_ID"]);
            LoadFamilyAddresses(family_id);
        }


        private void LoadFamilyAddresses(int family_id)
        {
            Family family = new Family(family_id);


            //
            // Make sure we are in a known valid state.
            //
            while (tblPeople.Rows.Count > 2)
                tblPeople.Rows.RemoveAt(1);

            foreach (FamilyMember fm in family.FamilyMembers)
            {
                PersonAddressCollection pac = new PersonAddressCollection();
                bool first = true;
                TableRow row = null;
                TableCell cell = null;
                Literal lt;

                //
                // Sort addresses by primary and then all the rest (in order of lookup), I don't
                // know a better way to do this.
                //
                foreach (PersonAddress address in fm.Addresses)
                {
                    if (address.Primary)
                        pac.Add(address);
                }
                foreach (PersonAddress address in fm.Addresses)
                {
                    if (!address.Primary)
                        pac.Add(address);
                }
                if (pac.Count == 0)
                    pac.Add(new PersonAddress());

                foreach (PersonAddress address in pac)
                {
                    ArenaCheckBox cb;

                    row = new TableRow();
                    tblPeople.Rows.AddAt(tblPeople.Rows.Count - 1, row);

                    //
                    // Add in the checkbox + name if applicable.
                    //
                    cell = new TableCell();
                    row.Cells.Add(cell);
                    if (first == true)
                    {
                        cb = new ArenaCheckBox();

                        cell.Controls.Add(cb);
                        cb.ID = "cbPerson_" + fm.PersonID.ToString();
                        cb.Text = fm.NickName;
                        cb.CssClass = "personName";
                        if (fm.RecordStatus == Arena.Enums.RecordStatus.Inactive)
                        {
                            cb.Text += " (Inactive)";
                            row.CssClass += " inactive";
                        }
                        else if (fm.RecordStatus == Arena.Enums.RecordStatus.Pending)
                        {
                            cb.Text += " (Pending)";
                            row.CssClass += " pending";
                        }
                        cb.Checked = false;
                    }
                    first = false;

                    if (address.AddressID != -1)
                    {
                        //
                        // Add in the address type.
                        //
                        cell = new TableCell();
                        row.Cells.Add(cell);
                        DropDownList ddlType = new DropDownList();
                        cell.Controls.Add(ddlType);
                        ddlType.ID = "ddlType_" + fm.PersonID.ToString() + "_" + address.AddressID.ToString() + "_" + address.AddressType.LookupID.ToString();
                        foreach (Lookup lookup in new LookupType(new Guid("9B4BE12C-C105-4F80-8254-8639B27D7640")).Values)
                        {
                            ddlType.Items.Add(new ListItem(lookup.Value, lookup.LookupID.ToString()));
                        }
                        ddlType.SelectedValue = address.AddressType.LookupID.ToString();
                        lt = new Literal();
                        cell.Controls.Add(lt);
                        lt.Text = "<span id=\"" + ddlType.ClientID + "_error\" class=\"errorText\" style=\"display: none;\"> *</span>";

                        //
                        // Add in the Address itself.
                        //
                        cell = new TableCell();
                        row.Cells.Add(cell);
                        Literal ltAddress = new Literal();
                        cell.Controls.Add(ltAddress);
                        ltAddress.Text = address.Address.ToString();

                        //
                        // Add in the primary checkbox.
                        //
                        cell = new TableCell();
                        row.Cells.Add(cell);
                        cell.CssClass = "button";
                        cb = new ArenaCheckBox();
                        cell.Controls.Add(cb);
                        cb.ID = "cbPrimary_" + fm.PersonID.ToString() + "_" + address.AddressID.ToString() + "_" + address.AddressType.LookupID.ToString();
                        cb.Attributes.Add("cbPrimary", fm.PersonID.ToString());
                        cb.Checked = address.Primary;

                        //
                        // Add in the Edit button.
                        //
                        cell = new TableCell();
                        row.Cells.Add(cell);
                        cell.CssClass = "button";
                        ArenaImageButton image = new ArenaImageButton();
                        cell.Controls.Add(image);
                        image.ID = "btnEdit_" + fm.PersonID.ToString() + "_" + address.AddressID.ToString() + "_" + address.AddressType.LookupID.ToString();
                        image.OnClientClick = ClientEditAddressScript(fm, address) + "return false;";
                        image.ImageUrl = "~/images/edit.gif";

                        //
                        // Add in the Delete button.
                        //
                        cell = new TableCell();
                        row.Cells.Add(cell);
                        cell.CssClass = "button";
                        image = new ArenaImageButton();
                        cell.Controls.Add(image);
                        image.ID = "btnDelete_" + fm.PersonID.ToString() + "_" + address.AddressID.ToString() + "_" + address.AddressType.LookupID.ToString();
                        image.Click += new ImageClickEventHandler(btnDeleteAddress_Click);
                        image.OnClientClick = "return confirm('Are you sure you want to remove " + fm.NickName.Replace("'", "\\'") + "\\'s " + address.AddressType.Value.Replace("'", "\\'") + "?');";
                        image.ImageUrl = "~/images/delete.gif";
                    }
                    else
                    {
                        cell = new TableCell();
                        row.Cells.Add(cell);
                        cell.ColumnSpan = 5;
                    }
                }

                if (row != null)
                {
                    row = new TableRow();
                    tblPeople.Rows.AddAt(tblPeople.Rows.Count - 1, row);
                    cell = new TableCell();
                    row.Cells.Add(cell);
                    cell.ColumnSpan = 6;
                    row.CssClass = "spacer";
                }
            }
        }


        protected void btnEditSave_Click(object sender, EventArgs e)
        {
            Person person = new Person(Convert.ToInt32(hfEditPerson.Value));
            PersonAddress address = person.Addresses.FindByType(Convert.ToInt32(hfEditType.Value));
            PersonAddressCollection pac = new PersonAddressCollection();


            //
            // Verify valid data.
            //
            if (address.AddressID == -1)
                throw new System.Exception("Invalid address type during edit.");

            //
            // Make sure this is the only person using this address, otherwise create a new one.
            //
            pac.LoadByAddressID(address.AddressID);
            if (pac.Count > 1)
            {
                person.Addresses.Remove(address);
                address = new PersonAddress();
                address.PersonID = person.PersonID;
                address.Address = new Address();
                address.AddressType = new Lookup(Convert.ToInt32(hfEditType.Value));
                person.Addresses.Add(address);
            }

            //
            // Set the address information.
            //
            address.Address.StreetLine1 = tbEditStreetAddress1.Text.Trim();
            address.Address.StreetLine2 = tbEditStreetAddress2.Text.Trim();
            address.Address.City = tbEditCity.Text.Trim();
            address.Address.State = tbEditState.Text.Trim();
            address.Address.PostalCode = tbEditPostal.Text.Trim();
            address.Address.Country = ddlEditCountry.SelectedValue;

            //
            // If this address is becoming primary (and wasn't already) make sure it is the
            // only one.
            //
            if (cbEditPrimary.Checked && address.Primary == false)
            {
                foreach (PersonAddress pa in person.Addresses)
                {
                    pa.Primary = false;
                }
            }
            address.Primary = cbEditPrimary.Checked;

            //
            // Standardize and save.
            //
            address.Address.Standardize();
            person.SaveAddresses(person.OrganizationID, CurrentUser.Identity.Name);
            LoadFamilyAddresses(family_id);
            cbSelectAll.Checked = false;
        }


        protected void btnDeleteAddress_Click(object sender, ImageClickEventArgs e)
        {
            ArenaImageButton button = (ArenaImageButton)sender;
            PersonAddress pa;
            Person person;
            int personID, addressType;


            //
            // Grab the information from the ID that we will use later.
            //
            personID = Convert.ToInt32(button.ID.Split(new char[] { '_' })[1]);
            addressType = Convert.ToInt32(button.ID.Split(new char[] { '_' })[3]);

            //
            // Find the person and address record and remove it.
            //
            person = new Person(personID);
            pa = person.Addresses.FindByType(addressType);
            person.Addresses.Remove(pa);

            person.SaveAddresses(person.OrganizationID, CurrentUser.Identity.Name);

            LoadFamilyAddresses(family_id);
        }


        protected void btnBulkUpdate_Click(object sender, EventArgs e)
        {
            List<String> pids = new List<String>();
            bool first = true, personActive = false;
            int i;

            //
            // Empty out the fields.
            //
            tbBulkStreetAddress1.Text = "";
            tbBulkStreetAddress2.Text = "";
            tbBulkCity.Text = "";
            tbBulkState.Text = "";
            tbBulkPostal.Text = "";
            ddlBulkCountry.SelectedValue = "US";
            cbBulkPrimary.Checked = false;

            for (i = 1; i < tblPeople.Rows.Count - 1; i++)
            {
                ArenaCheckBox cb;
                DropDownList ddl;

                if (tblPeople.Rows[i].Cells[0].Controls.Count > 0)
                {
                    cb = (ArenaCheckBox)tblPeople.Rows[i].Cells[0].Controls[0];
                    if (cb.Checked)
                    {
                        personActive = true;
                        pids.Add(cb.ID.Split('_')[1]);
                    }
                    else
                        personActive = false;
                }

                //
                // Skip spacer rows.
                //
                if (tblPeople.Rows[i].Cells.Count < 5)
                    continue;

                ddl = (DropDownList)tblPeople.Rows[i].Cells[1].Controls[0];

                //
                // If this person is active, is the right address type and the first one then
                // set the bulk update fields to match the current address.
                //
                if (personActive == true && ddl.ID.Split('_')[3] == ddlBulkType.SelectedValue && first)
                {
                    Address address = new Address(Convert.ToInt32(ddl.ID.Split('_')[2]));

                    first = false;

                    cb = (ArenaCheckBox)tblPeople.Rows[i].Cells[3].Controls[0];
                    tbBulkStreetAddress1.Text = address.StreetLine1;
                    tbBulkStreetAddress2.Text = address.StreetLine2;
                    tbBulkCity.Text = address.City;
                    tbBulkState.Text = address.State;
                    tbBulkPostal.Text = address.PostalCode;
                    ddlBulkCountry.SelectedValue = address.Country;
                    cbBulkPrimary.Checked = cb.Checked;
                }
            }

            lbBulkHeader.Text = "Bulk update " + ddlBulkType.SelectedItem.Text.Replace("'", "\\'");
            hfBulkType.Value = ddlBulkType.SelectedValue;
            hfBulkIDs.Value = String.Join(",", pids.ToArray());
            ScriptManager.RegisterStartupScript(this, this.GetType(), "bulk_update_start", "$(document).ready(function() { $.colorbox({inline: true, href:$('#" + pnlBulkUpdate.ClientID + "'), opacity:0.3}); });", true);
        }


        protected void btnBulkSave_Click(object sender, EventArgs e)
        {
            List<String> pids = hfBulkIDs.Value.Split(',').ToList();

            foreach (String pid in pids)
            {
                Person person;
                PersonAddress address;

                person = new Person(Convert.ToInt32(pid));
                address = person.Addresses.FindByType(Convert.ToInt32(hfBulkType.Value));

                //
                // Check if we need to store this address as a previous address for the person.
                //
                if (!String.IsNullOrEmpty(MainAddressSetting) && Convert.ToInt32(hfBulkType.Value) == Convert.ToInt32(MainAddressSetting) &&
                    !String.IsNullOrEmpty(PreviousAddressSetting) && address != null)
                {
                    PersonAddress previous;

                    previous = new PersonAddress();
                    previous.AddressType = new Lookup(Convert.ToInt32(PreviousAddressSetting));
                    previous.PersonID = person.PersonID;
                    person.Addresses.Add(previous);

                    previous.Address.StreetLine1 = address.Address.StreetLine1;
                    previous.Address.StreetLine2 = address.Address.StreetLine2;
                    previous.Address.City = address.Address.City;
                    previous.Address.State = address.Address.State;
                    previous.Address.PostalCode = address.Address.PostalCode;
                    previous.Address.Country = address.Address.Country;

                    //
                    // Maybe this isn't needed because it should already be standardized, but better safe than sorry.
                    //
                    previous.Address.Standardize();
                }

                //
                // Generate a new address for this person. Editing the existing seems to cause
                // weird things to happen.
                //
                address = new PersonAddress();
                address.AddressType = new Lookup(Convert.ToInt32(hfBulkType.Value));
                address.PersonID = person.PersonID;
                person.Addresses.Add(address);

                //
                // Store the address.
                //
                address.Address.StreetLine1 = tbBulkStreetAddress1.Text.Trim();
                address.Address.StreetLine2 = tbBulkStreetAddress2.Text.Trim();
                address.Address.City = tbBulkCity.Text.Trim();
                address.Address.State = tbBulkState.Text.Trim();
                address.Address.PostalCode = tbBulkPostal.Text.Trim();
                address.Address.Country = ddlBulkCountry.SelectedValue;

                //
                // If this address is becoming primary (and wasn't already) make sure it is the
                // only one.
                //
                if (cbBulkPrimary.Checked && address.Primary == false)
                {
                    foreach (PersonAddress pa in person.Addresses)
                    {
                        pa.Primary = false;
                    }
                }
                address.Primary = cbBulkPrimary.Checked;

                //
                // Standardize and save.
                //
                address.Address.Standardize();
                person.SaveAddresses(person.OrganizationID, CurrentUser.Identity.Name);
            }

            LoadFamilyAddresses(family_id);
            cbSelectAll.Checked = false;
        }

        
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int personID, addressID, addressType, i;
            Boolean saveAddresses = false;
            Person person = null;
            DropDownList ddlType;
            ArenaCheckBox cb;


            for (i = 1; i < (tblPeople.Rows.Count - 1); i++)
            {
                if (tblPeople.Rows[i].Cells.Count < 6 || tblPeople.Rows[i].Cells[1].Controls.Count < 1)
                    continue;

                ddlType = (DropDownList)tblPeople.Rows[i].Cells[1].Controls[0];
                cb = (ArenaCheckBox)tblPeople.Rows[i].Cells[3].Controls[0];

                //
                // Grab the information from the ID that we will use later.
                //
                personID = Convert.ToInt32(ddlType.ID.Split(new char[] { '_' })[1]);
                addressID = Convert.ToInt32(ddlType.ID.Split(new char[] { '_' })[2]);
                addressType = Convert.ToInt32(ddlType.ID.Split(new char[] { '_' })[3]);

                //
                // Check if we are on a new person. If so see if the old person needs to be saved.
                //
                if (person == null || person.PersonID != personID)
                {
                    if (person != null && saveAddresses)
                        person.SaveAddresses(person.OrganizationID, CurrentUser.Identity.Name);

                    person = new Person(personID);
                }

                //
                // If the address type nor the primary flag hasn't changed, ignore.
                //
                PersonAddress pa = person.Addresses.FindByType(addressType);
                if (addressType != Convert.ToInt32(ddlType.SelectedValue))
                {
                    pa.AddressType = new Lookup(Convert.ToInt32(ddlType.SelectedValue));
                    saveAddresses = true;
                }
                if (cb.Checked != pa.Primary)
                {
                    pa.Primary = cb.Checked;
                    saveAddresses = true;
                }
            }

            //
            // Save the last person if we need to.
            //
            if (person != null && saveAddresses)
                person.SaveAddresses(person.OrganizationID, CurrentUser.Identity.Name);

            //
            // Refresh the display to get everything re-ordered.
            //
            LoadFamilyAddresses(family_id);
            cbSelectAll.Checked = false;
        }


        protected void btnFinished_Click(object sender, EventArgs e)
        {
            btnSave_Click(this, null);

            if (!String.IsNullOrEmpty(Request.QueryString["REDIRECT"]))
            {
                Response.Redirect(Request.QueryString["REDIRECT"]);
            }
        }

        
        private String ClientEditAddressScript(Person person, PersonAddress address)
        {
            StringBuilder sb = new StringBuilder();


            sb.AppendFormat("$('#{0}').text('{1}\\'s {2}');", lbEditHeader.ClientID, person.NickName.Replace("'", "\\'"), address.AddressType.Value.Replace("'", "\\'"));
            sb.AppendFormat("$('#{0}').val('{1}');", tbEditStreetAddress1.ClientID, address.Address.StreetLine1.Replace("'", "\\'"));
            sb.AppendFormat("$('#{0}').val('{1}');", tbEditStreetAddress2.ClientID, address.Address.StreetLine2.Replace("'", "\\'"));
            sb.AppendFormat("$('#{0}').val('{1}');", tbEditCity.ClientID, address.Address.City.Replace("'", "\\'"));
            sb.AppendFormat("$('#{0}').val('{1}');", tbEditState.ClientID, address.Address.State.Replace("'", "\\'"));
            sb.AppendFormat("$('#{0}').val('{1}');", tbEditPostal.ClientID, address.Address.PostalCode.Replace("'", "\\'"));
            sb.AppendFormat("$('#{0}').val('{1}');", ddlEditCountry.ClientID, address.Address.Country.Replace("'", "\\'"));
            sb.AppendFormat("$('#{0}').attr('checked', {1});", cbEditPrimary.ClientID, (address.Primary ? "true" : "false"));

            sb.AppendFormat("$('#{0}').val('{1}');", hfEditPerson.ClientID, person.PersonID.ToString());
            sb.AppendFormat("$('#{0}').val('{1}');", hfEditType.ClientID, address.AddressType.LookupID.ToString());

            sb.Append("$.colorbox({inline: true, href:$('#" + pnlEditAddress.ClientID + "'), opacity:0.3});");

            return sb.ToString();
        }
    }
}