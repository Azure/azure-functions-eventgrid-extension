using System;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs
{
    public interface IPublisher
    {
        string PublisherName { get; }

        List<IDisposable> Recycles { get; }

        Dictionary<string, Type> ExtractBindingContract(Type t);

        Dictionary<string, object> ExtractBindingData(EventGridEvent e, Type t);

        object GetArgument(Dictionary<string, object> bindingData);
    }
}
