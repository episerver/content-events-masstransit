using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Optimizely.CMS.MassTransit.Events.Tests
{
    public class MassTransitEventProviderOptionsConfigurerTests
    {
        private readonly Mock<IConfiguration> _configuration;
        private readonly MassTransitEventProviderOptionsConfigurer _subject;
        private readonly MassTransitEventProviderOptions _options;

        public MassTransitEventProviderOptionsConfigurerTests()
        {
            _options = new MassTransitEventProviderOptions();
            _configuration = new Mock<IConfiguration>();
            _subject = new MassTransitEventProviderOptionsConfigurer(_configuration.Object);
        }

        [Fact]
        public void PostConfigure_WhenConnectionStringIsNullAndExistsShouldSet()
        {
            _configuration.Setup(x => x.GetSection("ConnectionStrings")["OptimizelyMassTransitEvents"]).Returns("amqp://guest:guest@localhost:5672");
            _subject.PostConfigure("test", _options);
            Assert.Equal("amqp://guest:guest@localhost:5672", _options.ConnectionString);
        }

        [Fact]
        public void PostConfigure_WhenExchangeNameIsNullShouldSet()
        {
            _subject.PostConfigure("test", _options);
            Assert.Equal("optimizely.fanout.siteevents", _options.ExchangeName);
        }

        [Fact]
        public void PostConfigure_WhenQueueNameIsNullShouldSet()
        {
            _subject.PostConfigure("test", _options);
            Assert.Equal("optimizely.queues.siteevents", _options.QueueName);
        }
    }
}
