using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    public abstract class ClientCertItem : SettingItem
    {
        protected ClientCertItem(string packageSource)
        {
            if (string.IsNullOrEmpty(packageSource)) throw new ArgumentException("ArgumentCannotBeNullOrEmpty", nameof(packageSource));

            AddAttribute(ConfigurationConstants.PackageSourceAttribute, packageSource);
        }

        internal ClientCertItem(XElement element, SettingsFile origin) : base(element, origin) { }

        public string PackageSource => Attributes[ConfigurationConstants.PackageSourceAttribute];

        protected override bool CanHaveChildren => false;

        public override bool Equals(object other)
            => !(other is ClientCertItem item) ? false
            : ReferenceEquals(this, item) ? true
            : !string.Equals(ElementName, item.ElementName, StringComparison.Ordinal) ? false
            : string.Equals(PackageSource, item.PackageSource, StringComparison.Ordinal);

        public override int GetHashCode()
        {
            var combiner = new HashCodeCombiner();
            combiner.AddObject(ElementName);
            combiner.AddObject(PackageSource);
            return combiner.CombinedHash;
        }

        public abstract IEnumerable<X509Certificate> Search();

        protected void SetPackageSource(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "PropertyCannotBeNullOrEmpty", nameof(PackageSource)));

            UpdateAttribute(ConfigurationConstants.PackageSourceAttribute, value);
        }
    }
}
