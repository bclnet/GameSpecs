using System;
using System.Collections.Generic;

namespace OpenStack.Configuration
{
    // Represents a wrapper for an immutable settings instance.
    // This means that any methods invoked on this instance that try to alter it will throw.
    internal class ImmutableSettings : ISettings
    {
        readonly ISettings _settings;

        internal ImmutableSettings(ISettings settings)
            => _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        public event EventHandler SettingsChanged
        {
            add => _settings.SettingsChanged += value;
            remove => _settings.SettingsChanged -= value;
        }

        public void AddOrUpdate(string sectionName, SettingItem item) => throw new NotSupportedException();

        public IList<string> GetConfigFilePaths() => _settings.GetConfigFilePaths();

        public IList<string> GetConfigRoots() => _settings.GetConfigRoots();

        public SettingSection GetSection(string sectionName) => _settings.GetSection(sectionName);

        public void Remove(string sectionName, SettingItem item) => throw new NotSupportedException();

        public void SaveToDisk() => throw new NotSupportedException();
    }
}
