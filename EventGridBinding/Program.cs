using Microsoft.Azure.WebJobs;

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

            config.UseEventGrid();

            JobHost host = new JobHost(config);
            host.RunAndBlock();
        }
    }
}
