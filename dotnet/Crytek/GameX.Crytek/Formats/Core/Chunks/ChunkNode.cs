using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class ChunkNode : Chunk // cccc000b:   Node
    {
        protected float VERTEX_SCALE = 1f / 100;

        /// <summary>Chunk Name (String[64])</summary>
        public string Name { get; internal set; }
        /// <summary>Mesh or Helper Object ID</summary>
        public int ObjectNodeID { get; internal set; }
        /// <summary>Node parent.  if 0xFFFFFFFF, it's the top node.  Maybe...</summary>
        public int ParentNodeID { get; internal set; }  // Parent nodeID
        public int __NumChildren;
        /// <summary>Material ID for this chunk</summary>
        public int MatID { get; internal set; }
        public bool IsGroupHead { get; internal set; }
        public bool IsGroupMember { get; internal set; }
        /// <summary>Transformation Matrix</summary>
        public Matrix4x4 Transform { get; internal set; }
        /// <summary>Position vector of Transform</summary>
        public Vector3 Pos { get; internal set; }
        /// <summary>Rotation component of Transform</summary>
        public Quaternion Rot { get; internal set; }
        /// <summary>Scalar component of Transform</summary>
        public Vector3 Scale { get; internal set; }
        /// <summary>Position Controller ID - Obsolete</summary>
        public int PosCtrlID { get; internal set; }
        /// <summary>Rotation Controller ID - Obsolete</summary>
        public int RotCtrlID { get; internal set; }
        /// <summary>Scalar Controller ID - Obsolete</summary>
        public int SclCtrlID { get; internal set; }
        /// <summary>Appears to be a Blob of properties, separated by new lines</summary>
        public string Properties { get; internal set; }

        // Calculated Properties
        public Matrix4x4 LocalTransform
            => Matrix4x4.Transpose(Transform);

        ChunkNode _parentNode;
        public ChunkNode ParentNode
        {
            get
            {
                if (ParentNodeID == ~0) return null; // aka 0xFFFFFFFF, or -1
                if (_parentNode == null) _parentNode = _model.ChunkMap.TryGetValue(ParentNodeID, out var node) ? node as ChunkNode : _model.RootNode;
                return _parentNode;
            }
            set
            {
                ParentNodeID = value == null ? ~0 : value.ID;
                _parentNode = value;
            }
        }

        public List<ChunkNode> ChildNodes { get; set; }

        Chunk _objectChunk;
        public Chunk ObjectChunk
        {
            get
            {
                if (_objectChunk == null) _model.ChunkMap.TryGetValue(ObjectNodeID, out _objectChunk);
                return _objectChunk;
            }
            set => _objectChunk = value;
        }

        public List<ChunkNode> AllChildNodes
            => __NumChildren == 0 ? null : _model.NodeMap.Values.Where(a => a.ParentNodeID == ID).ToList();

#region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** START Node Chunk ***");
            Log($"    ChunkType:           {ChunkType}");
            Log($"    Node ID:             {ID:X}");
            Log($"    Node Name:           {Name}");
            Log($"    Object ID:           {ObjectNodeID:X}");
            Log($"    Parent ID:           {ParentNodeID:X}");
            Log($"    Number of Children:  {__NumChildren}");
            Log($"    Material ID:         {MatID:X}"); // 0x1 is mtllib w children, 0x10 is mtl no children, 0x18 is child
            Log($"    Position:            {Pos.X:F7}   {Pos.Y:F7}   {Pos.Z:F7}");
            Log($"    Scale:               {Scale.X:F7}   {Scale.Y:F7}   {Scale.Z:F7}");
            Log($"    Transformation:      {Transform.M11:F7}  {Transform.M12:F7}  {Transform.M13:F7}  {Transform.M14:F7}");
            Log($"                         {Transform.M21:F7}  {Transform.M22:F7}  {Transform.M23:F7}  {Transform.M24:F7}");
            Log($"                         {Transform.M31:F7}  {Transform.M32:F7}  {Transform.M33:F7}  {Transform.M34:F7}");
            Log($"                         {Transform.M41 / 100:F7}  {Transform.M42 / 100:F7}  {Transform.M43 / 100:F7}  {Transform.M44:F7}");
            //Log($"    Transform_sum:       {TransformSoFar.X:F7}  {TransformSoFar.Y:F7}  {TransformSoFar.Z:F7}");
            //Log($"    Rotation_sum:");
            //RotSoFar.LogMatrix3x3();
            Log($"*** END Node Chunk ***");
        }
#endif
#endregion
    }
}



#if false
        public Vector3 TransformSoFar => ParentNode != null
            ? ParentNode.TransformSoFar + Transform.GetTranslation()
            : Transform.GetTranslation();

        public Matrix3x3 RotSoFar => ParentNode != null
            ? Transform.GetRotation() * ParentNode.RotSoFar
            : _model.RootNode.Transform.GetRotation();

        /// <summary>
        /// Gets the transform of the vertex.  This will be both the rotation and translation of this vertex, plus all the parents.
        /// The transform matrix is a 4x4 matrix.  Vector3 is a 3x1.  We need to convert vector3 to vector4, multiply the matrix, then convert back to vector3.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Vector3 GetTransform(Vector3 transform)
        {
            var vec3 = transform;
            // Apply the local transforms (rotation and translation) to the vector
            // Do rotations.  Rotations must come first, then translate.
            vec3 = RotSoFar * vec3;
            // Do translations.  I think this is right.  Objects in right place, not rotated right.
            vec3 += TransformSoFar;
            return vec3;
        }

        
#endif