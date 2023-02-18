using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading;
using Azure.Storage.Blobs;
using System.ComponentModel;
using System.IO.Pipes;
using System.Reflection.Metadata;

namespace OrderItemsReserver
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var name = await UploadJsonFileToBlobAsync(requestBody, log);

            string responseMessage = string.IsNullOrEmpty(requestBody)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"was uploaded, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        private static async Task<string> UploadJsonFileToBlobAsync(string jsonString, ILogger log)
        {
            var storageConfig = new
            {
                ConnectionString =
                    @"",
                FileContainerName = "reservation"
            };


            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConfig.ConnectionString);
            BlobContainerClient containerClient =
                blobServiceClient.GetBlobContainerClient(storageConfig.FileContainerName);

            BlobClient blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString());
            try
            {
                await blobClient.UploadAsync(BinaryData.FromString(jsonString));

                return blobClient.Name;
            }
            catch (Exception ex)
            {
                log.LogError($"File has not been uploaded to blob:{blobClient.Uri}", ex);
                throw;
            }
        }
    }
}
