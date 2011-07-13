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

        [TextSetting("SQL Query", "The SQL statement to run.", true)]
        public string SQLQuerySetting { get { return Setting("SQLQuery", null, true); } }

        [FileSetting("XsltUrl", "The path to the Xslt file to use.", true)]
        public string XsltUrlSetting { get { return Setting("XsltUrl", null, true); } }

        [TextSetting("Suppress Columns", "A semi-colon delmited list of column names that are returned by the query, but should not be displayed.", false)]
        public string[] SuppressColumnsSetting { get { return Setting("SuppressColumns", "", false).Split(';'); } }

        #endregion


        #region Event Handlers

        private void Page_Load(object sender, System.EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            XmlDocument doc = new XmlDocument();
            XmlNode root, fields, rows;
            SqlDataReader rdr;
            String query = SQLQuerySetting;
            int i;


            //
            // Do some custom replacement.
            //
            query = query.ReplaceNonCaseSensitive("@@PersonID@@", (ArenaContext.Current.Person != null ? ArenaContext.Current.Person.PersonID.ToString() : "-1"));

            //
            // Execute the reader.
            //
            if (true)
                rdr = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(query);
            else
                rdr = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(query, new ArrayList());

            root = doc.CreateElement("sql");
            doc.AppendChild(root);

            fields = doc.CreateElement("fields");
            root.AppendChild(fields);
            for (i = 0; i < rdr.FieldCount; i++)
            {
                XmlNode field;

                if (!SuppressColumnsSetting.Contains(rdr.GetName(i)))
                {
                    field = doc.CreateElement("field");
                    field.InnerText = rdr.GetName(i);
                    fields.AppendChild(field);
                }
            }

            rows = doc.CreateElement("rows");
            root.AppendChild(rows);
            while (rdr.Read())
            {
                XmlNode row;

                row = doc.CreateElement("row");
                rows.AppendChild(row);

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

            XPathNavigator navigator = doc.CreateNavigator();
            XslCompiledTransform transform = new XslCompiledTransform();
            transform.Load(base.Server.MapPath(XsltUrlSetting));

            transform.Transform((IXPathNavigable)navigator, null, new StringWriter(sb));

            ltContent.Text = sb.ToString();
        }

        #endregion
    }
}
