//#define DEBUG_SHADERS
using System.IO;
using System.Reflection;

namespace OpenStack.Graphics.OpenGL
{
    public class ShaderDebugLoader : ShaderLoader
    {
        const string ShaderDirectory = "OpenStack.Graphics.OpenGL.Shaders";

        // Map shader names to shader files
        protected override string GetShaderFileByName(string shaderName)
        {
            switch (shaderName)
            {
                case "plane": return "plane";
                case "vrf.error": return "error";
                case "vrf.grid": return "debug_grid";
                case "vrf.picking": return "picking";
                case "vrf.particle.sprite": return "particle_sprite";
                case "vrf.particle.trail": return "particle_trail";
                case "tools_sprite.vfx": return "sprite";
                case "vr_unlit.vfx": return "vr_unlit";
                case "vr_black_unlit.vfx": return "vr_black_unlit";
                case "water_dota.vfx": return "water";
                case "hero.vfx":
                case "hero_underlords.vfx": return "dota_hero";
                case "multiblend.vfx": return "multiblend";
                default:
                    if (shaderName.StartsWith("vr_")) return "vr_standard";
                    // Console.WriteLine($"Unknown shader {shaderName}, defaulting to simple.");
                    // Shader names that are supposed to use this:
                    // vr_simple.vfx
                    return "simple";
            }
        }

        protected override string GetShaderSource(string shaderFileName)
        {
#if DEBUG_SHADERS && DEBUG
            using (var stream = File.Open(GetShaderDiskPath(shaderFileName), FileMode.Open))
#else
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream($"{ShaderDirectory}.{shaderFileName}"))
#endif
            using (var reader = new StreamReader(stream)) return reader.ReadToEnd();
        }

#if DEBUG_SHADERS && DEBUG
        // Reload shaders at runtime
         static string GetShaderDiskPath(string name) => Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName), "../../../../", ShaderDirectory.Replace(".", "/"), name);
#endif
    }
}
