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
        }
    }
}
