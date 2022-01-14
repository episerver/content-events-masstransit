using System;
using System.Net.Mime;
using EPiServer.Events;
using EPiServer.Events.MassTransit;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configures MassTransit event provider and services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Default episerver content type
        /// </summary>
        public static readonly ContentType ContentType = new("application/vnd.masstransit+episerver");

        /// <summary>
        /// Configure azure event provider
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configureOptions">Optional action to configure blob provider</param>
        /// <param name="configureBus">Optional action to configure the bus</param>
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
            services.AddSingleton<DataContractBinarySerializer>();
            services.AddMassTransit(x => configureBus?.Invoke(x));
            services.AddMassTransitHostedService();
            return services;
        }

        /// <summary>
        /// Adds MQ transport to the event provider.
        /// </summary>
        /// <param name="busRegistrationConfigurator"></param>
        public static void AddRabbitMqTransport(this IBusRegistrationConfigurator busRegistrationConfigurator)
        {
            busRegistrationConfigurator.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetService<MassTransitEventProviderOptions>();
                cfg.Host(new Uri(options.ConnectionString));
                cfg.Message<EventMessage>(x => x.SetEntityName(options.ExchangeName));
                cfg.ReceiveEndpoint(new TemporaryEndpointDefinition(prefetchCount:options.PrefetchCount), e =>
                {
                    e.Consumer<SiteEventsConsumer>();
                    e.UseDataContractBinarySerializer(context.GetService<DataContractBinarySerializer>());
                    e.AutoDelete = true;
                    e.PrefetchCount = options.PrefetchCount;
                    e.Durable = false;
                    e.Bind(options.ExchangeName, x =>
                    {
                        x.Durable = true;
                        x.AutoDelete = false;
                    });
                });
            });
        }

        /// <summary>
        /// Adds data contract binary serializer
        /// </summary>
        /// <param name="configurator">The configurator</param>
        /// <param name="dataContractBinarySerializer">The dataContractBinarySerializer</param>
        public static void UseDataContractBinarySerializer(this IBusFactoryConfigurator configurator, DataContractBinarySerializer dataContractBinarySerializer)
        {
            configurator.AddMessageDeserializer(ContentType, () => dataContractBinarySerializer);
            configurator.SetMessageSerializer(() => dataContractBinarySerializer);
        }

        /// <summary>
        /// Adds data contract binary serializer
        /// </summary>
        /// <param name="configurator">The configurator</param>
        /// <param name="dataContractBinarySerializer">The dataContractBinarySerializer</param>
        public static void UseDataContractBinarySerializer(this IReceiveEndpointConfigurator configurator, DataContractBinarySerializer dataContractBinarySerializer)
        {
            configurator.AddMessageDeserializer(ContentType, () => dataContractBinarySerializer);
            configurator.SetMessageSerializer(() => dataContractBinarySerializer);
        }
    }
}
