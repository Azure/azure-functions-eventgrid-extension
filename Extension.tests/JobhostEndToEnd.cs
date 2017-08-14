using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Newtonsoft.Json;
using System.Collections.Generic;
using Xunit;
using System.IO;

namespace Extension.tests
{
    public class JobhostEndToEnd
    {
        const string singleEvent = @"{
  'topic': '/subscriptions/5b4b650e-28b9-4790-b3ab-ddbd88d727c4/resourcegroups/canaryeh/providers/Microsoft.EventHub/namespaces/canaryeh',
  'subject': 'eventhubs/test',
  'eventType': 'captureFileCreated',
  'eventTime': '2017-07-14T23:10:27.7689666Z',
  'id': '7b11c4ce-1c34-4416-848b-1730e766f126',
  'data': {
    'fileUrl': 'https://shunsouthcentralus.blob.core.windows.net/debugging/shunBlob.txt',
    'fileType': 'AzureBlockBlob',
    'partitionId': '1',
    'sizeInBytes': 0,
    'eventCount': 0,
    'firstSequenceNumber': -1,
    'lastSequenceNumber': -1,
    'firstEnqueueTime': '0001-01-01T00:00:00',
    'lastEnqueueTime': '0001-01-01T00:00:00'
  },
  'publishTime': '2017-07-14T23:10:29.5004788Z'
}";
        const string shunBlob = @"GoodBye World
	---Confused Dev";
        static private string functionOut = null;
        private JobHost host;

        public JobhostEndToEnd()
        {
            JobHostConfiguration config = new JobHostConfiguration();
            config.AddExtension(new EventGridExtensionConfig());
            config.AddExtension(new TestExtensionConfig());
            host = new JobHost(config);
        }

        [Fact]
        public void ConsumeEventGridEventTest()
        {

            EventGridEvent eve = JsonConvert.DeserializeObject<EventGridEvent>(singleEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var method = typeof(JobhostEndToEnd).GetMethod("TestEventGrid");
            host.CallAsync(method, args).Wait();
            Assert.Equal(functionOut, eve.Subject);
            functionOut = null;

            method = typeof(JobhostEndToEnd).GetMethod("TestEventGridToString");
            host.CallAsync(method, args).Wait();
            Assert.Equal(functionOut, eve.Subject);
            functionOut = null;
        }

        [Fact]
        public void UseInputBlobBinding()
        {
            EventGridEvent eve = JsonConvert.DeserializeObject<EventGridEvent>(singleEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var method = typeof(JobhostEndToEnd).GetMethod("TestBlobStream");
            host.CallAsync(method, args).Wait();
            Assert.Equal(@"https://shunsouthcentralus.blob.core.windows.net/debugging/shunBlob.txt", functionOut);
            functionOut = null;
        }

        public void TestEventGrid([EventGridTrigger] EventGridEvent value)
        {
            functionOut = value.Subject;
        }

        public void TestEventGridToString([EventGridTrigger] string value)
        {
            functionOut = JsonConvert.DeserializeObject<EventGridEvent>(value).Subject;
        }

        public void TestBlobStream([EventGridTrigger] EventGridEvent value, [BindingData("{data.fileUrl}")]string autoResolve)
        {
            functionOut = autoResolve;
        }
    }
}
