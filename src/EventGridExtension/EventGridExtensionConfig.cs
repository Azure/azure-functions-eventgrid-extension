using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    public class EventGridExtensionConfig : IExtensionConfigProvider,
                       IAsyncConverter<HttpRequestMessage, HttpResponseMessage>
    {
        private ILogger _logger;

        internal IConverterManager ConverterManager { get; private set; }

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            _logger = context.Config.LoggerFactory.CreateLogger<EventGridExtensionConfig>();

            Uri url = context.GetWebhookHandler();
            _logger.LogInformation($"registered EventGrid Endpoint = {url}");

            // use converterManager as a hashTable
            // also take benefit of identity converter
            ConverterManager = context.Config.GetService<IConverterManager>();
            context
                .AddConverter<JObject, string>((jobject) => jobject.ToString(Formatting.Indented))
                .AddConverter<JObject, EventGridEvent>((jobject) => jobject.ToObject<EventGridEvent>()) // surface the type to csx
                .AddOpenConverter<JObject, OpenType.Poco>(typeof(JObjectToPocoConverter<>));
            //ConverterManager.AddConverter<JObject, OpenType.Poco, EventGridTriggerAttribute>((tSrc, tDest) => (input => ((JObject)input).ToObject(tDest)));

            // Register our extension binding providers
            context.Config.RegisterBindingExtension(new EventGridTriggerAttributeBindingProvider(this));
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
            // webjobs.script uses req.GetQueryNameValuePairs();
            // which requires webapi.core...but this does not work for .netframework2.0
            // TODO change this once webjobs.script is migrated
            var functionName = HttpUtility.ParseQueryString(req.RequestUri.Query)["functionName"];
            if (String.IsNullOrEmpty(functionName) || !_listeners.ContainsKey(functionName))
            {
                _logger.LogInformation($"cannot find function: '{functionName}', available function names: [{string.Join(", ", _listeners.Keys.ToArray())}]");
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            IEnumerable<string> eventTypeHeaders = null;
            string eventTypeHeader = null;
            if (req.Headers.TryGetValues("aeg-event-type", out eventTypeHeaders))
            {
                eventTypeHeader = eventTypeHeaders.First();
            }

            if (String.Equals(eventTypeHeader, "SubscriptionValidation", StringComparison.OrdinalIgnoreCase))
            {
                string jsonArray = await req.Content.ReadAsStringAsync();
                SubscriptionValidationEvent validationEvent = null;
                List<JObject> events = JsonConvert.DeserializeObject<List<JObject>>(jsonArray);
                // TODO remove unnecessary serialization
                validationEvent = ((JObject)events[0]["data"]).ToObject<SubscriptionValidationEvent>();
                SubscriptionValidationResponse validationResponse = new SubscriptionValidationResponse { ValidationResponse = validationEvent.ValidationCode };
                var returnMessage = new HttpResponseMessage(HttpStatusCode.OK);
                returnMessage.Content = new StringContent(JsonConvert.SerializeObject(validationResponse));
                _logger.LogInformation($"perform handshake with eventGrid for endpoint: {req.RequestUri}");
                return returnMessage;
            }
            else if (String.Equals(eventTypeHeader, "Notification", StringComparison.OrdinalIgnoreCase))
            {
                string jsonArray = await req.Content.ReadAsStringAsync();
                List<JObject> events = JsonConvert.DeserializeObject<List<JObject>>(jsonArray);

                foreach (var ev in events)
                {
                    TriggeredFunctionData triggerData = new TriggeredFunctionData
                    {
                        TriggerValue = ev
                    };

                    await _listeners[functionName].Executor.TryExecuteAsync(triggerData, CancellationToken.None);
                }

                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            else if (String.Equals(eventTypeHeader, "Unsubscribe", StringComparison.OrdinalIgnoreCase))
            {
                // TODO disable function?
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);

        }

        private class JObjectToPocoConverter<T> : IConverter<JObject, T>
        {
            public T Convert(JObject input)
            {
                return input.ToObject<T>();
            }
        }
    }
}
