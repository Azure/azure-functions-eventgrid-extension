using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Microsoft.Azure.WebJobs
{
    public class EventHubArchivePublisher : IPublisher
    {
        public const string Name = "eventHubArchive";
        private List<IDisposable> _recycles = null;

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
            if (t == typeof(EventGridEvent))
            {
                contract.Add("EventGridTrigger", t);
            }
            else if (t == typeof(Stream) || t == typeof(string) || t == typeof(CloudBlob))
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
        public Dictionary<string, object> ExtractBindingData(EventGridEvent e, Type t)
        {
            var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (t == typeof(EventGridEvent))
            {
                bindingData.Add("EventGridTrigger", e);
            }
            else
            {
                StorageBlob data = e.Data.ToObject<StorageBlob>();
                var blob = new CloudBlob(data.fileUrl);
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

                if (t == typeof(Stream))
                {
                    HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(data.fileUrl);

                    _recycles = new List<IDisposable>();
                    // SHUN TODO async
                    HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                    Stream responseStream = myHttpWebResponse.GetResponseStream();
                    _recycles.Add(responseStream);
                    _recycles.Add(myHttpWebResponse);

                    bindingData.Add("EventGridTrigger", responseStream);
                }
                else if (t == typeof(string))
                {
                    // read all to buffer
                    HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(data.fileUrl);
                    using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
                    {
                        using (StreamReader responseStream = new StreamReader(myHttpWebResponse.GetResponseStream()))
                        {
                            string blobData = responseStream.ReadToEnd();
                            bindingData.Add("EventGridTrigger", blobData);
                        }
                    }
                }
                else if (t == typeof(CloudBlob))
                {
                    bindingData.Add("EventGridTrigger", blob);
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
