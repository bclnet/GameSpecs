using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    /// <summary>
    ///     A FileClientCertItem have 4 attributes:
    ///     - [Required] packageSource
    ///     - [Required] path
    ///     - [Optional] password
    ///     - [Optional] clearTextPassword
    /// </summary>
    public sealed class FileClientCertItem : ClientCertItem
    {
        public FileClientCertItem(string packageSource, string filePath, string password, bool storePasswordInClearText, string settingsFilePath)
            : this(packageSource, filePath, password, storePasswordInClearText, string.IsNullOrWhiteSpace(settingsFilePath) ? null : new SettingsFile(Path.GetDirectoryName(settingsFilePath), Path.GetFileName(settingsFilePath), isMachineWide: false, isReadOnly: false)) { }

        internal FileClientCertItem(string packageSource, string filePath, string password, bool storePasswordInClearText, SettingsFile origin) : base(packageSource)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("ArgumentCannotBeNullOrEmpty", nameof(filePath));

            Update(filePath, password, storePasswordInClearText);
            SetOrigin(origin);
        }

        internal FileClientCertItem(XElement element, SettingsFile origin) : base(element, origin)
        {
            var path = element.Attribute(XName.Get(ConfigurationConstants.PathAttribute))?.Value;
            var password = element.Attribute(XName.Get(ConfigurationConstants.PasswordAttribute))?.Value;
            var clearTextPassword = element.Attribute(XName.Get(ConfigurationConstants.ClearTextPasswordAttribute))?.Value;

            if (string.IsNullOrWhiteSpace(path)) throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "UserSettingsUnableToParseConfigFile", "FileCertItemPathFileNotSet", Origin?.ConfigFilePath ?? "<Config file path>"));
            else if (!string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(clearTextPassword)) throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "UserSettingsUnableToParseConfigFile", "FileCertItemPasswordAndClearTextPasswordAtSameTime", Origin?.ConfigFilePath ?? "<Config file path>"));

            AddAttribute(ConfigurationConstants.PathAttribute, path);

            if (!string.IsNullOrWhiteSpace(password)) AddAttribute(ConfigurationConstants.PasswordAttribute, password);
            if (!string.IsNullOrWhiteSpace(clearTextPassword)) AddAttribute(ConfigurationConstants.ClearTextPasswordAttribute, clearTextPassword);
        }

        public override string ElementName => ConfigurationConstants.FileCertificate;

        public string FilePath => Attributes[ConfigurationConstants.PathAttribute];

        public bool IsPasswordIsClearText => Attributes.ContainsKey(ConfigurationConstants.ClearTextPasswordAttribute);

        public string Password
        {
            get
            {
                string result = null;
                if (IsPasswordIsClearText) Attributes.TryGetValue(ConfigurationConstants.ClearTextPasswordAttribute, out result);
                else if (Attributes.TryGetValue(ConfigurationConstants.PasswordAttribute, out var encryptedPassword))
                    try { result = EncryptionUtility.DecryptString(encryptedPassword); }
                    catch (Exception e) { throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "FileCertItemPasswordCannotBeDecrypted", PackageSource), e); }
                return result;
            }
        }

        protected override IReadOnlyCollection<string> AllowedAttributes { get; }
            = new HashSet<string>(new[]
            {
                ConfigurationConstants.PackageSourceAttribute,
                ConfigurationConstants.PathAttribute,
                ConfigurationConstants.PasswordAttribute,
                ConfigurationConstants.ClearTextPasswordAttribute
            });

        protected override IReadOnlyCollection<string> RequiredAttributes { get; }
            = new HashSet<string>(new[]
            {
                ConfigurationConstants.PackageSourceAttribute,
                ConfigurationConstants.PathAttribute
            });

        internal override XNode AsXNode()
        {
            if (Node is XElement) return Node;

            var element = new XElement(ElementName);
            foreach (KeyValuePair<string, string> attr in Attributes) element.SetAttributeValue(attr.Key, attr.Value);
            return element;
        }

        public override SettingBase Clone()
        {
            return new FileClientCertItem(PackageSource, FilePath, Password, IsPasswordIsClearText, Origin);
        }

        public override IEnumerable<X509Certificate> Search()
        {
            var filePath = FindAbsoluteFilePath();
            if (string.IsNullOrWhiteSpace(filePath)) throw new OpenStackConfigurationException(string.Format(CultureInfo.CurrentCulture, "FileCertItemPathFileNotExist"));

            return new[] { string.IsNullOrWhiteSpace(Password) ? new X509Certificate2(filePath) : new X509Certificate2(filePath, Password) };
        }

        public void Update(string filePath, string password, bool storePasswordInClearText)
        {
            if (!string.IsNullOrWhiteSpace(filePath)) AddOrUpdateAttribute(ConfigurationConstants.PathAttribute, filePath);

            if (!string.IsNullOrWhiteSpace(password))
                if (storePasswordInClearText)
                {
                    AddOrUpdateAttribute(ConfigurationConstants.ClearTextPasswordAttribute, password);
                    UpdateAttribute(ConfigurationConstants.PasswordAttribute, null);
                }
                else
                {
                    var encryptedPassword = EncryptionUtility.EncryptString(password);
                    AddOrUpdateAttribute(ConfigurationConstants.PasswordAttribute, encryptedPassword);
                    UpdateAttribute(ConfigurationConstants.ClearTextPasswordAttribute, null);
                }
        }

        string FindAbsoluteFilePath()
        {
            var originalValue = FilePath;

            if (File.Exists(originalValue)) return originalValue;  // File was found by absolute file path
            if (PathValidator.IsValidRelativePath(originalValue) && Origin != null)
            {
                //File was found by relative to config file path
                var relativeToConfigFilePath = PathUtility.GetAbsolutePath(PathUtility.EnsureTrailingSlash(Origin.DirectoryPath), originalValue);
                if (File.Exists(relativeToConfigFilePath)) return relativeToConfigFilePath;
            }

            //File was not found
            return null;
        }
    }
}
