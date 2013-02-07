using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Arena.Portal;
using Arena.Core;
using Arena.Organization;
using Arena.Marketing;
using Arena.DataLib;

namespace ArenaWeb.UserControls.Custom.HDC.CheckIn
{
    public partial class DisplayBoard : PortalControl
    {
        #region Module Settings

        [LookupMultiSelectSetting("Topic Areas", "List of topic areas to include.", true, "1FE55E22-F67C-46BA-A6AE-35FD112AFD6D", "")]
        public string TopicAreaList { get { return Setting("TopicAreaList", "", true); } }

        [NumericSetting("Slide Time", "Enter the time in seconds you want each slide to display. (defaults to 5 seconds)", false)]
        public int SlideTimeSetting { get { return Convert.ToInt32(Setting("SlideTime", "5", false)); } }

        [NumericSetting("Transition Time", "Enter the time in milliseconds you want the transition duration to be. (defaults to 1000 = 1 second)", false)]
        public int TransitionTimeSetting { get { return Convert.ToInt32(Setting("TransitionTime", "1000", false)); } }

        #endregion


        /// <summary>
        /// Load in jQuery and CSS required to properly display this module.
        /// </summary>
        /// <param name="sender">The object that triggered this event.</param>
        /// <param name="e">Details about the event.</param>
        private void Page_Init(object sender, EventArgs e)
        {
            BasePage.AddJavascriptInclude(Page, "//ajax.googleapis.com/ajax/libs/jquery/1.7.0/jquery.min.js");
            BasePage.AddJavascriptInclude(Page, "UserControls/Custom/HDC/Misc/Includes/jquery.rs.slideshow.min.js");
            BasePage.AddCssLink(Page, "UserControls/Custom/HDC/Misc/Includes/DisplaySign.css");
        }


        /// <summary>
        /// Load the page. If the client is requesting data in an "xml" format then that means
        /// that we need to parse other URL parameters and send back some XML encoded data which
        /// tells it which image to load and display.
        /// </summary>
        /// <param name="sender">The object triggering this event.</param>
        /// <param name="e">Arguments and information about the event.</param>
        private void Page_Load(object sender, EventArgs e)
        {
            if (Request.Params["format"] == "xml")
                GetNextPromotion();
        }


        /// <summary>
        /// Get a collection of the current web requests active for these topic areas. I forget why
        /// I had to do this as a custom method, I think something about the way the data is returned
        /// by the standard built-in methods.
        /// </summary>
        /// <returns>A collection of PromotionRequest objects.</returns>
        private PromotionRequestCollection GetCurrentWebRequests()
        {
            SqlConnection conn = new SqlDbConnection().GetDbConnection();
            SqlCommand command = new SqlCommand("cust_sp_get_promotion_web_requests", conn);
            SqlDataReader reader = null;
            PromotionRequestCollection prc = new PromotionRequestCollection();
            ArrayList paramList = new ArrayList();


            //
            // Build up all the command parameters and options.
            //
            command.Parameters.Add(new SqlParameter("@TopicAreaIDs", (String.IsNullOrEmpty(TopicAreaList) ? "-1" : TopicAreaList)));
            command.Parameters.Add(new SqlParameter("@AreaFilter", "both"));
            command.Parameters.Add(new SqlParameter("@CampusID", -1));
            command.Parameters.Add(new SqlParameter("@MaxItems", 1000));
            command.Parameters.Add(new SqlParameter("@EventsOnly", false));
            command.Parameters.Add(new SqlParameter("@DocumentTypeID", -1));
            command.CommandText = "cust_sp_get_promotion_web_requests";
            command.Connection = conn;
            command.CommandType = CommandType.StoredProcedure;
            try
            {
                //
                // Run the SP and load in all the promotions.
                //
                conn.Open();
                reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (reader.Read())
                {
                    prc.Add(new PromotionRequest((int)reader["promotion_request_id"]));
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return prc;
        }


        /// <summary>
        /// Figure out which is the next promotion/image the client wants and then send the data
        /// back to the client.
        /// </summary>
        private void GetNextPromotion()
        {
            PromotionRequestCollection prc = GetCurrentWebRequests();
            int i, lastID = -1, nextID = -1, nextIndex = -1;


            //
            // Check for the previous ID number.
            //
            if (Request.Params["lastID"] != null)
            {
                String parm = Request.Params["lastID"];

                if (!String.IsNullOrEmpty(parm))
                {
                    lastID = Convert.ToInt32(parm.Split(',')[0]);
                    nextIndex = Convert.ToInt32(parm.Split(',')[1]) + 1;
                }
            }

            //
            // If there were no promotions found, return -1.
            //
            if (prc.Count == 0)
                nextID = -1;

            //
            // If this was the first time called, return the first image.
            //
            else if (lastID == -1)
            {
                nextID = prc[0].PromotionRequestID;
                nextIndex = 0;
            }

            //
            // We have all the ID numbers for this system, snag the next one.
            //
            else
            {
                for (i = 0; i < prc.Count; i++)
                {
                    if (prc[i].PromotionRequestID == lastID)
                    {
                        //
                        // Check if the image index is past the images available.
                        //
                        if (nextIndex < prc[i].Documents.Count)
                        {
                            nextID = lastID;
                            break;
                        }

                        //
                        // Otherwise determine if we have another promotion or if we start from beginning.
                        //
                        if (++i >= prc.Count)
                            nextID = prc[0].PromotionRequestID;
                        else
                            nextID = prc[i].PromotionRequestID;
                        nextIndex = 0;

                        break;
                    }
                }
            }

            SendDisplayXML(nextID, nextIndex);
        }


        /// <summary>
        /// Build and send the XML response for this request. The HTTP request is terminated
        /// at the end of this function so no further data can be sent.
        /// </summary>
        /// <param name="promotionID">The ID of the promotion being requested.</param>
        /// <param name="index">The numerical index of the Document/Image being requested.</param>
        private void SendDisplayXML(int promotionID, int index)
        {
            PromotionRequest promotion = new PromotionRequest(promotionID);
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration dec;
            XmlNode root, node;


            //
            // Setup the basic XML document.
            //
            dec = xdoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xdoc.InsertBefore(dec, xdoc.DocumentElement);
            root = xdoc.CreateElement("Display");

            //
            // Determine if we have a valid existing promotion to work with.
            //
            if (promotion.PromotionRequestID != -1)
            {
                //
                // We do, so store the promotion and the requested image.
                //
                node = xdoc.CreateElement("ID");
                node.AppendChild(xdoc.CreateTextNode(String.Format("{0},{1}", promotion.PromotionRequestID.ToString(), index.ToString())));
                root.AppendChild(node);

                node = xdoc.CreateElement("URL");
                node.AppendChild(xdoc.CreateTextNode(String.Format("CachedBlob.aspx?guid={0}", promotion.Documents[index].GUID.ToString())));
                root.AppendChild(node);
            }
            else
            {
                //
                // No, send back a blank response.
                //
                node = xdoc.CreateElement("ID");
                node.AppendChild(xdoc.CreateTextNode(""));
                root.AppendChild(node);
            }

            xdoc.AppendChild(root);

            //
            // Send the XML stream. The End() forces .NET to send the data and close
            // out the connection cleanly.
            //
            xdoc.Save(writer);
            Response.Write(sb.ToString());
            Response.End();
        }
    }
}
