using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using HsaDotnetBackend.Helpers;
using HsaDotnetBackend.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;

namespace HsaDotnetBackend.Controllers
{
    public class ReceiptOcrController : ApiController
    {
        //Identity 
        private readonly IIdentityHelper _identityHelper;
        private readonly Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();

        public ReceiptOcrController(IIdentityHelper identity)
        {
            _identityHelper = identity;
        }
        // OCR
        // TODO: REFACTOR
        [HttpPost]
        [Route("api/receipts/{receiptId:int}/receiptimageocr")]
        public async Task<IHttpActionResult> StartReceiptImageOcr(int receiptId)
        {
            var receipt = await db.Receipts.FindAsync(receiptId);
            var userGuid = _identityHelper.GetCurrentUserGuid();
            if (receipt?.UserObjectId != userGuid)
                return NotFound();

            // Access the queue storage account
            CloudStorageAccount queueStorageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["MessageConnectionString"]);
            CloudQueueClient queueClient = queueStorageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference("receiptstoprocess");

            queue.CreateIfNotExists();

            var message = new JObject();
            if (string.IsNullOrWhiteSpace(receipt.ImageRef))
                return BadRequest("Receipt does not include image id");
            message.Add("imageBlobReference", receipt.ImageRef);
            message.Add("receiptId", receiptId);

            CloudQueueMessage newMessage = new CloudQueueMessage(message.ToString());

            queue.AddMessage(newMessage);

            // Save status to database
            receipt.WaitingForOcr = true;
            await db.SaveChangesAsync();

            // Return OK
            return Ok();
        }
    }
}
