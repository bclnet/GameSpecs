//#define DEBUG_SHADERS
using OpenStack.Graphics.Algorithms;
using OpenStack.Graphics.Renderer1;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenStack.Graphics.OpenGL
{
    public abstract class ShaderLoader
    {
        const int ShaderSeed = 0x13141516;

#if !DEBUG_SHADERS || !DEBUG
        readonly Dictionary<uint, Shader> CachedShaders = new Dictionary<uint, Shader>();
        readonly Dictionary<string, List<string>> ShaderDefines = new Dictionary<string, List<string>>();

        uint CalculateShaderCacheHash(string shaderFileName, IDictionary<string, bool> args)
        {
            var b = new StringBuilder();
            b.AppendLine(shaderFileName);
            var parameters = ShaderDefines[shaderFileName].Intersect(args.Keys);
            foreach (var key in parameters)
            {
                b.AppendLine(key);
                b.AppendLine(args[key] ? "t" : "f");
            }
            return MurmurHash2.Hash(b.ToString(), ShaderSeed);
        }
#endif

        protected abstract string GetShaderFileByName(string shaderName);

        protected abstract string GetShaderSource(string shaderName);

        public Shader LoadShader(string shaderName, IDictionary<string, bool> args)
        {
            var shaderFileName = GetShaderFileByName(shaderName);

#if !DEBUG_SHADERS || !DEBUG
            if (ShaderDefines.ContainsKey(shaderFileName))
            {
                var shaderCacheHash = CalculateShaderCacheHash(shaderFileName, args);
                if (CachedShaders.TryGetValue(shaderCacheHash, out var cachedShader)) return cachedShader;
            }
#endif

            var defines = new List<string>();

            // Vertex shader
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            {
                var shaderSource = GetShaderSource($"{shaderFileName}.vert");
                GL.ShaderSource(vertexShader, PreprocessVertexShader(shaderSource, args));
                // Find defines supported from source
                defines.AddRange(FindDefines(shaderSource));
            }
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out var shaderStatus);
            if (shaderStatus != 1)
            {
                GL.GetShaderInfoLog(vertexShader, out var vsInfo);
                throw new Exception($"Error setting up Vertex Shader \"{shaderName}\": {vsInfo}");
            }

            // Fragment shader
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            {
                var shaderSource = GetShaderSource($"{shaderFileName}.frag");
                GL.ShaderSource(fragmentShader, UpdateDefines(shaderSource, args));
                // Find render modes supported from source, take union to avoid duplicates
                defines = defines.Union(FindDefines(shaderSource)).ToList();
            }
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out shaderStatus);
            if (shaderStatus != 1)
            {
                GL.GetShaderInfoLog(fragmentShader, out var fsInfo);
                throw new Exception($"Error setting up Fragment Shader \"{shaderName}\": {fsInfo}");
            }

            const string RenderMode = "renderMode_";
            var renderModes = defines
                .Where(k => k.StartsWith(RenderMode))
                .Select(k => k[RenderMode.Length..])
                .ToList();

            var shader = new Shader(GL.GetUniformLocation)
            {
                Name = shaderName,
                Parameters = args,
                Program = GL.CreateProgram(),
                RenderModes = renderModes,
            };
            GL.AttachShader(shader.Program, vertexShader);
            GL.AttachShader(shader.Program, fragmentShader);
            GL.LinkProgram(shader.Program);
            GL.ValidateProgram(shader.Program);
            GL.GetProgram(shader.Program, GetProgramParameterName.LinkStatus, out var linkStatus);
            if (linkStatus != 1)
            {
                GL.GetProgramInfoLog(shader.Program, out var linkInfo);
                throw new Exception($"Error linking shaders: {linkInfo} (link status = {linkStatus})");
            }

            GL.DetachShader(shader.Program, vertexShader);
            GL.DeleteShader(vertexShader);
            GL.DetachShader(shader.Program, fragmentShader);
            GL.DeleteShader(fragmentShader);

#if !DEBUG_SHADERS || !DEBUG
            ShaderDefines[shaderFileName] = defines;
            var newShaderCacheHash = CalculateShaderCacheHash(shaderFileName, args);
            CachedShaders[newShaderCacheHash] = shader;
            Console.WriteLine($"Shader {newShaderCacheHash} ({shaderName}) ({string.Join(", ", args.Keys)}) compiled and linked succesfully");
#endif
            return shader;
        }

        public Shader LoadPlaneShader(string shaderName, IDictionary<string, bool> args)
        {
            var shaderFileName = GetShaderFileByName(shaderName);

            // Vertex shader
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            {
                var shaderSource = GetShaderSource($"plane.vert");
                GL.ShaderSource(vertexShader, shaderSource);
            }
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out var shaderStatus);
            if (shaderStatus != 1)
            {
                GL.GetShaderInfoLog(vertexShader, out var vsInfo);
                throw new Exception($"Error setting up Vertex Shader \"{shaderName}\": {vsInfo}");
            }

            // Fragment shader
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            {
                var shaderSource = GetShaderSource($"{shaderFileName}.frag");
                GL.ShaderSource(fragmentShader, UpdateDefines(shaderSource, args));
            }
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out shaderStatus);
            if (shaderStatus != 1)
            {
                GL.GetShaderInfoLog(fragmentShader, out var fsInfo);
                throw new Exception($"Error setting up Fragment Shader \"{shaderName}\": {fsInfo}");
            }

            var shader = new Shader(GL.GetUniformLocation)
            {
                Name = shaderName,
                Program = GL.CreateProgram(),
            };
            GL.AttachShader(shader.Program, vertexShader);
            GL.AttachShader(shader.Program, fragmentShader);
            GL.LinkProgram(shader.Program);
            GL.ValidateProgram(shader.Program);
            GL.GetProgram(shader.Program, GetProgramParameterName.LinkStatus, out var linkStatus);
            if (linkStatus != 1)
            {
                GL.GetProgramInfoLog(shader.Program, out var linkInfo);
                throw new Exception($"Error linking shaders: {linkInfo} (link status = {linkStatus}");
            }

            GL.DetachShader(shader.Program, vertexShader);
            GL.DeleteShader(vertexShader);
            GL.DetachShader(shader.Program, fragmentShader);
            GL.DeleteShader(fragmentShader);
            return shader;
        }

        // Preprocess a vertex shader's source to include the #version plus #defines for parameters
        string PreprocessVertexShader(string source, IDictionary<string, bool> args)
        {
            // Update parameter defines
            var paramSource = UpdateDefines(source, args);
            // Inject code into shader based on #includes
            var includedSource = ResolveIncludes(paramSource);
            return includedSource;
        }

        // Update default defines with possible overrides from the model
        static string UpdateDefines(string source, IDictionary<string, bool> args)
        {
            // Find all #define param_(paramName) (paramValue) using regex
            var defines = Regex.Matches(source, @"#define param_(\S*?) (\S*?)\s*?\n");
            foreach (Match define in defines)
                // Check if this parameter is in the arguments
                if (args.TryGetValue(define.Groups[1].Value, out var value))
                {
                    // Overwrite default value
                    var index = define.Groups[2].Index;
                    var length = define.Groups[2].Length;
                    source = source.Remove(index, Math.Min(length, source.Length - index)).Insert(index, value ? "1" : "0");
                }

            return source;
        }

        // Remove any #includes from the shader and replace with the included code
        string ResolveIncludes(string source)
        {
            var includes = Regex.Matches(source, @"#include ""([^""]*?)"";?\s*\n");
            foreach (Match define in includes)
            {
                // Read included code
                var includedCode = GetShaderSource(define.Groups[1].Value);
                // Recursively resolve includes in the included code. (Watch out for cyclic dependencies!)
                includedCode = ResolveIncludes(includedCode);
                if (!includedCode.EndsWith("\n")) includedCode += "\n";
                // Replace the include with the code
                source = source.Replace(define.Value, includedCode);
            }

            return source;
        }

        static List<string> FindDefines(string source)
        {
            var defines = Regex.Matches(source, @"#define param_(\S+)");
            return defines.Cast<Match>().Select(_ => _.Groups[1].Value).ToList();
        }
    }
}
