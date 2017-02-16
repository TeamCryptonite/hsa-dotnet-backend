﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace HsaDotnetBackend.Helpers
{
    public class IdentityHelper
    {
        public static Guid GetCurrentUserGuid()
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