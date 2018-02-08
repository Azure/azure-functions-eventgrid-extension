using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests.Common;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Newtonsoft.Json.Linq;
using Xunit;
using static Microsoft.Azure.WebJobs.Extensions.EventGrid.EventGridTriggerAttributeBindingProvider;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests
{
    public class UnitTests
    {
        private void DummyMethod(JObject e)
        {
        }

        [Fact]
        public async Task bindAsyncTest()
        {
            MethodBase methodbase = this.GetType().GetMethod("DummyMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            ParameterInfo[] arrayParam = methodbase.GetParameters();
            Func<JObject, object, object, JObject> identityConverter = (src, attribute, context) => src;

            ITriggerBinding binding = new EventGridTriggerBinding(arrayParam[0], null, identityConverter);
            JObject eve = JObject.Parse(FakeData.singleEvent);
            JObject data = (JObject)eve["data"];

            // JObject as input
            ITriggerData triggerDataWithEvent = await binding.BindAsync(eve, null);
            Assert.Equal(data, triggerDataWithEvent.BindingData["data"]);

            // string as input (Test API from portal)
            ITriggerData triggerDataWithString = await binding.BindAsync(FakeData.singleEvent, null);
            Assert.Equal(data, triggerDataWithString.BindingData["data"]);

            // test invalid, batch of events
            FormatException formatException = await Assert.ThrowsAsync<FormatException>(() => binding.BindAsync(FakeData.arrayOfOneEvent, null));
            Assert.Equal($"Unable to parse {FakeData.arrayOfOneEvent} to {typeof(JObject)}", formatException.Message);

            // test invalid, random object
            var testObject = new TestClass();
            InvalidOperationException invalidException = await Assert.ThrowsAsync<InvalidOperationException>(() => binding.BindAsync(testObject, null));
            Assert.Equal($"Unable to bind {testObject} to type {arrayParam[0].ParameterType}", invalidException.Message);
        }

        private class TestClass
        {
            public override string ToString()
            {
                return "test object";
            }
        }
    }
}
