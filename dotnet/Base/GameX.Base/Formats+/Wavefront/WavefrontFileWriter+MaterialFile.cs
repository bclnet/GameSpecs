using GameX.Formats.Unknown;
using System.IO;
using System.Reflection;
using static OpenStack.Debug;

namespace GameX.Formats.Wavefront
{
    public partial class WavefrontFileWriter
    {
        void WriteMaterialFile(IUnknownFileModel file)
        {
            if (file.Materials == null) { Log("No materials loaded"); return; }

            if (!MaterialFile.Directory.Exists) MaterialFile.Directory.Create();

            using var w = new StreamWriter(MaterialFile.FullName);
            w.WriteLine($"# gamer .mtl export version {Assembly.GetExecutingAssembly().GetName().Version}");
            w.WriteLine("#");
            foreach (var material in file.Materials)
            {
                w.WriteLine($"newmtl {material.Name}");
                if (material.Diffuse != null)
                {
                    var diffuse = material.Diffuse.Value;
                    w.WriteLine($"Ka {diffuse.X:F6} {diffuse.Y:F6} {diffuse.Z:F6}"); // Ambient
                    w.WriteLine($"Kd {diffuse.X:F6} {diffuse.Y:F6} {diffuse.Z:F6}"); // Diffuse
                }
                else Log($"Skipping Diffuse for {material.Name}");
                if (material.Specular != null)
                {
                    var specular = material.Specular.Value;
                    w.WriteLine($"Ks {specular.X:F6} {specular.Y:F6} {specular.Z:F6}"); // Specular
                    w.WriteLine($"Ns {material.Shininess / 255D:F6}"); // Specular Exponent
                }
                else Log($"Skipping Specular for {material.Name}");
                w.WriteLine($"d {material.Opacity:F6}"); // Dissolve
                w.WriteLine("illum 2"); // Highlight on. This is a guess.

                // Phong materials

                // 0. Color on and Ambient off
                // 1. Color on and Ambient on
                // 2. Highlight on
                // 3. Reflection on and Ray trace on
                // 4. Transparency: Glass on, Reflection: Ray trace on
                // 5. Reflection: Fresnel on and Ray trace on
                // 6. Transparency: Refraction on, Reflection: Fresnel off and Ray trace on
                // 7. Transparency: Refraction on, Reflection: Fresnel on and Ray trace on
                // 8. Reflection on and Ray trace off
                // 9. Transparency: Glass on, Reflection: Ray trace off
                // 10. Casts shadows onto invisible surfaces
                foreach (var texture in material.Textures)
                {
                    var texturePath = DataDir != null ? Path.Combine(DataDir.FullName, texture.Path) : texture.Path;
                    texturePath = !TiffTextures ? texturePath.Replace(".tif", ".dds") : texturePath.Replace(".dds", ".tif");
                    texturePath = texturePath.Replace(@"/", @"\");
                    if ((texture.Maps & IUnknownTexture.Map.Diffuse) != 0) w.WriteLine($"map_Kd {texturePath}");
                    if ((texture.Maps & IUnknownTexture.Map.Specular) != 0) { w.WriteLine($"map_Ks {texturePath}"); w.WriteLine($"map_Ns {texturePath}"); }
                    // <Texture Map="Detail" File="textures/unified_detail/metal/metal_scratches_a_detail.tif" />
                    if ((texture.Maps & (IUnknownTexture.Map.Bumpmap | IUnknownTexture.Map.Detail)) != 0)  w.WriteLine($"map_bump {texturePath}");
                    // <Texture Map="Heightmap" File="objects/spaceships/ships/aegs/gladius/textures/aegs_switches_buttons_disp.tif"/>
                    if ((texture.Maps & IUnknownTexture.Map.Heightmap) != 0) w.WriteLine($"disp {texturePath}");
                    // <Texture Map="Decal" File="objects/spaceships/ships/aegs/textures/interior/metal/aegs_int_metal_alum_bare_diff.tif"/>
                    if ((texture.Maps & IUnknownTexture.Map.Decal) != 0) w.WriteLine($"disp {texturePath}");
                    // <Texture Map="SubSurface" File="objects/spaceships/ships/aegs/textures/interior/atlas/aegs_int_atlas_retaliator_spec.tif"/>
                    if ((texture.Maps & IUnknownTexture.Map.SubSurface) != 0) w.WriteLine($"map_Ns {texturePath}");
                    // <Texture Map="BlendDetail" File="textures/unified_detail/metal/metal_scratches-01_detail.tif">
                    if ((texture.Maps & IUnknownTexture.Map.BlendDetail) != 0) { }
                    // <Texture Map="Opacity" File="objects/spaceships/ships/aegs/textures/interior/blend/interior_blnd_a_diff.tif"/>
                    if ((texture.Maps & IUnknownTexture.Map.Opacity) != 0) w.WriteLine($"map_d {texturePath}");
                    // <Texture Map="Environment" File="nearest_cubemap" TexType="7"/>
                    if ((texture.Maps & IUnknownTexture.Map.Environment) != 0) { }
                    // <Texture Map="Custom" File="objects/spaceships/ships/aegs/textures/interior/metal/aegs_int_metal_painted_red_ddna.tif"/>
                    if ((texture.Maps & IUnknownTexture.Map.Custom) != 0) { /*w.WriteLine($"decal {texturePath}");*/ }
                }
                w.WriteLine();
            }
        }
    }
}