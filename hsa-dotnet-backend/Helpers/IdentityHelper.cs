using System;
using System.Security.Claims;
using System.Web;

namespace HsaDotnetBackend.Helpers
{
    public class IdentityHelper : IIdentityHelper
    {
        public Guid GetCurrentUserGuid()
        {
            var identity = HttpContext.Current.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                return new Guid(identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value);
            }
            else
            {
                return Guid.Empty;
            }
        }
    }
}