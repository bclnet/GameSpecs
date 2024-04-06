using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.Renderer1
{
    public class Shader
    {
        readonly Func<int, string, int> _getUniformLocation;

        public Shader(Func<int, string, int> getUniformLocation)
            => _getUniformLocation = getUniformLocation ?? throw new ArgumentNullException(nameof(getUniformLocation)); //: GL.GetUniformLocation

        public string Name { get; set; }
        public int Program { get; set; }
#pragma warning disable CA2227 // Collection properties should be read only
        public IDictionary<string, bool> Parameters { get; set; }
        public List<string> RenderModes { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        Dictionary<string, int> Uniforms { get; } = new Dictionary<string, int>();

        public int GetUniformLocation(string name)
        {
            if (Uniforms.TryGetValue(name, out var value)) return value;
            value = _getUniformLocation(Program, name);
            Uniforms[name] = value;
            return value;
        }
    }
}
