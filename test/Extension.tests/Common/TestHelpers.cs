using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests
{
    public class TestHelpers
    {
        public static JobHost NewHost<T>(EventGridExtensionConfig ext = null, INameResolver nameResolver = null)
        {
            JobHostConfiguration config = new JobHostConfiguration();
            config.HostId = Guid.NewGuid().ToString("n");
            config.StorageConnectionString = null;
            config.DashboardConnectionString = null;
            config.TypeLocator = new FakeTypeLocator<T>();
            config.AddExtension(ext ?? new EventGridExtensionConfig());
            config.AddExtension(new TestExtensionConfig());
            config.LoggerFactory = new LoggerFactory();
            config.NameResolver = nameResolver ?? new DefaultNameResolver();
            var host = new JobHost(config);
            return host;
        }
    }
}
