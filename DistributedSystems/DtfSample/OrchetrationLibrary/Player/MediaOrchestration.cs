using DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtfSample.Player
{
    public class MediaOrchestration :  TaskOrchestration<string, string>
    {       
        public override async Task<string> RunTask(OrchestrationContext context, string input)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"HelloOrchestration started: {context.OrchestrationInstance.InstanceId}");

            var result = await context.ScheduleTask<string>(typeof(PlayFileTask), input);

            return result;
        }
    }
}
