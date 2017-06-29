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

        public Dictionary<string, Type> ExtractBindingContract(Type t)
        {
            if (t == typeof(EventGridEvent))
            {
                var _contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                _contract.Add("EventGridTrigger", t);
                return _contract;
            }
            return null;
        }

        public Dictionary<string, object> ExtractBindingData(EventGridEvent e, Type t)
        {
            var _bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _bindingData.Add("EventGridTrigger", e);
            return _bindingData;
        }

        public object GetArgument(Dictionary<string, object> bindingData)
        {
            return bindingData["EventGridTrigger"];
        }

    }
}
