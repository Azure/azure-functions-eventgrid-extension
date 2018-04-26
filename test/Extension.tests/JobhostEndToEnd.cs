using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests.Common;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Indexers;
using Microsoft.Rest.Azure;
using Moq;
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

        [Fact]
        public async Task OutputBindingInvalidCredentialTests()
        {
            // validation is done at indexing time
            var host = TestHelpers.NewHost<OutputBindingParams>();
            // appsetting is missing
            var indexException = await Assert.ThrowsAsync<FunctionIndexingException>(() => host.StartAsync());
            Assert.Equal($"Unable to resolve app setting for property '{nameof(EventGridAttribute)}.{nameof(EventGridAttribute.TopicEndpointUri)}'. Make sure the app setting exists and has a valid value.", indexException.InnerException.Message);

            var nameResolverMock = new Mock<INameResolver>();
            // invalid uri
            nameResolverMock.Setup(x => x.Resolve("eventgridUri")).Returns("this could be anything...so lets try yolo");
            nameResolverMock.Setup(x => x.Resolve("eventgridKey")).Returns("thisismagic");

            host = TestHelpers.NewHost<OutputBindingParams>(nameResolver: nameResolverMock.Object);
            indexException = await Assert.ThrowsAsync<FunctionIndexingException>(() => host.StartAsync());
            Assert.Equal($"The '{nameof(EventGridAttribute.TopicEndpointUri)}' property must be a valid absolute Uri", indexException.InnerException.Message);

            nameResolverMock.Setup(x => x.Resolve("eventgridUri")).Returns("https://pccode.westus2-1.eventgrid.azure.net/api/events");
            // invalid sas token
            nameResolverMock.Setup(x => x.Resolve("eventgridKey")).Returns("");

            host = TestHelpers.NewHost<OutputBindingParams>(nameResolver: nameResolverMock.Object);
            indexException = await Assert.ThrowsAsync<FunctionIndexingException>(() => host.StartAsync());
            Assert.Equal($"The '{nameof(EventGridAttribute.TopicKeySetting)}' property must be the name of an application setting containing the Topic Key", indexException.InnerException.Message);
        }


        [Theory]
        [InlineData("SingleEvent", "0")]
        [InlineData("SingleReturnEvent", "0")]
        // space sperated string as event ids
        [InlineData("ArrayEvent", "0 1 2 3 4")]
        [InlineData("CollectorEvent", "0 1 2 3")]
        [InlineData("AsyncCollectorEvent", "0 1 2 3 4 5 6")]
        [InlineData("StringEvents", "0 1 2 3 4")]
        [InlineData("JObjectEvents", "0 1 2 3 4")]
        public async Task OutputBindingParamsTests(string functionName, string expectedCollection)
        {
            List<EventGridEvent> output = new List<EventGridEvent>();

            Func<EventGridAttribute, IAsyncCollector<EventGridEvent>> customConverter = (attr =>
            {
                var mockClient = new Mock<IEventGridClient>();
                mockClient.Setup(x => x.PublishEventsWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<IList<EventGridEvent>>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                      .Returns((string topicHostname, IList<EventGridEvent> events, Dictionary<string, List<string>> customHeaders, CancellationToken cancel) =>
                      {
                          foreach (EventGridEvent eve in events)
                          {
                              output.Add(eve);
                          }
                          return Task.FromResult(new AzureOperationResponse());
                      });
                return new EventGridAsyncCollector(mockClient.Object, attr.TopicEndpointUri);
            });
            // use moq eventgridclient for test extension
            var customExtension = new EventGridExtensionConfig(customConverter);

            var nameResolverMock = new Mock<INameResolver>();
            nameResolverMock.Setup(x => x.Resolve("eventgridUri")).Returns("https://pccode.westus2-1.eventgrid.azure.net/api/events");
            nameResolverMock.Setup(x => x.Resolve("eventgridKey")).Returns("thisismagic");

            var host = TestHelpers.NewHost<OutputBindingParams>(customExtension, nameResolverMock.Object);

            await host.CallAsync($"OutputBindingParams.{functionName}");

            var expectedEvents = new HashSet<string>(expectedCollection.Split(' '));
            foreach (EventGridEvent eve in output)
            {
                Assert.True(expectedEvents.Remove((string)eve.Data));
            }
            Assert.True(expectedEvents.Count == 0);
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

        public class OutputBindingParams
        {
            public void SingleEvent([EventGrid(TopicEndpointUri = "eventgridUri", TopicKeySetting = "eventgridKey")] out EventGridEvent single)
            {
                single = new EventGridEvent()
                {
                    Data = "0"
                };
            }

            [return: EventGrid(TopicEndpointUri = "eventgridUri", TopicKeySetting = "eventgridKey")]
            public EventGridEvent SingleReturnEvent()
            {
                return new EventGridEvent()
                {
                    Data = "0"
                };
            }

            public void ArrayEvent([EventGrid(TopicEndpointUri = "eventgridUri", TopicKeySetting = "eventgridKey")] out EventGridEvent[] array)
            {
                array = new EventGridEvent[5];
                for (int i = 0; i < 5; i++)
                {
                    array[i] = new EventGridEvent()
                    {
                        Data = i.ToString()
                    };
                }
            }

            public void CollectorEvent([EventGrid(TopicEndpointUri = "eventgridUri", TopicKeySetting = "eventgridKey")] ICollector<EventGridEvent> collector)
            {
                for (int i = 0; i < 4; i++)
                {
                    collector.Add(new EventGridEvent()
                    {
                        Data = i.ToString()
                    });
                }
            }

            public async Task AsyncCollectorEvent([EventGrid(TopicEndpointUri = "eventgridUri", TopicKeySetting = "eventgridKey")] IAsyncCollector<EventGridEvent> asyncCollector)
            {
                for (int i = 0; i < 7; i++)
                {
                    await asyncCollector.AddAsync(new EventGridEvent()
                    {
                        Data = i.ToString()
                    });

                    if (i % 3 == 0)
                    {
                        // flush mulitple times, test whether the internal buffer is cleared
                        await asyncCollector.FlushAsync();
                    }
                }
            }

            // assume converter is applied correctly with other output binding types
            public void StringEvents([EventGrid(TopicEndpointUri = "eventgridUri", TopicKeySetting = "eventgridKey")] out string[] strings)
            {
                strings = new string[5];
                for (int i = 0; i < 5; i++)
                {
                    strings[i] = $@"
                    {{
                        ""Data"" : ""{i}""
                    }}";
                }
            }

            // assume converter is applied correctly with other output binding types
            public void JObjectEvents([EventGrid(TopicEndpointUri = "eventgridUri", TopicKeySetting = "eventgridKey")] out JObject[] jobjects)
            {
                jobjects = new JObject[5];
                for (int i = 0; i < 5; i++)
                {
                    jobjects[i] = new JObject(new JProperty("Data", i.ToString()));
                }
            }
        }
    }
}
