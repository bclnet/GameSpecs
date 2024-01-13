using System.Collections.Generic;

namespace GameSpec.Platforms
{
    public abstract class ShaderBuilderBase<Shader>
    {
        public abstract Shader BuildShader(string path, IDictionary<string, bool> args);
        public abstract Shader BuildPlaneShader(string path, IDictionary<string, bool> args);
    }
}
