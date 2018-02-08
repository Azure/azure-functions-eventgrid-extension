using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        [Fact]
        public async Task ValidParameterTypeTests()
        {

            JObject eve = JObject.Parse(FakeData.singleEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var expectOut = (string)eve["subject"];

            var host = TestHelpers.NewHost<MyProg1>();

            await host.CallAsync("MyProg1.TestEventGridToObsolete", args);
            Assert.Equal(_functionOut, expectOut);
            _functionOut = null;

            await host.CallAsync("MyProg1.TestEventGridToString", args);
            Assert.Equal(_functionOut, expectOut);
            _functionOut = null;

            await host.CallAsync("MyProg1.TestEventGridToJObject", args);
            Assert.Equal(_functionOut, expectOut);
            _functionOut = null;

            await host.CallAsync("MyProg1.TestEventGridToNuget", args);
            Assert.Equal(_functionOut, expectOut);
            _functionOut = null;

            // when invoked
            var invocationException = await Assert.ThrowsAsync<FunctionInvocationException>(() => host.CallAsync("MyProg1.TestEventGridToCustom", args));
            Assert.Equal($"Can't bind EventGridTriggerAttribute to type '{typeof(Poco)}'.", invocationException.InnerException.InnerException.Message);

            // when indexed
            host = TestHelpers.NewHost<MyProg2>();
            var indexException = await Assert.ThrowsAsync<FunctionIndexingException>(() => host.StartAsync());
            Assert.Equal($"Can't bind EventGridTriggerAttribute to type '{typeof(int)}'.", indexException.InnerException.Message);
        }

        [Fact]
        public async Task ValidTriggerDataTests()
        {
            var host = TestHelpers.NewHost<MyProg3>();

            JObject eve = JObject.Parse(FakeData.singleEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            await host.CallAsync("MyProg3.TestJObject", args);
            Assert.Equal(@"https://shunsouthcentralus.blob.core.windows.net/debugging/shunBlob.txt", _functionOut);
            _functionOut = null;

            eve = JObject.Parse(FakeData.stringDataEvent);
            args = new Dictionary<string, object>{
                { "value", eve }
            };

            await host.CallAsync("MyProg3.TestString", args);
            Assert.Equal("goodBye world", _functionOut);
            _functionOut = null;

            eve = JObject.Parse(FakeData.arrayDataEvent);
            args = new Dictionary<string, object>{
                { "value", eve }
            };

            await host.CallAsync("MyProg3.TestArray", args);
            Assert.Equal("ConfusedDev", _functionOut);
            _functionOut = null;

            eve = JObject.Parse(FakeData.primitiveDataEvent);
            args = new Dictionary<string, object>{
                { "value", eve }
            };

            await host.CallAsync("MyProg3.TestPrimitive", args);
            Assert.Equal("123", _functionOut);
            _functionOut = null;

        }

        public class MyProg1
        {
            // different argument types
            public void TestEventGridToObsolete([EventGridTrigger] EventGridEvent value)
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

            public void TestEventGridToNuget([EventGridTrigger] Azure.EventGrid.Models.EventGridEvent value)
            {
                _functionOut = value.Subject;
            }

            public void TestEventGridToCustom([EventGridTrigger] Poco value)
            {
                _functionOut = value.Name;
            }
        }

        public class MyProg2
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

        public class MyProg3
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
