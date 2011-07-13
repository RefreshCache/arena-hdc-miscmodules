using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Document;
using Arena.Marketing;
using Arena.Portal;
using System.Xml;
using System.Web.Caching;

namespace ArenaWeb.UserControls.Custom.HDC.Misc
{
	public partial class ShowPromotion : PortalControl
	{
		#region Module Settings

		[NumericSetting( "Promotion ID", "The ID number of the promotion to display. Promotion needs web content, but does not need to be approved.", true )]
		public int PromotionIDSetting { get { return Convert.ToInt32(Setting( "PromotionID", "-1", true )); } }

		[TextSetting( "XsltUrl", "The path to the XSLT file to use. Default '~/UserControls/Custom/HDC/Misc/XSLT/details.xslt')", false )]
		public string XsltUrlSetting { get { return Setting( "XsltUrl", "~/UserControls/Custom/HDC/Misc/XSLT/details.xslt", false ); } }

		#endregion

		#region Event Handlers

		protected void Page_Load( object sender, EventArgs e )
		{
			xmlTransform.Document = BuildXMLForPromotion();
			xmlTransform.XslFileURL = XsltUrlSetting;
		}

		#endregion

		private XmlDocument BuildXMLForPromotion()
		{
			// Create an empty XML doc to hold our xml output
			XmlDocument document = null;

			document = new XmlDocument();
			XmlNode rootNode = document.CreateNode( XmlNodeType.Element, "promotion", document.NamespaceURI );
			document.AppendChild( rootNode );

			PromotionRequest item = new PromotionRequest(PromotionIDSetting);
			XmlNode itemNode = document.CreateNode( XmlNodeType.Element, "item", document.NamespaceURI );
			rootNode.AppendChild( itemNode );

			XmlAttribute itemAttrib = document.CreateAttribute( "tmp" );

			SetNodeAttribute( document, itemNode, itemAttrib, "id", item.PromotionRequestID.ToString() );
			SetNodeAttribute( document, itemNode, itemAttrib, "title", item.Title );
			SetNodeAttribute( document, itemNode, itemAttrib, "summary", item.WebSummary );
			SetNodeAttribute( document, itemNode, itemAttrib, "details", item.WebText );
			SetNodeAttribute( document, itemNode, itemAttrib, "summaryImageUrl", String.Format("CachedBlob.aspx?guid={0}", item.WebSummaryImageBlob.GUID.ToString()) );
			SetNodeAttribute( document, itemNode, itemAttrib, "detailsImageUrl", String.Format("CachedBlob.aspx?guid={0}", item.WebImageBlob.GUID.ToString()) );

			return document;
		}

		private void SetNodeAttribute( XmlDocument document, XmlNode node, XmlAttribute attrib, string attribName, string attribValue )
		{
			attrib = document.CreateAttribute( attribName );
			attrib.Value = attribValue;
			node.Attributes.Append( attrib );
		}
	}
}
