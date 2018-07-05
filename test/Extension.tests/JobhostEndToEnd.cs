using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests.Common;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Indexers;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests
{
    public class JobhostEndToEnd
    {
        private static string _functionOut = null;

        [Theory]
        [InlineData("EventGridParams.TestEventGridToString")]
        [InlineData("EventGridParams.TestEventGridToJObject")]
        [InlineData("EventGridParams.TestEventGridToNuget")]
        public async Task ConsumeEventGridEventTest(string functionName)
        {

            JObject eve = JObject.Parse(FakeData.eventGridEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var expectOut = (string)eve["subject"];

            var host = TestHelpers.NewHost<EventGridParams>();

            await host.CallAsync(functionName, args);
            Assert.Equal(_functionOut, expectOut);
            _functionOut = null;
        }

        [Fact]
        public async Task InvalidParamsTests()
        {
            JObject eve = JObject.Parse(FakeData.eventGridEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var host = TestHelpers.NewHost<EventGridParams>();

            // when invoked
            var invocationException = await Assert.ThrowsAsync<FunctionInvocationException>(() => host.CallAsync("EventGridParams.TestEventGridToCustom", args));
            Assert.Equal(@"Exception binding parameter 'value'", invocationException.InnerException.Message);

            // when indexed
            host = TestHelpers.NewHost<InvalidParam>();
            var indexException = await Assert.ThrowsAsync<FunctionIndexingException>(() => host.StartAsync());
            Assert.Equal($"Can't bind EventGridTrigger to type '{typeof(int)}'.", indexException.InnerException.Message);
        }

        [Theory]
        [InlineData("CloudEventParams.TestCloudEventToString")]
        [InlineData("CloudEventParams.TestCloudEventToJObject")]
        public async Task ConsumeCloudEventTest(string functionName)
        {
            JObject eve = JObject.Parse(FakeData.cloudEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var expectOut = (string)eve["eventType"];

            var host = TestHelpers.NewHost<CloudEventParams>();

            await host.CallAsync(functionName, args);
            Assert.Equal(_functionOut, expectOut);
            _functionOut = null;
        }

        [Theory]
        [InlineData("TriggerParamResolve.TestJObject", "eventGridEvent", @"https://shunsouthcentralus.blob.core.windows.net/debugging/shunBlob.txt")]
        [InlineData("TriggerParamResolve.TestString", "stringDataEvent", "goodBye world")]
        [InlineData("TriggerParamResolve.TestArray", "arrayDataEvent", "ConfusedDev")]
        [InlineData("TriggerParamResolve.TestPrimitive", "primitiveDataEvent", "123")]
        [InlineData("TriggerParamResolve.TestDataFieldMissing", "missingDataEvent", "")]
        public async Task ValidTriggerDataResolveTests(string functionName, string argument, string expectedOutput)
        {
            var host = TestHelpers.NewHost<TriggerParamResolve>();

            var args = new Dictionary<string, object>{
                { "value", JObject.Parse((string)typeof(FakeData).GetField(argument).GetValue(null)) }
            };

            await host.CallAsync(functionName, args);
            Assert.Equal(expectedOutput, _functionOut);
            _functionOut = null;
        }

        public class EventGridParams
        {
            // different argument types

            public void TestEventGridToString([EventGridTrigger] string value)
            {
                _functionOut = (string)JObject.Parse(value)["subject"];
            }

            public void TestEventGridToJObject([EventGridTrigger] JObject value)
            {
                _functionOut = (string)value["subject"];
            }

            public void TestEventGridToNuget([EventGridTrigger] EventGridEvent value)
            {
                _functionOut = value.Subject;
            }

            public void TestEventGridToCustom([EventGridTrigger] Poco value)
            {
                _functionOut = value.Name;
            }
        }

        public class InvalidParam
        {
            public void TestEventGridToValueType([EventGridTrigger] int value)
            {
                _functionOut = "failure";
            }
        }

        public class Poco
        {
            public string Name { get; set; }
            // in the json payload, Subject is a String, this should cause JObject conversion to fail
            public int Subject { get; set; }
        }

        public class TriggerParamResolve
        {
            public void TestJObject(
                [EventGridTrigger] JObject value,
                [BindingData("{data.fileUrl}")] string autoResolve)
            {
                _functionOut = autoResolve;
            }

            public void TestString(
                [EventGridTrigger] JObject value,
                [BindingData("{data}")] string autoResolve)
            {
                _functionOut = autoResolve;
            }

            public void TestDataFieldMissing(
                [EventGridTrigger] JObject value,
                [BindingData("{data}")] string autoResovle)
            {
                _functionOut = autoResovle;
            }

            // auto resolve only works for string
            public void TestArray(
                [EventGridTrigger] JObject value)
            {
                JArray data = (JArray)value["data"];
                _functionOut = (string)value["data"][0];
            }

            public void TestPrimitive(
                [EventGridTrigger] JObject value)
            {
                int data = (int)value["data"];
                _functionOut = data.ToString();
            }
        }

        public class CloudEventParams
        {
            public void TestCloudEventToString([EventGridTrigger] string value)
            {
                _functionOut = (string)JObject.Parse(value)["eventType"];
            }

            public void TestCloudEventToJObject([EventGridTrigger] JObject value)
            {
                _functionOut = (string)value["eventType"];
            }
        }
    }
}
