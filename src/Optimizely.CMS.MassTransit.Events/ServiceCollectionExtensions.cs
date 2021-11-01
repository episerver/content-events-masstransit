using System;
using EPiServer.Events;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Optimizely.CMS.MassTransit.Events;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// ss
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure azure event provider
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configureOptions">Optional action to configure blob provider</param>
        public static IServiceCollection AddMassTransitEventProvider(this IServiceCollection services,
            Action<MassTransitEventProviderOptions> configureOptions = null,
            Action<IServiceCollectionBusConfigurator> configureBus = null)
        {
            services.AddEventProvider<MassTransitEventProvider>();

            if (configureOptions is not null)
            {
                services.Configure(configureOptions);
            }

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<MassTransitEventProviderOptions>, MassTransitEventProviderOptionsConfigurer>());

            services.AddMassTransit(x => configureBus?.Invoke(x));

            services.AddMassTransitHostedService();
            return services;
        }

        public static void AddRabbitMqTransport(this IBusRegistrationConfigurator busRegistrationConfigurator)
        {
            busRegistrationConfigurator.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetService<MassTransitEventProviderOptions>();
                cfg.Host(new Uri(options.ConnectionString));
                cfg.Message<EventMessage>(x => x.SetEntityName(options.ExchangeName));
                cfg.ReceiveEndpoint(options.QueueName, e =>
                {
                    e.Bind(options.ExchangeName);
                    e.Consumer<SiteEventsConsumer>();
                });
            });
        }
    }
}
