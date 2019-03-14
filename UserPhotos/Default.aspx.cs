using System;
using System.Configuration;
using System.DirectoryServices;
using System.IO;
using System.Web;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sAMAccountName = Request.QueryString["SAMAccountName"];

        Response.Buffer = true;
        Response.Charset = "";
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.ContentType = "image/jpeg";
        Response.BinaryWrite(GetUserPicture(sAMAccountName));
        Response.Flush();
        Response.End();
    }
    private byte[] GetUserPicture(string sAMAccountName)
    {
        byte[] returnValue;
        try
        {
            // Get configuration
            string domainFQDN = ConfigurationManager.AppSettings["DomainFQDN"];
            string domainDN = ConfigurationManager.AppSettings["DomainDN"];
            string domainNetBIOS = ConfigurationManager.AppSettings["DomainNetBIOS"];
            string domainUserName = ConfigurationManager.AppSettings["DomainUserName"];
            string domainPassword = ConfigurationManager.AppSettings["DomainPassword"];

            DirectoryEntry directoryEntry;
            if ((!string.IsNullOrEmpty(domainNetBIOS)) && (!string.IsNullOrEmpty(domainUserName)) && (!string.IsNullOrEmpty(domainPassword)))
            {
                // Using credentials from Web.Config
                directoryEntry = new DirectoryEntry($@"LDAP://{domainFQDN}/{domainDN}", $"{domainNetBIOS}\\{domainUserName}", $"{domainPassword}");
            }
            else
            {
                // Using Application Pool Identity
                directoryEntry = new DirectoryEntry($@"LDAP://{domainFQDN}/{domainDN}");
            }
            
            DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry)
            {
                Filter = string.Format("(&(SAMAccountName={0}))", sAMAccountName)
            };
            SearchResult user = directorySearcher.FindOne();
            returnValue = user.Properties["thumbnailPhoto"][0] as byte[];
        }
        catch
        {
            // Handles error and default image
            System.Drawing.Image img = System.Drawing.Image.FromFile($@"{HttpRuntime.AppDomainAppPath}\DefaultImage.png");
            img = img.GetThumbnailImage(370, 370, null, IntPtr.Zero);
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                returnValue = ms.ToArray();
            }
        }
        return returnValue;
    }
}