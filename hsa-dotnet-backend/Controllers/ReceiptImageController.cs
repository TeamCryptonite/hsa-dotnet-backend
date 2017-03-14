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
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;

namespace HsaDotnetBackend.Controllers
{
    public class ReceiptImageController : ApiController
    {
        //Identity 
        private readonly IIdentityHelper _identityHelper;
        private readonly Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();

        public ReceiptImageController(IIdentityHelper identity)
        {
            _identityHelper = identity;
        }
        // Receipt Pictures

        [HttpPost]
        [Route("api/receipts/{receiptId:int}/receiptimage")]
        public async Task<IHttpActionResult> CreatePictureBlob(int receiptId, string imagetype = "jpg")
        {
            var receipt = await db.Receipts.FindAsync(receiptId);
            var userGuid = _identityHelper.GetCurrentUserGuid();
            if (receipt?.UserObjectId != userGuid)
                return NotFound();

            var newBlobObj = ReceiptPictureHelper.CreateEmptyReceiptPictureBlob(receipt, imagetype);
            receipt.ImageRef = newBlobObj.ReceiptRef;

            if (newBlobObj == null)
                return BadRequest("Could Not Create Blob");

            receipt.ImageRef = newBlobObj.ReceiptRef;
            await db.SaveChangesAsync();

            return Ok(new { PictureUrl = newBlobObj.SasUrl });
        }

        [HttpPatch]
        [Route("api/receipts/{receiptId:int}/receiptimage")]
        public async Task<IHttpActionResult> GetEditPictureBlob(int receiptId)
        {
            var receipt = await db.Receipts.FindAsync(receiptId);
            var userGuid = _identityHelper.GetCurrentUserGuid();
            if (receipt?.UserObjectId != userGuid)
                return NotFound();

            var blobUrl = ReceiptPictureHelper.GetEditReceiptPictureBlob(receipt);
            if (blobUrl == null)
                return BadRequest("Could Not Find Blob");

            return Ok(new { PictureUrl = blobUrl });
        }

        [HttpDelete]
        [Route("api/receipts/{receiptId:int}/receiptimage")]
        public async Task<IHttpActionResult> DeletePictureBlob(int receiptId)
        {
            var receipt = await db.Receipts.FindAsync(receiptId);
            var userGuid = _identityHelper.GetCurrentUserGuid();
            if (receipt?.UserObjectId != userGuid)
                return NotFound();

            var isDeleted = ReceiptPictureHelper.DeleteReceiptPictureBlob(receipt);
            if (isDeleted == false)
                return BadRequest("Could Not Delete Blob");

            return Ok("Receipt Picture Blob Deleted.");
        }


        // OCR
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

            

            // Access the results blob storage account
            CloudStorageAccount blobStorageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudBlobClient blobClient = blobStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer resultContainer =
                blobClient.GetContainerReference("receiptocrresults");

            resultContainer.CreateIfNotExists();

            string resultBlobReference = receipt.ImageRef.Replace("rec", "result") + ".json";

            CloudBlockBlob resultBlob = resultContainer.GetBlockBlobReference(resultBlobReference);

            // Set inital JSON Result
            var result = new JObject();
            result.Add("Status", "Queued");

            // Set result to the blob
            resultBlob.UploadText(result.ToString());

            // Finish message and add message to queue
            message.Add("resultReference", resultBlobReference);

            CloudQueueMessage newMessage = new CloudQueueMessage(message.ToString());

            queue.AddMessage(newMessage);

            // Return URL for result reference
            var policy = ReceiptPictureHelper.GenerateSasPolicy();
            string sasBlobToken = resultBlob.GetSharedAccessSignature(policy);

            return Ok(resultBlob.Uri + sasBlobToken);
        }
    }
}
