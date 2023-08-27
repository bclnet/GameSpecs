using GameSpec.Metadata;
using System;
using System.Resources;
using System.Threading.Tasks;

namespace GameSpec.Rsi.Apps
{
    /// <summary>
    /// SubsumptionApp
    /// </summary>
    /// <seealso cref="FamilyApp" />
    public class SubsumptionApp : FamilyApp
    {
        public override Task OpenAsync(Type explorerType, MetadataManager manager)
        {
            return base.OpenAsync(explorerType, manager);
        }
    }
}