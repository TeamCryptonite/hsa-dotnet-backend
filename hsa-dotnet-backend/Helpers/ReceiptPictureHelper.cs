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

        private static SharedAccessBlobPolicy GenerateSasPolicy(int urlValidForMinutes = 30, bool writeAccess = false, bool deleteAccess = false)
        {
            // https://azure.github.io/azure-sdk-for-java/com/microsoft/azure/storage/blob/SharedAccessBlobPolicy.html
            // A String that represents the shared access permissions. The string must contain one or more of the following values. Note they must all be lowercase.
            //r: Read access.  a: Add access.     c: Create access.
            //w: Write access. d: Delete access.  l: List access.
            string permissionsString = "r";
            if (writeAccess)
                permissionsString += "w";
            if (deleteAccess)
                permissionsString += "d";

            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy();
            policy.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
            policy.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(urlValidForMinutes);
            policy.Permissions = SharedAccessBlobPolicy.PermissionsFromString(permissionsString);

            return policy;
        }
        
        public static CreateEmptyReceiptReturn CreateEmptyReceiptPictureBlob(Receipt receipt, string imageType = "jpg", int urlValidForMinutes = 30)
        {
            string receiptImageId = GenerateNewReceiptImageName() + "." + imageType;
            CloudBlockBlob blockBlob = GetCloudBlockBlob(receiptImageId);

            using (var fileStream = System.IO.File.OpenRead(HostingEnvironment.MapPath("~/App_Data/Assets/missingreceipt.jpg")))
                blockBlob.UploadFromStream(fileStream);

            if (!blockBlob.Exists())
                return null;

            receipt.ImageId = receiptImageId;

            SharedAccessBlobPolicy policy = GenerateSasPolicy(urlValidForMinutes, true);
            string sasBlobToken = blockBlob.GetSharedAccessSignature(policy);

            return new CreateEmptyReceiptReturn(){ReceiptId = receiptImageId, SasUrl = blockBlob.Uri + sasBlobToken};
        }

        public static string GetEditReceiptPictureBlob(Receipt receipt, int urlValidForMinutes = 30)
        {
            string receiptImageId = receipt.ImageId;
            if (string.IsNullOrEmpty(receiptImageId))
                return null;

            CloudBlockBlob blockBlob = GetCloudBlockBlob(receiptImageId);

            if (!blockBlob.Exists())
                return null;

            SharedAccessBlobPolicy policy = GenerateSasPolicy(urlValidForMinutes, true);
            string sasBlobToken = blockBlob.GetSharedAccessSignature(policy);

            return blockBlob.Uri + sasBlobToken;
        }

        public static bool DeleteReceiptPictureBlob(Receipt receipt, int urlValidForMinutes = 30)
        {
            string receiptImageId = receipt.ImageId;
            if (string.IsNullOrEmpty(receiptImageId))
                return false;

            CloudBlockBlob blockBlob = GetCloudBlockBlob(receiptImageId);

            blockBlob.DeleteIfExists();

            if (!blockBlob.Exists())
                return true;
            
            return false;
            
        }
        public static string GetReceiptPictureUrl(string receiptBlobId, int urlValidForMinutes = 30)
        {
            // Check the receiptBlobId
            if (string.IsNullOrEmpty(receiptBlobId))
                return null;

            CloudBlockBlob blockBlob = GetCloudBlockBlob(receiptBlobId);

            if (!blockBlob.Exists())
                return null;

            SharedAccessBlobPolicy policy = GenerateSasPolicy(urlValidForMinutes);

            string sasBlobToken = blockBlob.GetSharedAccessSignature(policy);

            return blockBlob.Uri + sasBlobToken;
        }

        public static string GenerateNewReceiptImageName()
        {
            return Guid.NewGuid().ToString().Replace("-", "") + "rec";
        }
    }
}