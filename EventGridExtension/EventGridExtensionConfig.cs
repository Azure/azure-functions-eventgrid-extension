using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Host.Executors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

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

        private Dictionary<string, EventGridListener> _listeners = new Dictionary<string, EventGridListener>();

        internal void AddListener(string key, EventGridListener listener)
        {
            _listeners.Add(key, listener);
        }

        async Task<HttpResponseMessage> IAsyncConverter<HttpRequestMessage, HttpResponseMessage>.ConvertAsync(HttpRequestMessage input, CancellationToken cancellationToken)
        {
            var response = ProcessAsync(input);
            return await response;
        }

        private async Task<HttpResponseMessage> ProcessAsync(HttpRequestMessage req)
        {
            string jsonArray = await req.Content.ReadAsStringAsync();
            List<EventGridEvent> events = JsonConvert.DeserializeObject<List<EventGridEvent>>(jsonArray);
            var functionName = HttpUtility.ParseQueryString(req.RequestUri.Query)["functionName"];

            if (_listeners.ContainsKey(functionName))
            {
                var listener = _listeners[functionName];

                foreach (var ev in events)
                {
                    TriggeredFunctionData triggerData = new TriggeredFunctionData
                    {
                        TriggerValue = ev
                    };

                    await listener.Executor.TryExecuteAsync(triggerData, CancellationToken.None);
                }

                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
