using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MassTransit;

namespace EPiServer.Events.MassTransit
{
    /// <summary>
    /// Transport filter
    /// </summary>
    public class TransportHeaderFilter :
            Headers
    {
        private readonly Headers _headers;

        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="headers"></param>
        public TransportHeaderFilter(Headers headers)
        {
            _headers = headers;
        }

        /// <summary>
        /// Gets the enumrator
        /// </summary>
        /// <returns>Header values</returns>
        public IEnumerator<HeaderValue> GetEnumerator()
        {
            return GetAll().Select(x => new HeaderValue(x.Key, x.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Get all
        /// </summary>
        /// <returns>The key value par</returns>
        public IEnumerable<KeyValuePair<string, object>> GetAll()
        {
            return _headers.GetAll();
        }

        /// <summary>
        /// Try to get the header
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if header exist.</returns>
        public bool TryGetHeader(string key, out object value)
        {
            return _headers.TryGetHeader(key, out value);
        }

        /// <summary>
        /// The get instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns>The header value</returns>
        public T Get<T>(string key, T defaultValue = default)
            where T : class
        {
            return _headers.Get(key, defaultValue);
        }

        /// <summary>
        /// Get the header value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns>The value</returns>
        public T? Get<T>(string key, T? defaultValue = null)
            where T : struct
        {
            return _headers.Get(key, defaultValue);
        }
    }
}
