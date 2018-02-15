using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

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

}
