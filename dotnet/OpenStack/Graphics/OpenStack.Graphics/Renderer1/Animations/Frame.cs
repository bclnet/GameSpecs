using System;
using System.Numerics;

namespace OpenStack.Graphics.Renderer1.Animations
{
    public class Frame
    {
        public FrameBone[] Bones { get; }

        public Frame(ISkeleton skeleton)
        {
            Bones = new FrameBone[skeleton.Bones.Length];
            Clear(skeleton);
        }

        public void SetAttribute(int bone, ChannelAttribute attribute, Vector3 data)
        {
            switch (attribute)
            {
                case ChannelAttribute.Position: Bones[bone].Position = data; break;
#if DEBUG
                default: Console.WriteLine($"Unknown frame attribute '{attribute}' encountered with Vector3 data"); break;
#endif
            }
        }

        public void SetAttribute(int bone, ChannelAttribute attribute, Quaternion data)
        {
            switch (attribute)
            {
                case ChannelAttribute.Angle: Bones[bone].Angle = data; break;
#if DEBUG
                default: Console.WriteLine($"Unknown frame attribute '{attribute}' encountered with Quaternion data"); break;
#endif
            }
        }

        public void SetAttribute(int bone, ChannelAttribute attribute, float data)
        {
            switch (attribute)
            {
                case ChannelAttribute.Scale: Bones[bone].Scale = data; break;
#if DEBUG
                default: Console.WriteLine($"Unknown frame attribute '{attribute}' encountered with float data"); break;
#endif
            }
        }

        /// <summary>
        /// Resets frame bones to their bind pose.
        /// Should be used on animation change.
        /// </summary>
        /// <param name="skeleton">The same skeleton that was passed to the constructor.</param>
        public void Clear(ISkeleton skeleton)
        {
            for (var i = 0; i < Bones.Length; i++)
            {
                Bones[i].Position = skeleton.Bones[i].Position;
                Bones[i].Angle = skeleton.Bones[i].Angle;
                Bones[i].Scale = 1;
            }
        }
    }
}
