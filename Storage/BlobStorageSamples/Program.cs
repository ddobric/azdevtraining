using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlobStorageSamples
{
    class Program
    {
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=damiraztraining;AccountKey=EyazJ87KOZG/E8oWTtQxm6DkF4yc0WNF7FvlF4TFHwt9Cl0Z0GQ8UJjiQXtL5zEj/DTLu4KlnUlb1asomxpg3A==;EndpointSuffix=core.windows.net";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Blob Storage!");

            UploadAsync().Wait();

            DownloadAsync().Wait();
        }

        /// <summary>
        /// Upload a file to a blob.
        /// </summary>
        public static async Task UploadAsync()
        {

            // Get a connection string to our Azure Storage account.  You can
            // obtain your connection string from the Azure Portal (click
            // Access Keys under Settings in the Portal Storage account blade)
            // or using the Azure CLI with:
            //
            //     az storage account show-connection-string --name <account_name> --resource-group <resource_group>
            //
            // And you can provide the connection string to your application
            // using an environment variable.
            string connectionString = ConnectionString;

            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = new BlobContainerClient(connectionString, "sample-container");
            
            await container.CreateIfNotExistsAsync();
            
            try
            {
                // Get a reference to a blob
                BlobClient blob = container.GetBlobClient("Azure Storage.pptx");

                // Upload file data
                await blob.UploadAsync("Azure Storage.pptx");


                // Verify we uploaded some content
                BlobProperties properties = await blob.GetPropertiesAsync();
            }
            finally
            {
                // Clean up after the test when we're finished
                await container.DeleteAsync();

            }
        }


        /// <summary>
        /// Download a blob to a file.
        /// </summary>

        public static async Task DownloadAsync()
        {

            // Get a connection string to our Azure Storage account.
            string connectionString = ConnectionString;

            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = new BlobContainerClient(connectionString, "sample-container");
            await container.CreateIfNotExistsAsync();
            try
            {
                // Get a reference to a blob named "sample-file"
                BlobClient blob = container.GetBlobClient("Azure Storage.pptx");


                // Download the blob's contents and save it to a file
                BlobDownloadInfo download = await blob.DownloadAsync();
                using (FileStream file = File.OpenWrite("Azure Storage Downloaded.pptx"))
                {
                    download.Content.CopyTo(file);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                // Clean up after the test when we're finished
                container.Delete();
            }

        }
    }
}
