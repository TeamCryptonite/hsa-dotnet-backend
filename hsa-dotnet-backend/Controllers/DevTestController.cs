using System;
using System.Web.Http;

namespace HsaDotnetBackend.Controllers
{
    [Authorize]
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
