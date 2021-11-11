using System;

namespace EPiServer.Events.MassTransit
{
    /// <summary>
    /// Holds the envelope for the meesage
    /// </summary>
    public class DataContractEnvelope
    {
        /// <summary>
        /// The messge id
        /// </summary>
        public Guid? MessageId { get; set; }

        /// <summary>
        /// The request id
        /// </summary>
        public Guid? RequestId { get; set; }

        /// <summary>
        /// The correlation id
        /// </summary>
        public Guid? CorrelationId { get; set; }

        /// <summary>
        /// The converation Id
        /// </summary>
        public Guid? ConversationId { get; set; }

        /// <summary>
        /// Initiator Id
        /// </summary>
        public Guid? InitiatorId { get; set; }

        /// <summary>
        /// The source address
        /// </summary>
        public Uri SourceAddress { get; set; }

        /// <summary>
        /// The destination address
        /// </summary>
        public Uri DestinationAddress { get; set; }

        /// <summary>
        /// The response address
        /// </summary>
        public Uri ResponseAddress { get; set; }

        /// <summary>
        /// The fault address
        /// </summary>
        public Uri FaultAddress { get; set; }

        /// <summary>
        /// The message type
        /// </summary>
        public string[] MessageType { get; set; }

        /// <summary>
        /// The message
        /// </summary>
        public object Message { get; set; }

        /// <summary>
        /// The expiration time
        /// </summary>
        public DateTime? ExpirationTime { get; set; }

        /// <summary>
        /// The sent time
        /// </summary>
        public DateTime? SentTime { get; set; }
    }
}
