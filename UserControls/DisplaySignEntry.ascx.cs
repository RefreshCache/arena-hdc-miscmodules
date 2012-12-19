using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Arena.Core;
using Arena.Document;
using Arena.Marketing;
using Arena.Portal;
using Arena.Portal.UI;
using Arena.Security;

namespace ArenaWeb.UserControls.Custom.HDC.MiscModules
{
    public partial class DisplaySignEntry : PortalControl
    {
        protected PromotionRequest promotion = null;

        #region Module Settings

        [NumericSettingAttribute("Required Width", "The required width of an image. 0 or blank if not required.", false)]
        public int RequiredWidthSetting { get { return Convert.ToInt32(Setting("RequiredWidth", "0", false)); } }

        [NumericSettingAttribute("Required Height", "The required height of an image. 0 or blank if not required.", false)]
        public int RequiredHeightSetting { get { return Convert.ToInt32(Setting("RequiredHeight", "0", false)); } }

        [DocumentTypeSetting("Document Type", "Available document types, if nothing is selected then all are available.", false)]
        public string DocumentTypeSetting { get { return Setting("DocumentType", "", false); } }

        #endregion


        /// <summary>
        /// Initialize some of the page attributes and scripts.
        /// </summary>
        /// <param name="sender">Object causing this event to be triggered.</param>
        /// <param name="e">Parameters about the event.</param>
        protected void Page_Init(object sender, EventArgs e)
        {
            //
            // Request various javascript and CSS files to be included.
            //
            BasePage.AddJavascriptInclude(Page, BasePage.JQUERY_INCLUDE);
            BasePage.AddJavascriptInclude(Page, "include/scripts/jqueryui/js/jquery-ui-1.7.3.custom.min.js");
            BasePage.AddJavascriptInclude(Page, "UserControls/Custom/HDC/Misc/Includes/jquery-ui-timepicker-addon.js");
            BasePage.AddCssLink(Page, "include/scripts/jqueryui/css/custom-theme/jquery-ui-.custom.css");

            //
            // Setup the document browser/uploader.
            //
            mdlDocuments.Title = "Select a Document";
            mdlDocuments.Url = "DocumentBrowser.aspx?callback=selectDocument&DocumentTypeFilter=#documentTypeFilter#&SelectedID=#selectedID#&DocumentTypeID=#documentTypeID#";
            mdlDocuments.Width = 310;
            mdlDocuments.Height = 200;
            mdlDocuments.JSFunctionName = "openChooseDocumentWindow(documentTypeFilter, selectedID, documentTypeID)";

            //
            // Load the promotion into memory.
            //
            LoadRequest();
        }


        /// <summary>
        /// The page is being loaded, we need to do some initialization information.
        /// </summary>
        /// <param name="sender">The object causing the event to be triggered.</param>
        /// <param name="e">Information about the event itself.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //
            // If this is an initial page load, set the initial values.
            //
            if (!IsPostBack)
            {
                tbTitle.Text = promotion.Title;
                tbStartDate.Text = promotion.WebStartDate.ToString("MM/dd/yyyy hh:mm tt").ToLower();
                tbEndDate.Text = promotion.WebEndDate.ToString("MM/dd/yyyy hh:mm tt").ToLower();
                cbWeekly.Checked = (promotion.WebSummary == "Weekly");
            }

            //
            // Handle a document image being uploaded.
            //
            pnlWrongSize.Visible = false;
            if (this.ihBlobID.Value != string.Empty)
            {
                PromotionRequestDocument item = new PromotionRequestDocument(promotion.PromotionRequestID, Convert.ToInt32(this.ihBlobID.Value));
                Arena.Utility.ArenaImage img = new Arena.Utility.ArenaImage(Convert.ToInt32(this.ihBlobID.Value));
                System.Drawing.Image image = img.GetImage(0, 0);

                //
                // Check if the image being uploaded fits the required dimensions.
                //
                if ((RequiredWidthSetting == 0 || RequiredWidthSetting == image.Width) &&
                    (RequiredHeightSetting == 0 || RequiredHeightSetting == image.Height))
                {
                    bool flag = false;

                    //
                    // Look for an existing image in the promotion.
                    //
                    for (int i = 0; i < promotion.Documents.Count; i++)
                    {
                        if (promotion.Documents[i].DocumentID == item.DocumentID)
                        {
                            promotion.Documents[i] = item;
                            flag = true;
                            break;
                        }
                    }

                    //
                    // If not found, add this image to the promotion.
                    //
                    if (!flag)
                    {
                        int ignored = item.ByteArray.Length;

                        item.Description = "999";
                        item.Save(CurrentUser.Identity.Name);
                        promotion.Documents.Add(item);
                    }
                }
                else
                {
                    //
                    // Delete the image from the database.
                    //
                    img.Delete();

                    //
                    // Inform the user it was the wrong size.
                    //
                    ltWrongSize.Text = "";
                    if (RequiredWidthSetting != 0 && RequiredHeightSetting != 0)
                        ltWrongSize.Text = String.Format("Your image was not uploaded. It must be exactly {0} by {1} pixels in size.", RequiredWidthSetting, RequiredHeightSetting);
                    else if (RequiredWidthSetting != 0)
                        ltWrongSize.Text = String.Format("Your image was not uploaded. It must be exactly {0} pixels wide.", RequiredWidthSetting);
                    else
                        ltWrongSize.Text = String.Format("Your image was not uploaded. It must be exactly {0} pixels high.", RequiredHeightSetting);

                    pnlWrongSize.Visible = true;
                }
                this.ihBlobID.Value = string.Empty;
            }

            //
            // Do final initialization.
            //
            RegisterScripts();
            ShowDocuments();
            lbAddItem.OnClientClick = string.Format("openChooseDocumentWindow('{0}', '-1','-1'); return false;", DocumentTypeSetting);
        }


        /// <summary>
        /// Initialize the page event handlers.
        /// </summary>
        /// <param name="e">Event information.</param>
        protected override void OnInit(EventArgs e)
        {
            this.dgDocuments.ReBind += new DataGridReBindEventHandler(this.dgDocuments_ReBind);
            this.dgDocuments.DeleteCommand += new DataGridCommandEventHandler(this.dgDocuments_DeleteCommand);

            base.OnInit(e);
        }


        /// <summary>
        /// Register the Javascript scripts that we will need for the page to function
        /// correctly.
        /// </summary>
        private void RegisterScripts()
        {
            StringBuilder builder;


            //
            // Register the script that handles drag and drop re-order.
            //
            builder = new StringBuilder();
            builder.Append("\n<script type=\"text/javascript\">\n");
            builder.Append("\tfunction updateDocumentOrder(tableOrder)\n");
            builder.Append("\t{\n");
            builder.AppendFormat("\t\tdocument.frmMain.{0}.value = tableOrder;\n", hfDocumentOrder.ClientID);
            builder.AppendFormat("\t\tdocument.frmMain.{0}.click();\n", btnUpdateDocumentOrder.ClientID);
            builder.Append("\t}\n");
            builder.Append("</script>\n");
            this.Page.ClientScript.RegisterClientScriptBlock(base.GetType(), "updateDocumentOrder", builder.ToString());

            //
            // Register the script that handles initializing the datetimepicker.
            //
            builder = new StringBuilder();
            builder.Append("\n<script type=\"text/javascript\">\n");
            builder.Append("\t$(document).ready(function () {\n");
            builder.Append("\t\t$('#" + tbStartDate.ClientID + "').datetimepicker({ ampm: true, timeFormat: 'hh:mm tt', hour: " + promotion.WebStartDate.Hour.ToString() + ", minute: " + promotion.WebStartDate.Minute.ToString() + " });\n");
            builder.Append("\t\t$('#" + tbEndDate.ClientID + "').datetimepicker({ ampm: true, timeFormat: 'hh:mm tt', hour: " + promotion.WebEndDate.Hour.ToString() + ", minute: " + promotion.WebEndDate.Minute.ToString() + " });\n");
            builder.Append("\t});\n");
            builder.Append("</script>\n");
            this.Page.ClientScript.RegisterClientScriptBlock(base.GetType(), "datetimepicker", builder.ToString());
        }


        /// <summary>
        /// Load the promotion into memory either from the information on the URL or the session data.
        /// </summary>
        protected void LoadRequest()
        {
            //
            // Pull it from the session data if this is a postback event.
            //
            if (IsPostBack)
                promotion = (PromotionRequest)Session["HDC_PROMOTION"];

            if (promotion == null)
            {
                //
                // Grab it from the query string, or create a new one.
                //
                if (!String.IsNullOrEmpty(Request.QueryString["REQUEST"]))
                    promotion = new PromotionRequest(Convert.ToInt32(Request.QueryString["REQUEST"]));
                else
                    promotion = new PromotionRequest();

                //
                // Store it in the session data for later.
                //
                Session["HDC_PROMOTION"] = promotion;
            }
        }


        /// <summary>
        /// Update the data that is displayed in the datagrid, which is showing the images.
        /// </summary>
        protected void ShowDocuments()
        {
            dgDocuments.ItemType = "Image";
            dgDocuments.ItemBgColor = base.CurrentPortalPage.Setting("ItemBgColor", string.Empty, false);
            dgDocuments.ItemAltBgColor = base.CurrentPortalPage.Setting("ItemAltBgColor", string.Empty, false);
            dgDocuments.ItemMouseOverColor = base.CurrentPortalPage.Setting("ItemMouseOverColor", string.Empty, false);
            dgDocuments.AddEnabled = false;
            dgDocuments.MoveEnabled = true;
            dgDocuments.DeleteEnabled = true;
            dgDocuments.EditEnabled = false;
            dgDocuments.MergeEnabled = false;
            dgDocuments.MailEnabled = false;
            dgDocuments.ExportEnabled = false;
            dgDocuments.AllowSorting = false;
            dgDocuments.AllowPaging = false;
            dgDocuments.DeleteIsAsync = true;
            dgDocuments.DataSource = promotion.Documents.OrderBy(d => d.Description).ToList();
            dgDocuments.CustomMoveFunction = "updateDocumentOrder";
            dgDocuments.DataBind();
        }


        /// <summary>
        /// An update to the datagrid has been requested. Pass it along to the method that
        /// does that actual work.
        /// </summary>
        /// <param name="sender">The object that triggered this event.</param>
        /// <param name="e">Detailed information about the event.</param>
        private void dgDocuments_ReBind(object sender, EventArgs e)
        {
            ShowDocuments();
        }


        /// <summary>
        /// User is requesting to delete an image from the promotion. Find and destroy.
        /// </summary>
        /// <param name="sender">The object causing the event to be triggered.</param>
        /// <param name="e">Details about the event.</param>
        private void dgDocuments_DeleteCommand(object sender, DataGridCommandEventArgs e)
        {
            PromotionRequestDocument doc;


            //
            // Find the specific document, delete the document blob data and then remove
            // it from the promotion.
            //
            doc = promotion.Documents.OrderBy(d => d.Description).ElementAt(e.Item.ItemIndex);
            doc.Delete();
            promotion.Documents.Remove(doc);

            ShowDocuments();
        }


        /// <summary>
        /// Save the promotion and images to the database. Either create a whole new entry
        /// or update an existing one.
        /// </summary>
        /// <param name="sender">The object causing the event to be raised.</param>
        /// <param name="e">Information about the event.</param>
        protected void btnSave_Click(object sender, EventArgs e)
        {
            bool valid = true;
            DateTime startDate = DateTime.Now, endDate = DateTime.Now;


            //
            // Check if the user entered a valid title.
            //
            if (tbTitle.Text.Trim().Length == 0)
                valid = false;

            //
            // Check for valid date and time on both start and end dates.
            //
            try
            {
                startDate = DateTime.Parse(tbStartDate.Text);
                endDate = DateTime.Parse(tbEndDate.Text);
            }
            catch { valid = false; }

            //
            // Must have at-least one image.
            //
            if (promotion.Documents.Count == 0)
                valid = false;

            //
            // If anything was not valid then put up an error message.
            //
            if (valid == false)
            {
                lbMessage.Text = "Invalid information entered. Please make sure to enter a title, start and end dates as well as one or more images.";
                lbMessage.Visible = true;

                return;
            }
            else
                lbMessage.Visible = false;

            //
            // Set all the various information about the promotion.
            //
            promotion.Title = tbTitle.Text.Trim();
            promotion.ContactName = "";
            promotion.ContactEmail = "";
            promotion.ContactPhone = "";
            promotion.EventID = -1;
            promotion.Campus = null;
            promotion.TopicArea = new Lookup(888);
            promotion.WebPromote = true;
            promotion.WebStartDate = startDate;
            promotion.WebEndDate = endDate;
            promotion.WebApprovedDate = DateTime.Now;
            promotion.WebApprovedBy = CurrentUser.Identity.Name;
            promotion.WebSummary = (cbWeekly.Checked ? "Weekly" : "");

            promotion.Save(CurrentUser.Identity.Name);

            if (!String.IsNullOrEmpty(Request.Params["RETURN"]))
            {
                Session.Remove("HDC_PROMOTION");
                Response.Redirect(Request.Params["RETURN"]);
            }
        }


        /// <summary>
        /// Update the order of an AJAX re-order operation with the drag and drop table.
        /// Figure out the new order by GUID and then update the Description to store the
        /// order in a 3-digit numeric.
        /// </summary>
        /// <param name="sender">Object causing the event to be triggered.</param>
        /// <param name="e">The event information.</param>
        protected void btnUpdateDocumentOrder_Click(object sender, EventArgs e)
        {
            int count = 1;


            //
            // Walk each item in order.
            //
            foreach (string str in this.hfDocumentOrder.Value.Split(new char[] { '&' }))
            {
                //
                // Header and Footer rows have an empty GUID, so skip them.
                //
                if ((str != string.Empty) && !str.EndsWith("="))
                {
                    string[] strArray3 = str.Split(new char[] { '_' });
                    try
                    {
                        //
                        // Update this item with the new order number.
                        //
                        Guid guid = new Guid(strArray3[strArray3.Length - 1]);
                        PromotionRequestDocument doc = promotion.Documents.FirstOrDefault(d => d.GUID == guid);
                        int ignored = doc.ByteArray.Length; // Needed to save.

                        doc.Description = String.Format("{0:000}", count++);
                        doc.Save(CurrentUser.Identity.Name);
                    }
                    catch
                    {
                    }
                }
            }

            ShowDocuments();
        }
    }
}