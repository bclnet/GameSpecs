using System;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    public sealed class SourceItem : AddItem
    {
        public string ProtocolVersion
        {
            get => Attributes.TryGetValue(ConfigurationConstants.ProtocolVersionAttribute, out var attribute)
                    ? Settings.ApplyEnvironmentTransform(attribute)
                    : null;
            set => AddOrUpdateAttribute(ConfigurationConstants.ProtocolVersionAttribute, value);
        }

        public SourceItem(string key, string value, string protocolVersion = "") : base(key, value)
        {
            if (!string.IsNullOrEmpty(protocolVersion)) ProtocolVersion = protocolVersion;
        }

        public override int GetHashCode()
        {
            var combiner = new HashCodeCombiner();
            combiner.AddObject(Key);
            if (ProtocolVersion != null) combiner.AddObject(ProtocolVersion);
            return combiner.CombinedHash;
        }

        internal SourceItem(XElement element, SettingsFile origin) : base(element, origin) { }

        public override SettingBase Clone()
        {
            var newSetting = new SourceItem(Key, Value, ProtocolVersion);
            if (Origin != null) newSetting.SetOrigin(Origin);
            return newSetting;
        }

        public override bool Equals(object other)
            => !(other is SourceItem source)
                ? false
                : ReferenceEquals(this, source)
                ? true
                : string.Equals(Key, source.Key, StringComparison.Ordinal) &&
                string.Equals(ProtocolVersion, source.ProtocolVersion, StringComparison.Ordinal);
    }
}
