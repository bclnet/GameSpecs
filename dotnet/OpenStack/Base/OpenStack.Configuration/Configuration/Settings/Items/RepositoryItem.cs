using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    public sealed class RepositoryItem : TrustedSignerItem
    {
        public override string ElementName => ConfigurationConstants.Repository;

        public string ServiceIndex => Attributes[ConfigurationConstants.ServiceIndex];

        public new string Name
        {
            get => base.Name;
            set => SetName(value);
        }

        OwnersItem _owners;

        public IList<string> Owners { get; private set; }

        protected override IReadOnlyCollection<string> RequiredAttributes { get; } = new HashSet<string>(new[] { ConfigurationConstants.NameAttribute, ConfigurationConstants.ServiceIndex });

        public RepositoryItem(string name, string serviceIndex, params CertificateItem[] certificates) : this(name, serviceIndex, owners: null, certificates: certificates) { }

        public RepositoryItem(string name, string serviceIndex, string owners, params CertificateItem[] certificates) : base(name, certificates)
        {
            if (string.IsNullOrEmpty(serviceIndex)) throw new ArgumentNullException(nameof(serviceIndex));

            AddAttribute(ConfigurationConstants.ServiceIndex, serviceIndex);

            if (!string.IsNullOrEmpty(owners)) { _owners = new OwnersItem(owners); Owners = _owners.Content; }
            else Owners = new List<string>();
        }

        internal RepositoryItem(XElement element, SettingsFile origin) : base(element, origin)
        {
            var parsedOwners = _parsedDescendants.OfType<OwnersItem>();
            Owners = new List<string>();

            if (parsedOwners != null && parsedOwners.Any())
            {
                if (parsedOwners.Count() > 1) throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "UserSettingsUnableToParseConfigFile", string.Format(CultureInfo.CurrentCulture, "RepositoryMustHaveOneOwners", Name, ServiceIndex), origin.ConfigFilePath));
                _owners = parsedOwners.FirstOrDefault(); Owners = _owners.Content;
            }
            else Owners = new List<string>();
        }

        public override SettingBase Clone()
        {
            var newItem = new RepositoryItem(Name, ServiceIndex, string.Join(OwnersItem.OwnersListSeparator.ToString(CultureInfo.CurrentCulture), Owners), Certificates.Select(c => c.Clone() as CertificateItem).ToArray());
            if (Origin != null) newItem.SetOrigin(Origin);
            return newItem;
        }

        internal override XNode AsXNode()
        {
            if (Node is XElement) return Node;

            var element = new XElement(ElementName);
            foreach (var cert in Certificates) element.Add(cert.AsXNode());
            if (_owners != null) element.Add(_owners.AsXNode());
            foreach (var attr in Attributes) element.SetAttributeValue(attr.Key, attr.Value);
            return element;
        }

        public override bool Equals(object other)
            => other is RepositoryItem repository
                ? ReferenceEquals(this, repository) ? true : string.Equals(ServiceIndex, repository.ServiceIndex, StringComparison.Ordinal)
                : false;

        public override int GetHashCode()
        {
            var combiner = new HashCodeCombiner();
            combiner.AddObject(Name);
            combiner.AddObject(ServiceIndex);
            return combiner.CombinedHash;
        }

        internal override void SetOrigin(SettingsFile origin)
        {
            base.SetOrigin(origin);
            _owners?.SetOrigin(origin);
        }

        internal override void RemoveFromSettings()
        {
            base.RemoveFromSettings();
            _owners?.RemoveFromSettings();
        }

        internal override void Update(SettingItem other)
        {
            base.Update(other);
            var repository = other as RepositoryItem;
            if (!Owners.SequenceEqual(repository.Owners, StringComparer.Ordinal))
            {
                if (_owners == null || !Owners.Any())
                {
                    _owners = new OwnersItem(string.Join(OwnersItem.OwnersListSeparator.ToString(CultureInfo.CurrentCulture), repository.Owners));
                    Owners = _owners.Content;
                    if (Origin != null)
                    {
                        _owners.SetOrigin(Origin);
                        if (Node != null)
                        {
                            _owners.SetNode(_owners.AsXNode());
                            XElementUtility.AddIndented(Node as XElement, _owners.Node);
                            Origin.IsDirty = true;
                        }
                    }
                }
                else if (!repository.Owners.Any())
                {
                    XElementUtility.RemoveIndented(_owners.Node);
                    _owners = null;
                    Owners.Clear();
                    if (Origin != null) Origin.IsDirty = true;
                }
                else { _owners.Update(repository._owners); Owners = _owners.Content; }
            }
        }
    }
}
