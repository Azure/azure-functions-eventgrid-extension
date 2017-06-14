using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs
{
    public class StorageBlob
    {
        /*
         *  'data': {
    'destinationUrl': 'https://eventhubgriddemo.blob.core.windows.net/ehcaptures/cesardf/metrics/7/2017/06/10/00/31/57.avro',
    'destinationType': 'EventHubArchive.AzureBlockBlob',
    'partitionId': '7',
    'sizeInBytes': 680524,
    'eventCount': 5300,
    'firstSequenceNumber': 3382300,
    'lastSequenceNumber': 3387599,
    'firstEnqueueTime': '2017-06-10T00:31:58.343Z',
    'lastEnqueueTime': '2017-06-10T00:32:56.791Z'
  }
         */
        [JsonProperty(PropertyName = "destinationUrl")]
        public Uri destionationUrl { get; set; }

        [JsonProperty(PropertyName = "destinationType")]
        public string destionationType { get; set; }

        [JsonProperty(PropertyName = "partitionId")]
        public int partitionId { get; set; }

        [JsonProperty(PropertyName = "sizeInBytes")]
        public int sizeInBytes { get; set; }

        [JsonProperty(PropertyName = "eventCount")]
        public int eventCount { get; set; }

        [JsonProperty(PropertyName = "firstSequenceNumber")]
        public int firstSequenceNumber { get; set; }

        [JsonProperty(PropertyName = "lastSequenceNumber")]
        public int lastSequenceNumber { get; set; }

        [JsonProperty(PropertyName = "firstEnqueueTime")]
        public DateTime firstEnqueueTime { get; set; }

        [JsonProperty(PropertyName = "lastEnqueueTime")]
        public DateTime lastEnqueueTime { get; set; }

    }

    public class EventGridEvent
    {
        /*
        "topic": "/subscriptions/feda0306-5ce4-4423-91f6-26b4f2f55a45/resourcegroups/shunTest/providers/Microsoft.EventGridMockPublisher/dbAccounts/shunDbAccount",
        "subject": "tables/table1",
        "data": "{\"Size\":454566,\"Timestamp\":\"2017-06-05T23:18:33.1816208Z\",\"ETag\":\"686897696a7c876b7e\"}",
        "eventType": "tableCreated",
        "eventTime": "2017-06-05T23:18:33.1806217Z",
        "publishTime": "2017-06-05T23:18:33.2510851Z",
        "id": "0e917dd7-a70c-44a1-b3a3-5213034f76b9"
        */

        [JsonProperty(PropertyName = "topic")]
        public string Topic { get; set; }

        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }

        [JsonProperty(PropertyName = "data")]
        public StorageBlob Data { get; set; }

        [JsonProperty(PropertyName = "eventType")]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "publishTime")]
        public DateTime PublishTime { get; set; }

        [JsonProperty(PropertyName = "eventTime")]
        public DateTime EventTime { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public override string ToString()
        {
            return $@"topic : {Topic}
subject : {Subject}
data : {Data}
eventType : {EventType}
publishTime : {PublishTime}
eventTime : {EventTime}
id : {Id}";
        }
    }
}
