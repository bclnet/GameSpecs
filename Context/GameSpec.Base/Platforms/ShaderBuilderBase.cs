using System.Collections.Generic;

namespace GameSpec.Platforms
{
    public abstract class ShaderBuilderBase<Shader>
    {
        public abstract Shader BuildShader(string path, IDictionary<string, bool> arguments);
        public abstract Shader BuildPlaneShader(string path, IDictionary<string, bool> arguments);
    }
}
