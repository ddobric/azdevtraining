using DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtfSample
{
    public class UserInputTask : TaskActivity<string, string>
    {
        protected override string Execute(TaskContext context, string input)
        {           
            Console.Write($"Please enter status of your task '{input}'");
            Console.ForegroundColor = ConsoleColor.Cyan;
            return Console.ReadLine();
        }
    }
}
