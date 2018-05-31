﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    public sealed class EventGridOutputAsyncCollector : IAsyncCollector<EventGridEvent>
    {
        private readonly EventGridClient _client;
        private readonly IList<EventGridEvent> _eventsToSend = new List<EventGridEvent>();
        private readonly EventGridAttribute _attribute;

        public EventGridOutputAsyncCollector(EventGridAttribute attr)
        {
            _attribute = attr;

            if (_client == null)
            {
                _client = new EventGridClient(new TopicCredentials(_attribute.SasKey));
            }
        }

        public Task AddAsync(EventGridEvent item, CancellationToken cancellationToken = default(CancellationToken))
        {
            _eventsToSend.Add(item);

            return Task.CompletedTask;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_eventsToSend.Any())
            {
                await _client.PublishEventsAsync(_attribute.TopicHostname, _eventsToSend, cancellationToken);
                _eventsToSend.Clear();
            }
        }
    }
}