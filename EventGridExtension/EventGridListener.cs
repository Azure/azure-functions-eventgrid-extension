using Microsoft.Azure.WebJobs.Host.Executors;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs
{
    public class EventGridListener : Microsoft.Azure.WebJobs.Host.Listeners.IListener
    {
        private const string stringJson = @"{
  'topic': 'from timer:/subscriptions/feda0306-5ce4-4423-91f6-26b4f2f55a45/resourcegroups/shunTest/providers/Microsoft.EventGridMockPublisher/dbAccounts/shunDbAccount',
  'subject': 'tables/table1',
  'data': '{\'Size\':454566,\'Timestamp\':\'2017-06-05T23:18:33.1816208Z\',\'ETag\':\'686897696a7c876b7e\'}',
  'eventType': 'tableCreated',
  'eventTime': '2017-06-05T23:18:33.1806217Z',
  'publishTime': '2017-06-05T23:18:33.2510851Z',
  'id': '0e917dd7-a70c-44a1-b3a3-5213034f76b9'}";
        public ITriggeredFunctionExecutor Executor { private set; get; }

        private System.Timers.Timer _timer;
        private EventGridExtensionConfig _listenersStore;

        public EventGridListener(ITriggeredFunctionExecutor executor, EventGridExtensionConfig listenersStore)
        {
            _listenersStore = listenersStore;

            Executor = executor;

            // TODO: For this sample, we're using a timer to generate
            // trigger events. You'll replace this with your event source.
            /*
            _timer = new System.Timers.Timer(30 * 1000)
            {
                AutoReset = true
            };
            _timer.Elapsed += OnTimer;
            */
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // TODO: Start monitoring your event source
            // _timer.Start();
            _listenersStore.AddListener(this);
            return Task.FromResult(true);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // TODO: Stop monitoring your event source
            // _timer.Stop();
            // TODO unsubscribe
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            // TODO: Perform any final cleanup
            _timer.Dispose();
        }

        public void Cancel()
        {
            // TODO: cancel any outstanding tasks initiated by this listener
        }

        /*
        private void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            // TODO: When you receive new events from your event source,
            // invoke the function executor

            EventGridEvent param = JsonConvert.DeserializeObject<EventGridEvent>(stringJson);
            TriggeredFunctionData input = new TriggeredFunctionData
            {
                TriggerValue = param
            };

            Executor.TryExecuteAsync(input, CancellationToken.None).Wait();
        }*/
    }
}
