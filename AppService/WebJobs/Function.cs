using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace WebJobsSDKSample
{
    public class Functions
    {
        /// <summary>
        /// Multiple triggers.
        /// To test this:
        /// 1. Upload some file to 'democontainer'. I.e.: desktop.ini
        /// 2. Craete a new message in the queue 'inqueue' with content "desktop.ini"
        /// This will trigger function when message arrives. Blob binding will be activated to read the 
        /// blob with the name contained in the message content.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="myBlob"></param>
        /// <param name="logger"></param>
        public static void ProcessQueueMessage(
            [QueueTrigger("jobqueue")] string message,
            [Blob("democontainer/{queueTrigger}", FileAccess.Read, Connection = "StorageConnStr")] Stream myBlob, ILogger logger)
        {
            logger.LogInformation($"Blob name:{message} \n Size: {myBlob?.Length} bytes");

            using (StreamReader sr = new StreamReader(myBlob))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }      
    }
}