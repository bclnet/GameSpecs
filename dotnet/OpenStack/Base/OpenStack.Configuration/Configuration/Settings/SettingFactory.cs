using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    internal static class SettingFactory
    {
        internal static SettingBase Parse(XNode node, SettingsFile origin)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            else if (node is XText textNode) return new SettingText(textNode, origin);
            else if (node is XElement element)
            {
                var elementType = SettingElementType.Unknown;
                Enum.TryParse(element.Name.LocalName, ignoreCase: true, result: out elementType);

                var parentType = SettingElementType.Unknown;
                if (element.Parent != null) Enum.TryParse(element.Parent?.Name.LocalName, ignoreCase: true, result: out parentType);

                switch (parentType)
                {
                    case SettingElementType.Configuration: return new ParsedSettingSection(element, origin);
                    case SettingElementType.PackageSourceCredentials: return new CredentialsItem(element, origin);
                    case SettingElementType.PackageSources: if (elementType == SettingElementType.Add) return new SourceItem(element, origin); break;
                    case SettingElementType.PackageSourceMapping: if (elementType == SettingElementType.PackageSource) return new PackageSourceMappingSourceItem(element, origin); break;
                    case SettingElementType.PackageSource: if (elementType == SettingElementType.Package) return new PackagePatternItem(element, origin); break;
                }

                return elementType switch
                {
                    SettingElementType.Add => new AddItem(element, origin),
                    SettingElementType.Author => new AuthorItem(element, origin),
                    SettingElementType.Certificate => new CertificateItem(element, origin),
                    SettingElementType.Clear => new ClearItem(element, origin),
                    SettingElementType.Owners => new OwnersItem(element, origin),
                    SettingElementType.Repository => new RepositoryItem(element, origin),
                    SettingElementType.FileCert => new FileClientCertItem(element, origin),
                    SettingElementType.StoreCert => new StoreClientCertItem(element, origin),
                    _ => new UnknownItem(element, origin),
                };
            }
            return null;
        }

        class SettingElementKeyComparer : IComparer<SettingElement>, IEqualityComparer<SettingElement>
        {
            public int Compare(SettingElement x, SettingElement y)
                => ReferenceEquals(x, y) ? 0
                : ReferenceEquals(x, null) ? -1
                : ReferenceEquals(y, null) ? 1
                : StringComparer.OrdinalIgnoreCase.Compare(
                    x.ElementName + string.Join("", x.Attributes.Select(a => a.Value)),
                    y.ElementName + string.Join("", y.Attributes.Select(a => a.Value)));

            public bool Equals(SettingElement x, SettingElement y)
                => ReferenceEquals(x, y) ? true
                : ReferenceEquals(x, null) || ReferenceEquals(y, null) ? false
                : StringComparer.OrdinalIgnoreCase.Equals(
                    x.ElementName + string.Join("", x.Attributes.Select(a => a.Value)),
                    y.ElementName + string.Join("", y.Attributes.Select(a => a.Value)));

            public int GetHashCode(SettingElement obj)
                => ReferenceEquals(obj, null) ? 0
                    : StringComparer.OrdinalIgnoreCase.GetHashCode(obj.ElementName + string.Join("", obj.Attributes.Select(a => a.Value)));
        }

        internal static IEnumerable<T> ParseChildren<T>(XElement xElement, SettingsFile origin, bool canBeCleared) where T : SettingElement
        {
            var children = new List<T>();
            IEnumerable<T> descendants = xElement.Elements().Select(d => Parse(d, origin)).OfType<T>();
            SettingElementKeyComparer comparer = new SettingElementKeyComparer();
            HashSet<T> distinctDescendants = new HashSet<T>(comparer);
            List<T> duplicatedDescendants = null;

            foreach (var item in descendants)
                if (!distinctDescendants.Add(item))
                {
                    duplicatedDescendants ??= new List<T>();
                    duplicatedDescendants.Add(item);
                }

            if (xElement.Name.LocalName.Equals(ConfigurationConstants.PackageSourceMapping, StringComparison.OrdinalIgnoreCase) && duplicatedDescendants != null)
            {
                var duplicatedKey = string.Join(", ", duplicatedDescendants.Select(d => d.Attributes["key"]));
                var source = duplicatedDescendants.Select(d => d.Origin.ConfigFilePath).First();
                throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "Error_DuplicatePackageSource", duplicatedKey, source));
            }

            foreach (var descendant in distinctDescendants)
            {
                if (canBeCleared && descendant is ClearItem) children.Clear();
                children.Add(descendant);
            }
            return children;
        }
    }
}
