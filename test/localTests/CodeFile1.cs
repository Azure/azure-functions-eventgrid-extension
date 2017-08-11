using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestStuff;

namespace EventGridBinding
{
    // This should be a real test 
    public class Test
    {

        public async Task MyTest()
        {
            JobHostConfiguration config = new JobHostConfiguration();
            config.DashboardConnectionString = null;

            config.TypeLocator = new FakeTypeLocator<SampleFunc>();
            config.AddExtension(new EventGridExtensionConfig());
            config.AddExtension(new MyExtension());

            var data = new JObject();
            data["MyProp"] = "abc";

            var host = new JobHost(config);
            IDictionary<string, object> args = new Dictionary<String, object>
            {
                {  "trigger", new EventGridEvent
                {
                     Data = data
                }
                }
            };

            var method = typeof(SampleFunc).GetMethod("MyFunc");
            await host.CallAsync(method, args);
        }

        public class FakeTypeLocator<T> : ITypeLocator
        {
            public IReadOnlyList<Type> GetTypes()
            {
                return new Type[] { typeof(T) };
            }
        }

        public class SampleFunc
        {
            public async Task MyFunc(
                [EventGridTrigger] EventGridEvent trigger,
                [BindingData("{data.MyProp}")] string value
                )
            {
                if (value != "abc")
                {
                    throw new InvalidOperationException("Bad value");
                }
            }
        }
    }
}
