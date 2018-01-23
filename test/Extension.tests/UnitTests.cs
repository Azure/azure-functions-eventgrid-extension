using Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests.Common;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using static Microsoft.Azure.WebJobs.Extensions.EventGrid.EventGridTriggerAttributeBindingProvider;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests
{
    public class UnitTests
    {
        private void DummyMethod(EventGridEvent e)
        {
        }

        [Fact]
        public async Task bindAsyncTest()
        {
            MethodBase methodbase = this.GetType().GetMethod("DummyMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            ParameterInfo[] arrayParam = methodbase.GetParameters();

            ITriggerBinding binding = new EventGridTriggerBinding(arrayParam[0], null);
            // given GventGridEvent
            EventGridEvent eve = JsonConvert.DeserializeObject<EventGridEvent>(FakeData.singleEvent);
            JObject data = eve.Data;

            ITriggerData triggerDataWithEvent = await binding.BindAsync(eve, null);
            Assert.Equal(data, triggerDataWithEvent.BindingData["data"]);

            ITriggerData triggerDataWithString = await binding.BindAsync(FakeData.singleEvent, null);
            Assert.Equal(data, triggerDataWithString.BindingData["data"]);

            // test invalid, batch of events
            FormatException formatException = await Assert.ThrowsAsync<FormatException>(() => binding.BindAsync(FakeData.arrayOfOneEvent, null));
            Assert.Equal($"Unable to parse {FakeData.arrayOfOneEvent} to {typeof(EventGridEvent)}", formatException.Message);

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
