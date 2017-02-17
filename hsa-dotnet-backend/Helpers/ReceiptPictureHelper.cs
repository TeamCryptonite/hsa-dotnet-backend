using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HsaDotnetBackend.Helpers
{
    public class ReceiptPictureHelper
    {
        public static string GetReceiptPictureUri(string receiptBlobId, int uriValidForMinutes = 30)
        {
            // Check the receiptBlobId
            if (string.IsNullOrEmpty(receiptBlobId))
                return null;

            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = 
                blobClient.GetContainerReference(ConfigurationManager.AppSettings["ReceiptContainer"]);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(receiptBlobId);

            if (!blockBlob.Exists())
                return null;

            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(uriValidForMinutes),
                Permissions = SharedAccessBlobPermissions.Read
            };

            string sasBlobToken = blockBlob.GetSharedAccessSignature(policy);

            return blockBlob.Uri + sasBlobToken;
        }

        public static string GetNewReceiptPictureId()
        {
            return Guid.NewGuid().ToString().Replace("-", "") + "rec";
        }
    }
}