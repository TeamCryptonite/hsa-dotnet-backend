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
using System.Net;
using ImageMagick;
using Newtonsoft.Json.Linq;
using RestSharp;

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

            if (blob == null)
            {
                log.WriteLine("Empty blob at blob resultReference.");
                return;
            }

            // Download blob
            MemoryStream stream = new MemoryStream();
            blob.DownloadToStream(stream);

            // ImageMagick img
            var magickImg = new MagickImage(stream);
            magickImg.Deskew(new Percentage(50));
            magickImg.Grayscale(PixelIntensityMethod.Rec709Luminance);
            magickImg.Enhance();
            magickImg.Despeckle();
            magickImg.Sharpen();

            // Start API to Google Vision
            string googleApiKey = ConfigurationManager.AppSettings["GoogleApiKey"];

            // Create request body
            var body =
                JObject.Parse(
                    "{\"requests\":[{\"image\":{\"content\":\"\"},\"features\":[{\"type\":\"TEXT_DETECTION\",\"maxResults\":1}]}]}");
            body["requests"][0]["image"]["content"] = magickImg.ToBase64();

            // Create REST Call
            var client = new RestClient("https://vision.googleapis.com/v1");
            var request = new RestRequest("images:annotate", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddQueryParameter("key", googleApiKey);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                log.WriteLine("GoogleAPI Status did not return ok.");
                throw new Exception("GoogleAPI Status did not return OK");
            }

            var content = JObject.Parse(response.Content);

            var textAnnotations = content["responses"][0]["textAnnotations"][0];

            blob.UploadText(textAnnotations["description"].ToString());


            log.WriteLine(magickImg.ToBase64());
        }
    }
}
