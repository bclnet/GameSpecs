using System.Xml.Linq;

namespace OpenStack.Configuration
{
    public sealed class ClearItem : SettingItem
    {
        public override string ElementName => ConfigurationConstants.Clear;

        public override bool IsEmpty() => false;

        public ClearItem() { }

        internal ClearItem(XElement element, SettingsFile origin) : base(element, origin) { }

        public override SettingBase Clone()
        {
            var newItem = new ClearItem();
            if (Origin != null) newItem.SetOrigin(Origin);
            return newItem;
        }

        public override bool Equals(object other) => other is ClearItem;
        public override int GetHashCode() => ElementName.GetHashCode();
    }
}
