using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp2
{
    /// <summary>
    ///   Runs every minute
    ///    "schedule": "0 * * * * *"
    ///    Runs every 15 minutes
    ///    "schedule": "0 */15 * * * *"
    ///
    ///    Runs every hour (i.e. whenever the count of minutes is 0)
    ///    "schedule": "0 0 * * * *"
    ///
    ///    Runs every hour from 9 AM to 5 PM
    ///    "schedule": "0 0 9-17 * * *"
    ///
    ///    Runs at 9:30 AM every day
    ///    "schedule": "0 30 9 * * *"
    ///
    ///    Runs at 9:30 AM every week day
    ///    "schedule": "0 30 9 * * 1-5"
    /// </summary>
    public class TimerFunction
    {
        private readonly ILogger _logger;

        public TimerFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerFunction>();
        }

        [Function("TimerFunction")]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
