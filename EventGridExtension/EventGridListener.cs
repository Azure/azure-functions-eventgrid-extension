using Microsoft.Azure.WebJobs.Host.Executors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs
{
    public class EventGridListener : Microsoft.Azure.WebJobs.Host.Listeners.IListener
    {
        // this only works for anonymous blob access
        private const string stringJson = @"[{
  'topic': '/subscriptions/5f750a97-50d9-4e36-8081-c9ee4c0210d4/resourcegroups/cesarrg/providers/Microsoft.EventHub/namespaces/cesardf',
  'subject': 'eventhubs/metrics',
  'eventType': 'captureFileCreated',
  'eventTime': '2017-06-10T00:32:57.7135938Z',
  'id': '8f099658-29c6-48d5-96f2-7c23143def14',
  'data': {
    'destinationUrl': 'https://eventhubgriddemo.blob.core.windows.net/ehcaptures/cesardf/metrics/7/2017/06/10/00/31/57.avro',
    'destinationType': 'EventHubArchive.AzureBlockBlob',
    'partitionId': '7',
    'sizeInBytes': 680524,
    'eventCount': 5300,
    'firstSequenceNumber': 3382300,
    'lastSequenceNumber': 3387599,
    'firstEnqueueTime': '2017-06-10T00:31:58.343Z',
    'lastEnqueueTime': '2017-06-10T00:32:56.791Z'
  },
  'publishTime': '2017-06-10T00:32:58.6036558Z'
}]";
        public ITriggeredFunctionExecutor Executor { private set; get; }

        private System.Timers.Timer _timer;
        private EventGridExtensionConfig _listenersStore;
        private readonly string _functionName;

        public EventGridListener(ITriggeredFunctionExecutor executor, EventGridExtensionConfig listenersStore, string functionName)
        {
            _listenersStore = listenersStore;
            _functionName = functionName;
            Executor = executor;

            // TODO: For this sample, we're using a timer to generate
            // trigger events. You'll replace this with your event source.
            _timer = new System.Timers.Timer(5 * 1000)
            {
                AutoReset = false
            };
            _timer.Elapsed += OnTimer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // TODO: Start monitoring your event source
            _timer.Start();
            _listenersStore.AddListener(_functionName, this);
            return Task.FromResult(true);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // TODO: Stop monitoring your event source
            _timer.Stop();
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


        private void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            // TODO: When you receive new events from your event source,
            // invoke the function executor

            // do nothing
            /*
            List<EventGridEvent> events = JsonConvert.DeserializeObject<List<EventGridEvent>>(stringJson);
            foreach (var param in events)
            {
                TriggeredFunctionData input = new TriggeredFunctionData
                {
                    TriggerValue = param
                };

                Executor.TryExecuteAsync(input, CancellationToken.None).Wait();
            }*/
        }
    }
}
