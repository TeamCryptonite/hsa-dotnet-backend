using System;
using System.Web.Http;

namespace HsaDotnetBackend.Controllers
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
