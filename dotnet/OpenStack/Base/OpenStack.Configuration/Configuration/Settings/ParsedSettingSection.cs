using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    internal sealed class ParsedSettingSection : SettingSection
    {
        internal ParsedSettingSection(XElement element, SettingsFile origin) : base(element, origin) { }

        internal ParsedSettingSection(string name, params SettingItem[] children) : base(name, attributes: null, children: new HashSet<SettingItem>(children))
        {
            foreach (var child in Children) child.Parent = this;
        }

        public override SettingBase Clone()
            => new VirtualSettingSection(ElementName, Attributes, Items.Select(s => s.Clone() as SettingItem));
    }
}
