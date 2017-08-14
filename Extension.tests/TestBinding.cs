using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using System;

namespace Extension.tests
{
    [Binding]
    public class BindingDataAttribute : Attribute
    {
        public BindingDataAttribute(string toBeAutoResolve)
        {
            ToBeAutoResolve = toBeAutoResolve;
        }

        [AutoResolve]
        public string ToBeAutoResolve { get; set; }
    }

    public class TestExtensionConfig : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            context.AddBindingRule<BindingDataAttribute>().
                BindToInput<string>(attr => attr.ToBeAutoResolve);
        }
    }
}
