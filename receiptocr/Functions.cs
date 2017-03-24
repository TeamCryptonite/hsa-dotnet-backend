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

            // Set status in resultblob
            var resultJson = new JObject();
            resultJson.Add("Status", "Started");
            resultBlob.UploadText(resultJson.ToString());

            // Download blob
            MemoryStream stream = new MemoryStream();
            imageBlob.DownloadToStream(stream);

            resultJson["Status"] = "Image Processing";
            resultBlob.UploadText(resultJson.ToString());
            // ImageMagick img
            var magickImg = new MagickImage(stream);
            magickImg.Deskew(new Percentage(50));
            magickImg.Grayscale(PixelIntensityMethod.Rec709Luminance);
            magickImg.Enhance();
            magickImg.Despeckle();
            magickImg.Sharpen();
            magickImg.WhiteThreshold(new Percentage(50));
            magickImg.Trim();
            magickImg.AutoOrient();


            while (magickImg.ToByteArray().Length > 3888888)
            {
                magickImg.Thumbnail(new Percentage(95));
            }

            resultJson["Status"] = "OCR Running";
            resultBlob.UploadText(resultJson.ToString());




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
                resultJson["Status"] = "Failed";
                resultJson.Add("Message", "Could not receive OCR results.");
                resultBlob.UploadText(resultJson.ToString());
                throw new Exception("GoogleAPI Status did not return OK");
            }






            // Convert GoogleAPI Response into a JObject
            var content = JObject.Parse(response.Content);
            
            var buildJson = new JObject();
            buildJson.Add("Status", "Completed");

            var lineItems = new JArray();
            var linesDictionary = new SortedDictionary<double, List<string>>();
            var blocks = content["responses"][0]["fullTextAnnotation"]["pages"][0]["blocks"];
            foreach (var block in blocks)
            {
                // double in dictionary is the y value of a word's midline

                var paragraphs = block["paragraphs"];
                foreach (var paragraph in paragraphs)
                {
                    var words = paragraph["words"];
                    foreach (var word in words)
                    {
                        // Build each word
                        var wordStr = "";
                        var symbols = word["symbols"];
                        foreach (var symbol in symbols)
                            wordStr += symbol["text"] != null ? symbol["text"].Value<string>() : "";

                        // Find midline for each word
                        var yValues = new List<int>();
                        foreach (var vertex in word["boundingBox"]["vertices"])
                            if (vertex["y"] != null)
                                yValues.Add(vertex["y"].Value<int>());
                        var wordMidline = yValues.Average();

                        // Determine if there is a dictionary key within this word's midline
                        var addFlag = false;
                        foreach (var line in linesDictionary)
                            if (line.Key <= yValues.Max() && line.Key >= yValues.Min())
                            {
                                linesDictionary[line.Key].Add(wordStr);
                                addFlag = true;
                                break;
                            }
                        // Create new dictionary entry if no suitable one is found
                        if (!addFlag)
                            linesDictionary.Add(wordMidline, new List<string> { wordStr });
                    }
                }
            }

            foreach (var line in linesDictionary)
            {
                var lineString = "";
                foreach (var str in line.Value)
                {
                    lineString += str + " ";
                }
                //Condense prices so they don't include spaces.
                lineString = Regex.Replace(lineString, @"\s(\d+)\s*\.\s*(\d\d)\s", " $1.$2 ");

                var stopReceiptWords = new List<string> { "total", "debit", "credit", "change" };
                if (stopReceiptWords.Any(word => lineString.ToLower().Contains(word)))
                    break;

                var pricePattern = new Regex(@"(\d+\.\d\d)");
                var priceMatch = pricePattern.Match(lineString);
                if (priceMatch.Success)
                {
                    var lineItem = new JObject();
                    lineItem.Add("Price", priceMatch.Groups[1].Value);

                    // Remove the price from the string
                    lineString = lineString.Replace(priceMatch.Groups[1].Value, "").Trim();

                    // Remove any pattern of numbers greater than 5 (Like a UPC)
                    lineString = Regex.Replace(lineString, @"[\dO]{5,}", "");

                    // Remove anything after two spaces
                    lineString = Regex.Replace(lineString, @"\s\s.*", "");

                    var productPattern = new Regex(@"(.*)");
                    var productMatch = productPattern.Match(lineString);
                    if (productMatch.Success)
                    {
                        var product = new JObject();
                        product.Add("Name", productMatch.Groups[1].Value);
                        lineItem.Add("Product", product);
                    }
                    else
                    {
                        lineItem.Add("Product", null);
                    }
                    lineItems.Add(lineItem);
                }
            }

            // Add lineItems to buildJson
            buildJson.Add("LineItems", lineItems);

            // Upload results to resultBlob
            resultBlob.UploadText(buildJson.ToString());
        }
    }
}
