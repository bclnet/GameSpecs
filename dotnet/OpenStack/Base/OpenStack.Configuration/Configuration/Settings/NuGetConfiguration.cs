using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    internal sealed class NuGetConfiguration : SettingsGroup<SettingSection>, ISettingsGroup
    {
        public override string ElementName => ConfigurationConstants.Configuration;

        internal IReadOnlyDictionary<string, SettingSection> Sections => Children.ToDictionary(c => c.ElementName);

        protected override bool CanBeCleared => false;

        /// <remarks>
        /// There should not be a NuGetConfiguration without an Origin.
        /// This constructor should only be used for tests.
        /// </remarks>
        NuGetConfiguration(IReadOnlyDictionary<string, string> attributes, IEnumerable<SettingSection> children) : base(attributes, children) { }

        /// <remarks>
        /// There should not be a NuGetConfiguration without an Origin.
        /// This constructor should only be used for tests.
        /// </remarks>
        internal NuGetConfiguration(params SettingSection[] sections) : base()
        {
            foreach (var section in sections) { section.Parent = this; Children.Add(section); }
        }

        internal NuGetConfiguration(SettingsFile origin) : base()
        {
            var defaultSource = new SourceItem(NuGetConstants.FeedName, NuGetConstants.V3FeedUrl, protocolVersion: PackageSourceProvider.MaxSupportedProtocolVersion.ToString(CultureInfo.CurrentCulture));
            defaultSource.SetNode(defaultSource.AsXNode());
            var defaultSection = new ParsedSettingSection(ConfigurationConstants.PackageSources, defaultSource) { Parent = this };
            defaultSection.SetNode(defaultSection.AsXNode());
            Children.Add(defaultSection);
            SetNode(AsXNode());
            SetOrigin(origin);
        }

        internal NuGetConfiguration(XElement element, SettingsFile origin) : base(element, origin)
        {
            if (!string.Equals(element.Name.LocalName, ElementName, StringComparison.OrdinalIgnoreCase)) throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "ShowError_ConfigRootInvalid", origin.ConfigFilePath));
        }

        public void AddOrUpdate(string sectionName, SettingItem item)
        {
            if (string.IsNullOrEmpty(sectionName)) throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty", nameof(sectionName));
            else if (item == null) throw new ArgumentNullException(nameof(item));
            else if (Sections.TryGetValue(sectionName, out var section))
                // section exists, update or add the element on it
                if (section.Update(item) || section.Add(item)) return;

            // The section is new, add it with the item
            Add(new ParsedSettingSection(sectionName, item));
        }

        public void Remove(string sectionName, SettingItem item)
        {
            if (string.IsNullOrEmpty(sectionName)) throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty", nameof(sectionName));
            else if (item == null) throw new ArgumentNullException(nameof(item));
            else if (Sections.TryGetValue(sectionName, out var section)) section.Remove(item);
        }

        public SettingSection GetSection(string sectionName)
            => Sections.TryGetValue(sectionName, out var section) ? section.Clone() as SettingSection : null;

        internal void MergeSectionsInto(Dictionary<string, VirtualSettingSection> sectionsContainer)
        {
            // loop through the current element's sections: merge any overlapped sections, add any missing section
            foreach (var section in Sections)
                if (sectionsContainer.TryGetValue(section.Value.ElementName, out var settingsSection)) settingsSection.Merge(section.Value);
                else sectionsContainer.Add(section.Key, new VirtualSettingSection(section.Value));
        }

        public override SettingBase Clone()
           => new NuGetConfiguration(Attributes, Sections.Select(s => s.Value.Clone() as SettingSection));

        public override bool Equals(object other)
        {
            var nugetConfiguration = other as NuGetConfiguration;
            return nugetConfiguration == null ? false
                : ReferenceEquals(this, nugetConfiguration) ? true
                : Sections.OrderedEquals(nugetConfiguration.Sections, s => s.Key, StringComparer.Ordinal);
        }

        public override int GetHashCode() => Children.GetHashCode();
    }
}
