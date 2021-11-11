using System;
using EPiServer.Events;
using EPiServer.Events.MassTransit;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Serialization;
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
                cfg.ReceiveEndpoint(MassTransitEventProvider.UniqueServerName, e =>
                {
                    e.ClearMessageDeserializers();
                    e.UseDataContractBinarySerializer(context.GetService<EventsServiceKnownTypesLookup>());
                    e.AutoDelete = true;
                    e.PrefetchCount = options.PrefetchCount;
                    e.Durable = false;
                    e.Bind(options.ExchangeName, x =>
                    {
                        x.Durable = true;
                        x.AutoDelete = false;
                    });
                    e.UseConsumeFilter(typeof(SiteConsumeFilter<>), context);
                    e.Consumer<SiteEventsConsumer>();
                });
            });
        }

        /// <summary>
        /// Adds data contract binary serializer
        /// </summary>
        /// <param name="configurator">The configurator</param>
        /// <param name="typesLookup">The types lookup</param>
        public static void UseDataContractBinarySerializer(this IBusFactoryConfigurator configurator, EventsServiceKnownTypesLookup typesLookup)
        {
            var serializer = new DataContractBinarySerializer(typesLookup);
            configurator.AddMessageDeserializer(JsonMessageSerializer.JsonContentType, () => serializer);
            configurator.SetMessageSerializer(() => serializer);
        }

        /// <summary>
        /// Adds data contract binary serializer
        /// </summary>
        /// <param name="configurator">The configurator</param>
        /// <param name="typesLookup">The types lookup</param>
        public static void UseDataContractBinarySerializer(this IReceiveEndpointConfigurator configurator, EventsServiceKnownTypesLookup typesLookup)
        {
            var serializer = new DataContractBinarySerializer(typesLookup);
            configurator.AddMessageDeserializer(JsonMessageSerializer.JsonContentType, () => serializer);
            configurator.SetMessageSerializer(() => serializer);
        }
    }
}
