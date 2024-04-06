using System.Collections.Generic;

namespace OpenStack.Configuration
{
    /// <summary>
    /// Provides a common facility for locating extensions
    /// </summary>
    public interface IExtensionLocator
    {
        /// <summary>
        /// Find paths to all extensions
        /// </summary>
        IEnumerable<string> FindExtensions();

        /// <summary>
        /// Find paths to all credential providers
        /// </summary>
        IEnumerable<string> FindCredentialProviders();
    }
}
