using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Optimizely.CMS.MassTransit.Events
{
    internal class MassTransitEventProviderOptionsConfigurer : IPostConfigureOptions<MassTransitEventProviderOptions>
    {
        private readonly IConfiguration _configuration;

        public MassTransitEventProviderOptionsConfigurer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void PostConfigure(string name, MassTransitEventProviderOptions options)
        {
            //We want connection string to take precence if it exist
            var connectionString = _configuration.GetConnectionString("OptimizelyMassTransitEvents");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.ConnectionString = connectionString;
            }

            if (string.IsNullOrWhiteSpace(options.ExchangeName))
            {
                options.ExchangeName = "optimizely.fanout.siteevents";
            }

            if (string.IsNullOrWhiteSpace(options.QueueName))
            {
                options.QueueName = "optimizely.queues.siteevents";
            }
        }
    }
}
