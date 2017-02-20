using System;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HsaDotnetBackend.Helpers
{
    public class ReceiptPictureHelper
    {
        // TODO Add GenerateReceiptPictureBlobId method
        // TODO Add CreateEmptyReceiptPictureBlob meth
        public static string GetReceiptPictureUrl(string receiptBlobId, int urlValidForMinutes = 30)
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
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(urlValidForMinutes),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write
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