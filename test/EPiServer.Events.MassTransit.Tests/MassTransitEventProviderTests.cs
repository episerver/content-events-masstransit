using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EPiServer.Events.Clients;
using EPiServer.Events.Clients.Internal;
using EPiServer.Framework.TypeScanner;
using EPiServer.ServiceLocation;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EPiServer.Events.MassTransit.Tests
{
    public class MassTransitEventProviderTests : IDisposable
    {
        private readonly InMemoryTestHarness _testHarness;
        private readonly ConsumerTestHarness<SiteEventsConsumer> _siteEventsConsumer;
        private readonly DataContractBinarySerializer _dataContractBinarySerializer;

        public MassTransitEventProviderTests()
        {
            var scanner = new Mock<ITypeScannerLookup>();
            scanner.Setup(c => c.AllTypes).Returns(new List<Type> { typeof(StateMessage) });
            _dataContractBinarySerializer = new DataContractBinarySerializer(new EventsServiceKnownTypesLookup(scanner.Object));
            var services = new ServiceCollection()
                .AddSingleton(new Mock<ILogger<SiteEventsConsumer>>().Object)
                .AddSingleton(new MassTransitEventProviderOptions
                {
                    ExchangeName = "myexchange"
                })
                .AddSingleton<MassTransitEventProvider>()
                .AddSingleton(_dataContractBinarySerializer)
                .AddSingleton(new Mock<IPublishEndpoint>().Object)
                .AddSingleton(new Mock<IEventRegistry>().Object);
            ServiceLocator.SetScopedServiceProvider(services.BuildServiceProvider());
            _testHarness = new InMemoryTestHarness();
            _testHarness.OnConfigureInMemoryBus += x =>
            {
                x.ClearMessageDeserializers();
                x.UseDataContractBinarySerializer(_dataContractBinarySerializer);
            };
            _siteEventsConsumer = _testHarness.Consumer<SiteEventsConsumer>(() => new SiteEventsConsumer(new Mock<ILogger<SiteEventsConsumer>>().Object, ServiceLocator.Current.GetInstance<MassTransitEventProvider>()), MassTransitEventProvider.UniqueServerName);
            _testHarness.Start().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task SendMessageWithStringParameter_ShouldSend()
        {
            await _testHarness.Bus.Publish(new EventMessage
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
            }).ConfigureAwait(false);

            Thread.Sleep(2000);
            var receivedMessage = _siteEventsConsumer.Consumed.Select<EventMessage>().FirstOrDefault();
            Assert.Equal("test", receivedMessage.Context.Message.ApplicationName);
            Assert.Equal("parameter", receivedMessage.Context.Message.Parameter);
            Assert.Equal(123, receivedMessage.Context.Message.SequenceNumber);
            Assert.Equal("test", receivedMessage.Context.Message.ServerName);
            Assert.Equal("sdd", receivedMessage.Context.Message.SiteId);
            Assert.Equal(Encoding.UTF8.GetBytes("test"), receivedMessage.Context.Message.VerificationData);
        }

        [Fact]
        public async Task SendMessageWithBinaryParameter_ShouldSend()
        {
            var eventArgs = new CatalogContentUpdateEventArgs
            {
                ApplicationHasContentModelTypes = false,
                CatalogAssociationIds = new List<int>() { 32, 36, 24 },
                CatalogEntryIds = new List<int>() { 32, 36, 24 },
                CatalogIds = new List<int>() { 32, 36, 24 },
                CatalogNodeIds = new List<int>() { 32, 36, 24 },
                HasChangedParent = false,
                EventType = "Test Event"
            };

            var bytes = DoSerialize(eventArgs);
            await _testHarness.Bus.Publish(new EventMessage
            {
                ApplicationName = "test",
                EventId = Guid.NewGuid(),
                Parameter = bytes,
                RaiserId = Guid.NewGuid(),
                Sent = DateTime.UtcNow,
                SequenceNumber = 123,
                ServerName = "test",
                SiteId = "sdd",
                VerificationData = Encoding.UTF8.GetBytes("test")
            }).ConfigureAwait(false);

            Thread.Sleep(2000);
            var receivedMessage = _siteEventsConsumer.Consumed.Select<EventMessage>().FirstOrDefault();
            eventArgs = DoDeserialize<CatalogContentUpdateEventArgs>(receivedMessage.Context.Message.Parameter as byte[]);
            Assert.Equal("test", receivedMessage.Context.Message.ApplicationName);
            Assert.Equal(123, receivedMessage.Context.Message.SequenceNumber);
            Assert.Equal("test", receivedMessage.Context.Message.ServerName);
            Assert.Equal("sdd", receivedMessage.Context.Message.SiteId);
            Assert.Equal(Encoding.UTF8.GetBytes("test"), receivedMessage.Context.Message.VerificationData);
            Assert.False(eventArgs.ApplicationHasContentModelTypes);
            Assert.Equal(new List<int>() { 32, 36, 24 }, eventArgs.CatalogAssociationIds);
            Assert.Equal(new List<int>() { 32, 36, 24 }, eventArgs.CatalogEntryIds);
            Assert.Equal(new List<int>() { 32, 36, 24 }, eventArgs.CatalogIds);
            Assert.Equal(new List<int>() { 32, 36, 24 }, eventArgs.CatalogNodeIds);
            Assert.Equal("Test Event", eventArgs.EventType);
            Assert.False(eventArgs.HasChangedParent);
        }

        [Fact]
        public async Task SendMessageWithGuidParameter_ShouldSend()
        {
            var guid = Guid.NewGuid();
            await _testHarness.Bus.Publish(new EventMessage
            {
                ApplicationName = "test",
                EventId = Guid.NewGuid(),
                Parameter = guid,
                RaiserId = Guid.NewGuid(),
                Sent = DateTime.UtcNow,
                SequenceNumber = 123,
                ServerName = "test",
                SiteId = "sdd",
                VerificationData = Encoding.UTF8.GetBytes("test")
            }).ConfigureAwait(false);

            Thread.Sleep(2000);
            var receivedMessage = _siteEventsConsumer.Consumed.Select<EventMessage>().FirstOrDefault();
            Assert.Equal("test", receivedMessage.Context.Message.ApplicationName);
            Assert.Equal(123, receivedMessage.Context.Message.SequenceNumber);
            Assert.Equal("test", receivedMessage.Context.Message.ServerName);
            Assert.Equal("sdd", receivedMessage.Context.Message.SiteId);
            Assert.Equal(Encoding.UTF8.GetBytes("test"), receivedMessage.Context.Message.VerificationData);
            Assert.Equal(guid, (Guid)receivedMessage.Context.Message.Parameter);
        }

        [Fact]
        public async Task SendMessageWithObjectParameter_ShouldSend()
        {
            var guid = Guid.NewGuid();
            await _testHarness.Bus.Publish(new EventMessage
            {
                ApplicationName = "test",
                EventId = Guid.NewGuid(),
                Parameter = new StateMessage
                {
                    ApplicationName = "test",
                    Sent = DateTime.UtcNow,
                    ServerName = "test",
                    Type = StateMessageType.Awesome
                },
                RaiserId = Guid.NewGuid(),
                Sent = DateTime.UtcNow,
                SequenceNumber = 123,
                ServerName = "test",
                SiteId = "sdd",
                VerificationData = Encoding.UTF8.GetBytes("test")
            }).ConfigureAwait(false);

            Thread.Sleep(2000);
            var receivedMessage = _siteEventsConsumer.Consumed.Select<EventMessage>().FirstOrDefault();
            var result = (StateMessage)receivedMessage.Context.Message.Parameter;
            Assert.Equal("test", receivedMessage.Context.Message.ApplicationName);
            Assert.Equal(123, receivedMessage.Context.Message.SequenceNumber);
            Assert.Equal("test", receivedMessage.Context.Message.ServerName);
            Assert.Equal("sdd", receivedMessage.Context.Message.SiteId);
            Assert.Equal(Encoding.UTF8.GetBytes("test"), receivedMessage.Context.Message.VerificationData);
            Assert.Equal("test", result.ApplicationName);
            Assert.Equal("test", result.ServerName);
            Assert.Equal(StateMessageType.Awesome, result.Type);
        }

        public void Dispose()
        {
            _testHarness?.Stop().GetAwaiter().GetResult();
            _testHarness?.Dispose();
            GC.SuppressFinalize(this);
        }

        private static byte[] DoSerialize<TItem>(TItem instance)
        {
            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream();
#pragma warning disable SYSLIB0011
            formatter.Serialize(stream, instance);
#pragma warning restore SYSLIB0011
            return stream.ToArray();
        }

        private static TItem DoDeserialize<TItem>(byte[] bytes) where TItem : class
        {
            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream(bytes);
#pragma warning disable SYSLIB0011
            var item = formatter.Deserialize(stream) as TItem;
#pragma warning restore SYSLIB0011
            return item;
        }
    }
}
