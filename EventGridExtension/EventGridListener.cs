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
  'topic': '/subscriptions/5b4b650e-28b9-4790-b3ab-ddbd88d727c4/resourcegroups/canaryeh/providers/Microsoft.EventHub/namespaces/canaryeh',
  'subject': 'eventhubs/test',
  'eventType': 'captureFileCreated',
  'eventTime': '2017-07-14T23:10:27.7689666Z',
  'id': '7b11c4ce-1c34-4416-848b-1730e766f126',
  'data': {
    'fileUrl': 'https://shunsouthcentralus.blob.core.windows.net/archivecontainershun/canaryeh/test/1/2017/07/14/23/09/27.avro',
    'fileType': 'AzureBlockBlob',
    'partitionId': '1',
    'sizeInBytes': 0,
    'eventCount': 0,
    'firstSequenceNumber': -1,
    'lastSequenceNumber': -1,
    'firstEnqueueTime': '0001-01-01T00:00:00',
    'lastEnqueueTime': '0001-01-01T00:00:00'
  },
  'publishTime': '2017-07-14T23:10:29.5004788Z'
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
            /*
            _timer = new System.Timers.Timer(5 * 1000)
            {
                AutoReset = false
            };
            _timer.Elapsed += OnTimer;
            */
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // TODO: Start monitoring your event source
            if (_timer != null)
            {
                _timer.Start();
            }
            _listenersStore.AddListener(_functionName, this);
            return Task.FromResult(true);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // TODO: Stop monitoring your event source
            if (_timer != null)
            {
                _timer.Stop();
            }
            // TODO unsubscribe
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            // TODO: Perform any final cleanup
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }

        public void Cancel()
        {
            // TODO: cancel any outstanding tasks initiated by this listener
        }


        private void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            // TODO: When you receive new events from your event source,
            // invoke the function executor

            List<EventGridEvent> events = JsonConvert.DeserializeObject<List<EventGridEvent>>(stringJson);
            foreach (var param in events)
            {
                TriggeredFunctionData input = new TriggeredFunctionData
                {
                    TriggerValue = param
                };

                Executor.TryExecuteAsync(input, CancellationToken.None).Wait();
            }
        }
    }
}
