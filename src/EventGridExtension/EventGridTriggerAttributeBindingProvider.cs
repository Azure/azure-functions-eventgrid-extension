// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        private readonly EventGridExtensionConfig _extensionConfigProvider;
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


            var method = typeof(IConverterManager).GetMethod("GetConverter");
            // we convert data returned from listener or from test API to JObject first
            var generic = method.MakeGenericMethod(typeof(JObject), parameter.ParameterType, typeof(EventGridTriggerAttribute));
            object converter = generic.Invoke(_extensionConfigProvider.ConverterManager, null);
            if (converter == null)
            {
                // since we use openType, we defere JObject deserialization error to runtime
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                    "Can't bind EventGridTriggerAttribute to type '{0}'.", parameter.ParameterType));
            }

            return Task.FromResult<ITriggerBinding>(new EventGridTriggerBinding(context.Parameter, _extensionConfigProvider, converter));

        }

        internal class EventGridTriggerBinding : ITriggerBinding
        {
            private readonly ParameterInfo _parameter;
            private readonly Dictionary<string, Type> _bindingContract;
            private readonly EventGridExtensionConfig _listenersStore;
            private readonly dynamic _converter;

            public EventGridTriggerBinding(ParameterInfo parameter, EventGridExtensionConfig listenersStore, dynamic converter)
            {
                _listenersStore = listenersStore;
                _parameter = parameter;
                _converter = converter;
                _bindingContract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
                {
                    { "data", typeof(object) }
                };
            }

            public IReadOnlyDictionary<string, Type> BindingDataContract
            {
                get { return _bindingContract; }
            }

            public Type TriggerValueType
            {
                get { return typeof(JObject); }
            }

            public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
            {
                JObject triggerValue = null;
                if (value is string stringValue)
                {
                    try
                    {
                        triggerValue = JObject.Parse(stringValue);
                    }
                    catch (Exception)
                    {
                        throw new FormatException($"Unable to parse {stringValue} to {typeof(JObject)}");
                    }

                }
                else
                {
                    // default casting
                    triggerValue = value as JObject;
                }

                if (triggerValue == null)
                {
                    throw new InvalidOperationException($"Unable to bind {value} to type {_parameter.ParameterType}");
                }

                var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "data", triggerValue["data"] }
                };

                // convert to parameterType
                // 1. JObject to JObject
                // 2. JObject to POCO, EventGridEvent(obsolete)/EventGridEvent(nuget)
                // 3. JObject to String
                try
                {
                    // public delegate TDestination FuncConverter<TSource, TAttribute, TDestination>(TSource src, TAttribute attribute, ValueBindingContext context)
                    object argument = _converter(triggerValue, null, null);
                    IValueBinder valueBinder = new EventGridValueBinder(_parameter, argument);
                    return Task.FromResult<ITriggerData>(new TriggerData(valueBinder, bindingData));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        "Can't bind EventGridTriggerAttribute to type '{0}'.", _parameter.ParameterType), ex);
                }
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
