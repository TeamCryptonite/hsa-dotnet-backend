using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace hsa_dotnet_backend.Controllers
{
    public class DevTestController : ApiController
    {
        [Route("devtest/")]
        [HttpGet]
        public Object DevTest()
        {
            return new
            {
                Test = "First Test",
                Success = true
            };
        }
    }
}
