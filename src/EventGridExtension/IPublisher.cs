using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid
{
    public interface IPublisher
    {
        string PublisherName { get; }

        List<IDisposable> Recycles { get; }

        // this method needs to filter invalid datatype
        // return null
        Dictionary<string, Type> ExtractBindingContract(Type t);

        Task<Dictionary<string, object>> ExtractBindingData(EventGridEvent e, Type t);

        object GetArgument(Dictionary<string, object> bindingData);
    }
}
