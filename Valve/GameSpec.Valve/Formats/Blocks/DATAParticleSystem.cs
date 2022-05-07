using OpenStack.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace GameSpec.Valve.Formats.Blocks
{
    public class DATAParticleSystem : DATABinaryKV3OrNTRO, IParticleSystemInfo
    {
        public IEnumerable<IDictionary<string, object>> Renderers => Data.GetArray("m_Renderers") ?? Enumerable.Empty<IDictionary<string, object>>();

        public IEnumerable<IDictionary<string, object>> Operators => Data.GetArray("m_Operators") ?? Enumerable.Empty<IDictionary<string, object>>();

        public IEnumerable<IDictionary<string, object>> Initializers => Data.GetArray("m_Initializers") ?? Enumerable.Empty<IDictionary<string, object>>();

        public IEnumerable<IDictionary<string, object>> Emitters => Data.GetArray("m_Emitters") ?? Enumerable.Empty<IDictionary<string, object>>();

        public IEnumerable<string> GetChildParticleNames(bool enabledOnly = false)
        {
            var children = Data.GetArray("m_Children");
            if (children == null) return Enumerable.Empty<string>();
            if (enabledOnly) children = children.Where(c => !c.ContainsKey("m_bDisableChild") || !c.Get<bool>("m_bDisableChild")).ToArray();
            return children.Select(c => c.Get<string>("m_ChildRef")).ToList();
        }
    }
}
