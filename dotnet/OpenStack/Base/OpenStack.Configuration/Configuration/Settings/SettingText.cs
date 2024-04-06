using System;
using System.Globalization;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    public sealed class SettingText : SettingBase
    {
        string _value;
        public string Value
        {
            get => _value;
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty", nameof(value));
                _value = value;
            }
        }

        public SettingText(string value) : base()
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty", nameof(value));
            _value = value;
        }

        public override bool Equals(object other)
            => !(other is SettingText text)
                ? false
                : ReferenceEquals(this, text) ? true : string.Equals(Value, text.Value, StringComparison.Ordinal);

        public override int GetHashCode() => Value.GetHashCode();

        public override bool IsEmpty() => string.IsNullOrEmpty(Value);

        public override SettingBase Clone()
        {
            var newSetting = new SettingText(Value);
            if (Origin != null) newSetting.SetOrigin(Origin);
            return newSetting;
        }

        internal SettingText(XText text, SettingsFile origin) : base(text, origin)
        {
            var value = text.Value.Trim();
            if (string.IsNullOrEmpty(value)) throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "UserSettings_UnableToParseConfigFile", "TextShouldNotBeEmpty", origin.ConfigFilePath));
            Value = value;
        }

        internal override XNode AsXNode() => Node is XText xText ? xText : (XNode)new XText(Value);
    }
}
