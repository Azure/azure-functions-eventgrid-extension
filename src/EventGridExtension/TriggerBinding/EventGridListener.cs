// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    internal class EventGridListener : Host.Listeners.IListener
    {
        public ITriggeredFunctionExecutor Executor { private set; get; }

        private EventGridExtensionConfigProvider _extensionConfigProvider;
        private readonly string _functionName;

        public EventGridListener(ITriggeredFunctionExecutor executor, EventGridExtensionConfigProvider listenersStore, string functionName)
        {
            _extensionConfigProvider = listenersStore;
            _functionName = functionName;
            Executor = executor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _extensionConfigProvider.AddListener(_functionName, this);
            return Task.FromResult(true);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // calling order stop -> cancel -> dispose
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            // TODO unsubscribe
        }

        public void Cancel()
        {
            // TODO cancel any outstanding tasks initiated by this listener
        }
    }
}
