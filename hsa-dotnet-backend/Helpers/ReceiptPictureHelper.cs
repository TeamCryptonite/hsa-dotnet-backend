using System;
using System.Configuration;
using System.Web.Hosting;
using HsaDotnetBackend.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HsaDotnetBackend.Helpers
{
    public class ReceiptPictureHelper
    {
        public class CreateEmptyReceiptReturn
        {
            public string ReceiptId { get; set; }
            public string SasUrl { get; set; }
        }
        private static CloudBlockBlob GetCloudBlockBlob(string BlockBlobReference)
        {
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container =
                blobClient.GetContainerReference(ConfigurationManager.AppSettings["ReceiptContainer"]);

            return container.GetBlockBlobReference(BlockBlobReference);
        }
        // TODO Add GenerateReceiptPictureBlobId method
        // TODO Add CreateEmptyReceiptPictureBlob method
        public static CreateEmptyReceiptReturn CreateEmptyReceiptPictureBlob(Receipt receipt, string imageType = "jpg", int urlValidForMinutes = 30)
        {
            string receiptImageId = GetNewReceiptImageId() + "." + imageType;
            CloudBlockBlob blockBlob = GetCloudBlockBlob(receiptImageId);

            using (var fileStream = System.IO.File.OpenRead(HostingEnvironment.MapPath("~/App_Data/Assets/missingreceipt.jpg")))
            {
                blockBlob.UploadFromStream(fileStream);
            }

            if (!blockBlob.Exists())
                return null;

            receipt.ImageId = receiptImageId;

            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(urlValidForMinutes),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write
            };

            string sasBlobToken = blockBlob.GetSharedAccessSignature(policy);

            return new CreateEmptyReceiptReturn(){ReceiptId = receiptImageId, SasUrl = blockBlob.Uri + sasBlobToken};
        }
        public static string GetReceiptPictureUrl(string receiptBlobId, int urlValidForMinutes = 30)
        {
            // Check the receiptBlobId
            if (string.IsNullOrEmpty(receiptBlobId))
                return null;

            CloudBlockBlob blockBlob = GetCloudBlockBlob(receiptBlobId);

            if (!blockBlob.Exists())
                return null;

            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(urlValidForMinutes),
                Permissions = SharedAccessBlobPermissions.Read
            };

            string sasBlobToken = blockBlob.GetSharedAccessSignature(policy);

            return blockBlob.Uri + sasBlobToken;
        }

        public static string GetNewReceiptImageId()
        {
            return Guid.NewGuid().ToString().Replace("-", "") + "rec";
        }
    }
}