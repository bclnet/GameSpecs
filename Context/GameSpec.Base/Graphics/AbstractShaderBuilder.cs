using System.Collections.Generic;

namespace GameSpec.Graphics
{
    public abstract class AbstractShaderBuilder<Shader>
    {
        public abstract Shader BuildShader(string path, IDictionary<string, bool> arguments);
        public abstract Shader BuildPlaneShader(string path, IDictionary<string, bool> arguments);
    }
}
