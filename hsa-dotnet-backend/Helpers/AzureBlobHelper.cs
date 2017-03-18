using System;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using HsaDotnetBackend.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HsaDotnetBackend.Helpers
{
    public class AzureBlobHelper
    {
        private const string UserReceiptContainerName = "userreceipts";
        private const string ReceiptOcrContainerName = "receiptocrresults";
        private const string ProductImagesContainerName = "productimages";

        private static CloudBlockBlob GetCloudBlockBlob(string blockBlobReference, string containerName)
        {
            var storageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            var blobClient = storageAccount.CreateCloudBlobClient();

            var container =
                blobClient.GetContainerReference(containerName);

            container.CreateIfNotExists();

            return container.GetBlockBlobReference(blockBlobReference);
        }

        public static SharedAccessBlobPolicy GenerateSasPolicy(int urlValidForMinutes = 30, bool writeAccess = false,
            bool deleteAccess = false)
        {
            // https://azure.github.io/azure-sdk-for-java/com/microsoft/azure/storage/blob/SharedAccessBlobPolicy.html
            // A String that represents the shared access permissions. The string must contain one or more of the following values. Note they must all be lowercase.
            //r: Read access.  a: Add access.     c: Create access.
            //w: Write access. d: Delete access.  l: List access.
            var permissionsString = "r";
            if (writeAccess)
                permissionsString += "w";
            if (deleteAccess)
                permissionsString += "d";

            var policy = new SharedAccessBlobPolicy();
            policy.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
            policy.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(urlValidForMinutes);
            policy.Permissions = SharedAccessBlobPolicy.PermissionsFromString(permissionsString);

            return policy;
        }

        public static CreateEmptyReceiptReturn CreateEmptyReceiptPictureBlob(Receipt receipt, string imageType = "jpg",
            int urlValidForMinutes = 30)
        {
            var receiptImageRef = GenerateNewReceiptImageName() + "." + imageType;
            var blockBlob = GetCloudBlockBlob(receiptImageRef, UserReceiptContainerName);

            using (var fileStream = File.OpenRead(HostingEnvironment.MapPath("~/App_Data/Assets/missingreceipt.jpg")))
            {
                blockBlob.UploadFromStream(fileStream);
            }

            if (!blockBlob.Exists())
                return null;

            receipt.ImageRef = receiptImageRef;

            var policy = GenerateSasPolicy(urlValidForMinutes, true);
            var sasBlobToken = blockBlob.GetSharedAccessSignature(policy);

            return new CreateEmptyReceiptReturn {ReceiptRef = receiptImageRef, SasUrl = blockBlob.Uri + sasBlobToken};
        }

        public static string GetEditReceiptPictureBlob(Receipt receipt, int urlValidForMinutes = 30)
        {
            var receiptImageId = receipt.ImageRef;
            if (string.IsNullOrEmpty(receiptImageId))
                return null;

            var blockBlob = GetCloudBlockBlob(receiptImageId, UserReceiptContainerName);

            if (!blockBlob.Exists())
                return null;

            var policy = GenerateSasPolicy(urlValidForMinutes, true);
            var sasBlobToken = blockBlob.GetSharedAccessSignature(policy);

            return blockBlob.Uri + sasBlobToken;
        }


        public static bool DeleteReceiptPictureBlob(Receipt receipt, int urlValidForMinutes = 30)
        {
            var receiptImageId = receipt.ImageRef;
            if (string.IsNullOrEmpty(receiptImageId))
                return false;

            var blockBlob = GetCloudBlockBlob(receiptImageId, UserReceiptContainerName);

            blockBlob.DeleteIfExists();

            if (!blockBlob.Exists())
                return true;

            return false;
        }

        public static string GetOcrResultsUrl(string ocrRef, int urlValidForMinutes)
        {
            if (string.IsNullOrWhiteSpace(ocrRef))
                return null;

            CloudBlockBlob blockBlob;
            try
            {
                blockBlob = GetCloudBlockBlob(ocrRef, ReceiptOcrContainerName);
            }
            catch
            {
                return null;
            }

            if (!blockBlob.Exists())
                return null;

            var policy = GenerateSasPolicy(urlValidForMinutes);

            var sasBlobToken = blockBlob.GetSharedAccessSignature(policy);

            return blockBlob.Uri + sasBlobToken;
        }

        public static string GetOcrResultsUrl(string ocrRef)
        {
            return GetOcrResultsUrl(ocrRef, 30);
        }

        public static string GetReceiptImageUrl(string receiptBlobId, int urlValidForMinutes)
        {
            // Check the receiptBlobId
            if (string.IsNullOrEmpty(receiptBlobId))
                return null;

            CloudBlockBlob blockBlob;
            try
            {
                blockBlob = GetCloudBlockBlob(receiptBlobId, UserReceiptContainerName);
            }
            catch
            {
                return null;
            }

            if (!blockBlob.Exists())
                return null;

            var policy = GenerateSasPolicy(urlValidForMinutes);

            var sasBlobToken = blockBlob.GetSharedAccessSignature(policy);

            return blockBlob.Uri + sasBlobToken;
        }

        public static string GetReceiptImageUrl(string receiptBlobId)
        {
            return GetReceiptImageUrl(receiptBlobId, 30);
        }

        public static string GenerateNewReceiptImageName()
        {
            return Guid.NewGuid().ToString().Replace("-", "") + "rec";
        }

        public class CreateEmptyReceiptReturn
        {
            public string ReceiptRef { get; set; }
            public string SasUrl { get; set; }
        }
    }
}