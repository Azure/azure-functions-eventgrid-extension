using Microsoft.Azure.WebJobs;
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
            Console.WriteLine(value.ToString());
        }

        public void TestEventGridToString([EventGridTrigger] string value)
        {
            Console.WriteLine(value);
        }

        public void TestBlobStream([EventGridTrigger] EventGridEvent value, [Blob("{data.container}/{data.blob}", FileAccess.Read, Connection = "ShunTestConnectionString")]Stream myBlob)
        {
            var reader = new StreamReader(myBlob);
            Console.WriteLine(reader.ReadToEnd());
        }
    }
}
