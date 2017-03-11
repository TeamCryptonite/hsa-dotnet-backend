using System.Configuration;
using System.Web.Http;
using AutoMapper;
using HsaDotnetBackend.Helpers;
using HsaDotnetBackend.Models;
using HsaDotnetBackend.Models.DTOs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;

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
            return ReceiptPictureHelper.GetReceiptImageUrl("20170216_140140[1].jpg");
        }

        [Route("devtest/messagequeue")]
        [HttpPost]
        public IHttpActionResult AddToMessageQueue([FromBody] JObject message)
        {
            if(message["imageBlobReference"] == null || message["resultReference"] == null)
                return BadRequest("Missing required params.");
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["MessageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference("receiptstoprocess");

            queue.CreateIfNotExists();

            CloudQueueMessage newMessage = new CloudQueueMessage(message.ToString());

            queue.AddMessage(newMessage);

            return Ok("Message saved to queue");
        }
    }
}
