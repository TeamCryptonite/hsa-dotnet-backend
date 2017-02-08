using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web.Http;

namespace hsa_dotnet_backend.Controllers
{
    [Route("/logintest/")]
    public class LoginTestController : ApiController
    {
        public Object test()
        {
            return new
            {
                msg = "Success"
            };
        }
    }
}
