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
    ///     This control pulls a list of child tags from the given
    ///     module setting tag and presents a list of check-boxes
    ///     for the user to select which tag they are interested in
    ///     joining. If the tags are serving tags then they are
    ///     added in with the appropriate serving profile information.
	/// </summary>
	public partial class SelfJoinTags : PortalControl
	{
		#region Module Settings

		// Module Settings
		[PageSetting("Redirect Page", "The page that the user will be redirected to after joining tags.", true)]
		public string RedirectPageIDSetting { get { return Setting("RedirectPageID", "", true); } }

        [TagSetting("Profile ID", "This is the parent tag whose first level descendents will be used to populate the list.", true)]
        public string ProfileIDSetting { get { return Setting("ProfileID", "", true); } }

        [LookupSetting("Source ID", "This value must be set to a valid Profile Source lookup value.", true, "43DB58F9-C43F-4913-84FF-2E3CEA59C134")]
		public string SourceLUIDSetting { get { return Setting("SourceLUID", "", true); } }

		[LookupSetting("Status ID", "This value must be set to a valid Profile Status lookup value.", true, "705F785D-36DB-4BF2-9C35-2A7F72A55731")]
		public string StatusLUIDSetting { get { return Setting("StatusLUID", "", true); } }

        [BooleanSetting("None of the above", "Include a none of the above option when creating choices.", false, true)]
        public bool NoneAboveSetting { get { return Convert.ToBoolean(Setting("NoneAbove", "true", true)); } }

        [NumericSetting("Maximum Answers", "The maximum number of answers to allow. Setting this to 1 switches to radio buttons. Setting this above 1 uses javascript to only allow setting a limited number of buttons. An invalid setting or 0 implies no limit.", true)]
        public int MaxAnswersSetting { get { return Convert.ToInt32(Setting("MaxAnswers", "0", true)); } }

        #endregion
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
            int profileID = -1, i;
            ProfileCollection profiles;
            ServingProfile servingProfile;
            ProfileMember pm;
            CheckBox cb;
            Literal lt;

            //
            // Deal with redirects.
            //
            if (!Page.IsPostBack)
            {
                iRedirect.Value = string.Empty;
                if (Request.QueryString["requestpage"] != null)
                    iRedirect.Value = string.Format("default.aspx?page={0}", Request.QueryString["requestpage"]);
                if (iRedirect.Value == string.Empty && Request.QueryString["requestUrl"] != null)
                    iRedirect.Value = Request.QueryString["requestUrl"];
                if (iRedirect.Value == string.Empty)
                    iRedirect.Value = string.Format("default.aspx?page={0}", RedirectPageIDSetting);
            }

            //
            // Retrieve the profile ID we are going to work with.
            //
            if (ProfileIDSetting.Contains("|"))
                profileID = Int32.Parse(ProfileIDSetting.Split('|')[1]);
            else
                profileID = Int32.Parse(ProfileIDSetting);
            
            //
            // Walk all the child profiles (only one level deep, non-recursive)
            // and add them as check-boxes to the page. If the user is already
            // a member of one of the profiles then that box is checked and
            // disabled.
            //
            profiles = new Profile(profileID).ChildProfiles;
            for (i = 0; i < profiles.Count; i++)
            {
                //
                // Find the member in the profile if they are already there.
                //
                pm = new ProfileMember(profiles[i].ProfileID, ArenaContext.Current.Person);

                //
                // Create either a CheckBox or a Radio button depending on module
                // settings.
                //
                if (MaxAnswersSetting == 1)
                {
                    cb = (CheckBox)new RadioButton();
                    ((RadioButton)cb).GroupName = "ProfileGroup" + profileID.ToString();
                }
                else
                    cb = new CheckBox();

                //
                // Fill in all the control information and add it to the list.
                //
                cb.ID = profiles[i].ProfileID.ToString();
                cb.Text = profiles[i].Title;
                cb.Enabled = (pm.ProfileID == -1 ? true : false);
                cb.Checked = (pm.ProfileID == -1 ? false : true);
                phProfiles.Controls.Add(cb);

                //
                // If this is a serving profile then make sure it isn't full.
                //
                if (profiles[i].ProfileType == ProfileType.Serving)
                {
                    servingProfile = new ServingProfile(profiles[i].ProfileID);
                    if (servingProfile.ProfileActiveMemberCount >= servingProfile.VolunteersNeeded)
                    {
                        cb.Enabled = false;
                        cb.Text += "(currently full)";
                    }
                }

                //
                // Add in a newline character.
                //
                lt = new Literal();
                lt.Text = "<br />";
                phProfiles.Controls.Add(lt);
            }

            //
            // Add in a "none of the above" option if requested. This does
            // not do anything on submit, but resolves any "I changed my mind
            // how do I get out" questions.
            //
            if (NoneAboveSetting != false)
            {
                //
                // Create either the checkbox or the radio button depending on
                // the module setting.
                //
                if (MaxAnswersSetting == 1)
                {
                    cb = (CheckBox)new RadioButton();
                    ((RadioButton)cb).GroupName = "ProfileGroup" + profileID.ToString();
                }
                else
                    cb = new CheckBox();

                //
                // Setup the information about the none of the above control.
                //
                cb.ID = "-1";
                cb.Text = "None of the above";
                phProfiles.Controls.Add(cb);

                //
                // Add in a newline character.
                //
                lt = new Literal();
                lt.Text = "<br />";
                phProfiles.Controls.Add(lt);
            }
        }

		protected void Page_PreRender(object sender, EventArgs e)
		{
		}

		private void btnSubmit_Click(object sender, EventArgs e)
		{
            int i, profileID;
            CheckBox cbox;
            ProfileMember pm;
            Profile profile;
            Lookup luSource, luStatus;
            string userID = CurrentUser.Identity.Name;

            //
            // Lookup the profile source.
            //
            luSource = new Lookup(Int32.Parse(SourceLUIDSetting));
            luStatus = new Lookup(Int32.Parse(StatusLUIDSetting));
            if (luSource.LookupID == -1 || luStatus.LookupID == -1)
            {
            }

            //
            // Walk each of the controls found and determine if we need to
            // take any action for the value of that control.
            //
            for (i = 0; i < phProfiles.Controls.Count; i++)
            {
                //
                // If the control is not a checkbox or radio button then
                // ignore it.
                //
                if (phProfiles.Controls[i].GetType() != typeof(CheckBox) && phProfiles.Controls[i].GetType() != typeof(RadioButton))
                    continue;

                //
                // Pretend the control is a checkbox, if it is a radio button
                // it will cast fine since the radio button inherits from a
                // check box.
                //
                cbox = (CheckBox)phProfiles.Controls[i];
                profileID = Int32.Parse(cbox.ID);
                profile = new Profile(profileID);

                //
                // If this control is turned on and it is not a "none of the above"
                // setting, then we need to take action.
                //
                if (cbox.Checked == true && profile.ProfileID != -1)
                {
                    //
                    // Verify this person is not already in the profile.
                    //
                    pm = new ProfileMember(profileID, ArenaContext.Current.Person);
                    if (pm.ProfileID == -1)
                    {
                        //
                        // The person needs to be added to the profile, generate all
                        // the standard information.
                        //
                        pm = new ProfileMember();
                        pm.ProfileID = profileID;
                        pm.PersonID = ArenaContext.Current.Person.PersonID;
                        pm.Source = luSource;
                        pm.Status = luStatus;
                        pm.DatePending = DateTime.Now;
                        pm.Save(userID);

                        //
                        // If the profile is a serving tag then we need to generate
                        // a little bit more information.
                        //
                        if (profile.ProfileType == ProfileType.Serving)
                        {
                            ServingProfile sProfile = new ServingProfile(profile.ProfileID);
                            ServingProfileMember sMember = new ServingProfileMember(pm.ProfileID, pm.PersonID);
                            sMember.HoursPerWeek = sProfile.DefaultHoursPerWeek;
                            sMember.Save();
                        }
                    }
                }
            }

            //
            // Redirect browser back to originating page.
            //
            Response.Redirect(iRedirect.Value);
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
		}
		#endregion
	}
}
