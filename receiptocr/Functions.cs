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
using System.Text.RegularExpressions;
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
            string imageBlobReference = "";
            string resultReference = "";
            try
            {
                JObject jMessage = JObject.Parse(message);
                imageBlobReference = (string) jMessage["imageBlobReference"];
                resultReference = (string) jMessage["resultReference"];
            }
            catch (Exception ex)
            {
                log.WriteLine("Could not parse message. " + ex.Message);
                return;
            }

            if (string.IsNullOrWhiteSpace(imageBlobReference) || string.IsNullOrWhiteSpace(resultReference))
            {
                log.WriteLine("Missing blobUri or resultReference.");
                return;
            }

            //Get cloudStorage blob

            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer resultContainer =
                blobClient.GetContainerReference("receiptocrresults");

            resultContainer.CreateIfNotExists();

            CloudBlockBlob resultBlob = resultContainer.GetBlockBlobReference(resultReference);

            CloudBlobContainer imageContainer =
                blobClient.GetContainerReference(ConfigurationManager.AppSettings["ReceiptContainer"]);
            CloudBlockBlob imageBlob = imageContainer.GetBlockBlobReference(imageBlobReference);

            if (resultBlob == null)
            {
                log.WriteLine("Empty blob at blob resultReference.");
                return;
            }

            // Download blob
            MemoryStream stream = new MemoryStream();
            imageBlob.DownloadToStream(stream);

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

            var textAnnotations = content["responses"][0]["textAnnotations"][0]["description"].ToString();

            string pattern =
                @"(?<pline>(?<price>\d+\.\d\d)\s*(?:[TXR]\s)?\s*)?(?<name>[a-zA-Z].+)\s(?<UPC>\d+\s+)?[F]?\s*(?(pline)|(?<price2>\d+\.\d\d)\s*(?:[TXR]\s)?)";
            MatchCollection matches = Regex.Matches(textAnnotations, pattern);
            var buildJson = new JArray();
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var lineItem = new JObject();
                    Match upcNameMatch = Regex.Match(match.Groups["name"].Value, @"(?<name>.*)\s+(?<upc>\d+)[^\.]");
                    if (upcNameMatch.Success)
                    {
                        if (upcNameMatch.Groups["name"].Success)
                            lineItem.Add("name", upcNameMatch.Groups["name"].Value);
                        if (upcNameMatch.Groups["upc"].Success)
                            lineItem.Add("upc", upcNameMatch.Groups["upc"].Value);
                    }
                    else
                    {
                        if (match.Groups["name"].Success)
                            lineItem.Add("name", match.Groups["name"].Value);
                    }
                    if (match.Groups["price"].Success)
                        lineItem.Add("price", match.Groups["price"].Value);
                    else if (match.Groups["price2"].Success)
                        lineItem.Add("price", match.Groups["price2"].Value);

                    buildJson.Add(lineItem);
                }
            }

            resultBlob.UploadText(buildJson.ToString());

            // Kindof works: (?<pline>(?<price>\d+\.\d\d)\s*(?:[TXR]\s)?\s*)?(?<name>[a-zA-Z].+)\s(?<UPC>\d+\s+)?[F]?\s*(?(pline)|(?<price2>\d+\.\d\d)\s*(?:[TXR]\s)?)
            //log.WriteLine(magickImg.ToBase64());
        }
    }
}
