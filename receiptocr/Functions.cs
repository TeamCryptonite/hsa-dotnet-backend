using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.Drawing;
using ImageMagick;
using Newtonsoft.Json.Linq;

namespace receiptocr
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("receiptstoprocess")] string message, TextWriter log)
        {
            string blobUri = "";
            string resultReference = "";
            try
            {
                JObject jMessage = JObject.Parse(message);
                blobUri = (string) jMessage["blobUri"];
                resultReference = (string) jMessage["resultReference"];
            }
            catch (Exception ex)
            {
                log.WriteLine("Could not parse message. " + ex.Message);
                return;
            }

            if (string.IsNullOrWhiteSpace(blobUri) || string.IsNullOrWhiteSpace(resultReference))
            {
                log.WriteLine("Missing blobUri or resultReference.");
                return;
            }

            //Get cloudStorage blob

            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container =
                blobClient.GetContainerReference(ConfigurationManager.AppSettings["receiptocrresults"]);

            container.CreateIfNotExists();

            CloudBlockBlob blob = container.GetBlockBlobReference(resultReference);

            // Download blob
            MemoryStream stream = new MemoryStream();
            blob.DownloadToStream(stream);

            Bitmap img = new Bitmap(stream);

            // ImageMagick img
            var magickImg = new MagickImage(img);
            magickImg.Deskew(new Percentage(50));
            magickImg.Grayscale(PixelIntensityMethod.Rec709Luminance);
            magickImg.Enhance();
            magickImg.Despeckle();
            magickImg.Sharpen();

            // Start API to Google Vision


            log.WriteLine(magickImg.ToBase64());
        }
    }
}
