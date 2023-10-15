using GameSpec.Metadata;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace GameSpec
{
    /// <summary>
    /// FamilyApp
    /// </summary>
    public class FamilyApp
    {
        /// <summary>
        /// Gets or sets the family.
        /// </summary>
        public Family Family { get; set; }
        /// <summary>
        /// Gets or sets the game identifier.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the game name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the explorer type.
        /// </summary>
        public Type ExplorerType { get; set; }
        /// <summary>
        /// Gets or sets the explorer2 type.
        /// </summary>
        public Type Explorer2Type { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name;

        /// <summary>
        /// Gets or sets the game name.
        /// </summary>
        public virtual Task OpenAsync(Type explorerType, MetadataManager manager)
        {
            var explorer = Activator.CreateInstance(explorerType);
            var startupMethod = explorerType.GetMethod("Application_Startup", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new ArgumentOutOfRangeException(nameof(explorerType), "No Application_Startup found");
            startupMethod.Invoke(explorer, new object[] { this, null });
            return Task.CompletedTask;
        }
    }
}