using System.Collections.Generic;
using System.Linq;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/World
    public class DATAWorld : DATABinaryKV3OrNTRO
    {
        public IEnumerable<string> GetEntityLumpNames() => Data.Get<string[]>("m_entityLumps");
        public IEnumerable<string> GetWorldNodeNames() => Data.GetArray("m_worldNodes").Select(nodeData => nodeData.Get<string>("m_worldNodePrefix")).ToList();
    }
}
