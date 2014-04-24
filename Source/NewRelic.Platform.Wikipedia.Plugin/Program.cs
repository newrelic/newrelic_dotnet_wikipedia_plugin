using System;
using NewRelic.Platform.Sdk;

namespace NewRelic.Platform.Wikipedia.Plugin
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Runner runner = new Runner();

                runner.Add(new WikipediaAgentFactory());

                runner.SetupAndRun();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred, unable to continue.\n", e.Message);
                return -1;
            }

            return 0;
        }
    }
}
