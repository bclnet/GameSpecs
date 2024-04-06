using System;
using System.Collections.Generic;
using System.Linq;

namespace GameX.Valve.Formats.Animations
{
    //was:Resource/ResourceTypes/ModelAnimation/AnimationDataChannel
    public class AnimationDataChannel
    {
        public int[] RemapTable { get; } // Bone ID => Element Index
        public string ChannelAttribute { get; }

        public AnimationDataChannel(Skeleton skeleton, IDictionary<string, object> dataChannel, int channelElements)
        {
            RemapTable = Enumerable.Range(0, skeleton.Bones.Length).Select(_ => -1).ToArray();
            var elementNameArray = dataChannel.Get<string[]>("m_szElementNameArray");
            var elementIndexArray = dataChannel.Get<int[]>("m_nElementIndexArray");
            for (var i = 0; i < elementIndexArray.Length; i++)
            {
                var elementName = elementNameArray[i];
                var boneID = Array.FindIndex(skeleton.Bones, bone => bone.Name == elementName);
                if (boneID != -1) RemapTable[boneID] = (int)elementIndexArray[i];
            }
            ChannelAttribute = dataChannel.Get<string>("m_szVariableName");
        }
    }
}
