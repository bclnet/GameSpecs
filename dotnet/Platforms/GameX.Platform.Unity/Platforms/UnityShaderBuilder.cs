using System.Collections.Generic;
using UnityEngine;

namespace GameX.Platforms
{
    public class UnityShaderBuilder : ShaderBuilderBase<Shader>
    {
        public override Shader BuildShader(string path, IDictionary<string, bool> args) => Shader.Find(path);
        public override Shader BuildPlaneShader(string path, IDictionary<string, bool> args) => Shader.Find(path);
    }
}