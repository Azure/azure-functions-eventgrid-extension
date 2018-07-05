using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    /// <summary></summary>
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
        public string SasKey { get; set; }
    }
}
