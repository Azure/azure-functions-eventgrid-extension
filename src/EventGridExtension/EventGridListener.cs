using Microsoft.Azure.WebJobs.Host.Executors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    public class EventGridListener : Microsoft.Azure.WebJobs.Host.Listeners.IListener
    {
        public ITriggeredFunctionExecutor Executor { private set; get; }

        private EventGridExtensionConfig _listenersStore;
        private readonly string _functionName;

        public EventGridListener(ITriggeredFunctionExecutor executor, EventGridExtensionConfig listenersStore, string functionName)
        {
            _listenersStore = listenersStore;
            _functionName = functionName;
            Executor = executor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _listenersStore.AddListener(_functionName, this);
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
