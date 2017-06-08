using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs
{

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
        public string Data { get; set; }

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
