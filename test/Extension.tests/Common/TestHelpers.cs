using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests
{
    public class TestHelpers
    {
        public static JobHost NewHost<T>(EventGridExtensionConfig ext = null)
        {
            JobHostConfiguration config = new JobHostConfiguration();
            config.HostId = Guid.NewGuid().ToString("n");
            config.StorageConnectionString = null;
            config.DashboardConnectionString = null;
            config.TypeLocator = new FakeTypeLocator<T>();
            config.AddExtension(ext ?? new EventGridExtensionConfig());
            config.AddExtension(new TestExtensionConfig());
            var host = new JobHost(config);
            return host;
        }
    }
}
