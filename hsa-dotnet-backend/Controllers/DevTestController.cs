using System;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using HsaDotnetBackend.Models;
using HsaDotnetBackend.Models.DTOs;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

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
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("userreceipts");

            CloudBlockBlob blockBlob = container.GetBlockBlobReference("20170216_140140[1].jpg");

            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(30),
                Permissions = SharedAccessBlobPermissions.Read
            };

            string sasBlobToken = blockBlob.GetSharedAccessSignature(policy);

            return blockBlob.Uri + sasBlobToken;
        }
    }
}
