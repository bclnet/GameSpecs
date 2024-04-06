using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem
{
    public interface IVectorProvider
    {
        Vector3 NextVector();
    }

    public class LiteralVectorProvider : IVectorProvider
    {
        readonly Vector3 _value;
        public LiteralVectorProvider(Vector3 value) => _value = value;
        public LiteralVectorProvider(double[] value) => _value = new Vector3((float)value[0], (float)value[1], (float)value[2]);
        public Vector3 NextVector() => _value;
    }

    public static class IVectorProviderExtensions
    {
        public static IVectorProvider GetVectorProvider(this IDictionary<string, object> keyValues, string propertyName, IVectorProvider defaultValue = default)
        {
            if (!keyValues.TryGetValue(propertyName, out var property)) return defaultValue;
            if (property is IDictionary<string, object> numberProviderParameters && numberProviderParameters.ContainsKey("m_nType"))
            {
                var type = numberProviderParameters.Get<string>("m_nType");
                return type switch
                {
                    "PVEC_TYPE_LITERAL" => new LiteralVectorProvider(numberProviderParameters.Get<double[]>("m_vLiteralValue")),
                    _ => throw new InvalidCastException($"Could not create vector provider of type {type}."),
                };
            }
            return new LiteralVectorProvider(keyValues.Get<double[]>(propertyName));
        }
    }
}
