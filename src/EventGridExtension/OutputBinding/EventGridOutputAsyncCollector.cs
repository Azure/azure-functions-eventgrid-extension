using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    public sealed class EventGridOutputAsyncCollector : IAsyncCollector<EventGridEvent>
    {
        private static EventGridClient _client;

        private EventGridAttribute _attribute;

        public EventGridOutputAsyncCollector(EventGridAttribute attr)
        {
            _attribute = attr;

            if (_client == null)
            {
                _client = new EventGridClient(new TopicCredentials(_attribute.SasKey));
            }
        }

        public Task AddAsync(EventGridEvent item, CancellationToken cancellationToken = default(CancellationToken)) => _client.PublishEventsAsync(_attribute.GetTopicHostname(), new[] { item }, cancellationToken);

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // do nothing; we are sending every request as they come in to AddAsync so there's nothing to flush
            return Task.CompletedTask;
        }
    }
}