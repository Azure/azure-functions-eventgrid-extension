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

                // When sending EventGrid messages with the client, we only need the Hostname of the
                // endpoint because the item itself contains the topic. So, extract the hostname from
                // the full topic endpoint Uri and store that for later usage by the output collector
                // (via a call to GetTopicHostname)
                if (Uri.TryCreate(value, UriKind.Absolute, out Uri endpointUri))
                {
                    this.TopicHostname = endpointUri.Host;
                }
                else
                {
                    System.Diagnostics.Debug.Write($@"WARNING: Invalid topic endpoint URI ({value}). Expected an absolute URI value");
                    this.TopicHostname = null; // if this was set previously, need to clear it out now
                }
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>Gets or sets the sas key setting.</summary>
        [AppSetting]
#pragma warning restore CS0618 // Type or member is obsolete
        public string SasKey { get; set; }

        // Internal because we don't want this showing up in the Attribute's intellisense when a user adds
        // it to the parameter signature in the Function definition
        internal string TopicHostname { get; private set; }
    }
}
