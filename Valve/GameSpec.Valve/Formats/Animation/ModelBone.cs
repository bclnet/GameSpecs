using System.Collections.Generic;
using System.Numerics;

namespace GameSpec.Valve.Formats.Blocks.Animation
{
    public class ModelBone
    {
        public ModelBone Parent { get; private set; }
        public List<ModelBone> Children { get; } = new List<ModelBone>();

        public string Name { get; }
        public List<int> SkinIndices { get; }

        public Vector3 Position { get; }
        public Quaternion Angle { get; }

        public Matrix4x4 BindPose { get; }
        public Matrix4x4 InverseBindPose { get; }

        public ModelBone(string name, List<int> index, Vector3 position, Quaternion rotation)
        {
            Name = name;
            SkinIndices = index;
            Position = position;
            Angle = rotation;
            // Calculate matrices
            BindPose = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
            Matrix4x4.Invert(BindPose, out var inverseBindPose);
            InverseBindPose = inverseBindPose;
        }

        public void AddChild(ModelBone child) => Children.Add(child);

        public void SetParent(ModelBone parent) => Parent = parent;
    }
}
