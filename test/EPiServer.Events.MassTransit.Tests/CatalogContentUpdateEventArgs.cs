using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPiServer.Events.MassTransit.Tests
{
    [Serializable]
    public class CatalogContentUpdateEventArgs : EventArgs
    {
        private int[] _catalogIds = Array.Empty<int>();
        private int[] _catalogNodeIds = Array.Empty<int>();
        private int[] _catalogEntryIds = Array.Empty<int>();
        private int[] _catalogAssociationIds = Array.Empty<int>();

        public string EventType { get; set; }

        public IEnumerable<int> CatalogIds
        {
            get => _catalogIds;
            set => _catalogIds = value as int[] ?? value?.ToArray() ?? Array.Empty<int>();
        }

        public IEnumerable<int> CatalogNodeIds
        {
            get => _catalogNodeIds;
            set => _catalogNodeIds = value as int[] ?? value?.ToArray() ?? Array.Empty<int>();
        }

        public IEnumerable<int> CatalogEntryIds
        {
            get => _catalogEntryIds;
            set => _catalogEntryIds = value as int[] ?? value?.ToArray() ?? Array.Empty<int>();
        }

        public IEnumerable<int> CatalogAssociationIds
        {
            get => _catalogAssociationIds;
            set => _catalogAssociationIds = value as int[] ?? value?.ToArray() ?? Array.Empty<int>();
        }

        public bool HasChangedParent { get; set; }

        public bool? ApplicationHasContentModelTypes { get; set; }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("Event type '").Append(EventType).Append("' for");

            AppendCollection(buffer, "Catalogs", _catalogIds);
            AppendCollection(buffer, "Nodes", _catalogNodeIds);
            AppendCollection(buffer, "Entries", _catalogEntryIds);
            AppendCollection(buffer, "Associations", _catalogAssociationIds);
            return buffer.ToString();
        }

        private static void AppendCollection(StringBuilder buffer, string collectionName, int[] collection)
        {
            const int maxPrintCount = 10;

            if (collection?.Any() != true)
            {
                return;
            }

            buffer.Append(' ').Append(collectionName).Append(" [ ");

            if (collection.Length <= maxPrintCount)
            {
                buffer.AppendJoin(", ", collection);
            }
            else
            {
                buffer.AppendJoin(", ", collection.Take(maxPrintCount))
                    .Append(" ... and ").Append(collection.Length - maxPrintCount).Append(" more");
            }
            buffer.Append(" ]");
        }
    }
}
