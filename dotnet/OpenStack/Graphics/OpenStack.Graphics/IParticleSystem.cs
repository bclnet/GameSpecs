using System.Collections.Generic;

namespace OpenStack.Graphics
{
    public interface IParticleSystem
    {
        IDictionary<string, object> Data { get; }
        IEnumerable<IDictionary<string, object>> Renderers { get; }
        IEnumerable<IDictionary<string, object>> Operators { get; }
        IEnumerable<IDictionary<string, object>> Initializers { get; }
        IEnumerable<IDictionary<string, object>> Emitters { get; }
        IEnumerable<string> GetChildParticleNames(bool enabledOnly = false);
    }
}
