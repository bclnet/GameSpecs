using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    public abstract class SettingSection : SettingsGroup<SettingItem>
    {
        string _elementName;
        public override string ElementName
        {
            get => XmlConvert.DecodeName(_elementName);
            protected set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentException("PropertyCannotBeNullOrEmpty", nameof(ElementName));
                _elementName = XmlUtility.GetEncodedXMLName(value);
            }
        }

        public IReadOnlyCollection<SettingItem> Items => Children.ToList();

        public T GetFirstItemWithAttribute<T>(string attributeName, string expectedAttributeValue) where T : SettingItem
            => Items.OfType<T>().FirstOrDefault(c =>
                c.Attributes.TryGetValue(attributeName, out var attributeValue) &&
                string.Equals(attributeValue, expectedAttributeValue, StringComparison.OrdinalIgnoreCase));

        protected SettingSection(string name, IReadOnlyDictionary<string, string> attributes, IEnumerable<SettingItem> children) : base(attributes, children)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("ArgumentCannotBeNullOrEmpty", nameof(name));

            ElementName = name;
        }

        internal SettingSection(XElement element, SettingsFile origin) : base(element, origin) { }

        internal bool Update(SettingItem item)
        {
            if (item == null || (Origin != null && Origin.IsReadOnly)) return false;

            if (TryGetChild(item, out var currentChild))
            {
                if (currentChild.Origin != null && currentChild.Origin.IsReadOnly) return false;
                currentChild.Update(item);
                return true;
            }
            return false;
        }

        public override bool Equals(object other)
            => !(other is SettingSection section)
                ? false
                : ReferenceEquals(this, section) || string.Equals(ElementName, section.ElementName, StringComparison.Ordinal);

        public override int GetHashCode() => ElementName.GetHashCode();
    }
}
