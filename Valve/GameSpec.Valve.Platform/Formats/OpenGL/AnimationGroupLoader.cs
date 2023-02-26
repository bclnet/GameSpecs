using GameSpec.Valve.Formats.Blocks;
using GameSpec.Valve.Formats.Blocks.Animation.SegmentDecoders;
using OpenStack;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.Valve.Formats.OpenGL
{
    public static class AnimationGroupLoader
    {
        static IDictionary<string, object> GetData(BinaryPak resource) => resource.DATA is DATABinaryNTRO ntro
           ? ntro.Data
           : ((DATABinaryKV3)resource.DATA).Data;

        public static IEnumerable<CCompressedAnimQuaternion> LoadAnimationGroup(IOpenGLGraphic graphic, BinaryPak resource)
        {
            var data = GetData(resource);
            var animArray = data.Get<string[]>("m_localHAnimArray").Where(a => a != null); // Get the list of animation files
            var decodeKey = data.GetSub("m_decodeKey"); // Get the key to decode the animations

            // Load animation files
            var list = new List<CCompressedAnimQuaternion>();
            foreach (var animationFile in animArray) list.AddRange(LoadAnimationFile(graphic, animationFile, decodeKey));
            return list;
        }

        public static IEnumerable<CCompressedAnimQuaternion> TryLoadSingleAnimationFileFromGroup(IOpenGLGraphic graphic, BinaryPak resource, string animationName)
        {
            var data = GetData(resource);
            var animArray = data.Get<string[]>("m_localHAnimArray").Where(a => a != null); // Get the list of animation files
            var decodeKey = data.GetSub("m_decodeKey"); // Get the key to decode the animations
            // Load animation files
            var animation = animArray.FirstOrDefault(a => a != null && a.EndsWith($"{animationName}.vanim"));
            return animation != default ? LoadAnimationFile(graphic, animation, decodeKey) : null;
        }

        static IEnumerable<CCompressedAnimQuaternion> LoadAnimationFile(IOpenGLGraphic graphic, string animationFile, IDictionary<string, object> decodeKey)
        {
            var animResource = graphic.LoadFileObjectAsync<BinaryPak>(animationFile).Result ?? throw new FileNotFoundException($"Failed to load {animationFile}_c. Did you configure game paths correctly?");
            // Build animation classes
            return CCompressedAnimQuaternion.FromResource(animResource, decodeKey);
        }
    }
}
