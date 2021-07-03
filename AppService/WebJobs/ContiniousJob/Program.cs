using System;
using System.Threading;

namespace ContiniousJob
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            while (true)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error : {0}", ex.Message);
                }

                Thread.Sleep(TimeSpan.FromSeconds(10));
            }

        }
    }
}
