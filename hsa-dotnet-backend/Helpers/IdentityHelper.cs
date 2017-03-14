using System;
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
            //var DisplayName = identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value;
            var EmailAddress =
                identity.FindFirst(ClaimTypes.Email).Value;
            var GivenName = identity.FindFirst(ClaimTypes.GivenName).Value;
            var SurName = identity.FindFirst(ClaimTypes.Surname).Value;

            if (db.Users.Find(userGuid) == null)
            {
                User user = new User()
                {
                    UserObjectId = userGuid,
                    //DisplayName = identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value,
                    EmailAddress = identity.FindFirst(ClaimTypes.Email).Value,
                    GivenName = identity.FindFirst(ClaimTypes.GivenName).Value,
                    SurName = identity.FindFirst(ClaimTypes.Surname).Value
                };

                db.Users.Add(user);
                db.SaveChangesAsync();
            }

            return userGuid;
        }
    }
}