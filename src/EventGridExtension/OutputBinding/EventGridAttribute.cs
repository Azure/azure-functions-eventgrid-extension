// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    /// <summary>Attribute to specify parameters for the Event Grid output binding</summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class EventGridAttribute : Attribute
    {
        /// <summary>Gets or sets the topic events endpoint URI. Eg: https://topic1.westus2-1.eventgrid.azure.net/api/events </summary>
        [AppSetting]
        public string TopicEndpointUri { get; set; }

        /// <summary>Gets or sets the sas key setting.</summary>
        [AppSetting]
        public string SasKeySetting { get; set; }
    }
}
