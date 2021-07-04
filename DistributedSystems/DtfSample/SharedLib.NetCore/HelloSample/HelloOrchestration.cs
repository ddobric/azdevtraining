using DtfSample.Player;
using DurableTask;
using DurableTask.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtfSample
{
    public class HelloOrchestration : TaskOrchestration<string, string>
    {
     
        public override async Task<string> RunTask(OrchestrationContext context, string input)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"HelloOrchestration started: {context.OrchestrationInstance.InstanceId}");

            while (true)
            {
                var result = await context.ScheduleTask<string>(typeof(UserInputTask), input);
                if (result == "done")
                    break;
            }
            
            var playerResult = await context.ScheduleTask<string>(typeof(PlayFileTask), "applause.wav");

            Console.WriteLine($"HelloOrchestration ended: {context.OrchestrationInstance.InstanceId}");

         
            return ":)";
        }
    }
}
