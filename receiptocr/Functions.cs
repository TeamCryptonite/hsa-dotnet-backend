using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HsaDotnetBackend.Models;
using ImageMagick;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
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
            var imageBlobReference = "";
            var receiptId = 0;
            try
            {
                var jMessage = JObject.Parse(message);
                imageBlobReference = (string) jMessage["imageBlobReference"];
                receiptId = (int) jMessage["receiptId"];
            }
            catch (Exception ex)
            {
                log.WriteLine("Could not parse message. " + ex.Message);
                return;
            }

            if (string.IsNullOrWhiteSpace(imageBlobReference) || receiptId < 1)
            {
                log.WriteLine("Missing blobUri or resultReference.");
                return;
            }

            // Create Receipt to start building on
            var db = new Fortress_of_SolitudeEntities();
            var dbReceipt = db.Receipts.Find(receiptId);

            if (dbReceipt == null)
                throw new Exception("Could not find Receipt");

            //Get cloudStorage blob
            var storageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            var blobClient = storageAccount.CreateCloudBlobClient();

            var imageContainer =
                blobClient.GetContainerReference(ConfigurationManager.AppSettings["ReceiptContainer"]);
            var imageBlob = imageContainer.GetBlockBlobReference(imageBlobReference);


            // Download blob
            var stream = new MemoryStream();
            imageBlob.DownloadToStream(stream);

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
                magickImg.Thumbnail(new Percentage(95));


            // Start API to Google Vision
            var googleApiKey = ConfigurationManager.AppSettings["GoogleApiKey"];

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

            var response = client.Execute(request);


            

            // Convert GoogleAPI Response into a JObject
            var content = JObject.Parse(response.Content);
            var linesDictionary = new SortedDictionary<double, List<string>>();
            JToken blocks;
            try
            {
                blocks = content["responses"][0]["fullTextAnnotation"]["pages"][0]["blocks"];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception on blocks! " + ex.Message);

                dbReceipt.Provisional = true;
                dbReceipt.WaitingForOcr = false;
                db.SaveChanges();

                return;
            }

            if (blocks == null || !blocks.HasValues)
            {
                Console.WriteLine("Google Response Has No Return Blocks!");
                
                dbReceipt.Provisional = true;
                dbReceipt.WaitingForOcr = false;
                db.SaveChanges();

                return;
            }

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
                            linesDictionary.Add(wordMidline, new List<string> {wordStr});
                    }
                }
            }
            
            foreach (var line in linesDictionary)
            {
                var lineString = "";
                foreach (var str in line.Value)
                    lineString += str + " ";
                //Condense prices so they don't include spaces.
                lineString = Regex.Replace(lineString, @"\s(\d+)\s*\.\s*(\d\d)\s", " $1.$2 ");

                var stopReceiptWords = new List<string> {"total", "debit", "credit", "change"};
                if (stopReceiptWords.Any(word => lineString.ToLower().Contains(word)))
                    break;

                var pricePattern = new Regex(@"(\d+\.\d\d)");
                var priceMatch = pricePattern.Match(lineString);
                if (priceMatch.Success)
                {
                    var lineItem = new LineItem {Price = decimal.Parse(priceMatch.Groups[1].Value)};

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
                        var product = new Product() {Name = productMatch.Groups[1].Value};
                        lineItem.Product = product;
                    }
                    dbReceipt.LineItems.Add(lineItem);
                }
            }

            // Save receipt to the database
            dbReceipt.Provisional = true;
            dbReceipt.WaitingForOcr = false;
            db.SaveChanges();
        }
    }
}