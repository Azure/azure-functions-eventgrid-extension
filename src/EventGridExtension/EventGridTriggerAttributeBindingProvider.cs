// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventGridOfficial = Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.Bindings;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
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

            if (!IsSupportedBindingType(parameter.ParameterType))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                    "Can't bind EventGridTriggerAttribute to type '{0}'.", parameter.ParameterType));
            }

            return Task.FromResult<ITriggerBinding>(new EventGridTriggerBinding(context.Parameter, _extensionConfigProvider));

        }

        public bool IsSupportedBindingType(Type t)
        {
            return (t == typeof(EventGridOfficial.EventGridEvent) || t == typeof(EventGridEvent) || t == typeof(string));
        }

        internal class EventGridTriggerBinding : ITriggerBinding
        {
            private readonly ParameterInfo _parameter;
            private readonly Dictionary<string, Type> _bindingContract;
            private EventGridExtensionConfig _listenersStore;

            public EventGridTriggerBinding(ParameterInfo parameter, EventGridExtensionConfig listenersStore)
            {
                _listenersStore = listenersStore;
                _parameter = parameter;
                _bindingContract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
                {
                    { "data", typeof(JObject) }
                };
            }

            public IReadOnlyDictionary<string, Type> BindingDataContract
            {
                get { return _bindingContract; }
            }

            public Type TriggerValueType
            {
                get { return typeof(EventGridOfficial.EventGridEvent); }
            }

            public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
            {
                // convert value to EventGridEvent, extract {data} as JObject
                EventGridOfficial.EventGridEvent triggerValue = null;
                if (value is string stringValue)
                {
                    try
                    {
                        triggerValue = JsonConvert.DeserializeObject<EventGridOfficial.EventGridEvent>(stringValue);
                    }
                    catch (Exception)
                    {
                        throw new FormatException($"Unable to parse {stringValue} to {typeof(EventGridOfficial.EventGridEvent)}");
                    }

                }
                else
                {
                    // default casting
                    triggerValue = value as EventGridOfficial.EventGridEvent;
                }

                if (triggerValue == null)
                {
                    throw new InvalidOperationException($"Unable to bind {value} to type {_parameter.ParameterType}");
                }

                var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "data", triggerValue.Data }
                };

                // convert EventGridEvent to parameterType
                // TODO use converters
                object argument;
                string argumentAsString = JsonConvert.SerializeObject(triggerValue, Formatting.Indented); ;
                if (_parameter.ParameterType == typeof(string))
                {
                    argument = argumentAsString;
                }
                else if (_parameter.ParameterType == typeof(EventGridEvent))
                {
                    argument = JsonConvert.DeserializeObject<EventGridEvent>(argumentAsString);
                }
                else
                {
                    argument = JsonConvert.DeserializeObject<EventGridOfficial.EventGridEvent>(argumentAsString);
                }

                IValueBinder valueBinder = new EventGridValueBinder(_parameter, argument);
                return Task.FromResult<ITriggerData>(new TriggerData(valueBinder, bindingData));
            }

            public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
            {
                // for csharp function, shortName == functionNameAttribute.Name
                // for csharpscript function, shortName == Functions.FolderName (need to strip the first half)
                string functionName = context.Descriptor.ShortName.Split('.').Last();
                return Task.FromResult<IListener>(new EventGridListener(context.Executor, _listenersStore, functionName));
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

            private class EventGridTriggerParameterDescriptor : TriggerParameterDescriptor
            {
                public override string GetTriggerReason(IDictionary<string, string> arguments)
                {
                    // TODO: Customize your Dashboard display string
                    return string.Format("EventGrid trigger fired at {0}", DateTime.Now.ToString("o"));
                }
            }

            // dispose IO resources
            private class EventGridValueBinder : ValueBinder, IDisposable
            {
                private readonly object _value;
                private List<IDisposable> _disposables = null;

                public EventGridValueBinder(ParameterInfo parameter, object value, List<IDisposable> disposables = null)
                    : base(parameter.ParameterType)
                {
                    _value = value;
                    _disposables = disposables;
                }

                public void Dispose()
                {
                    if (_disposables != null)
                    {
                        foreach (var d in _disposables)
                        {
                            d.Dispose();
                        }
                        _disposables = null;
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
