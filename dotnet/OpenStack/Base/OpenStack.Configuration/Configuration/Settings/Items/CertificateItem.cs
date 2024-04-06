using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    public sealed class CertificateItem : SettingItem
    {
        public override string ElementName => ConfigurationConstants.Certificate;

        public string Fingerprint
        {
            get => Attributes[ConfigurationConstants.Fingerprint];
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "PropertyCannotBeNullOrEmpty", nameof(Fingerprint)));

                UpdateAttribute(ConfigurationConstants.Fingerprint, value);
            }
        }

        public HashAlgorithmName HashAlgorithm
        {
            get => CryptoHashUtility.GetHashAlgorithmName(Attributes[ConfigurationConstants.HashAlgorithm]);
            set
            {
                if (value == HashAlgorithmName.Unknown) throw new ArgumentException("UnknownHashAlgorithmNotSupported");

                UpdateAttribute(ConfigurationConstants.HashAlgorithm, value.ToString().ToUpper(CultureInfo.InvariantCulture));
            }
        }

        public bool AllowUntrustedRoot
        {
            get => bool.TryParse(Attributes[ConfigurationConstants.AllowUntrustedRoot], out var parsedAttribute) && parsedAttribute;
            set => UpdateAttribute(ConfigurationConstants.AllowUntrustedRoot, value.ToString(CultureInfo.CurrentCulture).ToLower(CultureInfo.InvariantCulture));
        }

        protected override IReadOnlyCollection<string> RequiredAttributes { get; } = new HashSet<string>(new[] { ConfigurationConstants.Fingerprint, ConfigurationConstants.HashAlgorithm, ConfigurationConstants.AllowUntrustedRoot });

        public CertificateItem(string fingerprint, HashAlgorithmName hashAlgorithm, bool allowUntrustedRoot = false) : base()
        {
            if (string.IsNullOrEmpty(fingerprint)) throw new ArgumentException("ArgumentCannotBeNullOrEmpty", nameof(fingerprint));
            else if (hashAlgorithm == HashAlgorithmName.Unknown) throw new ArgumentException("UnknownHashAlgorithmNotSupported");

            AddAttribute(ConfigurationConstants.Fingerprint, fingerprint);
            AddAttribute(ConfigurationConstants.HashAlgorithm, hashAlgorithm.ToString().ToUpper(CultureInfo.InvariantCulture));
            AddAttribute(ConfigurationConstants.AllowUntrustedRoot, allowUntrustedRoot.ToString(CultureInfo.CurrentCulture).ToLower(CultureInfo.InvariantCulture));
        }

        internal CertificateItem(XElement element, SettingsFile origin)
            : base(element, origin)
        {
            if (HashAlgorithm == HashAlgorithmName.Unknown)
                throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "UserSettingsUnableToParseConfigFile",
                    string.Format(CultureInfo.CurrentCulture, "UnsupportedHashAlgorithm", Attributes[ConfigurationConstants.HashAlgorithm]),
                    origin.ConfigFilePath));

            // Update attributes with propert casing
            UpdateAttribute(ConfigurationConstants.HashAlgorithm, HashAlgorithm.ToString().ToUpper(CultureInfo.InvariantCulture));
            UpdateAttribute(ConfigurationConstants.AllowUntrustedRoot, AllowUntrustedRoot.ToString(CultureInfo.CurrentCulture).ToLower(CultureInfo.InvariantCulture));
        }

        public override SettingBase Clone()
        {
            var newItem = new CertificateItem(Fingerprint, HashAlgorithm, AllowUntrustedRoot);
            if (Origin != null) newItem.SetOrigin(Origin);
            return newItem;
        }

        public override bool Equals(object other)
            => other is CertificateItem cert
                ? ReferenceEquals(this, cert) ? true : string.Equals(Fingerprint, cert.Fingerprint, StringComparison.Ordinal)
                : false;

        public override int GetHashCode() => Fingerprint.GetHashCode();
    }
}
