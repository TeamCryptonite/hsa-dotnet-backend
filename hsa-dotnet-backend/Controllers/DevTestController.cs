using System;
using System.Linq;
using System.Web.Http;
using HsaDotnetBackend.Models;

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

        //[Route("devtest/")]
        //[HttpGet]
        //public IQueryable<Product> DevTest()
        //{
        //    var products = context
        //}
    }
}
