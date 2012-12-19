using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
using Arena.DataLayer.Marketing;
using Arena.Marketing;
using Arena.Portal;
using Arena.Security;


namespace ArenaWeb.UserControls.Custom.HDC.MiscModules
{
    public partial class DisplaySignList : PortalControl
    {
        #region Module Settings

        [TextSetting("Topic Area", "Comma separated list of topic areas to include.", true)]
        public String TopicAreaSetting { get { return Setting("TopicArea", "", true); } }

        [PageSetting("Edit Page", "Display Sign Entry edit module page.", true)]
        public String EditPageSetting { get { return Setting("EditPage", "", true); } }

        #endregion

        private bool EditEnabled = false;


        /// <summary>
        /// The page is loading, set some initial information and display data.
        /// </summary>
        /// <param name="sender">The object causing this event to be triggered.</param>
        /// <param name="e">Information about the event.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            EditEnabled = CurrentModule.Permissions.Allowed(OperationType.Edit, base.CurrentUser);

            if (!IsPostBack)
            {
                ShowList();
            }
        }


        /// <summary>
        /// The page is being initialized, set event handlers on the data grid.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        protected override void OnInit(EventArgs e)
        {
            dgPromotions.ReBind += new Arena.Portal.UI.DataGridReBindEventHandler(dgPromotions_ReBind);

            base.OnInit(e);
        }


        /// <summary>
        /// Datagrid needs its information updated, pass this on to the real method handler.
        /// </summary>
        /// <param name="source">The object causing this event to be triggered.</param>
        /// <param name="e">Information about the event.</param>
        private void dgPromotions_ReBind(object source, EventArgs e)
        {
            ShowList();
        }


        /// <summary>
        /// Update the information displayed in the datagrid.
        /// </summary>
        private void ShowList()
        {
            DataTable table = new PromotionRequestData().GetPromotionWebList_DT(true, -1);
            String[] topicAreas;


            //
            // Filter out rows we don't want. We only want rows that are promoted in the correct areas.
            //
            if (!String.IsNullOrEmpty(TopicAreaSetting))
            {
                topicAreas = TopicAreaSetting.Split(',');

                //
                // Walk each row in the table and process all rows that are not marked as deleted.
                //
                foreach (DataRow row in table.Rows)
                {
                    if (row.RowState != DataRowState.Deleted)
                    {
                        bool flag = false;
                        string[] strArray = row["cross_promote_values"].ToString().Split(new char[] { ',' });

                        //
                        // We start with flag = false and look for a match on each of the possible
                        // topic areas.
                        //
                        foreach (String area in topicAreas)
                        {
                            //
                            // If the primary topic area matches, set flag = true so it will be kept.
                            //
                            if (area.Trim() == row["topic_area_luid"].ToString())
                                flag = true;

                            //
                            // Check if topic area against the secondary topic areas. If we find a match
                            // set flag = true so the row will be kept.
                            //
                            foreach (string str in strArray)
                            {
                                if (str == area.Trim())
                                {
                                    flag = true;
                                }
                            }
                        }

                        //
                        // If no topic area match was found, delete the row.
                        //
                        if (!flag)
                        {
                            row.Delete();
                        }
                    }
                }

                table.AcceptChanges();
            }

            //
            // Setup all the parameters for the datagrid.
            //
            dgPromotions.AllowPaging = false;
            dgPromotions.PagerStyle.Visible = false;
            dgPromotions.ItemType = "Display Request";
            dgPromotions.ItemBgColor = base.CurrentPortalPage.Setting("ItemBgColor", string.Empty, false);
            dgPromotions.ItemAltBgColor = base.CurrentPortalPage.Setting("ItemAltBgColor", string.Empty, false);
            dgPromotions.ItemMouseOverColor = base.CurrentPortalPage.Setting("ItemMouseOverColor", string.Empty, false);
            dgPromotions.AddEnabled = false;
            dgPromotions.DeleteEnabled = false;
            dgPromotions.EditEnabled = false;
            dgPromotions.MergeEnabled = false;
            dgPromotions.MailEnabled = false;
            dgPromotions.ExportEnabled = true;
            dgPromotions.DataSource = table;
            dgPromotions.DataBind();
        }


        /// <summary>
        /// Format the date into a format we want. Basically if the date is "1/1/1900" then
        /// make it an empty string so it looks clean.
        /// </summary>
        /// <param name="dateCol">The DateTime value of this date.</param>
        /// <returns>A user-readable formatted string representing the date and time.</returns>
        public string GetFormattedDateLong(object dateCol)
        {
            DateTime time = (DateTime)dateCol;

            
            if (time == DateTime.Parse("1/1/1900"))
            {
                return string.Empty;
            }

            return time.ToShortDateTimeString();
        }


        /// <summary>
        /// Format the title to include a URL to the editor page.
        /// </summary>
        /// <param name="idCol">The ID number of the promotion request.</param>
        /// <param name="titleCol">The title of the promotion itself.</param>
        /// <returns></returns>
        protected string GetFormattedTitle(object idCol, object titleCol)
        {
            int id = Convert.ToInt32(idCol);
            string title = titleCol.ToString();


            if (EditEnabled)
            {
                return String.Format("<a href=\"default.aspx?page={0}&request={1}&return={3}\">{2}</a>", EditPageSetting, id, title, Server.UrlEncode(Request.Url.PathAndQuery));
            }
            else
                return title;
        }


        /// <summary>
        /// Delete the identified promotion and then reload the list.
        /// </summary>
        /// <param name="sender">The object causing this event to be triggered.</param>
        /// <param name="e">The arguments for this event.</param>
        protected void btnDelete_Command(object sender, CommandEventArgs e)
        {
            PromotionRequest promotion = new PromotionRequest(Convert.ToInt32(e.CommandArgument));


            //
            // Delete all documents/images.
            //
            while (promotion.Documents.Count > 0)
            {
                promotion.Documents[0].Delete();
                promotion.Documents.RemoveAt(0);
            }

            //
            // Delete the promotion and reload the list.
            //
            promotion.Delete();
            dgPromotions_ReBind(this, null);
        }
    }
}