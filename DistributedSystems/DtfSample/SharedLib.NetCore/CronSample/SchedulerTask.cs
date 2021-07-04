using DurableTask;
using DurableTask.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtfSample
{
    public class SchedulerTask : TaskActivity<string, string>
    {
        protected override string Execute(TaskContext context, string input)
        {
            string ret = "Executing Cron Job.  Started At: '" + DateTime.Now.ToString() + "' Number: " + input;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(ret);
            return ret;
        }
    }
}
