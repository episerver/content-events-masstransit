using System;
using System.Threading.Tasks;
using EPiServer.Events;
using EPiServer.ServiceLocation;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Optimizely.CMS.MassTransit.Events
{
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

        public async Task Consume(ConsumeContext<EventMessage> context)
        {
            try
            {
                if (context.Headers.Get<string>("AppId") == MassTransitEventProvider.UniqueServerName)
                {
                    _logger.LogDebug("Message processor received it's own message, message will be ignored.");
                    return;
                }

                _massTransitEventProvider.RaiseOnMessageReceived(new EventMessageEventArgs(context.Message));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed deserialize event", e);
                await Task.FromException(e);
            }
        }
    }
}
