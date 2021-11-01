using System;
using System.Text;
using System.Threading;
using EPiServer.Events;
using GreenPipes;
using MassTransit;
using Moq;
using Xunit;

namespace Optimizely.CMS.MassTransit.Events.Tests
{
    public class MassTransitEventProviderTests
    {
        private readonly Mock<IPublishEndpoint> _publishEndpoint;
        private readonly MassTransitEventProviderOptions _massTransitEventProviderOptions;
        private readonly MassTransitEventProvider _subject;

        public MassTransitEventProviderTests()
        {
            _publishEndpoint = new Mock<IPublishEndpoint>();
            _massTransitEventProviderOptions = new MassTransitEventProviderOptions
            {
                ExchangeName = "exchange",
                QueueName = "queue"
            };
            _subject = new MassTransitEventProvider(_massTransitEventProviderOptions, _publishEndpoint.Object);
        }

        [Fact]
        public void SendMessage_ShouldSend()
        {
            _subject.SendMessage(new EventMessage
            {
                ApplicationName = "test",
                EventId = Guid.NewGuid(),
                Parameter = "parameter",
                RaiserId = Guid.NewGuid(),
                Sent = DateTime.UtcNow,
                SequenceNumber = 123,
                ServerName = "test",
                SiteId = "sdd",
                VerificationData = Encoding.UTF8.GetBytes("test")
            });
            _publishEndpoint.Verify(x => x.Publish(It.IsAny<EventMessage>(), It.IsAny<IPipe<PublishContext<EventMessage>>>(), It.IsAny<CancellationToken>()));
        }
    }
}
