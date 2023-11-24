using GameSpec.Metadata;
using GameSpec.Cig.Apps.DataForge;
using System;
using System.Resources;
using System.Threading.Tasks;

namespace GameSpec.Cig.Apps
{
    /// <summary>
    /// DataForgeApp
    /// </summary>
    /// <seealso cref="FamilyApp" />
    public class DataForgeApp : FamilyApp
    {
        public readonly Database Db = new Database();

        public override async Task OpenAsync(Type explorerType, MetadataManager manager)
        {
            await Db.OpenAsync(manager);
            await base.OpenAsync(explorerType, manager);
        }
    }
}