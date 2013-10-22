/**********************************************************************
* Description:	Runs a SQL query (or stored procedure) and formats the
*               output via XSLT.
* Created By:	Daniel Hazelbaker @ High Desert Church
* Date Created:	8/24/2010 3:20:55 PM
**********************************************************************/

using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using Arena.Portal;
using Arena.Core;
using Arena.Feed;

namespace ArenaWeb.UserControls.Custom.HDC.Misc
{
    public partial class SqlViaXslt : PortalControl
    {
        #region Module Settings

        [TextSetting("SQL Query", "The SQL statement to run. (ex: exec sp_name @sp_param1 = @url_param1, @sp_param2 = @url_param2, @sp_param3 = @url_param3, @sp_param4 = 1234 )", true)]
        public string SQLQuerySetting { get { return Setting("SQLQuery", null, true); } }

        [FileSetting("XsltUrl", "The path to the Xslt file to use. (ex: ~/UserControls/custom/OHC/RawXml/RawXml.xslt)", true)]
        public string XsltUrlSetting { get { return Setting("XsltUrl", null, true); } }

        [TextSetting("Suppress Columns", "A semi-colon delimited list of column names that are returned by the query, but should not be displayed.", false)]
        public string[] SuppressColumnsSetting { get { return Setting("SuppressColumns", "", false).Split(';'); } }

        [TextSetting("Query Parameters", "A semi-colon delimited list of SQL Parameters whose values will be pulled from the query string. (ex: url_param1;url_param2;url_param3)", false)]
        public string[] QueryParametersSetting { get { return Setting("QueryParameters", "", false).Split(';'); } }

        [TextSetting("XSLT Parameters", "A semi-colon delimited list of static parameters that will be passed to the XSLT parser. (ex: Color=blue;Width=400px)", false)]
        public string[] XSLTParametersSetting { get { return Setting("XSLTParameters", "", false).Split(';'); } }

        [BooleanSetting("Output Raw XML", "Indicates if only XML should be returned.", false, false)]
        public bool RawXmlSetting { get { return Convert.ToBoolean(Setting("RawXml", "false", false)); } }

        #endregion


        #region Event Handlers

        private void Page_Load(object sender, System.EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            XmlDocument doc = new XmlDocument();
            XmlNode root, fields, rows;
            SqlConnection con = null;
            SqlDataReader rdr = null;
            SqlCommand cmd;
            ArrayList parameters = new ArrayList();
            int i;


            //
            // Connect to SQL.
            //
            try {
                con = new Arena.DataLib.SqlDbConnection().GetDbConnection();
                con.Open();
                cmd = con.CreateCommand();

                //
                // Do some custom replacement.
                //
                cmd.CommandText = SQLQuerySetting.ReplaceNonCaseSensitive("@@PersonID@@", (ArenaContext.Current.Person != null ? ArenaContext.Current.Person.PersonID.ToString() : "-1"));

                //
                // Put in all Query Parameters configured.
                //
                foreach (String qp in QueryParametersSetting)
                {
                    if (!String.IsNullOrEmpty(qp))
                    {
                        String[] opts = qp.Split('=');
                        String o, v = null;
                        
                        o = opts[0];
                        if (opts.Length == 2)
                            v = opts[1];

                        if (Request.QueryString[o] != null)
                            cmd.Parameters.Add(new SqlParameter(String.Format("@{0}", o), Request.QueryString[o]));
                        else
                            cmd.Parameters.Add(new SqlParameter(String.Format("@{0}", o), (v == null ? (object)DBNull.Value : (object)v)));
                    }
                }

                //
                // Execute the reader.
                //
                rdr = cmd.ExecuteReader();

                //
                // Start creating the XML output.
                //
                root = doc.CreateElement("sql");
                doc.AppendChild(root);

                //
                // Put in all the field names under a fields element.
                //
                fields = doc.CreateElement("fields");
                root.AppendChild(fields);
                for (i = 0; i < rdr.FieldCount; i++)
                {
                    XmlNode field;

                    if (!SuppressColumnsSetting.Contains(rdr.GetName(i)))
                    {
                        field = doc.CreateElement("field");
                        field.InnerText = rdr.GetName(i).Replace("_", " ");
                        fields.AppendChild(field);
                    }
                }

                //
                // Load in each row of data under a rows element.
                //
                rows = doc.CreateElement("rows");
                root.AppendChild(rows);
                while (rdr.Read())
                {
                    XmlNode row;

                    row = doc.CreateElement("row");
                    rows.AppendChild(row);

                    //
                    // Each row is comprised of one or more field name elements.
                    //
                    for (i = 0; i < rdr.FieldCount; i++)
                    {
                        XmlNode node;

                        if (!SuppressColumnsSetting.Contains(rdr.GetName(i)))
                        {
                            node = doc.CreateElement(rdr.GetName(i));
                            node.InnerText = rdr[i].ToString();
                            row.AppendChild(node);
                        }
                    }
                }

                //
                // Prepare the translator to convert the XML via XSLT.
                //
                XPathNavigator navigator = doc.CreateNavigator();
                XslCompiledTransform transform = new XslCompiledTransform();
                XsltArgumentList argsList = new XsltArgumentList();

                transform.Load(base.Server.MapPath(XsltUrlSetting));
                argsList.AddParam("ControlID", "", this.ClientID);
                foreach (String p in XSLTParametersSetting) {
                    try {
                        String[] s = p.Split('=');

                        if (s.Length == 1)
                        {
                            if (Request.QueryString[s[0]] != null)
                                argsList.AddParam(s[0], "", Request.QueryString[s[0]]);
                        }
                        else
                            argsList.AddParam(s[0], "", s[1]);
                    }
                    catch (System.Exception ex)
                    {
                    }
                }

                //
                // Translate and store the data.
                //
                transform.Transform((IXPathNavigable)navigator, argsList, new StringWriter(sb));
                ltContent.Text = sb.ToString();

                //
                // If RawXmlSetting is True, output only XML.
                //
                if (RawXmlSetting == true)
                {
                    Response.Write(sb.ToString());
                    Response.ContentType = "application/xml";
                    Response.End();
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                //
                // Close all our SQL connections.
                //
                if (rdr != null)
                    rdr.Close();
                if (con != null)
                    con.Close();
            }
        }

        #endregion
    }
}
