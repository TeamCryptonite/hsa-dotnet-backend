using System.Web.Http;
using AutoMapper;
using HsaDotnetBackend.Helpers;
using HsaDotnetBackend.Models;
using HsaDotnetBackend.Models.DTOs;

namespace HsaDotnetBackend.Controllers
{ 
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

        [Route("devtest/post")]
        [HttpPost]
        public ReceiptDto PostTest(Receipt receipt)
        {
            return Mapper.Map<Receipt, ReceiptDto>(receipt);
        }

        //[Route("devtest/")]
        //[HttpGet]
        //public IQueryable<Product> DevTest()
        //{
        //    var products = context
        //}

        [Route("devtest/blob")]
        [HttpGet]
        public string GetBlob()
        {
            return ReceiptPictureHelper.GetReceiptPictureUri("20170216_140140[1].jpg");
        }
    }
}
