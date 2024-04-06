using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem
{
    public interface INumberProvider
    {
        double NextNumber();
    }

    public class LiteralNumberProvider : INumberProvider
    {
        readonly double _value;
        public LiteralNumberProvider(double value) => _value = value;
        public double NextNumber() => _value;
    }

    public static class INumberProviderExtensions
    {
        public static INumberProvider GetNumberProvider(this IDictionary<string, object> keyValues, string propertyName, INumberProvider defaultValue = default)
        {
            if (!keyValues.TryGetValue(propertyName, out var property)) return defaultValue;

            if (property is IDictionary<string, object> numberProviderParameters)
            {
                var type = numberProviderParameters.Get<string>("m_nType");
                return type switch
                {
                    "PF_TYPE_LITERAL" => new LiteralNumberProvider(numberProviderParameters.GetDouble("m_flLiteralValue")),
                    _ => throw new InvalidCastException($"Could not create number provider of type {type}."),
                };
            }
            else return new LiteralNumberProvider(Convert.ToDouble(property));
        }

        public static int NextInt(this INumberProvider numberProvider)
            => (int)numberProvider.NextNumber();
    }
}
