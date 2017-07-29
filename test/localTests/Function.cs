using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using System;
using System.IO;

namespace EventGridBinding
{

    public class Function
    {
        // TODO test fields
        public void TestEventGrid([EventGridTrigger] EventGridEvent value)
        {
            Console.WriteLine(value);
        }

        public void TestInputStream([EventGridTrigger("eventhubcapture")] Stream myBlob, string blobTrigger)
        {
            Console.WriteLine($"file name {blobTrigger}");
            var reader = new StreamReader(myBlob);
            string line = null;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                Console.WriteLine(line);
            }
        }

    }
}
