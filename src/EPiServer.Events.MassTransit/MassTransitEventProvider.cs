using System;
using System.Threading.Tasks;
using MassTransit;

namespace EPiServer.Events.MassTransit
{
    /// <summary>
    /// <see cref="Providers.EventProvider"/> that uses MassTransit for exchanging messages between sites.
    /// </summary>
    public class MassTransitEventProvider : Providers.EventProvider
    {
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitEventProvider"/> class.
        /// </summary>
        /// <param name="options">The options</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        public MassTransitEventProvider(MassTransitEventProviderOptions options,
            IPublishEndpoint publishEndpoint)
        {
            Name = options.Name;
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// Unique name for the queue name.
        /// </summary>
        public static string UniqueServerName { get; } = Environment.MachineName.Replace('/', '_').Replace(':', '_') + Guid.NewGuid().ToString("N");

        /// <inheritdoc/>
        public override bool ValidateMessageIntegrity => false;

        /// <summary>
        /// Method to raise OnMessageReeceived event.
        /// </summary>
        /// <param name="messageEventArgs">The message event args.</param>
        public void RaiseOnMessageReceived(EventMessageEventArgs messageEventArgs) => OnMessageReceived(messageEventArgs);

        /// <inheritdoc/>
        public override Task InitializeAsync()
        {

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void Uninitialize()
        {

        }

        /// <inheritdoc/>
        public override void SendMessage(EventMessage message)
        {
            _publishEndpoint.Publish(message, (msg) =>
            {
                msg.TimeToLive = TimeSpan.FromMinutes(30);
                msg.Headers.Set("AppId", UniqueServerName);
            });
        }
    }
}
