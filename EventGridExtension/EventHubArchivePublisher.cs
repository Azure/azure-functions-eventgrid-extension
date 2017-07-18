using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Microsoft.Azure.WebJobs
{
    public class EventHubArchivePublisher : IPublisher
    {
        public const string Name = "eventHubArchive";
        public string PublisherName
        {
            get { return Name; }
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
                //not necessary since we don't always use the content of the stream
                var byteStream = new MemoryStream();
                StorageBlob data = e.Data.ToObject<StorageBlob>();

                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(data.fileUrl);
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
                {
                    using (Stream responseStream = myHttpWebResponse.GetResponseStream())
                    {
                        var buffer = new byte[4096];
                        var bytesRead = 0;
                        while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            byteStream.Write(buffer, 0, bytesRead);
                        }
                        byteStream.Position = 0;
                        bindingData.Add("EventGridTrigger", byteStream);
                        bindingData.Add("name", data.fileUrl.LocalPath);
                    }
                }
            }
            else if (t == typeof(string))
            {
                // TODO read blob requires a lot of memory consumption
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
