using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameSpec.Explorer
{
    [DebuggerDisplay("{Name}, items: {Items.Count} [{Tag}]")]
    public class ExplorerInfoNode
    {
        public string Name { get; set; }
        public object Tag { get; }
        public IEnumerable<ExplorerInfoNode> Items { get; }
        public bool Clickable { get; set; }

        public ExplorerInfoNode(string name, object tag = null, IEnumerable<ExplorerInfoNode> items = null, bool clickable = false)
        {
            Name = name;
            Tag = tag;
            Items = items ?? Enumerable.Empty<ExplorerInfoNode>();
            Clickable = clickable;
        }

        public static ExplorerInfoNode WrapWithGroup<T>(IList<T> source, string groupName, IEnumerable<ExplorerInfoNode> body)
            => source.Count == 0 ? null
            : source.Count == 1 ? body.First()
            : new ExplorerInfoNode(groupName, body);
    }
}
