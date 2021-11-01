using System;
using System.Threading.Tasks;
using EPiServer.Events;
using MassTransit;

namespace Optimizely.CMS.MassTransit.Events
{
    /// <summary>
    /// <see cref="EPiServer.Events.Providers.EventProvider"/> that uses Azure ServiceBus for exchanging messages between sites.
    /// </summary>
    public class MassTransitEventProvider : EPiServer.Events.Providers.EventProvider
    {
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitEventProvider"/> class.
        /// </summary>
        public MassTransitEventProvider(MassTransitEventProviderOptions options,
            IPublishEndpoint publishEndpoint)
        {
            Name = options.Name;
            _publishEndpoint = publishEndpoint;
        }

        public static string UniqueServerName { get; } = Environment.MachineName.Replace('/', '_').Replace(':', '_') + Guid.NewGuid().ToString("N");

        /// <inheritdoc/>
        public override bool ValidateMessageIntegrity => false;

        public void RaiseOnMessageReceived(EventMessageEventArgs messageEventArgs) => OnMessageReceived(messageEventArgs);

        /// <inheritdoc/>
        public override Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void Uninitialize() { }

        /// <inheritdoc/>
        public override void SendMessage(EventMessage message)
        {
            _publishEndpoint.Publish(message, (msg) =>
            {
                msg.Headers.Set("AppId", UniqueServerName);
            });
        }
    }
}
