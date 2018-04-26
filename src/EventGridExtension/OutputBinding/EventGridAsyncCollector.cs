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
    internal sealed class EventGridAsyncCollector : IAsyncCollector<EventGridEvent>
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
            var newEventList = new List<EventGridEvent>();
            lock (_syncroot)
            {
                // swap the events to send out with a new list; locking so 'AddAsync' doesn't take place while we do this
                events = _eventsToSend;
                _eventsToSend = newEventList;
            }

            if (events.Any())
            {
                await _client.PublishEventsAsync(_topicHostname, events, cancellationToken);
            }
        }
    }
}