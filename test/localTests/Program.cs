using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;

namespace EventGridBinding
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var ext = new EventGridExtensionConfig();
            config.AddExtension(ext);

            //config.TypeLocator = new ;
            //config.JobActivator = new ;

            Function prog = new Function();

            JobHost host = new JobHost(config);
            host.RunAndBlock();

            
        }
    }
}
