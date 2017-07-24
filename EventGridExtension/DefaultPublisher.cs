using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs
{
    public class DefaultPublisher : IPublisher
    {
        public const string Name = "DefaultPublisher";

        public string PublisherName
        {
            get { return Name; }
        }

        public List<IDisposable> Recycles
        {
            get { return null; }
        }

        public Dictionary<string, Type> ExtractBindingContract(Type t)
        {
            if (t == typeof(EventGridEvent) || t == typeof(string))
            {
                var contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                // for javascript, 1st attempt is to return JSON string of EventGridEvent
                contract.Add("EventGridTrigger", t);
                return contract;
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, object> ExtractBindingData(EventGridEvent e, Type t)
        {
            var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (t == typeof(EventGridEvent))
            {
                bindingData.Add("EventGridTrigger", e);
            }
            else if (t == typeof(string))
            {
                bindingData.Add("EventGridTrigger", JsonConvert.SerializeObject(e, Formatting.Indented));
            }

            return bindingData;
        }

        public object GetArgument(Dictionary<string, object> bindingData)
        {
            return bindingData["EventGridTrigger"];
        }

    }
}
