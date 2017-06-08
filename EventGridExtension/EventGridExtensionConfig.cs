using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Host.Executors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs
{
    public class EventGridExtensionConfig : IExtensionConfigProvider,
                       IAsyncConverter<HttpRequestMessage, HttpResponseMessage>
    {
        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            Uri url = context.GetWebhookHandler();

            // Register our extension binding providers
            context.Config.RegisterBindingExtensions(new EventGridTriggerAttributeBindingProvider(this));
        }

        private List<EventGridListener> _listeners = new List<EventGridListener>();

        internal void AddListener(EventGridListener listener)
        {
            _listeners.Add(listener);
        }

        async Task<HttpResponseMessage> IAsyncConverter<HttpRequestMessage, HttpResponseMessage>.ConvertAsync(HttpRequestMessage input, CancellationToken cancellationToken)
        {
            var response = ProcessAsync(input);
            return await response;
        }

        private async Task<HttpResponseMessage> ProcessAsync(HttpRequestMessage input)
        {
            string jsonArray = await input.Content.ReadAsStringAsync();
            List<EventGridEvent> events = JsonConvert.DeserializeObject<List<EventGridEvent>>(jsonArray);

            foreach (var ev in events)
            {
                TriggeredFunctionData pass = new TriggeredFunctionData
                {
                    TriggerValue = ev
                };

                foreach (var listener in _listeners)
                {
                    await listener.Executor.TryExecuteAsync(pass, CancellationToken.None);
                }
                // TODO need a map between http requests and listener
            }

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
