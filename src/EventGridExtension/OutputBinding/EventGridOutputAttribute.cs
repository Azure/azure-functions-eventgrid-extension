using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    /// <summary></summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(validOn: AttributeTargets.Parameter)]
    [Binding]
    public sealed class EventGridOutputAttribute : Attribute
    {
#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>Gets or sets the topic hostname setting. Eg: topic1.westus2-1.eventgrid.azure.net</summary>
        [AppSetting]
#pragma warning restore CS0618 // Type or member is obsolete
        public string TopicHostname { get; set; }

#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>Gets or sets the sas key setting.</summary>
        [AppSetting]
#pragma warning restore CS0618 // Type or member is obsolete
        public string SasKey { get; set; }
    }
}
