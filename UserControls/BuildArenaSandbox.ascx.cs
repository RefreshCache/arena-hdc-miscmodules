/**********************************************************************
* Description:	Build an Arena Sandbox for the logged in user.
* Created By:	Daniel Hazelbaer @ High Desert Church
* Date Created:	4/21/2010 7:41:46 PM
**********************************************************************/

namespace ArenaWeb.UserControls.Custom.HDC.MiscModules
{
	using System;
	using System.Data;
    using System.Data.SqlClient;
    using System.DirectoryServices;
	using System.Configuration;
	using System.Collections;
	using System.Collections.Generic;
    using System.IO;
	using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Security.Permissions;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.WebControls.WebParts;
	using System.Web.UI.HtmlControls;

	using Arena.Portal;
	using Arena.Core;
    using Arena.Security;

	public partial class BuildArenaSandbox : PortalControl
    {
        #region Module Settings

        public String AuthenticationDomain { get { return "REFRESHCACHE"; } }
        public String SandboxPath { get { return "C:\\Arena\\Sandboxes"; } }

        #endregion


        #region DLL Imports

        [DllImport("advapi32.DLL", SetLastError = true)]
        public static extern int LogonUser(string lpszUsername, string lpszDomain,
            string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr
            phToken);
        [DllImport("advapi32.DLL")]
        public static extern bool ImpersonateLoggedOnUser(IntPtr hToken);
        [DllImport("advapi32.DLL")]
        public static extern bool RevertToSelf();
        [DllImport("kernel32.DLL")]
        public static extern int GetLastError();

        #endregion


        #region Event Handlers

        private void Page_Load(object sender, System.EventArgs e)
		{
			if (!IsPostBack)
			{
                DirectoryInfo dir = new DirectoryInfo(SandboxPath + "\\Templates");


                foreach (FileInfo file in dir.GetFiles())
                {
                    if (file.Extension.Equals(".BAK", StringComparison.CurrentCultureIgnoreCase) == true)
                        ddlVersion.Items.Add(file.Name.Substring(0, file.Name.Length - 4));
                }
			}
		}

        public void btnBuild_Click(object sender, EventArgs e)
        {
            pnlError.Visible = false;
            pnlReady.Visible = false;
            if (String.IsNullOrEmpty(ddlVersion.SelectedValue) == false)
            {
                if (CreateSandboxFolder() == false ||
                    CreateIISApplication() == false ||
                    CreateSandboxDatabase() == false)
                {
                    pnlError.Visible = true;
                }
                else
                {
                    String username = ArenaContext.Current.User.Identity.Name;
                    ltReady.Text = "Your sandbox is ready for use at <a href=\"http://arena.refreshcache.com/Sandbox/" + username + "\">http://arena.refreshcache.com/Sandbox/" + username + "</a>. You may login with your same username and password.";
                    pnlReady.Visible = true;
                }
            }
        }

        #endregion

        private bool CreateSandboxFolder()
        {
            String username = ArenaContext.Current.User.Identity.Name;
            IntPtr admin_token;

            
            //
            // Impersonate the logged in user.
            //
            try
            {
                Credentials creds = new Credentials(CurrentOrganization.OrganizationID, CredentialType.ActiveDirectory);

                if (LogonUser(creds.Username, AuthenticationDomain, creds.Password, 2, 0, out admin_token) != 0)
                {
                    ImpersonateLoggedOnUser(admin_token);

                    //
                    // Copy the sandbox arena folder to the user's sandbox.
                    //
                    DirectoryInfo src, dst;
                    src = new DirectoryInfo(SandboxPath + "\\Templates\\" + ddlVersion.SelectedValue);
                    dst = new DirectoryInfo(SandboxPath + "\\" + username);
                    try
                    {
                        dst.Delete(true);
                    }
                    catch { }
                    CopyAll(src, dst);

                    //
                    // Modify the web.config to connect to the correct database.
                    //
                    StreamReader configRdr = new StreamReader(SandboxPath + "\\" + username + "\\web.config");
                    String config = configRdr.ReadToEnd();
                    configRdr.Close();
                    int start = config.IndexOf("Initial Catalog=");
                    int end = config.IndexOf(";", start);
                    config = config.Remove(start, (end - start));
                    config = config.Insert(start, "Initial Catalog=ArenaSandbox_" + username);
                    config = config.Replace("Templates\\" + ddlVersion.SelectedValue, username);
                    config = config.Replace("Templates/" + ddlVersion.SelectedValue, username);
                    StreamWriter configWrt = new StreamWriter(SandboxPath + "\\" + username + "\\web.config");
                    configWrt.Write(config);
                    configWrt.Close();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                RevertToSelf();
                throw ex;
            }

            //
            // Cleanup after pretending to be the user.
            //
            RevertToSelf();

            return true;
        }


        private bool CreateSandboxDatabase()
        {
            SqlConnection sql = null;
            SqlCommand sqlCommand;
            SqlDataReader rdr;
            String username = ArenaContext.Current.User.Identity.Name;
            String database = "ArenaSandbox_" + username;
            String dbVersion = ddlVersion.SelectedValue.Replace('.', '_');
            IntPtr admin_token;


            //
            // Impersonate the logged in user.
            //
            try
            {

                Credentials creds = new Credentials(CurrentOrganization.OrganizationID, CredentialType.ActiveDirectory);

                if (LogonUser(creds.Username, AuthenticationDomain, creds.Password, 2, 0, out admin_token) != 0)
                {
                    ImpersonateLoggedOnUser(admin_token);

                    //
                    // Open a connection to the SQL server.
                    //
                    sql = new SqlConnection("Data Source=refreshcacheare\\arena;Trusted_Connection=yes");
                    sql.Open();

                    //
                    // Drop and re-create the database.
                    //
                    try
                    {
                        sqlCommand = new SqlCommand("DROP DATABASE " + database, sql);
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch { }
                    sqlCommand = new SqlCommand("RESTORE DATABASE " + database +
                        " FROM DISK = '" + SandboxPath + "\\Templates\\" + ddlVersion.SelectedValue + ".bak'" +
                        " WITH MOVE 'Template" + dbVersion + "DB' TO '" + SandboxPath + "\\" + username + ".mdf'" +
                        ", MOVE 'Template" + dbVersion + "DB_Log' TO '" + SandboxPath + "\\" + username + ".ldf'",
                        sql);
                    sqlCommand.ExecuteNonQuery();

                    //
                    // Create the current users login as the Administrator of
                    // the new database.
                    //
                    Arena.DataLayer.Organization.OrganizationData org = new Arena.DataLayer.Organization.OrganizationData();
                    rdr = org.ExecuteReader("SELECT [password] FROM secu_login WHERE login_id = '" + username + "'");
                    rdr.Read();
                    sql.ChangeDatabase(database);
                    sqlCommand = new SqlCommand("secu_sp_save_login", sql);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.Add(new SqlParameter("@PersonID", 1));
                    sqlCommand.Parameters.Add(new SqlParameter("@LoginID", username));
                    sqlCommand.Parameters.Add(new SqlParameter("@Password", rdr[0]));
                    sqlCommand.Parameters.Add(new SqlParameter("@UserID", "Sandbox"));
                    sqlCommand.Parameters.Add(new SqlParameter("@Active", true));
                    sqlCommand.Parameters.Add(new SqlParameter("@AuthenticationProvider", AuthenticationProvider.Database));
                    sqlCommand.Parameters.Add(new SqlParameter("@AccountLocked", false));
                    sqlCommand.Parameters.Add(new SqlParameter("@DateLockExpires", DateTime.Parse("1900-01-01 00:00:00")));
                    sqlCommand.Parameters.Add(new SqlParameter("@ForceChangePassword", false));
                    sqlCommand.ExecuteNonQuery();

                    //
                    // Close the database.
                    //
                    sql.Close();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (sql != null)
                    sql.Close();
                RevertToSelf();
                throw ex;
            }

            //
            // Cleanup after pretending to be the user.
            //
            RevertToSelf();

            return true;
        }


        private bool CreateIISApplication()
        {
            String username = ArenaContext.Current.User.Identity.Name;
            IntPtr admin_token;


            //
            // Impersonate the administrator user now.
            //
            try
            {
                Credentials creds = new Credentials(CurrentOrganization.OrganizationID, CredentialType.ActiveDirectory);
                DirectoryEntry w3svc, sandbox;

                if (LogonUser(creds.Username, AuthenticationDomain, creds.Password, 2, 0, out admin_token) != 0)
                {
                    ImpersonateLoggedOnUser(admin_token);

                    //
                    // Delete the old entry if it exists.
                    //
                    if (DirectoryEntry.Exists("IIS://localhost/W3SVC/1/Root/Sandbox/" + username))
                    {
                        sandbox = new DirectoryEntry("IIS://localhost/W3SVC/1/Root/Sandbox/" + username);
                        sandbox.DeleteTree();
                    }

                    //
                    // Create the new directory entry.
                    //
                    w3svc = new DirectoryEntry("IIS://localhost/W3SVC/1/Root/Sandbox");
                    sandbox = w3svc.Children.Add(username, w3svc.SchemaClassName.Replace("Service", "VirtualDir"));
                    sandbox.Properties["Path"][0] = SandboxPath + "\\" + username;
                    sandbox.Properties["AccessScript"][0] = true;
                    sandbox.Properties["AppFriendlyName"][0] = username;
                    sandbox.Properties["AppIsolated"][0] = "1";
                    sandbox.Properties["AppRoot"][0] = "/LM/W3SVC/1/Root/Sandbox/" + username;
                    sandbox.CommitChanges();
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                RevertToSelf();
                throw ex;
            }

            //
            // Cleanup after pretending to be the administrator.
            //
            RevertToSelf();

            return true;
        }

        #region Directory manipulation.

        public void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        #endregion

	}
}