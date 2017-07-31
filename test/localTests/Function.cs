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

        public void TestInputStream([EventGridTrigger("eventhubcapture", Connection = "ShunTestConnectionString")] Stream myBlob, string blobTrigger)
        {
            Console.WriteLine($"file name {blobTrigger}");
            var reader = new StreamReader(myBlob);
            Console.WriteLine(reader.ReadToEnd());
        }

        public void TestByteArray([EventGridTrigger("eventhubcapture", Connection = "ShunTestConnectionString")] byte[] myBlob)
        {
            foreach (var b in myBlob)
            {
                Console.Write(b);
            }
        }

        public void TestString([EventGridTrigger("eventhubcapture", Connection = "ShunTestConnectionString")] string myBlob)
        {
            Console.WriteLine(myBlob);
        }
    }
}
