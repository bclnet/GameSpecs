using GameX.Meta;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameX.Cig.Apps
{
    /// <summary>
    /// SubsumptionApp
    /// </summary>
    /// <seealso cref="FamilyApp" />
    public class SubsumptionApp : FamilyApp
    {
        public SubsumptionApp(Family family, string id, JsonElement elem) : base(family, id, elem) { }

        public override Task OpenAsync(Type explorerType, MetaManager manager) => base.OpenAsync(explorerType, manager);
    }
}