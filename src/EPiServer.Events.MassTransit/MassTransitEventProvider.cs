using System;
using System.Threading.Tasks;
using EPiServer.Events.Clients;
using MassTransit;

namespace EPiServer.Events.MassTransit
{
    /// <summary>
    /// <see cref="Providers.EventProvider"/> that uses MassTransit for exchanging messages between sites.
    /// </summary>
    public class MassTransitEventProvider : Providers.EventProvider
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly DataContractBinarySerializer _dataContractBinarySerializer;
        private readonly IEventRegistry _eventRegistry;
        private readonly Guid _raiserId = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitEventProvider"/> class.
        /// </summary>
        /// <param name="options">The options</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        /// <param name="dataContractBinarySerializer">The serializer</param>
        /// <param name="eventRegistry"></param>
        public MassTransitEventProvider(MassTransitEventProviderOptions options,
            IPublishEndpoint publishEndpoint,
            DataContractBinarySerializer dataContractBinarySerializer,
            IEventRegistry eventRegistry)
        {
            Name = options.Name;
            _publishEndpoint = publishEndpoint;
            _dataContractBinarySerializer = dataContractBinarySerializer;
            _eventRegistry = eventRegistry;
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
        public void RaiseOnMessageReceived(EventMessageEventArgs messageEventArgs)
        {
            var ev = _eventRegistry.Get(messageEventArgs.Message.EventId);
            ev?.Raise(_raiserId, messageEventArgs.Message.Parameter, EventRaiseOption.RaiseSite);
        }

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
                msg.Serializer = _dataContractBinarySerializer;
                msg.Headers.Set("AppId", UniqueServerName);
            });
        }
    }
}
