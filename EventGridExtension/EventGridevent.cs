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
{
    "fileUrl": "https://shunsouthcentralus.blob.core.windows.net/archivecontainershun/canaryeh/test/1/2017/07/14/23/09/27.avro",
    "fileType": "AzureBlockBlob",
    "partitionId": "1",
    "sizeInBytes": 0,
    "eventCount": 0,
    "firstSequenceNumber": -1,
    "lastSequenceNumber": -1,
    "firstEnqueueTime": "0001-01-01T00:00:00",
    "lastEnqueueTime": "0001-01-01T00:00:00"
}
         */
        [JsonProperty(PropertyName = "fileUrl")]
        public Uri fileUrl { get; set; }

        [JsonProperty(PropertyName = "fileType")]
        public string fileType { get; set; }

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
{
  "topic": "/subscriptions/5b4b650e-28b9-4790-b3ab-ddbd88d727c4/resourcegroups/canaryeh/providers/Microsoft.EventHub/namespaces/canaryeh",
  "subject": "eventhubs/test",
  "eventType": "captureFileCreated",
  "eventTime": "2017-07-14T23:10:27.7689666Z",
  "id": "7b11c4ce-1c34-4416-848b-1730e766f126",
  "data": {},
  "publishTime": "2017-07-14T23:10:29.5004788Z"
}
        */

        [JsonProperty(PropertyName = "topic")]
        public string Topic { get; set; }

        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }

        // the content of this depends on the publisher
        [JsonProperty(PropertyName = "data")]
        public JObject Data { get; set; }

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
