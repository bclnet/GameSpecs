using System.Collections.Generic;
using UnityEngine;

namespace GameSpec.Graphics
{
    public class UnityShaderBuilder : AbstractShaderBuilder<Shader>
    {
        public override Shader BuildShader(string path, IDictionary<string, bool> arguments) => Shader.Find(path);
        public override Shader BuildPlaneShader(string path, IDictionary<string, bool> arguments) => Shader.Find(path);
    }
}