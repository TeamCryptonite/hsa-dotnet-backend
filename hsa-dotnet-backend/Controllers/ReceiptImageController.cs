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

            if (string.IsNullOrWhiteSpace(receipt.ImageRef))
            {
                var newBlobObj = AzureBlobHelper.CreateEmptyReceiptPictureBlob(receipt, imagetype);
                receipt.ImageRef = newBlobObj.ReceiptRef;

                if (newBlobObj == null)
                    return BadRequest("Could Not Create Blob");

                receipt.ImageRef = newBlobObj.ReceiptRef;
                await db.SaveChangesAsync();

                return Ok(new { PictureUrl = newBlobObj.SasUrl });
            }
            else
            {
                var blobUrl = AzureBlobHelper.GetEditReceiptPictureBlob(receipt);
                if (blobUrl == null)
                    return BadRequest("Could Not Find Blob");

                return Ok(new { PictureUrl = blobUrl });
            }
            
        }

        [HttpPatch]
        [Route("api/receipts/{receiptId:int}/receiptimage")]
        public async Task<IHttpActionResult> GetEditPictureBlob(int receiptId)
        {
            var receipt = await db.Receipts.FindAsync(receiptId);
            var userGuid = _identityHelper.GetCurrentUserGuid();
            if (receipt?.UserObjectId != userGuid)
                return NotFound();

            var blobUrl = AzureBlobHelper.GetEditReceiptPictureBlob(receipt);
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

            var isDeleted = AzureBlobHelper.DeleteReceiptPictureBlob(receipt);
            if (isDeleted == false)
                return BadRequest("Could Not Delete Blob");

            return Ok("Receipt Picture Blob Deleted.");
        }


        
    }
}
