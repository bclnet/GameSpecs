using StereoKit;
using System.Collections.Generic;

namespace GameSpec.Platforms
{
    public class StereoKitShaderBuilder : ShaderBuilderBase<Shader>
    {
        public override Shader BuildShader(string path, IDictionary<string, bool> arguments) => Shader.FromFile(path);
        public override Shader BuildPlaneShader(string path, IDictionary<string, bool> arguments) => Shader.FromFile(path);
    }
}