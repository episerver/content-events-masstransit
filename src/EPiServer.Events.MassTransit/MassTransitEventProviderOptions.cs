using EPiServer.ServiceLocation;

namespace EPiServer.Events.MassTransit
{
    /// <summary>
    /// Options for configuring a <see cref="MassTransitEventProvider"/>.
    /// </summary>
    [Options(ConfigurationSection = ConfigurationSectionConstants.Cms)]
    public class MassTransitEventProviderOptions
    {
        /// <summary>
        /// The connection string that should be used to connect to the ServiceBus.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The exchange name that should be used. Must adhere to RabbitMQ limitations.
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// The pre-fetch count for processing message..
        /// </summary>
        public ushort PrefetchCount { get; set; } = 100;

        /// <summary>
        /// Name of provider.
        /// </summary>
        public string Name { get; set; } = "MassTransit";
    }
}
