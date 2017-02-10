using System;
using System.Web.Http;

namespace HsaDotnetBackend.Controllers
{
    [Authorize]
    public class DevTestController : ApiController
    {
        [Route("")]
        [HttpGet]
        public object DefaultRouteTesting()
        {
            return new
            {
                msg = "Default Route Testing"
            };
        }

        [Route("devtest/")]
        [HttpGet]
        public object DevTest()
        {
            return new
            {
                Test = "First Test",
                Success = true
            };
        }
    }
}
