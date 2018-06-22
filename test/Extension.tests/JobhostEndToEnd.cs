using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests
{
    public class JobhostEndToEnd
    {
        static private string functionOut = null;

        [Fact]
        public async Task ConsumeEventGridEventTest()
        {

            JObject eve = JObject.Parse(FakeData.eventGridEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var expectOut = (string)eve["subject"];

            var host = TestHelpers.NewHost<MyProg1>();

            await host.CallAsync("MyProg1.TestEventGrid", args);
            Assert.Equal(functionOut, expectOut);
            functionOut = null;

            await host.CallAsync("MyProg1.TestEventGridToString", args);
            Assert.Equal(functionOut, expectOut);
            functionOut = null;

            await host.CallAsync("MyProg1.TestEventGridToJObject", args);
            Assert.Equal(functionOut, expectOut);
            functionOut = null;
        }

        [Fact]
        public async Task ConsumeCloudEventTest()
        {
            JObject eve = JObject.Parse(FakeData.cloudEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var expectOut = (string)eve["eventType"];

            var host = TestHelpers.NewHost<MyProg2>();

            await host.CallAsync("MyProg2.TestCloudEventToString", args);
            Assert.Equal(functionOut, expectOut);
            functionOut = null;

            await host.CallAsync("MyProg2.TestCloudEventToJObject", args);
            Assert.Equal(functionOut, expectOut);
            functionOut = null;
        }

        [Fact]
        public async Task ValidJsonBindingDataTests()
        {
            var host = TestHelpers.NewHost<MyProg3>();

            var args = new Dictionary<string, object>{
                { "value", JObject.Parse(FakeData.eventGridEvent) }
            };

            await host.CallAsync("MyProg3.TestJObject", args);
            Assert.Equal(@"https://shunsouthcentralus.blob.core.windows.net/debugging/shunBlob.txt", functionOut);
            functionOut = null;

            args["value"] = JObject.Parse(FakeData.stringDataEvent);
            await host.CallAsync("MyProg3.TestString", args);
            Assert.Equal("goodBye world", functionOut);
            functionOut = null;

            args["value"] = JObject.Parse(FakeData.arrayDataEvent);
            await host.CallAsync("MyProg3.TestArray", args);
            Assert.Equal("ConfusedDev", functionOut);
            functionOut = null;

            args["value"] = JObject.Parse(FakeData.primitiveDataEvent);
            await host.CallAsync("MyProg3.TestPrimitive", args);
            Assert.Equal("123", functionOut);
            functionOut = null;

            args["value"] = JObject.Parse(FakeData.missingDataEvent);
            await host.CallAsync("MyProg3.TestDataFieldMissing", args);
            Assert.Equal("", functionOut);
            functionOut = null;
        }

        public class MyProg1
        {
            // different argument types
            public void TestEventGrid([EventGridTrigger] EventGridEvent value)
            {
                functionOut = value.Subject;
            }

            public void TestEventGridToString([EventGridTrigger] string value)
            {
                functionOut = (string)JObject.Parse(value)["subject"];
            }

            public void TestEventGridToJObject([EventGridTrigger] JObject value)
            {
                functionOut = (string)value["subject"];
            }
        }

        public class MyProg2
        {
            public void TestCloudEventToString([EventGridTrigger] string value)
            {
                functionOut = (string)JObject.Parse(value)["eventType"];
            }

            public void TestCloudEventToJObject([EventGridTrigger] JObject value)
            {
                functionOut = (string)value["eventType"];
            }

        }

        public class MyProg3
        {
            public void TestJObject(
                [EventGridTrigger] JObject value,
                [BindingData("{data.fileUrl}")] string autoResolve)
            {
                functionOut = autoResolve;
            }

            public void TestString(
                [EventGridTrigger] JObject value,
                [BindingData("{data}")] string autoResolve)
            {
                functionOut = autoResolve;
            }

            public void TestDataFieldMissing(
                [EventGridTrigger] JObject value,
                [BindingData("{data}")] string autoResovle)
            {
                functionOut = autoResovle;
            }

            // auto resolve only works for string
            public void TestArray(
                [EventGridTrigger] JObject value)
            {
                JArray data = (JArray)value["data"];
                functionOut = (string)value["data"][0];
            }

            public void TestPrimitive(
                [EventGridTrigger] JObject value)
            {
                int data = (int)value["data"];
                functionOut = data.ToString();
            }
        }
    }
}
