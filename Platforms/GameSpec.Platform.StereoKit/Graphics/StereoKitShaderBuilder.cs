using StereoKit;
using System.Collections.Generic;

namespace GameSpec.Graphics
{
    public class StereoKitShaderBuilder : AbstractShaderBuilder<Shader>
    {
        public override Shader BuildShader(string path, IDictionary<string, bool> arguments) => Shader.FromFile(path);
        public override Shader BuildPlaneShader(string path, IDictionary<string, bool> arguments) => Shader.FromFile(path);
    }
}