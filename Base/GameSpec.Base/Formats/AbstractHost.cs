using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    /// <summary>
    /// AbstractHost
    /// </summary>
    public abstract class AbstractHost
    {
        /// <summary>
        /// Gets the set asynchronous.
        /// </summary>
        /// <param name="shouldThrow">if set to <c>true</c> [should throw].</param>
        /// <returns></returns>
        public abstract Task<HashSet<string>> GetSetAsync(bool shouldThrow = false);

        /// <summary>
        /// Gets the file asynchronous.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="shouldThrow">if set to <c>true</c> [should throw].</param>
        /// <returns></returns>
        public abstract Task<Stream> GetFileAsync(string filePath, bool shouldThrow = false);
    }
}