// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public sealed class EventGridTriggerAttribute : Attribute
    {
        public EventGridTriggerAttribute()
        {
            Publisher = new DefaultPublisher(); // if this is not provided, only EventGridEvent can be parsed
        }

        // publisher provider
        public EventGridTriggerAttribute(string publisher)
        {
            // FIXME
            if (String.Equals(publisher, EventHubArchivePublisher.Name, StringComparison.OrdinalIgnoreCase))
            {
                Publisher = new EventHubArchivePublisher();
            }
            else
            {
                throw new InvalidOperationException($"unsupported publisher {publisher}");
            }
        }

        public IPublisher Publisher { get; }
    }
}
