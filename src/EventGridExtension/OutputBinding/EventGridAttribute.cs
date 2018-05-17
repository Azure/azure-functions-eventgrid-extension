using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    /// <summary></summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(validOn: AttributeTargets.Parameter)]
    [Binding]
    public sealed class EventGridAttribute : Attribute
    {
        private string _topicEndpointUri;
        private Uri _uriValueOfTopicEndpoint;
#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>Gets or sets the topic events endpoint URI. Eg: https://topic1.westus2-1.eventgrid.azure.net/api/events </summary>
        [AppSetting]
#pragma warning restore CS0618 // Type or member is obsolete
        public string TopicEndpointUri
        {
            get => _topicEndpointUri;
            set
            {
                _topicEndpointUri = value;
                _uriValueOfTopicEndpoint = !string.IsNullOrWhiteSpace(value) ? new Uri(value) : null;
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>Gets or sets the sas key setting.</summary>
        [AppSetting]
#pragma warning restore CS0618 // Type or member is obsolete
        public string SasKey { get; set; }


        internal string GetTopicHostname() => _uriValueOfTopicEndpoint?.Host;
    }
}
