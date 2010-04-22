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

	public partial class BuildArenaSandbox : PortalControl
    {
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
			if ( ! IsPostBack )
			{
				
			}
		}

        public void btnBuild_Click(object sender, EventArgs e)
        {
            SqlConnection sql = null;
            SqlCommand sqlCommand;
            SqlDataReader rdr;
            String userName = "daniel";
            String password = txtPassword.Text;
            String database = "ArenaSandbox_" + userName;
            IntPtr admin_token;
            int result = 0;

            try
            {
                if ((result = LogonUser(userName, "CONSTANTINE", password, 2, 0, out admin_token)) != 0)
                {
                    ImpersonateLoggedOnUser(admin_token);

                    //
                    // Copy the sandbox arena folder to the user's sandbox.
                    //
                    System.IO.DirectoryInfo src, dst;
                    src = new System.IO.DirectoryInfo("C:\\Users\\daniel\\Source");
                    dst = new System.IO.DirectoryInfo("C:\\Users\\daniel\\Dest");
                    CopyAll(src, dst);

                    //
                    // Modify the web.config to connect to the correct database.
                    //

                    //
                    // Open a connection to the SQL server.
                    //
                    sql = new SqlConnection("Data Source=constantine\\hdcarena;Trusted_Connection=yes");
                    sql.Open();

                    //
                    // Run a test command.
                    //
                    sqlCommand = new SqlCommand("SELECT SYSTEM_USER", sql);
                    rdr = sqlCommand.ExecuteReader();
                    rdr.Read();
                    lbStatus.Text += "Connected to SQL as " + rdr[0].ToString() + "\n";
                    rdr.Close();

                    //
                    // Commands that need to be run:
                    // DROP DATABASE [database]
                    // CREATE DATABASE [database]
                    // ** Assign permissions to the Arena user **
                    // ** Restore database from snapshot **
                    // Create user record / login for the current Arena user in new sandbox.
                    // Assign database permissions to the Arena user.
                    //
                }
                else
                    lbStatus.Text += "Fail to logon errorcode = " + GetLastError().ToString();
            }
            catch (Exception ex)
            {
                lbStatus.Text += ex.Message;
            }
            finally
            {
                if (sql != null)
                    sql.Close();
                RevertToSelf();
            }
        }

        #endregion


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
                lbStatus.Text += String.Format("Copying {0}\\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        #endregion

	}
}