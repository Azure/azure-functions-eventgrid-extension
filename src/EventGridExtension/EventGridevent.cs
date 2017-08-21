using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
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

    public class EventGridEvent : EventGridEvent<JObject>
    {
    }

    public class EventGridEvent<TEventData>
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

        [JsonProperty(PropertyName = "data")]
        public TEventData Data { get; set; }

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
