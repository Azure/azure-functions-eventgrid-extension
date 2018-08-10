using System;
using Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Config;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests
{
    internal static class TestHelpers
    {
        public static IHost NewHost<T>(EventGridExtensionConfigProvider ext = null, INameResolver nameResolver = null)
        {
            IHost host = new HostBuilder()
           .ConfigureServices(services =>
           {
               services.AddSingleton(nameResolver ?? new DefaultNameResolver());
               services.AddSingleton<ITypeLocator>(new FakeTypeLocator<T>());
               if (ext != null)
               {
                   services.AddSingleton<IExtensionConfigProvider>(ext);
               }
               services.AddSingleton<IExtensionConfigProvider>(new TestExtensionConfig());
           })
           .ConfigureWebJobs(builder =>
           {
               builder.AddEventGrid();
               builder.UseHostId(Guid.NewGuid().ToString("n"));
           })
           .ConfigureLogging(logging =>
           {
               logging.ClearProviders();
               logging.AddProvider(new TestLoggerProvider());
           })
           .Build();

            return host;
        }

        public static JobHost GetJobHost(this IHost host)
        {
            return host.Services.GetService<IJobHost>() as JobHost;
        }
    }
}