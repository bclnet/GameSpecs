using System;

namespace OpenStack.Configuration
{
    /// <summary>
    /// Machine wide settings based on the default machine wide config directory.
    /// </summary>
    public class XPlatMachineWideSetting : IMachineWideSettings
    {
        Lazy<ISettings> _settings;

        public XPlatMachineWideSetting()
        {
            var baseDirectory = NuGetEnvironment.GetFolderPath(NuGetFolderPath.MachineWideConfigDirectory);
            _settings = new Lazy<ISettings>(() => Configuration.Settings.LoadMachineWideSettings(baseDirectory));
        }

        public ISettings Settings => _settings.Value;
    }
}
