namespace ArenaWeb.UserControls.Custom.HDC.Misc
{
	using System;
	using System.Text;
    using System.Text.RegularExpressions;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Web.Security;
	using Arena.Core;
	using Arena.Enums;
	using Arena.Exceptions;
	using Arena.Portal;
	using Arena.Security;
    using Arena.Organization;

	/// <summary>
	///		This control overrides the default behavior of the Arena
    ///		new account module. It auto-generates a username for the
    ///		user based upon their first and last names. This override
    ///		is only done if the Allow Custom Login option is turned
    ///		off (on by default), otherwise the behavior is the same
    ///		as the original.
    ///		This control also strips out any whitespace before and
    ///		after the user's first and last names. (Prevents login
    ///		names with 2 spaces between the first/last name)
	/// </summary>
	public partial class NewAccount : PortalControl
	{
		#region Module Settings

		// Module Settings
        [PageSetting("Redirect Page", "The page that the user will be redirected to after creating their account.", true)]
        public string RedirectPageIDSetting { get { return Setting("RedirectPageID", "", true); } }

        [PageSetting("Request Login Info Page", "The page that the user can use to request their login information.", true)]
        public string RequestInfoPageIDSetting { get { return Setting("RequestInfoPageID", "", true); } }

		[LookupSetting("Member Status", "The Member Status Lookup value to set a user to when they add themself through this form.", true, "0B4532DB-3188-40F5-B188-E7E6E4448C85")]
		public string MemberStatusIDSetting { get { return Setting("MemberStatusID", "", true); } }
		
        [CampusSetting("Default Campus", "The campus to assign a user to when they add themself through this form.", false)]
        public string CampusSetting { get { return Setting("Campus", "", false); } }

        [TagSetting("Profile ID", "An optional profile ID that the user will be automatically added to when they complete this new account form.", false)]
		public string ProfileIDSetting { get { return Setting("ProfileID", "", false); } }

		[LookupSetting("Source ID", "If using the Profile ID setting, then this value must be set to a valid Profile Source lookup value.", false, "43DB58F9-C43F-4913-84FF-2E3CEA59C134")]
		public string SourceLUIDSetting { get { return Setting("SourceLUID", "", false); } }

		[LookupSetting("Status ID", "If using the Profile ID setting, then this value must be set to a valid Profile Status lookup value.", false, "705F785D-36DB-4BF2-9C35-2A7F72A55731")]
		public string StatusLUIDSetting { get { return Setting("StatusLUID", "", false); } }

        [BooleanSetting("Custom Login ID", "If set to true then the user is allowed to customize their login ID, otherwise one is assigned as 'firstname lastname'.", true, true)]
        public bool CustomLoginSetting { get { return Convert.ToBoolean(Setting("CustomLogin", "true", true)); } }
		#endregion
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
			pnlMessage.Visible = false;
			lblMessage.Visible = false;
			pnlEmailExists.Visible = false;
            if (CustomLoginSetting == true)
            {
                lblDesiredLoginID.Visible = true;
                tbLoginID.Visible = true;
                reqLoginID.Enabled = true;
            }
            else
            {
                lblDesiredLoginID.Visible = false;
                tbLoginID.Visible = false;
                reqLoginID.Enabled = false;
            }

			if (!Page.IsPostBack)
			{
				iRedirect.Value = string.Empty;
				if (Request.QueryString["requestpage"] != null)
					iRedirect.Value = string.Format("default.aspx?page={0}", Request.QueryString["requestpage"]);
				if (iRedirect.Value == string.Empty && Request.QueryString["requestUrl"] != null)
					iRedirect.Value = Request.QueryString["requestUrl"];
				if (iRedirect.Value == string.Empty)
					iRedirect.Value = string.Format("default.aspx?page={0}", RedirectPageIDSetting);

				LookupType maritalStatus = new LookupType(SystemLookupType.MaritalStatus);
				maritalStatus.Values.LoadDropDownList(ddlMaritalStatus);

                //
                // Setup the password strength regular expression.
                //
                regexPassword.ValidationExpression = Arena.Security.Login.GetPasswordStrengthRegularExpression(CurrentPortal.OrganizationID);
                regexPassword.ErrorMessage = Arena.Security.Login.GetPasswordStrengthDescription(CurrentPortal.OrganizationID);
            }

			StringBuilder sbScript = new StringBuilder();
			sbScript.Append("\n\n<script language=\"javascript\" FOR=\"document\" EVENT=\"onreadystatechange\">\n");
			sbScript.Append("\tif(document.readyState==\"complete\");\n");
			sbScript.Append("\t{\n");
			sbScript.AppendFormat("\t\tdocument.frmMain.{0}.value = document.frmMain.{1}.value;\n", tbPassword.ClientID, iPassword.ClientID);
			sbScript.AppendFormat("\t\tdocument.frmMain.{0}.value = document.frmMain.{1}.value;\n", tbPassword2.ClientID, iPassword.ClientID);
			sbScript.Append("\t}\n");
			sbScript.Append("</script>\n\n");
			Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "setPassword", sbScript.ToString());
			Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "setPassword", sbScript.ToString());
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
			iPassword.Value = tbPassword.Text;
		}

		private void btnSubmit_Click(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
                tbFirstName.Text = tbFirstName.Text.Trim();
                tbLastName.Text = tbLastName.Text.Trim();
				if (tbEmail.Text.Trim().ToLower() != iEmail.Value)
				{
					PersonCollection people = new PersonCollection();
					people.LoadByEmail(tbEmail.Text.Trim());
					if (people.Count > 0)
					{
						phExistingAccounts.Controls.Clear();

						StringBuilder sbNames = new StringBuilder();
						foreach(Person person in people)
							foreach(Arena.Security.Login login in person.Logins)
								{
									sbNames.AppendFormat("{0} - <a href='default.aspx?page={1}&email={2}'>Send Info</a><br>",
										person.FullName,
										RequestInfoPageIDSetting,
										tbEmail.Text.Trim());

									break;
								}

						if (sbNames.Length > 0)
						{
							phExistingAccounts.Controls.Add(new LiteralControl(sbNames.ToString()));
							pnlMessage.Visible = true;
							pnlEmailExists.Visible = true;
							iEmail.Value = tbEmail.Text.Trim().ToLower();
						}
						else
							CreateAccount();
					}
					else
						CreateAccount();
				}
				else
					CreateAccount();
			}
			else
				Page.FindControl("valSummary").Visible = true;
		}

		private void btnCreate_Click(object sender, EventArgs e)
		{
			if (Page.IsValid)
				CreateAccount();
			else
				Page.FindControl("valSummary").Visible = true;
		}

        private void btnContinue_Click(object sender, EventArgs e)
        {
            // Redirect browser back to originating page
            Response.Redirect(iRedirect.Value);
        }

        private void CreateAccount()
        {
            Arena.Security.Login login;
            string loginID;

            if (CustomLoginSetting == true)
            {
                // Ensure that login ID is unique 
                loginID = tbLoginID.Text;
                login = new Arena.Security.Login(loginID);
                if (login.PersonID != -1)
                {
                    int loginCount = 0;
                    loginID = tbFirstName.Text.Substring(0, 1).ToLower() + tbLastName.Text.Trim().ToLower();
                    if (loginID != loginID.ToLower())
                        login = new Arena.Security.Login(loginID);

                    while (login.PersonID != -1)
                    {
                        loginCount++;
                        login = new Arena.Security.Login(loginID + loginCount.ToString());
                    }

                    lblMessage.Text = "The Desired Login ID you selected is already in use in our system.  Please select a different Login ID.  Suggestion: <b>" + loginID + loginCount.ToString() + "</b>";
                    pnlMessage.Visible = true;
                    lblMessage.Visible = true;

                    return;
                }
            }
            else
            {
                Int32 loginCount = 0;

                //
                // Construct a login Id that can be used.
                //
                do
                {
                    if (loginCount == 0)
                        loginID = tbFirstName.Text + " " + tbLastName.Text;
                    else
                        loginID = tbFirstName.Text + " " + tbLastName.Text + loginCount.ToString();
                    loginID = loginID.ToLower();

                    login = new Arena.Security.Login(loginID);
                    loginCount++;
                } while (login.PersonID != -1);
            }

            Lookup memberStatus;
            try
            {
                memberStatus = new Lookup(Int32.Parse(MemberStatusIDSetting));
                if (memberStatus.LookupID == -1)
                    throw new ModuleException(CurrentPortalPage, CurrentModule, "Member Status setting must be a valid Member Status Lookup value.");
            }
            catch (System.Exception ex)
            {
                throw new ModuleException(CurrentPortalPage, CurrentModule, "Member Status setting must be a valid Member Status Lookup value.", ex);
            }

            int organizationID = CurrentPortal.OrganizationID;
            string userID = CurrentUser.Identity.Name;
            if (userID == string.Empty)
                userID = "NewAccount.ascx";

            Person person = new Person();
            person.RecordStatus = RecordStatus.Pending;
            person.MemberStatus = memberStatus;

            if (CampusSetting != string.Empty)
                try { person.Campus = new Arena.Organization.Campus(Int32.Parse(CampusSetting)); }
                catch { person.Campus = null; }

            person.FirstName = tbFirstName.Text.Trim();
            person.LastName = tbLastName.Text.Trim();

            if (tbBirthDate.Text.Trim() != string.Empty)
                try { person.BirthDate = DateTime.Parse(tbBirthDate.Text); }
                catch { }

            if (ddlMaritalStatus.SelectedValue != string.Empty)
                person.MaritalStatus = new Lookup(Int32.Parse(ddlMaritalStatus.SelectedValue));

            if (ddlGender.SelectedValue != string.Empty)
                try { person.Gender = (Gender)Enum.Parse(typeof(Gender), ddlGender.SelectedValue); }
                catch { }

            PersonAddress personAddress = new PersonAddress();
            personAddress.Address = new Address(
                tbStreetAddress.Text.Trim(),
                string.Empty,
                tbCity.Text.Trim(),
                ddlState.SelectedValue,
                tbZipCode.Text.Trim(),
                false);
            personAddress.AddressType = new Lookup(SystemLookup.AddressType_Home);
            personAddress.Primary = true;
            person.Addresses.Add(personAddress);

            PersonPhone phone = new PersonPhone();
            phone.Number = tbHomePhone.PhoneNumber.Trim();
            phone.PhoneType = new Lookup(SystemLookup.PhoneType_Home);
            person.Phones.Add(phone);

            if (tbWorkPhone.PhoneNumber.Trim() != string.Empty)
            {
                phone = new PersonPhone();
                phone.Number = tbWorkPhone.PhoneNumber.Trim();
                phone.Extension = tbWorkPhone.Extension;
                phone.PhoneType = new Lookup(SystemLookup.PhoneType_Business);
                person.Phones.Add(phone);
            }

            if (tbCellPhone.PhoneNumber.Trim() != string.Empty)
            {
                phone = new PersonPhone();
                phone.Number = tbCellPhone.PhoneNumber.Trim();
                phone.PhoneType = new Lookup(SystemLookup.PhoneType_Cell);
                phone.SMSEnabled = cbSMS.Checked;
                person.Phones.Add(phone);
            }

            if (tbEmail.Text.Trim() != string.Empty)
            {
                PersonEmail personEmail = new PersonEmail();
                personEmail.Active = true;
                personEmail.Email = tbEmail.Text.Trim();
                person.Emails.Add(personEmail);
            }

            person.Save(organizationID, userID, false);
            person.SaveAddresses(organizationID, userID);
            person.SavePhones(organizationID, userID);
            person.SaveEmails(organizationID, userID);

            Family family = new Family();
            family.OrganizationID = organizationID;
            family.FamilyName = tbLastName.Text.Trim() + " Family";
            family.Save(userID);

            FamilyMember fm = new FamilyMember(family.FamilyID, person.PersonID);
            fm.FamilyID = family.FamilyID;
            fm.FamilyRole = new Lookup(SystemLookup.FamilyRole_Adult);
            fm.Save(userID);

            Arena.Security.Login personLogin = new Arena.Security.Login();
            personLogin.PersonID = person.PersonID;
            personLogin.LoginID = loginID;
            personLogin.Password = tbPassword.Text.Trim();
            personLogin.Active = true;
            personLogin.Save(userID);

            // Use security system to set the UserID within a client-side Cookie
            FormsAuthentication.SetAuthCookie(personLogin.LoginID, false);
            Response.Cookies["portalroles"].Value = string.Empty;

            if (ProfileIDSetting != string.Empty)
            {
                int profileID = -1;
                int sourceLUID = -1;
                int statusLUID = -1;

                try
                {
                    if (ProfileIDSetting.Contains("|"))
                        profileID = Int32.Parse(ProfileIDSetting.Split('|')[1]);
                    else
                        profileID = Int32.Parse(ProfileIDSetting);

                    sourceLUID = Int32.Parse(SourceLUIDSetting);
                    statusLUID = Int32.Parse(StatusLUIDSetting);
                }
                catch (System.Exception ex)
                {
                    throw new ModuleException(CurrentPortalPage, CurrentModule, "If using a ProfileID setting for the NewAccount module, " +
                        "then a valid numeric 'ProfileID', 'SourceLUID', and 'StatusLUID' setting must all be used!", ex);
                }

                Profile profile = new Profile(profileID);
                Lookup sourceLu = new Lookup(sourceLUID);
                Lookup statusLu = new Lookup(statusLUID);

                if (profile.ProfileID != -1 && sourceLu.LookupID != -1 && statusLu.LookupID != -1)
                {
                    ProfileMember profileMember = new ProfileMember();
                    profileMember.ProfileID = profile.ProfileID;
                    profileMember.PersonID = person.PersonID;
                    profileMember.Source = sourceLu;
                    profileMember.Status = statusLu;
                    profileMember.DatePending = DateTime.Now;
                    profileMember.Save(userID);

                    if (profile.ProfileType == ProfileType.Serving)
                    {
                        ServingProfile sProfile = new ServingProfile(profile.ProfileID);
                        ServingProfileMember sMember = new ServingProfileMember(profileMember.ProfileID, profileMember.PersonID);
                        sMember.HoursPerWeek = sProfile.DefaultHoursPerWeek;
                        sMember.Save();
                    }
                }
                else
                {
                    throw new ModuleException(CurrentPortalPage, CurrentModule, "'ProfileID', 'SourceLUID', and 'StatusLUID' must all be valid IDs");
                }
            }

            //
            // If we are letting the user pick their own login ID then just redirect
            // the browser back to the originating page. Otherwise put up some text to
            // tell the user what their new login ID is.
            //
            if (CustomLoginSetting == true)
                Response.Redirect(iRedirect.Value);
            else
            {
                pnlCreateAccount.Visible = false;
                lbLoginCreated.Text = "Your account has been created. Your login ID is \"" + loginID + "\".<BR /><BR />You may use this login ID the next time you visit this site.<BR />";
                pnlLoginCreated.Visible = true;
            }
        }

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnSubmit.Click += new EventHandler(btnSubmit_Click);
			this.btnCreate.Click += new EventHandler(btnCreate_Click);
            this.btnContinue.Click += new EventHandler(btnContinue_Click);
		}
		#endregion

	}
}
