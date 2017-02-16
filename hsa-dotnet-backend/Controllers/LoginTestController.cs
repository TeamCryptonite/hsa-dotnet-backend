using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace HsaDotnetBackend.Controllers
{
   [RoutePrefix("logintest")]
   [Authorize]
    public class LoginTestController : ApiController
    {
        [Route("test")]
        [HttpGet]
        public string Test()
        {
            var identity = User.Identity as ClaimsIdentity;

            var userName = identity.Name;

            return identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            //return userName;
            //return identity.Claims.Select(c => new
            //{
            //    Type = c.Type,
            //    Value = c.Value
            //});
        }
    }
}
