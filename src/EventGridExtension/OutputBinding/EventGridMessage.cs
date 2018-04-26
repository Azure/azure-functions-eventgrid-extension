using System;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    /// <summary></summary>
    public sealed class EventGridMessage
    {
        private DateTime _eventTime = DateTime.UtcNow;

        public EventGridMessage(string subject, string eventType, string id = null, DateTime? eventTime = null)
        {
            Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;
            Subject = !string.IsNullOrWhiteSpace(subject) ? subject : throw new ArgumentNullException(nameof(subject));
            EventType = !string.IsNullOrWhiteSpace(eventType) ? eventType : throw new ArgumentNullException(nameof(eventType));
            EventTime = eventTime ?? DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [JsonProperty(@"data")]
        public object Data { get; set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        [JsonProperty(@"id")]
        public string Id { get; }

        /// <summary>
        /// Gets the subject.
        /// </summary>
        [JsonProperty(@"subject")]
        public string Subject { get; }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        [JsonProperty(@"eventType")]
        public string EventType { get; }

        [JsonProperty(@"eventTime")]
        /// <summary>
        /// Gets or sets the event time (UTC).
        /// </summary>
        public DateTime EventTime
        {
            get => _eventTime;
            set
            {
                System.Diagnostics.Debug.Assert(value.Kind == DateTimeKind.Utc, "It is strongly recommended to use UTC when timestamping events to Event Grid");

                _eventTime = value;
            }
        }
    }
}
