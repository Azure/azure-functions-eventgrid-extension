using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using System;

// binding 
namespace TestStuff
{
    [Binding]
    public class BindingDataAttribute : Attribute
    {
        public BindingDataAttribute(string path)
        {
            this.Path = path;
        }

        [AutoResolve]
        public string Path { get; set; }
    }

    public class MyExtension : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            context.AddBindingRule<BindingDataAttribute>().
                BindToInput<string>(attr => attr.Path);
        }
    }
}
