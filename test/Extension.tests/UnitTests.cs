using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task BindAsyncTest()
        {
            MethodBase methodbase = this.GetType().GetMethod("DummyMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            ParameterInfo[] arrayParam = methodbase.GetParameters();

            ITriggerBinding binding = new EventGridTriggerBinding(arrayParam[0], null, singleDispatch: true);
            JObject eve = JObject.Parse(FakeData.eventGridEvent);
            JObject data = (JObject)eve["data"];

            // Data for batch binding
            ITriggerBinding bindingBatch = new EventGridTriggerBinding(arrayParam[0], null, singleDispatch: false);
            JArray events = JArray.Parse(FakeData.multipleEventGridEvents);
            IEnumerable<JToken> dataEvents = events.Select(ev => ev["data"]);

            // JObject as input
            ITriggerData triggerDataWithEvent = await binding.BindAsync(eve, null);
            Assert.Equal(data, triggerDataWithEvent.BindingData["data"]);

            // JArray as input
            ITriggerData triggerDataWithEvents = await bindingBatch.BindAsync(events, null);
            Assert.Equal(dataEvents, triggerDataWithEvents.BindingData["data"]);

            // string as input
            ITriggerData triggerDataWithString = await binding.BindAsync(FakeData.eventGridEvent, null);
            Assert.Equal(data, triggerDataWithString.BindingData["data"]);

            // test invalid, batch of events
            FormatException formatException = await Assert.ThrowsAsync<FormatException>(() => binding.BindAsync(FakeData.eventGridEvents, null));
            Assert.Equal($"Unable to parse {FakeData.eventGridEvents} to {typeof(JObject)}", formatException.Message);

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
