using DurableTask;
using DurableTask.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtfSample
{
    public class TimerOrchestration : TaskOrchestration<string, string>
    {
        public override async Task<string> RunTask(OrchestrationContext context, string input)
        {
            if(context.IsReplaying == false)
                Console.WriteLine("Cron Orchestration started..");
            else
                Console.WriteLine("Replaying");

            DateTime currentTime = context.CurrentUtcDateTime;
            DateTime fireAt;

            fireAt = context.CurrentUtcDateTime.AddMinutes(1);

            var attempt = await context.CreateTimer<string>(fireAt, input);

            Task<string> resultTask = context.ScheduleTask<string>(typeof(SchedulerTask), attempt);

            return "Started";
        }
    }
}
