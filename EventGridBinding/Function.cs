using Microsoft.Azure.WebJobs;
using System;
using System.IO;

namespace EventGridBinding
{
    public static class Function
    {
        /*
        public static void Sample_BindToStream([Sample(@"sample\path")] Stream stream)
        {
            using (StreamWriter sw = new StreamWriter(stream))
            {
                sw.Write("Sample");
            }
        }

        public static void Sample_BindToString([Sample(@"sample\path")] out string data)
        {
            data = "Sample";
        }

        public static void SampleTrigger([SampleTrigger(@"sample\path")] SampleTriggerValue value)
        {
            Console.WriteLine("Sample trigger job called!");
        }*/

        /*
        public static void SampleTrigger_BindToString([EventGridTrigger] EventGridEvent value)
        {
            Console.WriteLine(value);
        }*/

        public static void testInputStream([EventGridTrigger] Stream myBlob, string name)
        {
            Console.WriteLine($"file name {name}");
            var reader = new StreamReader(myBlob);
            string line = null;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                Console.WriteLine(line);
            }
        }

        /*
        public static void displayUrl([EventGridTrigger] string url)
        {
            Console.WriteLine(url);
            Console.ReadLine();
        }
        */
    }
}
