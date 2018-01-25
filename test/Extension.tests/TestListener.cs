using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host.Config;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests
{
    public class TestListener : IWebHookProvider
    {

        Uri IWebHookProvider.GetUrl(IExtensionConfigProvider extension)
        {
            // Called by configuration registration. URI here doesn't matter.
            return new Uri("http://localhost");
        }

        static private StringBuilder _log = new StringBuilder();

        public TestListener()
        {
            _log.Clear();
        }

        // Unsubscribe gives a 202.
        [Fact]
        public async Task TestUnsubscribe()
        {
            var ext = new EventGridExtensionConfig();

            var host = TestHelpers.NewHost<MyProg1>(ext);

            await host.StartAsync(); // add listener

            var request = CreateUnsubscribeRequest("TestEventGrid");
            IAsyncConverter<HttpRequestMessage, HttpResponseMessage> handler = ext;
            var response = await handler.ConvertAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }


        // Test that an event payload with multiple events causes multiple dispatches,
        /// and that each instance has correct binding data .
        // This is the fundamental difference between a regular HTTP trigger and a EventGrid trigger.
        [Fact]
        public async Task TestDispatch()
        {
            var ext = new EventGridExtensionConfig();

            var host = TestHelpers.NewHost<MyProg1>(ext);

            await host.StartAsync(); // add listener

            var request = CreateDispatchRequest("TestEventGrid",
                JObject.Parse(@"{'subject':'one','data':{'prop':'alpha'}}"),
                JObject.Parse(@"{'subject':'two','data':{'prop':'beta'}}"));

            IAsyncConverter<HttpRequestMessage, HttpResponseMessage> handler = ext;
            var response = await handler.ConvertAsync(request, CancellationToken.None);

            // Verify that the user function was dispatched twice, in order.
            // Also verifies each instance gets its own proper binding data (from FakePayload.Prop)
            Assert.Equal("[Dispatch:one, alpha][Dispatch:two, beta]", _log.ToString());

            // TODO - Verify that we return from webhook before the dispatch is finished
            // https://github.com/Azure/azure-functions-eventgrid-extension/issues/10
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        static HttpRequestMessage CreateUnsubscribeRequest(string funcName)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/?functionName=" + funcName);
            request.Headers.Add("aeg-event-type", "Unsubscribe");
            return request;
        }

        static HttpRequestMessage CreateDispatchRequest(string funcName, params JObject[] items)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/?functionName=" + funcName);
            request.Headers.Add("aeg-event-type", "Notification");
            var payloadArray = new JArray();
            foreach (var item in items)
            {
                payloadArray.Add(item);
            }
            request.Content = new StringContent(
                payloadArray.ToString(),
                Encoding.UTF8,
                "application/json");
            return request;
        }

        public class FakePayload
        {
            public string Prop { get; set; }
        }

        public class MyProg1
        {
            [FunctionName("TestEventGrid")]
            public void Run(
                [EventGridTrigger] JObject value,
                [BindingData("{data.prop}")] string prop)
            {
                _log.Append($"[Dispatch:{(string)value["subject"]}, {prop}]");
            }
        }
    }
}
