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
        public const string eventHubArchive = "eventHubArchive";
        public EventGridTriggerAttribute()
        {
            Publisher = null; // if this is not provided, only EventGridEvent can be parsed
        }

        public EventGridTriggerAttribute(string publisher)
        {
            // FIXME
            if (String.Equals(publisher, eventHubArchive, StringComparison.OrdinalIgnoreCase))
            {
                Publisher = eventHubArchive;
            }
            else
            {
                throw new InvalidOperationException($"unsupported publisher {publisher}");
            }
        }

        public string Publisher { get; private set; }
    }
}
