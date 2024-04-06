using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    internal sealed class SettingsFile
    {
        /// <summary>
        /// Full path to the settings file
        /// </summary>
        internal string ConfigFilePath { get; }

        /// <summary>
        /// Folder under which the settings file is present
        /// </summary>
        internal string DirectoryPath { get; }

        /// <summary>
        /// Filename of the settings file
        /// </summary>
        internal string FileName { get; }

        /// <summary>
        /// Defines if the configuration settings have been changed but have not been saved to disk
        /// </summary>
        internal bool IsDirty { get; set; }

        /// <summary>
        /// Defines if the settings file is considered a machine wide settings file
        /// </summary>
        /// <remarks>Machine wide settings files cannot be edited.</remarks>
        internal bool IsMachineWide { get; }

        /// <summary>
        /// Determines if the settings file is considered read-only from NuGet perspective.
        /// </summary>
        /// <remarks>User-wide configuration files imported from non-default locations are not considered editable.
        /// Note that this is different from <see cref="IsMachineWide"/>. <see cref="IsReadOnly"/> will return <see langword="true"/> for every machine-wide config. </remarks>
        internal bool IsReadOnly { get; }

        /// <summary>
        /// XML element for settings file
        /// </summary>
        readonly XDocument _xDocument;

        /// <summary>
        /// Root element of configuration file.
        /// By definition of a nuget.config, the root element has to be a 'configuration' element
        /// </summary>
        readonly NuGetConfiguration _root;

        /// <summary>
        /// Creates an instance of a non machine wide SettingsFile with the default filename.
        /// </summary>
        /// <param name="directoryPath">path to the directory where the file is</param>
        public SettingsFile(string directoryPath) : this(directoryPath, Settings.DefaultSettingsFileName, isMachineWide: false, isReadOnly: false) { }

        /// <summary>
        /// Creates an instance of a non machine wide SettingsFile.
        /// </summary>
        /// <param name="directoryPath">path to the directory where the file is</param>
        /// <param name="fileName">name of config file</param>
        public SettingsFile(string directoryPath, string fileName) : this(directoryPath, fileName, isMachineWide: false, isReadOnly: false) { }

        /// <summary>
        /// Creates an instance of a SettingsFile
        /// </summary>
        /// <remarks>It will parse the specified document,
        /// if it doesn't exist it will create one with the default configuration.</remarks>
        /// <param name="directoryPath">path to the directory where the file is</param>
        /// <param name="fileName">name of config file</param>
        /// <param name="isMachineWide">specifies if the SettingsFile is machine wide.</param>
        /// <param name="isReadOnly">specifies if the SettingsFile is read only. If the config is machine wide, the value passed here is irrelevant. <see cref="IsReadOnly"/> will return <see langword="true"/> for every machine-wide config.</param>
        public SettingsFile(string directoryPath, string fileName, bool isMachineWide, bool isReadOnly)
        {
            if (string.IsNullOrEmpty(directoryPath)) throw new ArgumentException("ArgumentCannotBeNullOrEmpty", nameof(directoryPath));
            else if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("ArgumentCannotBeNullOrEmpty", nameof(fileName));
            else if (!FileSystemUtility.IsPathAFile(fileName)) throw new ArgumentException("SettingsFileNameCannotBeAPath", nameof(fileName));

            DirectoryPath = directoryPath;
            FileName = fileName;
            ConfigFilePath = Path.GetFullPath(Path.Combine(DirectoryPath, FileName));
            IsMachineWide = isMachineWide;
            IsReadOnly = IsMachineWide || isReadOnly;

            XDocument config = null;
            ExecuteSynchronized(() => config = FileSystemUtility.GetOrCreateDocument(CreateDefaultConfig(), ConfigFilePath));
            _xDocument = config;

            _root = new NuGetConfiguration(_xDocument.Root, origin: this);
        }

        /// <summary>
        /// Gets the section with a given name.
        /// </summary>
        /// <param name="sectionName">name to match sections</param>
        /// <returns>null if no section with the given name was found</returns>
        public SettingSection GetSection(string sectionName)
            => _root.GetSection(sectionName);

        /// <summary>
        /// Adds or updates the given <paramref name="item"/> to the settings.
        /// </summary>
        /// <param name="sectionName">section where the <paramref name="item"/> has to be added. If this section does not exist, one will be created.</param>
        /// <param name="item">item to be added to the settings.</param>
        /// <returns>true if the item was successfully updated or added in the settings</returns>
        internal void AddOrUpdate(string sectionName, SettingItem item)
            => _root.AddOrUpdate(sectionName, item);

        /// <summary>
        /// Removes the given <paramref name="item"/> from the settings.
        /// If the <paramref name="item"/> is the last item in the section, the section will also be removed.
        /// </summary>
        /// <param name="sectionName">Section where the <paramref name="item"/> is stored. If this section does not exist, the method will throw</param>
        /// <param name="item">item to be removed from the settings</param>
        /// <remarks> If the SettingsFile is a machine wide config this method will throw</remarks>
        internal void Remove(string sectionName, SettingItem item)
            => _root.Remove(sectionName, item);

        /// <summary>
        /// Flushes any in-memory updates in the SettingsFile to disk.
        /// </summary>
        internal void SaveToDisk()
        {
            if (IsDirty)
            {
                ExecuteSynchronized(() => FileSystemUtility.AddFile(ConfigFilePath, _xDocument.Save));
                IsDirty = false;
            }
        }

        internal bool IsEmpty() => _root == null || _root.IsEmpty();

        /// <remarks>
        /// This method gives you a reference to the actual abstraction instead of a clone of it.
        /// It should be used only when intended. For most purposes you should be able to use
        /// GetSection(...) instead.
        /// </remarks>
        internal bool TryGetSection(string sectionName, out SettingSection section)
            => _root.Sections.TryGetValue(sectionName, out section);

        internal void MergeSectionsInto(Dictionary<string, VirtualSettingSection> sectionsContainer)
            => _root.MergeSectionsInto(sectionsContainer);

        XDocument CreateDefaultConfig()
           => new XDocument(new NuGetConfiguration(this).AsXNode());

        void ExecuteSynchronized(Action ioOperation)
            => ConcurrencyUtilities.ExecuteWithFileLocked(filePath: ConfigFilePath, action: () =>
            {
                try { ioOperation(); }
                catch (InvalidOperationException e) { throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "ShowErrorConfigInvalidOperation", ConfigFilePath, e.Message), e); }
                catch (UnauthorizedAccessException e) { throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "ShowErrorConfigUnauthorizedAccess", ConfigFilePath, e.Message), e); }
                catch (XmlException e) { throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "ShowErrorConfigInvalidXml", ConfigFilePath, e.Message), e); }
                catch (Exception e) { throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "UnknownConfigException", ConfigFilePath, e.Message), e); }
            });
    }
}
