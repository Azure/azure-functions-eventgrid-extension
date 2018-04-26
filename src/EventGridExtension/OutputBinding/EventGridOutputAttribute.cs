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
        /// <summary>
        /// Gets or sets the topic endpoint setting.
        /// </summary>
        /// <value>
        /// The topic endpoint setting.
        /// </value>
        [AppSetting]
#pragma warning restore CS0618 // Type or member is obsolete
        public string TopicEndpoint { get; set; }

#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>
        /// Gets or sets the sas key setting.
        /// </summary>
        /// <value>
        /// The sas key setting.
        /// </value>
        [AppSetting]
#pragma warning restore CS0618 // Type or member is obsolete
        public string SasKey { get; set; }
    }
}
