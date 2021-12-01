using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Xml;
using GreenPipes;
using MassTransit;
using MassTransit.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EPiServer.Events.MassTransit
{
    /// <summary>
    /// Replica of the DataContractBinarySerializer in the Microsoft.ServiceBus assembly
    /// that supports passing in a list of known types to the serializer.
    /// </summary>
    public sealed class DataContractBinarySerializer : XmlObjectSerializer, IMessageDeserializer, IMessageSerializer
    {
        private readonly DataContractSerializer _dataContractSerializer;
        private readonly ILogger<DataContractSerializer> _logger;
#pragma warning disable CS0436 // Type conflicts with imported type
        /// <summary>
        /// The content type
        /// </summary>
        public ContentType ContentType => ServiceCollectionExtensions.ContentType;
#pragma warning restore CS0436 // Type conflicts with imported type

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="eventsServiceKnownTypesLookup"></param>
        public DataContractBinarySerializer(EventsServiceKnownTypesLookup eventsServiceKnownTypesLookup)
        {
            _dataContractSerializer = new DataContractSerializer(typeof(EventMessage), eventsServiceKnownTypesLookup.KnownTypes ?? Enumerable.Empty<Type>());
            _logger = new LoggerFactory().CreateLogger<DataContractSerializer>();
        }

        /// <inheritdoc/>
        public override object ReadObject(Stream stream)
        {
            return ReadObject(XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max));
        }

        /// <inheritdoc/>
        public override void WriteObject(Stream stream, object graph)
        {
            var binaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream, null, null, false);
            WriteObject(binaryWriter, graph);
            binaryWriter.Flush();
        }

        /// <inheritdoc/>
        public override void WriteObject(XmlDictionaryWriter writer, object graph)
        {
            _dataContractSerializer.WriteObject(writer, graph);
        }

        /// <inheritdoc/>
        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            return _dataContractSerializer.IsStartObject(reader);
        }

        /// <inheritdoc/>
        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            return _dataContractSerializer.ReadObject(reader, verifyObjectName);
        }

        /// <inheritdoc/>
        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            _dataContractSerializer.WriteEndObject(writer);
        }

        /// <inheritdoc/>
        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            _dataContractSerializer.WriteObjectContent(writer, graph);
        }

        /// <inheritdoc/>
        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            _dataContractSerializer.WriteStartObject(writer, graph);
        }

        /// <inheritdoc/>
        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateScope("json");
            scope.Add("contentType", JsonMessageSerializer.JsonContentType.MediaType);
        }

        /// <inheritdoc/>
        ConsumeContext IMessageDeserializer.Deserialize(ReceiveContext receiveContext)
        {
            EventMessage message;
            try
            {
                var envelope = new DataContractEnvelope
                {
                    ConversationId = receiveContext.GetConversationId(),
                    CorrelationId = receiveContext.GetCorrelationId(),
                    DestinationAddress = receiveContext.InputAddress,
                    ExpirationTime = null,
                    FaultAddress = GetEndpointAddress(receiveContext.TransportHeaders, MessageHeaders.FaultAddress),
                    InitiatorId = receiveContext.GetInitiatorId(),
                    MessageId = receiveContext.GetMessageId(),
                    RequestId = receiveContext.GetRequestId(),
                    ResponseAddress = GetEndpointAddress(receiveContext.TransportHeaders, MessageHeaders.ResponseAddress),
                    SourceAddress = GetEndpointAddress(receiveContext.TransportHeaders, MessageHeaders.SourceAddress)
                };
                using (var msgStream = receiveContext.GetBodyStream())
                {
                    message = (EventMessage)ReadObject(msgStream);
                }
                return new DataContractConsumeContext(receiveContext, envelope, message);
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred while deserializing the message envelope", ex);
                return null;
            }
        }

        /// <inheritdoc/>
        void IMessageSerializer.Serialize<T>(Stream stream, SendContext<T> context)
            where T : class
        {
            try
            {
                WriteObject(stream, context.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to serialize message", ex);
            }
        }

        private static Uri GetEndpointAddress(Headers headers, string key)
        {
            try
            {
                var address = headers.Get<string>(key);
                return string.IsNullOrWhiteSpace(address)
                    ? default
                    : new Uri(address);
            }
            catch (FormatException)
            {
                return default;
            }
        }
    }
}
