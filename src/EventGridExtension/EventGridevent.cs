using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs
{
    public class SubscriptionValidationResponse
    {
        [JsonProperty(PropertyName = "validationResponse")]
        public string ValidationResponse { get; set; }
    }

    public class SubscriptionValidationEvent
    {
        [JsonProperty(PropertyName = "validationCode")]
        public string ValidationCode { get; set; }
    }

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
        public Uri FileUrl { get; set; }

        [JsonProperty(PropertyName = "fileType")]
        public string FileType { get; set; }

        [JsonProperty(PropertyName = "partitionId")]
        public int PartitionId { get; set; }

        [JsonProperty(PropertyName = "sizeInBytes")]
        public int SizeInBytes { get; set; }

        [JsonProperty(PropertyName = "eventCount")]
        public int EventCount { get; set; }

        [JsonProperty(PropertyName = "firstSequenceNumber")]
        public int FirstSequenceNumber { get; set; }

        [JsonProperty(PropertyName = "lastSequenceNumber")]
        public int LastSequenceNumber { get; set; }

        [JsonProperty(PropertyName = "firstEnqueueTime")]
        public DateTime FirstEnqueueTime { get; set; }

        [JsonProperty(PropertyName = "lastEnqueueTime")]
        public DateTime LastEnqueueTime { get; set; }

    }

    public class EventGridFaultyEvent
    {
        /*
{
  'id': 'eac180e8-92e0-436d-8699-a0324e2a5fef',
  'topic': '/subscriptions/5b4b650e-28b9-4790-b3ab-ddbd88d727c4/resourceGroups/canaryeh/providers/microsoft.eventhub/namespaces/canaryeh',
  'subject': 'eventhubs/test',
  'data': '{\""validationCode\"":\""85fe9560-f63f-469b-b40a-5a6327db05e6\""}',  <-- String instead of JObject
  'eventType': 'Microsoft.EventGrid/SubscriptionValidationEvent',
  'eventTime': '2017-07-28T00:43:28.6153503Z'
}
        */

        [JsonProperty(PropertyName = "topic")]
        public string Topic { get; set; }

        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonProperty(PropertyName = "eventType")]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "publishTime")]
        public DateTime PublishTime { get; set; }

        [JsonProperty(PropertyName = "eventTime")]
        public DateTime EventTime { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
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
