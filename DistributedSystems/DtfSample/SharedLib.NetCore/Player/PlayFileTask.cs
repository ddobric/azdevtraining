using DurableTask;
using DurableTask.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DtfSample.Player
{
    public class PlayFileTask : TaskActivity<string, string>
    {
        protected override string Execute(TaskContext context, string fileName)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine($"Beeping");

            for (int i = 0; i < 100; i++)
            {
                Console.Beep(100 + i * 10, 250);
            }

            return "Played";
        }
    }
}
