using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    public sealed class EventGridAsyncCollector : IAsyncCollector<EventGridEvent>
    {
        // use IEventGridClient for mocking test
        private readonly IEventGridClient _client;
        private readonly IList<EventGridEvent> _eventsToSend = new List<EventGridEvent>();
        private readonly string _topicHostname;

        private ManualResetEventSlim _canAdd = new ManualResetEventSlim(true);

        public EventGridAsyncCollector(IEventGridClient client, string topicHostname)
        {
            _client = client;
            _topicHostname = topicHostname;
        }

        public Task AddAsync(EventGridEvent item, CancellationToken cancellationToken = default(CancellationToken))
        {
            _canAdd.Wait(); // spinlock while flushing takes place; avoid modifying collection during the Publish() operation

            _eventsToSend.Add(item);

            return Task.CompletedTask;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                _canAdd.Reset();

                if (_eventsToSend.Any())
                {
                    await _client.PublishEventsAsync(_topicHostname, _eventsToSend, cancellationToken);
                    _eventsToSend.Clear();
                }
            }
            finally
            {
                _canAdd.Set();
            }
        }
    }
}