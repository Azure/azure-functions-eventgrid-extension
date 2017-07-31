using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    public class EventHubCapturePublisher : IPublisher
    {
        public const string Name = "eventHubCapture";
        private List<IDisposable> _recycles = null;
        private StorageCredentials _credentials;
        public EventHubCapturePublisher(string connectionStringName)
        {
            if (String.IsNullOrEmpty(connectionStringName))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                   "Can't bind EventGridTriggerAttribute with publisher '{0}': missing ConnectionString for Storageblob.", Name));
            }
            var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(connectionStringName);
            _credentials = CloudStorageAccount.Parse(connectionString).Credentials;
        }

        public string PublisherName
        {
            get { return Name; }
        }

        public List<IDisposable> Recycles
        {
            get { return _recycles; }
        }

        public Dictionary<string, Type> ExtractBindingContract(Type t)
        {
            var contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            // TODO we can determine the ACTION in this function, so that when calling ExtractBindingData, we don't have to do the comparison again
            if (t == typeof(EventGridEvent))
            {
                contract.Add("EventGridTrigger", t);
            }
            else if (t == typeof(Stream) || t == typeof(string) || t == typeof(CloudBlob) || t == typeof(byte[]))
            {
                contract.Add("EventGridTrigger", t);
                contract.Add("BlobTrigger", typeof(string));
                contract.Add("Uri", typeof(Uri));
                contract.Add("Properties", typeof(BlobProperties));
                contract.Add("Metadata", typeof(IDictionary<string, string>));
            }
            else
            {
                // fail
                return null;
            }
            return contract;

        }

        public async Task<Dictionary<string, object>> ExtractBindingData(EventGridEvent e, Type t)
        {
            var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (t == typeof(EventGridEvent))
            {
                bindingData.Add("EventGridTrigger", e);
            }
            else
            {
                StorageBlob data = e.Data.ToObject<StorageBlob>();
                var blob = new CloudBlob(data.FileUrl, _credentials);
                // set metadata based on https://github.com/MicrosoftDocs/azure-docs/blob/master/articles/azure-functions/functions-bindings-storage-blob.md#trigger-metadata
                //BlobTrigger.Type string.The triggering blob path
                bindingData.Add("BlobTrigger", blob.Container.Name + "/" + blob.Name);
                //Uri.Type System.Uri.The blob's URI for the primary location.
                bindingData.Add("Uri", blob.Uri);
                //Properties.Type Microsoft.WindowsAzure.Storage.Blob.BlobProperties.The blob's system properties.
                bindingData.Add("Properties", blob.Properties);
                //Metadata.Type IDictionary<string, string>.The user - defined metadata for the blob
                bindingData.Add("Metadata", blob.Metadata);
                // [Blob("output/copy-{name}")] out string output, does not apply here
                // bindingData.Add("name", blob.Name);
                if (t == typeof(CloudBlob))
                {
                    bindingData.Add("EventGridTrigger", blob);
                }
                else
                {
                    // convert from stream 
                    var blobStream = await blob.OpenReadAsync();
                    if (t == typeof(Stream))
                    {
                        _recycles = new List<IDisposable>();
                        _recycles.Add(blobStream); // close after function call
                        bindingData.Add("EventGridTrigger", blobStream);
                    }
                    // copy to memory => use case javascript
                    else if (t == typeof(Byte[]))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            await blobStream.CopyToAsync(ms);
                            bindingData.Add("EventGridTrigger", ms.ToArray());
                        }
                        blobStream.Close(); // close before the function call
                    }
                    else if (t == typeof(string))
                    {
                        using (StreamReader responseStream = new StreamReader(blobStream))
                        {
                            string blobData = await responseStream.ReadToEndAsync();
                            bindingData.Add("EventGridTrigger", blobData);
                        }
                        blobStream.Close(); // close before the function call
                    }
                }
            }
            return bindingData;
        }

        public object GetArgument(Dictionary<string, object> bindingData)
        {
            return bindingData["EventGridTrigger"];
        }
    }
}
