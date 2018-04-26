using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    public sealed class EventGridOutputAsyncCollector : IAsyncCollector<EventGridMessage>
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private EventGridOutputAttribute _attribute;

        public EventGridOutputAsyncCollector(EventGridOutputAttribute attr)
        {
            _attribute = attr;
        }

        public async Task AddAsync(EventGridMessage item, CancellationToken cancellationToken = default(CancellationToken))
        {
            // re/set sas-key value to match attribute
            if (_httpClient.DefaultRequestHeaders.Contains(@"aeg-sas-key"))
                _httpClient.DefaultRequestHeaders.Remove(@"aeg-sas-key");
            _httpClient.DefaultRequestHeaders.Add(@"aeg-sas-key", _attribute.SasKey);

            // event grid posts are required to be in a JSON Array
            var requestBody = new JArray(JObject.FromObject(item));

            var response = await _httpClient.PostAsync(_attribute.TopicEndpoint, new StringContent(requestBody.ToString(), System.Text.Encoding.Default, @"application/json"));
            response.EnsureSuccessStatusCode();
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // do nothing; we are sending every request as they come in to AddAsync so there's nothing to flush
            return Task.CompletedTask;
        }
    }
}