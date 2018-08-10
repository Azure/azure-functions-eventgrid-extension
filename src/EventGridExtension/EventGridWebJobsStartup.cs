// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    class EventGridWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddEventGrid();
        }
    }
}
