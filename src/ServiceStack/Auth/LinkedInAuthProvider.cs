using System.Collections.Generic;
using System.Xml.Linq;
using ServiceStack.Configuration;

namespace ServiceStack.Auth
{
    /// <summary>
    /// Create new App at: https://www.linkedin.com/secure/developer
    /// </summary>
    public class LinkedInAuthProvider : OAuth2Provider
    {
        public const string Name = "linkedin";

        public const string Realm = "https://www.linkedin.com/uas/oauth2/authorization";

        public LinkedInAuthProvider(IAppSettings appSettings)
            : base(appSettings, Realm, Name)
        {
            this.AuthorizeUrl = appSettings.Get($"oauth.{Name}.AuthorizeUrl", Realm);
            this.AccessTokenUrl = appSettings.Get($"oauth.{Name}.AccessTokenUrl", "https://www.linkedin.com/uas/oauth2/accessToken");

            //Fields available at: http://developer.linkedin.com/documents/profile-fields
            this.UserProfileUrl = appSettings.Get($"oauth.{Name}.UserProfileUrl", 
                "https://api.linkedin.com/v1/people/~:(id,email-address,formatted-name,first-name,last-name,date-of-birth,public-profile-url,picture-url)");

            if (this.Scopes == null || this.Scopes.Length == 0)
            {
                this.Scopes = new[] {
                    "r_emailaddress", 
                    "r_basicprofile"
                };
            }
        }

        protected override Dictionary<string, string> CreateAuthInfo(string accessToken)
        {
            var url = this.UserProfileUrl.AddQueryParam("oauth2_access_token", accessToken);
            var contents = url.GetXmlFromUrl();
            var xml = XDocument.Parse(contents);
            var el = xml.Root;
            
            var authInfo = new Dictionary<string, string>
            {
                { "user_id", el.GetString("id") }, 
                { "username", el.GetString("email-address") }, 
                { "email", el.GetString("email-address") }, 
                { "name", el.GetString("formatted-name") }, 
                { "first_name", el.GetString("first-name") }, 
                { "last_name", el.GetString("last-name") },
                { "birthday", el.GetString("date-of-birth") },
                { "link", el.GetString("public-profile-url") },
                { AuthMetadataProvider.ProfileUrlKey,  el.GetString("picture-url") },
            };

            return authInfo;
        }
        
    }
}