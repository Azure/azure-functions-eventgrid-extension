using Microsoft.Azure.WebJobs.Host.Executors;
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

            // Register the listener as part of create time initialization
            _listenersStore.AddListener(_functionName, this);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {
        }

        public void Cancel()
        {
        }
    }
}
