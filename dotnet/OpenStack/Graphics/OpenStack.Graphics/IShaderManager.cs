using System.Collections.Generic;

namespace OpenStack.Graphics
{
    public interface IShaderManager<Shader>
    {
        public Shader LoadShader(string path, IDictionary<string, bool> args = null);
        public Shader LoadPlaneShader(string path, IDictionary<string, bool> args = null);
    }
}