// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Extensions.Bindings;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs
{
    internal class EventGridTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private EventGridExtensionConfig _extensionConfigProvider;
        internal EventGridTriggerAttributeBindingProvider(EventGridExtensionConfig extensionConfigProvider)
        {
            _extensionConfigProvider = extensionConfigProvider;
        }

        // called when loading the function
        // no input yet
        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ParameterInfo parameter = context.Parameter;
            EventGridTriggerAttribute attribute = parameter.GetCustomAttribute<EventGridTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            // TODO: Define the types your binding supports here
            if (parameter.ParameterType != typeof(EventGridEvent) && parameter.ParameterType != typeof(Stream) && parameter.ParameterType != typeof(string))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                    "Can't bind EventGridTriggerAttribute to type '{0}'.", parameter.ParameterType));
            }

            return Task.FromResult<ITriggerBinding>(new EventGridTriggerBinding(context.Parameter, _extensionConfigProvider, context.Parameter.Member.Name));
        }


        private class EventGridTriggerBinding : ITriggerBinding
        {
            private readonly ParameterInfo _parameter;
            private readonly IReadOnlyDictionary<string, Type> _bindingContract;
            private EventGridExtensionConfig _listenersStore;
            private readonly string _functionName;
            private object _value;

            public EventGridTriggerBinding(ParameterInfo parameter, EventGridExtensionConfig listenersStore, string functionName)
            {
                _listenersStore = listenersStore;
                _parameter = parameter;
                _bindingContract = CreateBindingDataContract();
                _functionName = functionName;
            }

            public IReadOnlyDictionary<string, Type> BindingDataContract
            {
                // TODO? not per parameter?
                get { return _bindingContract; }
            }

            public Type TriggerValueType
            {
                /*
                TriggeredFunctionData input = new TriggeredFunctionData
                {
                    TriggerValue = param
                };
                */
                get { return typeof(EventGridEvent); }
            }

            public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
            {
                EventGridEvent triggerValue = value as EventGridEvent;
                if (_parameter.ParameterType == typeof(EventGridEvent))
                {
                    _value = triggerValue;
                }
                else if (_parameter.ParameterType == typeof(Stream))
                {
                    var byteStream = new MemoryStream();
                    HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(triggerValue.Data.destionationUrl);
                    using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
                    {
                        using (Stream responseStream = myHttpWebResponse.GetResponseStream())
                        {
                            var buffer = new byte[4096];
                            var bytesRead = 0;
                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                byteStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                    byteStream.Position = 0;
                    _value = byteStream;
                }
                else
                {
                    _value = triggerValue.Data.destionationUrl.ToString();
                }
                IValueBinder valueBinder = new EventGridValueBinder(_parameter, _value);
                return Task.FromResult<ITriggerData>(new TriggerData(valueBinder, GetBindingData(_value, triggerValue)));
            }

            public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
            {
                return Task.FromResult<IListener>(new EventGridListener(context.Executor, _listenersStore, _functionName));
            }

            public ParameterDescriptor ToParameterDescriptor()
            {
                return new EventGridTriggerParameterDescriptor
                {
                    Name = _parameter.Name,
                    DisplayHints = new ParameterDisplayHints
                    {
                        // TODO: Customize your Dashboard display strings
                        Prompt = "EventGrid",
                        Description = "EventGrid trigger fired",
                        DefaultValue = "Sample"
                    }
                };
            }

            private IReadOnlyDictionary<string, object> GetBindingData(object value, EventGridEvent triggerValue)
            {
                Dictionary<string, object> bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                bindingData.Add("EventGridTrigger", value);
                bindingData.Add("name", triggerValue.Data.destionationUrl.LocalPath); // conditional to eventhub archive

                return bindingData;
            }

            private IReadOnlyDictionary<string, Type> CreateBindingDataContract()
            {
                Dictionary<string, Type> contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                contract.Add("EventGridTrigger", _parameter.ParameterType);
                contract.Add("name", typeof(string)); // conditional to eventhub archive

                return contract;
            }

            private class EventGridTriggerParameterDescriptor : TriggerParameterDescriptor
            {
                public override string GetTriggerReason(IDictionary<string, string> arguments)
                {
                    // TODO: Customize your Dashboard display string
                    return string.Format("EventGrid trigger fired at {0}", DateTime.Now.ToString("o"));
                }
            }

            private class EventGridValueBinder : ValueBinder
            {
                private readonly object _value;

                public EventGridValueBinder(ParameterInfo parameter, object value)
                    : base(parameter.ParameterType)
                {
                    _value = value;
                }

                public override Task<object> GetValueAsync()
                {
                    return Task.FromResult<object>(_value);
                }

                public override string ToInvokeString()
                {
                    // TODO: Customize your Dashboard invoke string
                    return _value.ToString();
                }
            }

        }
    }
}
