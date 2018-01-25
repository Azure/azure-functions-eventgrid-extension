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

            JObject eve = JObject.Parse(FakeData.singleEvent);
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
        public async Task UseInputBlobBinding()
        {
            JObject eve = JObject.Parse(FakeData.singleEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var host = TestHelpers.NewHost<MyProg3>();

            await host.CallAsync("MyProg3.TestBlobStream", args);
            Assert.Equal(@"https://shunsouthcentralus.blob.core.windows.net/debugging/shunBlob.txt", functionOut);
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

        public class MyProg3
        {
            public void TestBlobStream(
            [EventGridTrigger] JObject value,
            [BindingData("{data.fileUrl}")] string autoResolve)
            {
                functionOut = autoResolve;
            }
        }
    }
}
