using GameX.Valve.Formats.Blocks;
using OpenStack.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace GameX.Valve.Formats.Animations
{
    //was:Resource/ResourceTypes/ModelAnimation/AnimationGroupLoader
    public static class AnimationGroupLoader
    {
        public static IEnumerable<Animation> LoadAnimationGroup(Binary_Pak resource, IOpenGraphic graphic, Skeleton skeleton)
        {
            var data = resource.DATA.AsKeyValue();
            var decodeKey = data.GetSub("m_decodeKey"); // Get the key to decode the animations

            // Load animation files
            var animationList = new List<Animation>();
            if (resource.ContainsBlockType<ANIM>())
            {
                var animBlock = (DATABinaryKV3OrNTRO)resource.GetBlockByType<ANIM>();
                animationList.AddRange(Animation.FromData(animBlock.Data, decodeKey, skeleton));
                return animationList;
            }
            var animArray = data.Get<string[]>("m_localHAnimArray").Where(a => a != null); // Get the list of animation files
            foreach (var animationFile in animArray)
            {
                var animResource = graphic.LoadFileObject<Binary_Pak>($"{animationFile}_c").Result;
                if (animResource != null) animationList.AddRange(Animation.FromResource(animResource, decodeKey, skeleton)); // Build animation classes
            }
            return animationList;
        }
    }
}
