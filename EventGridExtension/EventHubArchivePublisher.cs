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
            var _contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            if (t == typeof(EventGridEvent))
            {
                _contract.Add("EventGridTrigger", t);
            }
            else if (t == typeof(Stream))
            {
                _contract.Add("EventGridTrigger", t);
                _contract.Add("name", typeof(string));
            }
            else if (t == typeof(string))
            {
                _contract.Add("EventGridTrigger", t);
                _contract.Add("name", typeof(string));
            }
            else
            {
                return null;
            }
            return _contract;

        }
        public Dictionary<string, object> ExtractBindingData(EventGridEvent e, Type t)
        {
            var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (t == typeof(EventGridEvent))
            {
                bindingData.Add("EventGridTrigger", e);
            }
            else if (t == typeof(Stream))
            {
                StorageBlob data = e.Data.ToObject<StorageBlob>();

                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(data.fileUrl);

                _recycles = new List<IDisposable>();
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                Stream responseStream = myHttpWebResponse.GetResponseStream();
                _recycles.Add(responseStream);
                _recycles.Add(myHttpWebResponse);

                bindingData.Add("EventGridTrigger", responseStream);
                bindingData.Add("name", data.fileUrl.LocalPath);
            }
            else if (t == typeof(string))
            {
                // XXX read blob may require a lot of memory consumption
                StorageBlob data = e.Data.ToObject<StorageBlob>();
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(data.fileUrl);
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
                {
                    using (StreamReader responseStream = new StreamReader(myHttpWebResponse.GetResponseStream()))
                    {
                        string blobData = responseStream.ReadToEnd();
                        bindingData.Add("EventGridTrigger", blobData);
                        bindingData.Add("name", data.fileUrl.LocalPath);
                    }
                }
            }
            else
            {
                return null;
            }
            return bindingData;
        }

        public object GetArgument(Dictionary<string, object> bindingData)
        {
            return bindingData["EventGridTrigger"];
        }
    }
}
