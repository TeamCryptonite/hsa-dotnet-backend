using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using HsaDotnetBackend.Models;

namespace HsaDotnetBackend.Helpers
{
    public class IdentityHelper : IIdentityHelper
    {
        private readonly Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();
        public Guid GetCurrentUserGuid()
        {
            var identity = HttpContext.Current.User.Identity as ClaimsIdentity;

            if (identity == null)
                return Guid.Empty;

            Guid userGuid = new Guid(identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value);

            if (db.Users.Find(userGuid) == null)
            {
                User user = new User()
                {
                    UserObjectId = userGuid,
                    //DisplayName = identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value,
                    DisplayName = "No Display Name",
                    EmailAddress  = identity.Claims.SingleOrDefault(m => m.Type.ToLower() == "emails")?.Value,
                    GivenName = identity.Claims.SingleOrDefault(m => m.Type.ToLower() == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value,
                    SurName = identity.Claims.SingleOrDefault(m => m.Type.ToLower() == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value
                };

                db.Users.Add(user);
                db.SaveChangesAsync();
            }

            return userGuid;
        }
    }
}