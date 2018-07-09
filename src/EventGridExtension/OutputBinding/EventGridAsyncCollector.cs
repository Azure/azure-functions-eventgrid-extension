// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
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
        private readonly string _topicHostname;
        private readonly object _syncroot = new object();

        private IList<EventGridEvent> _eventsToSend = new List<EventGridEvent>();

        public EventGridAsyncCollector(IEventGridClient client, string topicEndpointUri)
        {
            _client = client;
            _topicHostname = new Uri(topicEndpointUri).Host;
        }

        public Task AddAsync(EventGridEvent item, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_syncroot)
            {
                // Don't let FlushAsyc take place while we're doing this
                _eventsToSend.Add(item);
            }

            return Task.CompletedTask;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IList<EventGridEvent> events;
            lock (_syncroot)
            {
                // pull out events to send, reset the list. Don't let AddAsync take place while we do this
                events = _eventsToSend;
                _eventsToSend = new List<EventGridEvent>();
            }

            if (events.Any())
            {
                await _client.PublishEventsAsync(_topicHostname, events, cancellationToken);
            }
        }
    }
}