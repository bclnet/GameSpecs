using System.IO;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public static class GlslShaders
    {
        static Lazy<string> GetLazyDocument(string path) => new(() =>
        {
            using var resource = new StreamReader(typeof(GlslShaders).Assembly.GetManifestResourceStream($"System.NumericsX.OpenStack.Gngine.Render.GlslShaders{path}.glsl"));
            return resource.ReadToEnd();
        });

        public static readonly Lazy<string> blendLightShaderVP = GetLazyDocument(nameof(blendLightShaderVP));
        public static readonly Lazy<string> cubeMapShaderFP = GetLazyDocument(nameof(cubeMapShaderFP));
        public static readonly Lazy<string> diffuseCubeShaderVP = GetLazyDocument(nameof(diffuseCubeShaderVP));
        public static readonly Lazy<string> diffuseMapShaderFP = GetLazyDocument(nameof(diffuseMapShaderFP));
        public static readonly Lazy<string> diffuseMapShaderVP = GetLazyDocument(nameof(diffuseMapShaderVP));
        public static readonly Lazy<string> fogShaderFP = GetLazyDocument(nameof(fogShaderFP));
        public static readonly Lazy<string> fogShaderVP = GetLazyDocument(nameof(fogShaderVP));
        public static readonly Lazy<string> interactionPhongShaderFP = GetLazyDocument(nameof(interactionPhongShaderFP));
        public static readonly Lazy<string> interactionPhongShaderVP = GetLazyDocument(nameof(interactionPhongShaderVP));
        public static readonly Lazy<string> interactionShaderFP = GetLazyDocument(nameof(interactionShaderFP));
        public static readonly Lazy<string> interactionShaderVP = GetLazyDocument(nameof(interactionShaderVP));
        public static readonly Lazy<string> reflectionCubeShaderVP = GetLazyDocument(nameof(reflectionCubeShaderVP));
        public static readonly Lazy<string> skyboxCubeShaderVP = GetLazyDocument(nameof(skyboxCubeShaderVP));
        public static readonly Lazy<string> stencilShadowShaderFP = GetLazyDocument(nameof(stencilShadowShaderFP));
        public static readonly Lazy<string> stencilShadowShaderVP = GetLazyDocument(nameof(stencilShadowShaderVP));
        public static readonly Lazy<string> zfillClipShaderFP = GetLazyDocument(nameof(zfillClipShaderFP));
        public static readonly Lazy<string> zfillClipShaderVP = GetLazyDocument(nameof(zfillClipShaderVP));
        public static readonly Lazy<string> zfillShaderFP = GetLazyDocument(nameof(zfillShaderFP));
        public static readonly Lazy<string> zfillShaderVP = GetLazyDocument(nameof(zfillShaderVP));
    }
}
