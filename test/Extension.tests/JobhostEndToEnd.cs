using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests.Common;
using Newtonsoft.Json;
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

            JObject eve = JsonConvert.DeserializeObject<JObject>(FakeData.singleEvent);
            var args = new Dictionary<string, object>{
                { "value", eve }
            };

            var host = TestHelpers.NewHost<MyProg1>();

            await host.CallAsync("MyProg1.TestEventGrid", args);
            Assert.Equal(functionOut, eve["subject"].Value<string>());
            functionOut = null;

            await host.CallAsync("MyProg1.TestEventGridToString", args);
            Assert.Equal(functionOut, eve["subject"].Value<string>());
            functionOut = null;
        }

        [Fact]
        public async Task UseInputBlobBinding()
        {
            JObject eve = JsonConvert.DeserializeObject<JObject>(FakeData.singleEvent);
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
            public void TestEventGrid([EventGridTrigger] EventGridEvent value)
            {
                functionOut = value.Subject;
            }

            public void TestEventGridToString([EventGridTrigger] string value)
            {
                functionOut = JsonConvert.DeserializeObject<EventGridEvent>(value).Subject;
            }
        }

        public class MyProg3
        {
            public void TestBlobStream(
            [EventGridTrigger] EventGridEvent value,
            [BindingData("{data.fileUrl}")] string autoResolve)
            {
                functionOut = autoResolve;
            }
        }
    }
}
