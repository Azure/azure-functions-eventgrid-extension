// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

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
