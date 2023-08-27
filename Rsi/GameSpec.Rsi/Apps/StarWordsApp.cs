using GameSpec.Metadata;
using GameSpec.Rsi.Apps.StarWords;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace GameSpec.Rsi.Apps
{
    /// <summary>
    /// StarWordsApp
    /// </summary>
    /// <seealso cref="FamilyApp" />
    public class StarWordsApp : FamilyApp
    {
        public readonly Database Db = new Database();

        public override async Task OpenAsync(Type explorerType, MetadataManager manager)
        {
            await Db.OpenAsync(manager);
            await base.OpenAsync(explorerType, manager);
        }
    }
}