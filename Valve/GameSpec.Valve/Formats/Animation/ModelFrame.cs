using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameSpec.Valve.Formats.Blocks.Animation
{
    public class ModelFrame
    {
        public Dictionary<string, ModelFrameBone> Bones { get; } = new Dictionary<string, ModelFrameBone>();

        public void SetAttribute(string bone, string attribute, object data)
        {
            switch (attribute)
            {
                case "Position": InsertIfUnknown(bone); Bones[bone].Position = (Vector3)data; break;
                case "Angle": InsertIfUnknown(bone); Bones[bone].Angle = (Quaternion)data; break;
                case "data": break;
#if DEBUG
                default: Console.WriteLine($"Unknown frame attribute '{attribute}' encountered"); break;
#endif
            }
        }

        void InsertIfUnknown(string name)
        {
            if (!Bones.ContainsKey(name))
                Bones[name] = new ModelFrameBone(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1));
        }
    }
}
