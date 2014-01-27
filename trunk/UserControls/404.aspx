<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1"/>
<title>404 - File or directory not found.</title>
<style type="text/css">
<!--
body{margin:0;font-size:.7em;font-family:Verdana, Arial, Helvetica, sans-serif;background:#EEEEEE;}
fieldset{padding:0 15px 10px 15px;} 
h1{font-size:2.4em;margin:0;color:#FFF;}
h2{font-size:1.7em;margin:0;color:#CC0000;} 
h3{font-size:1.2em;margin:10px 0 0 0;color:#000000;} 
#header{width:96%;margin:0 0 0 0;padding:6px 2% 6px 2%;font-family:"trebuchet MS", Verdana, sans-serif;color:#FFF;
background-color:#555555;}
#content{margin:0 0 0 2%;position:relative;}
.content-container{background:#FFF;width:96%;margin-top:8px;padding:10px;position:relative;}
-->
</style>
</head>
<body>
<div id="header"><h1>Server Error</h1></div>
<div id="content">
 <div class="content-container"><fieldset>
  <h2>404 - File or directory not found.</h2>
  <h3>The resource you are looking for might have been removed, had its name changed, or is temporarily unavailable.</h3>
 </fieldset></div>
</div>
<asp:label style="display: none;" id="lbDebug" runat="server" />
</body>
</html>


<%@ Import Namespace="System" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Web.Configuration" %>
<%@ Import Namespace="Arena.Core" %>
<%@ Import Namespace="Arena.Portal" %>
<%@ Import Namespace="System.Text.RegularExpressions" %>

<script runat="server">

public void Page_Load(object sender, System.EventArgs e)
{
    //
    // Instructions:
    // Step 1) Create a Lookup in Arena with the following information:
    //     Name: Friendly URLs
    //     Description: Friendly URLs can be listed here. Enter the URL path
    //                  to be redirected (example /mypage, the beginning / is
    //                  important, do not include any trailing slash) in the
    //                  Value field. The Destination field should be the
    //                  target URL to redirect to. You may optionally enter a
    //                  Portal ID number to restrict the redirect to just that
    //                  portal. If you enter any value in the RegEx field the
    //                  Value field will be treated as a regular expression
    //                  match.
    //     Qualifier 1 Title: Destination
    //     Qualifier 2 Title: PortalID
    //     Qualifier 3 Title: RegEx
    //
    // Step 2) Install this file in your root Arena folder, the same location
    //         as the Default.aspx file. Then update the "FriendlyUrlID"
    //         value below to reflect the value of the lookup type ID number.
    //
    // Step 3) In IIS (this assumes you are using IIS 7.x), go to the Site
    //         your Arena installation uses and into the Error Pages. Add a
    //         new error page with the following information:
    //     Status code: 404
    //     Execute a URL on this site: /404.aspx
    //
    // Step 4) Test!
    //
    // Note: You may customize the standard 404 message displayed above.
    //
    // Regular Expressions:
    //     You can use regular expressions to match and redirect users to pages
    //     that relate to their original url. For example you could redirect all
    //     pages like /events/<number> to the proper Arena page for displaying
    //     that event promotion. You can use "back references" in the destination
    //     to take the value from the original string. A back reference is denoted
    //     by a \n (e.g. \1, \2, etc.) in the Destination field. In the regular
    //     expression each grouping of () denotes the \nth replacement. i.e. the
    //     first grouping of () is \1, the second grouping of () is \2, and so on.
    //
    // Example:
    //     Value: /events/([0-9]+)
    //     Destination: Default.aspx?page=3459&promotionId=\1
    //     RegEx: yes
    //
    // Primer: http://marvin.cs.uidaho.edu/Handouts/regex.html
    //
    int FriendlyUrlID = 138;

    try
    {
        string Path;
        LookupCollection lc = new LookupCollection(FriendlyUrlID);
        
        if (Request.RawUrl.Contains(";"))
            Path = new Uri(Request.RawUrl.Split(';')[1]).AbsolutePath;
        else if (Request.RawUrl.Contains("aspxerrorpath"))
            Path = Request.RawUrl.Split('=')[1];
        else
            Path = Request.RawUrl;
        lbDebug.Text = "URL: " + Path + "    Raw: " + Request.RawUrl;

        //
        // Get the Portal.
        //
        string domain = HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToLower();
        PortalCollection portals = new PortalCollection();
        portals.LoadByOrganizationId(ArenaContext.Current.Organization.OrganizationID);
        Portal portal = portals.FindByDomain(domain);
        if (portal == null)
        {
            portal = new Portal(int.Parse(WebConfigurationManager.AppSettings["DefaultPortalId"]));
        }

        //
        // Look through each lookup and look for a match.
        //
        foreach (Lookup lk in lc)
        {
            //
            // Test if we are in the correct portal.
            //
            if (String.IsNullOrEmpty(lk.Qualifier2) || Convert.ToInt32(lk.Qualifier2) == portal.PortalID)
            {
                if (String.IsNullOrEmpty(lk.Qualifier3))
                {
                    if (Path.Equals(lk.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        Response.Redirect(lk.Qualifier);
                        break;
                    }
                }
                else
                {
                    Match match = Regex.Match(Path, lk.Value, RegexOptions.IgnoreCase);
                    
                    if (match.Success)
                    {
                        String target = lk.Qualifier;
                        int i;

                        for (i = 1; i <= match.Groups.Count; i++)
                        {
                            target = target.Replace("\\1", match.Groups[i].Value);
                        }

                        Response.Redirect(target);
                        break;
                    }
                }
            }
        }
    }
    catch
    {
    }
}

public class JunkPage : BasePage
{
    public override Control DynamicContentContainer { get { return null; } }
}
</script>
