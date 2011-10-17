namespace ArenaWeb.UserControls.Custom.HDC.Misc
{
	using System;
	using System.Text;
    using System.Text.RegularExpressions;
	using System.Data;
	using System.Drawing;
	using System.Linq;
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
    ///     This control pulls a person attributes and presents them
    ///     to the user for entry. Currently only check boxes are
    ///     supported.
    ///     TODO: Support other input methods.
    ///     TODO: Support the attribute order.
	/// </summary>
	public partial class MyAttributes : PortalControl
	{
		#region Module Settings

		// Module Settings
		[PageSetting("Redirect Page", "The page that the user will be redirected to when finished.", true)]
		public string RedirectPageIDSetting { get { return Setting("RedirectPageID", "", true); } }

        [NumericSetting("Person Attribute Group", "The numerical group of the Person Attributes to make available.", true)]
        public int AttributeGroupSetting { get { return Convert.ToInt32(Setting("AttributeGroup", "", true)); } }

        [BooleanSetting("None of the above", "Include a none of the above option when creating check boxes", false, true)]
        public bool NoneAboveSetting { get { return Convert.ToBoolean(Setting("NoneAbove", "true", true)); } }
        #endregion
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
            int i;
            PersonAttributeCollection attributes;
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
            // Walk all the person attributes for the given group and add the
            // appropriate field types to the page. If the user already has
            // a value in the type then put that value in so they can see what
            // they have already entered.
            //
            attributes = new PersonAttributeCollection();
            attributes.LoadByGroup(new AttributeGroup(Convert.ToInt32(AttributeGroupSetting)), CurrentPerson.PersonID);
			
			// Sort them
			var sortedAttributes = from attribute in attributes orderby attribute.AttributeOrder select attribute;

			foreach (var attribute in sortedAttributes)
			{
				if (attribute.AttributeType == DataType.YesNo)
				{
					cb = new CheckBox();
					cb.Checked = (attribute.IntValue == 1 ? true : false);
					SetFromAttributeAndAdd(cb, attribute);
				}
				else if (attribute.AttributeType == DataType.String)
				{
					TextBox tb = new TextBox();
					tb.Text = HttpUtility.HtmlDecode(attribute.StringValue);
					SetFromAttributeAndAdd(tb, attribute);
				}
				lt = new Literal();
				lt.Text = "<br />";
				phAttributes.Controls.Add(lt);
			}

            //
            // Add in a "none of the above" option if requested. This does
            // not do anything on submit, but resolves any "I changed my mind
            // how do I get out" questions.
            //
            if (NoneAboveSetting != false)
            {
                cb = new CheckBox();
                cb.ID = "-1";
                cb.Text = "None of the above";
                phAttributes.Controls.Add(cb);
            }
        }

		protected void SetFromAttributeAndAdd(WebControl control, Arena.Core.Attribute attribute)
		{
			Label label = new Label();
			label.Text = attribute.AttributeName + " ";

			control.ID = attribute.AttributeId.ToString();
			control.Enabled = (attribute.Readonly == false ? true : false);

			phAttributes.Controls.Add(label);
			phAttributes.Controls.Add(control);
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
		}

		private void btnSubmit_Click(object sender, EventArgs e)
		{
            PersonAttribute attribute;
            int i;

            for (i = 0; i < phAttributes.Controls.Count; i++)
            {
                //
                // Load the person attribute if the ID is not -1.
                //
                if (phAttributes.Controls[i].ID == "-1")
                    continue;
                attribute = new PersonAttribute(CurrentPerson.PersonID, Convert.ToInt32(phAttributes.Controls[i].ID));

                if (phAttributes.Controls[i].GetType() == typeof(CheckBox))
                {
                    CheckBox cbox = (CheckBox)phAttributes.Controls[i];
                    attribute.IntValue = Convert.ToInt32(cbox.Checked);
                    attribute.Save(CurrentOrganization.OrganizationID, CurrentUser.Identity.Name);
                }
				else if (phAttributes.Controls[i].GetType() == typeof(TextBox))
				{
					TextBox tb = (TextBox)phAttributes.Controls[i];
					attribute.StringValue = HttpUtility.HtmlEncode(tb.Text);
					attribute.Save(CurrentOrganization.OrganizationID, CurrentUser.Identity.Name);
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
