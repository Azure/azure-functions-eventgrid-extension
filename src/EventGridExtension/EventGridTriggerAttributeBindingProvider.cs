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
using System.Reflection;
using System.Threading.Tasks;

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

            // depends on the publisher, we could have different expectation for paramter
            // TODO javascript, you cannot sepcify parameterType?
            string publisherName = attribute.Publisher;
            IPublisher publisher = null;
            // factory pattern
            if (String.IsNullOrEmpty(publisherName))
            {
                publisher = new DefaultPublisher();
            }
            else if (String.Equals(publisherName, EventHubCapturePublisher.Name, StringComparison.OrdinalIgnoreCase))
            {
                publisher = new EventHubCapturePublisher();
            }

            var contract = publisher?.ExtractBindingContract(parameter.ParameterType);
            if (contract == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                    "Can't bind EventGridTriggerAttribute with publisher '{0}' to type '{1}'.", publisherName, parameter.ParameterType));
            }
            // unsupported publisher is caught in attribute constrcutor
            return Task.FromResult<ITriggerBinding>(new EventGridTriggerBinding(context.Parameter, _extensionConfigProvider, context.Parameter.Member.Name, publisher, contract));

        }


        private class EventGridTriggerBinding : ITriggerBinding
        {
            private readonly ParameterInfo _parameter;
            private readonly IReadOnlyDictionary<string, Type> _bindingContract;
            private EventGridExtensionConfig _listenersStore;
            private readonly string _functionName;
            private readonly IPublisher _publisher;

            public EventGridTriggerBinding(ParameterInfo parameter, EventGridExtensionConfig listenersStore, string functionName, IPublisher publisher, Dictionary<string, Type> contract)
            {
                _publisher = publisher;
                _listenersStore = listenersStore;
                _parameter = parameter;
                _functionName = functionName;
                _bindingContract = contract;
            }

            public IReadOnlyDictionary<string, Type> BindingDataContract
            {
                // TODO? not per parameter?
                get { return _bindingContract; }
            }

            public Type TriggerValueType
            {
                get { return typeof(EventGridEvent); }
            }

            public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
            {
                EventGridEvent triggerValue = value as EventGridEvent;
                var bindingData = _publisher.ExtractBindingData(triggerValue, _parameter.ParameterType);
                IValueBinder valueBinder = new EventGridValueBinder(_parameter, _publisher.GetArgument(bindingData), _publisher.Recycles);
                return Task.FromResult<ITriggerData>(new TriggerData(valueBinder, bindingData));
            }

            public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
            {
                // listenersStore is of Type "EventGridExtensionConfig"
                if (_listenersStore.IsTest)
                {
                    return Task.FromResult<IListener>(new TestListener(context.Executor));
                }
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

                public EventGridValueBinder(ParameterInfo parameter, object value, List<IDisposable> disposables)
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
