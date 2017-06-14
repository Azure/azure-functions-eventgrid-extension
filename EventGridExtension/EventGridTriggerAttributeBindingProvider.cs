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
                IValueBinder valueBinder = new EventGridValueBinder(_parameter, triggerValue);
                return Task.FromResult<ITriggerData>(new TriggerData(valueBinder, GetBindingData(triggerValue)));
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

            private IReadOnlyDictionary<string, object> GetBindingData(EventGridEvent value)
            {
                Dictionary<string, object> bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                if (_parameter.ParameterType == typeof(EventGridEvent))
                {
                    bindingData.Add("EventGridTrigger", value);
                }
                else if (_parameter.ParameterType == typeof(Stream))
                {
                    HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(value.Data.destionationUrl);
                    // Sends the HttpWebRequest and waits for the response.			
                    HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                    // Gets the stream associated with the response.
                    bindingData.Add("EventGridTrigger", myHttpWebResponse.GetResponseStream());
                }
                else
                {
                    bindingData.Add("EventGridTrigger", value.Data.destionationUrl.ToString());
                }
                bindingData.Add("name", value.Data.destionationUrl.LocalPath);


                // TODO: Add any additional binding data

                return bindingData;
            }

            private IReadOnlyDictionary<string, Type> CreateBindingDataContract()
            {
                Dictionary<string, Type> contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                contract.Add("EventGridTrigger", _parameter.ParameterType);
                contract.Add("name", typeof(string));

                // TODO: Add any additional binding contract members

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

                public EventGridValueBinder(ParameterInfo parameter, EventGridEvent value)
                    : base(parameter.ParameterType)
                {
                    if (parameter.ParameterType == typeof(EventGridEvent))
                    {
                        _value = value;
                    }
                    else if (parameter.ParameterType == typeof(Stream))
                    {
                        HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(value.Data.destionationUrl);
                        // Sends the HttpWebRequest and waits for the response.			
                        HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                        // Gets the stream associated with the response.
                        _value = myHttpWebResponse.GetResponseStream();
                        // SHUNTODO since we know the size, we can put it in an array
                    }
                    else
                    {
                        _value = value.Data.destionationUrl.ToString();
                    }

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
