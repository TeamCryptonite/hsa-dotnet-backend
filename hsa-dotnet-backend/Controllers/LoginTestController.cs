using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web.Http;

namespace hsa_dotnet_backend.Controllers
{
   [RoutePrefix("logintest")]
   [Authorize]
    public class LoginTestController : ApiController
    {
        [Route("test")]
        [HttpGet]
        public Object Test()
        {
            return new
            {
                msg = "Success"
            };
        }
    }
}
