using Microsoft.Azure.WebJobs;
using System;
using System.IO;

namespace EventGridBinding
{
    public static class Function
    {
        public static void testEventGrid([EventGridTrigger("eventhubarchive")] EventGridEvent value)
        {
            Console.WriteLine(value);
        }

        public static void testInputStream([EventGridTrigger("eventhubarchive")] Stream myBlob, string name)
        {
            Console.WriteLine($"file name {name}");
            var reader = new StreamReader(myBlob);
            string line = null;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                Console.WriteLine(line);
            }
        }

    }
}
