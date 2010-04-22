
namespace ArenaWeb.UserControls.Custom.HDC.Misc
{
	using System;
	using System.Data;
	using System.Configuration;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.WebControls.WebParts;
	using System.Web.UI.HtmlControls;

	using Arena.Portal;
	using Arena.Core;

    /// <summary>
    /// This module provides the functionality to force a users logout without
    /// the user needing to click on a logout link. Our use is for an auto-logout
    /// feature with publick kiosks that require the user to login. Each page has
    /// a javascript snippit that runs and after a timeout sends them to a logout
    /// page with this module.
    /// </summary>
	public partial class ForceLogout : PortalControl
	{
        #region Module Settings

        // Module Settings
        [PageSetting("Redirect Page", "The page that the user will be redirected to after logout.", true)]
        public string RedirectPageIDSetting { get { return Setting("RedirectPageID", "", true); } }

        #endregion

		#region Event Handlers

		private void Page_Load(object sender, System.EventArgs e)
		{
            FormsAuthentication.SignOut();

            //
            // Redirect browser somewhere else.
            //
            Response.Redirect(string.Format("default.aspx?page={0}", RedirectPageIDSetting));
        }
		
		#endregion

	}
}