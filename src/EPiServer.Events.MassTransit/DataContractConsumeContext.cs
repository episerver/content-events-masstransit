using System;
using System.Collections.Generic;
using System.Linq;
using MassTransit;
using MassTransit.Context;

namespace EPiServer.Events.MassTransit
{
    /// <summary>
    /// The data contract consume context
    /// </summary>
    public class DataContractConsumeContext : DeserializerConsumeContext
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);
        private readonly DataContractEnvelope _envelope;
        private readonly Headers _headers;
        private readonly IDictionary<Type, ConsumeContext> _messageTypes;
        private readonly EventMessage _eventMessage;
        private Headers _headerFilter;
        private HostInfo _host;
        private string[] _supportedTypes;
        private DateTime? _sentTime;

        /// <summary>
        /// /Default constructor
        /// </summary>
        /// <param name="receiveContext">The request context</param>
        /// <param name="envelope">The envelope</param>
        /// <param name="eventMessage">The event message</param>
        public DataContractConsumeContext(ReceiveContext receiveContext,
            DataContractEnvelope envelope,
            EventMessage eventMessage) : base(receiveContext)
        {
            _envelope = envelope;
            _headers = receiveContext.TransportHeaders;
            _eventMessage = eventMessage;
            _messageTypes = new Dictionary<Type, ConsumeContext>()
            {
                { typeof(EventMessage),  new MessageConsumeContext<EventMessage>(this, _eventMessage) }
            };
        }

        /// <summary>
        /// The Message Id
        /// </summary>
        public override Guid? MessageId => _envelope.MessageId;

        /// <summary>
        /// The request Id
        /// </summary>
        public override Guid? RequestId => _envelope.RequestId;

        /// <summary>
        /// The correlation if
        /// </summary>
        public override Guid? CorrelationId => _envelope.CorrelationId;

        /// <summary>
        /// The conversation id.
        /// </summary>
        public override Guid? ConversationId => _envelope.ConversationId;

        /// <summary>
        /// The initiator id
        /// </summary>
        public override Guid? InitiatorId => _envelope.InitiatorId;

        /// <summary>
        /// The expiration time
        /// </summary>
        public override DateTime? ExpirationTime { get; }

        /// <summary>
        /// The source address
        /// </summary>
        public override Uri SourceAddress => _envelope.SourceAddress;

        /// <summary>
        /// The destination address
        /// </summary>
        public override Uri DestinationAddress => _envelope.DestinationAddress;

        /// <summary>
        /// The response addrress
        /// </summary>
        public override Uri ResponseAddress => _envelope.ResponseAddress;

        /// <summary>
        /// The failure address
        /// </summary>
        public override Uri FaultAddress => _envelope.FaultAddress;

        /// <summary>
        /// The sent time
        /// </summary>
        public override DateTime? SentTime => _sentTime ??= GetSentTime();

        /// <summary>
        /// The headers
        /// </summary>
        public override Headers Headers => _headerFilter ??= new TransportHeaderFilter(_headers);

        /// <summary>
        /// The host
        /// </summary>
        public override HostInfo Host => _host ??= GetHostInfo();

        /// <summary>
        /// The supported message types.
        /// </summary>
        public override IEnumerable<string> SupportedMessageTypes => _supportedTypes ??= GetMessageTypes().ToArray();

        /// <summary>
        /// Has message type
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <returns>True if has message type</returns>
        public override bool HasMessageType(Type messageType)
        {
            lock (_messageTypes)
            {
                if (_messageTypes.TryGetValue(messageType, out var existing))
                {
                    return existing != null;
                }
            }

            return false;
        }

        /// <summary>
        /// Try to get message
        /// </summary>
        /// <typeparam name="T">The type T</typeparam>
        /// <param name="consumeContext">The message</param>
        /// <returns>True if get message</returns>
        public override bool TryGetMessage<T>(out ConsumeContext<T> consumeContext)
        {
            lock (_messageTypes)
            {
                if (_messageTypes.TryGetValue(typeof(T), out var existing))
                {
                    consumeContext = existing as ConsumeContext<T>;
                    return consumeContext != null;
                }
                _messageTypes[typeof(T)] = consumeContext = null;
                return false;
            }
        }

        private DateTime? GetSentTime()
        {
            try
            {
                var sentTime = MessageId?.ToNewId().Timestamp;

                return sentTime > UnixEpoch ? sentTime : default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        private HostInfo GetHostInfo()
        {
            return _headers.Get<HostInfo>(MessageHeaders.Host.Info);
        }

        private static IEnumerable<string> GetMessageTypes()
        {
            return new List<string>() { MessageUrn.ForTypeString(typeof(EventMessage)) };
        }
    }
}
