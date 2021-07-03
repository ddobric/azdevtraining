using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebJobSample
{
    /// <summary>
    /// Demonstrates how to run the code that continiously is running.
    /// </summary>
    public static class ContiniousJob
    {
        [NoAutomaticTriggerAttribute]
        public static async Task MyJob(TextWriter log)
        {
            while (true)
            {
                try
                {
                    log.WriteLine($"{DateTime.Now}");
                }
                catch (Exception ex)
                {
                    log.WriteLine("Error : {0}", ex.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }
}
