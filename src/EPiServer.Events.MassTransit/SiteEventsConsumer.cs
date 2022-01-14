using System;
using System.Threading.Tasks;
using EPiServer.ServiceLocation;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EPiServer.Events.MassTransit
{
    /// <summary>
    /// Consumer for EventMessage on the bus
    /// </summary>
    public class SiteEventsConsumer : IConsumer<EventMessage>
    {
        private readonly ILogger _logger;
        private readonly MassTransitEventProvider _massTransitEventProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteEventsConsumer"/> class.
        /// </summary>
        public SiteEventsConsumer()
        {
            _logger = ServiceLocator.Current.GetInstance<ILogger<SiteEventsConsumer>>();
            _massTransitEventProvider = ServiceLocator.Current.GetInstance<MassTransitEventProvider>();
        }

        /// <summary>
        /// Handles when a message is received from the bus.
        /// </summary>
        /// <param name="context">The conetxt</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task Consume(ConsumeContext<EventMessage> context)
        {
            try
            {
                if (context.Headers.Get<string>("AppId") != MassTransitEventProvider.UniqueServerName)
                {
                    _massTransitEventProvider.RaiseOnMessageReceived(new EventMessageEventArgs(context.Message));
                    await Task.CompletedTask.ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed deserialize event", e);
            }
        }
    }
}
