using DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtfSample
{
    public class TimerOrchestration : TaskOrchestration<string, string>
    {
        public override async Task<string> RunTask(OrchestrationContext context, string state)
        {
            DateTime currentTime = context.CurrentUtcDateTime;
            DateTime fireAt;

            fireAt = context.CurrentUtcDateTime.AddMinutes(2);

            var attempt = await context.CreateTimer<string>(fireAt, state);

            Task<string> resultTask = context.ScheduleTask<string>(typeof(SchedulerTask), attempt);

            return "Started";
        }
    }
}
