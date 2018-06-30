using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests
{
    public class JobhostEndToEnd
    {
        private static string _functionOut = null;

        [Fact]
        public async Task ConsumeEventGridEventTest()
        {

            JObject eve = JObject.Parse(FakeData.eventGridEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var expectOut = (string)eve["subject"];

            var host = TestHelpers.NewHost<EventGridParams>();

            await host.CallAsync("EventGridParams.TestEventGrid", args);
            Assert.Equal(_functionOut, expectOut);
            _functionOut = null;

            await host.CallAsync("EventGridParams.TestEventGridToString", args);
            Assert.Equal(_functionOut, expectOut);
            _functionOut = null;

            await host.CallAsync("EventGridParams.TestEventGridToJObject", args);
            Assert.Equal(_functionOut, expectOut);
            _functionOut = null;
        }

        [Fact]
        public async Task ConsumeCloudEventTest()
        {
            JObject eve = JObject.Parse(FakeData.cloudEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var expectOut = (string)eve["eventType"];

            var host = TestHelpers.NewHost<CloudEventParams>();

            await host.CallAsync("CloudEventParams.TestCloudEventToString", args);
            Assert.Equal(_functionOut, expectOut);
            _functionOut = null;

            await host.CallAsync("CloudEventParams.TestCloudEventToJObject", args);
            Assert.Equal(_functionOut, expectOut);
            _functionOut = null;
        }

        [Fact]
        public async Task ValidTriggerDataResolveTests()
        {
            var host = TestHelpers.NewHost<TriggerParamResolve>();

            var args = new Dictionary<string, object>{
                { "value", JObject.Parse(FakeData.eventGridEvent) }
            };

            await host.CallAsync("TriggerParamResolve.TestJObject", args);
            Assert.Equal(@"https://shunsouthcentralus.blob.core.windows.net/debugging/shunBlob.txt", _functionOut);
            _functionOut = null;

            args["value"] = JObject.Parse(FakeData.stringDataEvent);
            await host.CallAsync("TriggerParamResolve.TestString", args);
            Assert.Equal("goodBye world", _functionOut);
            _functionOut = null;

            args["value"] = JObject.Parse(FakeData.arrayDataEvent);
            await host.CallAsync("TriggerParamResolve.TestArray", args);
            Assert.Equal("ConfusedDev", _functionOut);
            _functionOut = null;

            args["value"] = JObject.Parse(FakeData.primitiveDataEvent);
            await host.CallAsync("TriggerParamResolve.TestPrimitive", args);
            Assert.Equal("123", _functionOut);
            _functionOut = null;

            args["value"] = JObject.Parse(FakeData.missingDataEvent);
            await host.CallAsync("TriggerParamResolve.TestDataFieldMissing", args);
            Assert.Equal("", _functionOut);
            _functionOut = null;
        }

        public class EventGridParams
        {
            // different argument types
            public void TestEventGrid([EventGridTrigger] EventGridEvent value)
            {
                _functionOut = value.Subject;
            }

            public void TestEventGridToString([EventGridTrigger] string value)
            {
                _functionOut = (string)JObject.Parse(value)["subject"];
            }

            public void TestEventGridToJObject([EventGridTrigger] JObject value)
            {
                _functionOut = (string)value["subject"];
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
    }
}
