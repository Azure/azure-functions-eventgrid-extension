using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests.Common;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using static Microsoft.Azure.WebJobs.Extensions.EventGrid.EventGridTriggerAttributeBindingProvider;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests
{
    public class UnitTests
    {
        private void TestFunction(EventGridEvent e)
        {
        }

        [Fact]
        public async Task BindAsyncThrowsForNullValue()
        {
            var testFunctionMethod = this.GetType().GetMethod(nameof(TestFunction), BindingFlags.NonPublic | BindingFlags.Instance);
            var testFunctionMethodParameters = testFunctionMethod.GetParameters();

            var binding = new EventGridTriggerBinding(testFunctionMethodParameters[0], null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(
                "value", 
                () => binding.BindAsync(null, null));
        }

        [Fact]
        public async Task BindAsyncThrowsForNonJObject()
        {
            var testFunctionMethod = this.GetType().GetMethod(nameof(TestFunction), BindingFlags.NonPublic | BindingFlags.Instance);
            var testFunctionMethodParameters = testFunctionMethod.GetParameters();

            var binding = new EventGridTriggerBinding(testFunctionMethodParameters[0], null, null);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => binding.BindAsync(new InvalidBindingValue(), null));
        }

        [Fact]
        public async Task BindAsyncProvidesCorrectBindingData()
        {
            var testFunctionMethod = this.GetType().GetMethod(nameof(TestFunction), BindingFlags.NonPublic | BindingFlags.Instance);
            var testFunctionMethodParameters = testFunctionMethod.GetParameters();

            var binding = new EventGridTriggerBinding(testFunctionMethodParameters[0], null, null);

            var fullJsonEvent = JsonConvert.DeserializeObject<JObject>(FakeData.singleEvent);
            var expectedJsonEventData = fullJsonEvent["data"].Value<JObject>();

            ITriggerData triggerDataWithEvent = await binding.BindAsync(fullJsonEvent, null);

            Assert.Equal(expectedJsonEventData, triggerDataWithEvent.BindingData["data"]);
        }

        private class InvalidBindingValue
        {
            public override string ToString()
            {
                return "This is an invalid binding value.";
            }
        }
    }
}
