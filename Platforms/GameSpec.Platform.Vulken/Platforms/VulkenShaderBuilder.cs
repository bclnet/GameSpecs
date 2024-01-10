using OpenStack.Graphics.OpenGL;
using OpenStack.Graphics.Renderer1;
using System.Collections.Generic;

namespace GameSpec.Platforms
{
    public class VulkenShaderBuilder : ShaderBuilderBase<Shader>
    {
        static readonly ShaderLoader _loader = new ShaderDebugLoader();

        public override Shader BuildShader(string path, IDictionary<string, bool> arguments) => _loader.LoadShader(path, arguments);
        public override Shader BuildPlaneShader(string path, IDictionary<string, bool> arguments) => _loader.LoadPlaneShader(path, arguments);
    }
}