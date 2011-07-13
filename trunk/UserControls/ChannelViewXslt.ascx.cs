/**********************************************************************
* Description:	Displays Channel Topics via an XSLT transformation.
* Created By:	Daniel Hazelbaker @ High Desert Church
* Date Created:	8/20/2010 2:24:31 PM
**********************************************************************/

using System;
using System.Data;
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
using Arena.Utility;

namespace ArenaWeb.UserControls.Custom.HDC.Misc
{

	public partial class ChannelViewXslt : PortalControl
    {
        #region Module Settings

        [TextSetting("Channels", "Comma separated list of channels to pull from.", true)]
        public string ChannelsSetting { get { return Setting("Channels", null, true); } }

        [FileSetting("XsltUrl", "The path to the Xslt file to use.", true)]
        public string XsltUrlSetting { get { return Setting("XsltUrl", null, true); } }

        #endregion


        #region Event Handlers

        private void Page_Load(object sender, System.EventArgs e)
		{
            StringBuilder sb = new StringBuilder();

            
            foreach (String chan in ChannelsSetting.Split(','))
            {
                List<Topic> topics = new List<Topic>();
                Channel channel = new Channel(Convert.ToInt32(chan));

                foreach (Item item in channel.Items)
                {
                    Topic topic = null;

                    if (item.Active == false || item.PublishDate > DateTime.Now)
                        continue;

                    foreach (Topic t in topics)
                    {
                        if (t.TopicId == item.Topic.TopicId)
                        {
                            topic = t;
                            break;
                        }
                    }

                    if (topic == null)
                        topics.Add(item.Topic);
                }

                XmlDocument doc = new XmlDocument();
                XmlNode root = doc.CreateElement("topics");
                doc.AppendChild(root);
                foreach (Topic topic in topics)
                {
                    XmlNode node = doc.CreateElement("topic");

                    ArenaTextTools.AddXmlAttribute(node, "id", topic.TopicId.ToString());
                    ArenaTextTools.AddXmlAttribute(node, "title", topic.Title.ToString());
                    ArenaTextTools.AddXmlAttribute(node, "count", topic.Items.Count.ToString());
                    ArenaTextTools.AddXmlAttribute(node, "imageguid", topic.Image.GUID.ToString());
                    ArenaTextTools.AddXmlAttribute(node, "description", topic.Description.ToString());
                    ArenaTextTools.AddXmlAttribute(node, "channelid", topic.ChannelId.ToString());
                    ArenaTextTools.AddXmlAttribute(node, "activecount", topic.ActiveItems.Count.ToString());
                    ArenaTextTools.AddXmlAttribute(node, "active", Convert.ToInt32(topic.Active).ToString());
                    root.AppendChild(node);

                }
                XPathNavigator navigator = doc.CreateNavigator();
                XslCompiledTransform transform = new XslCompiledTransform();
                transform.Load(base.Server.MapPath(XsltUrlSetting));

                transform.Transform((IXPathNavigable)navigator, null, new StringWriter(sb));
            }

            ltContent.Text = sb.ToString();
		}
		
		#endregion

	}
}
