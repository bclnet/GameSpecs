using OpenStack.Graphics.Algorithms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameSpec.Valve.Formats.Blocks.Animation
{
    public class ModelAnimation
    {
        public string Name { get; private set; }
        public float Fps { get; private set; }

        long FrameCount;
        ModelFrame[] Frames;

        ModelAnimation(IDictionary<string, object> animDesc, IDictionary<string, object> decodeKey, ModelAnimDecoderType[] decoderArray, IDictionary<string, object>[] segmentArray)
        {
            Name = string.Empty;
            Fps = 0;
            Frames = Array.Empty<ModelFrame>();
            ConstructFromDesc(animDesc, decodeKey, decoderArray, segmentArray);
        }

        public static IEnumerable<ModelAnimation> FromData(IDictionary<string, object> animationData, IDictionary<string, object> decodeKey)
        {
            var animArray = animationData.Get<IDictionary<string, object>[]>("m_animArray");
            if (animArray.Length == 0)
            {
                Console.WriteLine("Empty animation file found.");
                return Enumerable.Empty<ModelAnimation>();
            }
            var decoderArray = MakeDecoderArray(animationData.GetArray("m_decoderArray"));
            var segmentArray = animationData.GetArray("m_segmentArray");
            return animArray.Select(anim => new ModelAnimation(anim, decodeKey, decoderArray, segmentArray));
        }

        public static IEnumerable<ModelAnimation> FromResource(BinaryPak resource, IDictionary<string, object> decodeKey) => FromData(GetAnimationData(resource), decodeKey);

        static IDictionary<string, object> GetAnimationData(BinaryPak resource)
        {
            var data = resource.DATA;
            if (data is DATABinaryNTRO ntro) return ntro.Data;
            else if (data is DATABinaryKV3 kv3) return kv3.Data;
            return default;
        }

        /// <summary>
        /// Get animation matrices as an array.
        /// </summary>
        public float[] GetAnimationMatricesAsArray(float time, ModelSkeleton skeleton) => GetAnimationMatrices(time, skeleton).Flatten();

        /// <summary>
        /// Get the animation matrix for each bone.
        /// </summary>
        public Matrix4x4[] GetAnimationMatrices(float time, ModelSkeleton skeleton)
        {
            var matrices = new Matrix4x4[skeleton.AnimationTextureSize + 1];
            // Get bone transformations
            var transforms = GetTransformsAtTime(time);
            foreach (var root in skeleton.Roots)
                GetAnimationMatrixRecursive(root, Matrix4x4.Identity, Matrix4x4.Identity, transforms, ref matrices);
            return matrices;
        }

        /// <summary>
        /// Get animation matrix recursively.
        /// </summary>
        void GetAnimationMatrixRecursive(ModelBone bone, Matrix4x4 parentBindPose, Matrix4x4 parentInvBindPose, ModelFrame transforms, ref Matrix4x4[] matrices)
        {
            // Calculate world space bind and inverse bind pose
            var bindPose = parentBindPose;
            var invBindPose = parentInvBindPose * bone.InverseBindPose;

            // Calculate transformation matrix
            var transformMatrix = Matrix4x4.Identity;
            if (transforms.Bones.ContainsKey(bone.Name))
            {
                var transform = transforms.Bones[bone.Name];
                transformMatrix = Matrix4x4.CreateFromQuaternion(transform.Angle) * Matrix4x4.CreateTranslation(transform.Position);
            }

            // Apply tranformation
            var transformed = transformMatrix * bindPose;

            // Store result
            var skinMatrix = invBindPose * transformed;
            foreach (var index in bone.SkinIndices)
                matrices[index] = skinMatrix;

            // Propagate to childen
            foreach (var child in bone.Children)
                GetAnimationMatrixRecursive(child, transformed, invBindPose, transforms, ref matrices);
        }

        /// <summary>
        /// Get the transformation matrices at a time.
        /// </summary>
        /// <param name="time">The time to get the transformation for.</param>
        ModelFrame GetTransformsAtTime(float time)
        {
            // Create output frame
            var frame = new ModelFrame();
            if (FrameCount == 0)
                return frame;

            // Calculate the index of the current frame
            var frameIndex = (int)(time * Fps) % FrameCount;
            var t = ((time * Fps) - frameIndex) % 1;

            // Get current and next frame
            var frame1 = Frames[frameIndex];
            var frame2 = Frames[(frameIndex + 1) % FrameCount];

            // Interpolate bone positions and angles
            foreach (var bonePair in frame1.Bones)
            {
                var position = Vector3.Lerp(frame1.Bones[bonePair.Key].Position, frame2.Bones[bonePair.Key].Position, t);
                var angle = Quaternion.Slerp(frame1.Bones[bonePair.Key].Angle, frame2.Bones[bonePair.Key].Angle, t);
                frame.Bones[bonePair.Key] = new ModelFrameBone(position, angle);
            }
            return frame;
        }

        /// <summary>
        /// Construct an animation class from the animation description.
        /// </summary>
        void ConstructFromDesc(IDictionary<string, object> animDesc, IDictionary<string, object> decodeKey, ModelAnimDecoderType[] decoderArray, IDictionary<string, object>[] segmentArray)
        {
            // Get animation properties
            Name = animDesc.Get<string>("m_name");
            Fps = animDesc.GetFloat("fps");
            var pDataObject = animDesc.Get<object>("m_pData");
            var pData = pDataObject is object[] ntroArray
                ? ntroArray[0] as IDictionary<string, object>
                : pDataObject as IDictionary<string, object>;
            var frameBlockArray = pData.GetArray("m_frameblockArray");
            FrameCount = pData.GetInt64("m_nFrames");
            Frames = new ModelFrame[FrameCount];

            // Figure out each frame
            for (var frame = 0; frame < FrameCount; frame++)
            {
                // Create new frame object
                Frames[frame] = new ModelFrame();
                // Read all frame blocks
                foreach (var frameBlock in frameBlockArray)
                {
                    var startFrame = frameBlock.GetInt64("m_nStartFrame");
                    var endFrame = frameBlock.GetInt64("m_nEndFrame");
                    // Only consider blocks that actual contain info for this frame
                    if (frame >= startFrame && frame <= endFrame)
                    {
                        var segmentIndexArray = frameBlock.GetInt64Array("m_segmentIndexArray");
                        foreach (var segmentIndex in segmentIndexArray)
                        {
                            var segment = segmentArray[segmentIndex];
                            ReadSegment(frame - startFrame, segment, decodeKey, decoderArray, ref Frames[frame]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Read segment.
        /// </summary>
        void ReadSegment(long frame, IDictionary<string, object> segment, IDictionary<string, object> decodeKey, ModelAnimDecoderType[] decoderArray, ref ModelFrame outFrame)
        {
            // Clamp the frame number to be between 0 and the maximum frame
            frame = frame < 0 ? 0 : frame;
            frame = frame >= FrameCount ? FrameCount - 1 : frame;

            var localChannel = segment.GetInt64("m_nLocalChannel");
            var dataChannel = decodeKey.GetArray("m_dataChannelArray")[localChannel];
            var boneNames = dataChannel.Get<string[]>("m_szElementNameArray");

            var channelAttribute = dataChannel.Get<string>("m_szVariableName");

            // Read container
            var container = segment.Get<byte[]>("m_container");
            using (var containerReader = new BinaryReader(new MemoryStream(container)))
            {
                var elementIndexArray = dataChannel.GetInt64Array("m_nElementIndexArray");
                var elementBones = new int[decodeKey.Get<int>("m_nChannelElements")];
                for (var i = 0; i < elementIndexArray.Length; i++)
                    elementBones[elementIndexArray[i]] = i;

                // Read header
                var decoder = decoderArray[containerReader.ReadInt16()];
                var cardinality = containerReader.ReadInt16();
                var numBones = containerReader.ReadInt16();
                var totalLength = containerReader.ReadInt16();

                // Read bone list
                var elements = new List<int>();
                for (var i = 0; i < numBones; i++)
                    elements.Add(containerReader.ReadInt16());

                // Skip data to find the data for the current frame.
                // Structure is just | Bone 0 - Frame 0 | Bone 1 - Frame 0 | Bone 0 - Frame 1 | Bone 1 - Frame 1|
                if (containerReader.BaseStream.Position + (decoder.Size() * frame * numBones) < containerReader.BaseStream.Length)
                    containerReader.BaseStream.Position += decoder.Size() * frame * numBones;

                // Read animation data for all bones
                for (var element = 0; element < numBones; element++)
                {
                    // Get the bone we are reading for
                    var bone = elementBones[elements[element]];

                    // Look at the decoder to see what to read
                    switch (decoder)
                    {
                        case ModelAnimDecoderType.CCompressedStaticFullVector3:
                        case ModelAnimDecoderType.CCompressedFullVector3:
                        case ModelAnimDecoderType.CCompressedDeltaVector3:
                            outFrame.SetAttribute(boneNames[bone], channelAttribute, new Vector3(
                                containerReader.ReadSingle(),
                                containerReader.ReadSingle(),
                                containerReader.ReadSingle()));
                            break;
                        case ModelAnimDecoderType.CCompressedAnimVector3:
                        case ModelAnimDecoderType.CCompressedStaticVector3:
                            outFrame.SetAttribute(boneNames[bone], channelAttribute, new Vector3(
                                ReadHalfFloat(containerReader),
                                ReadHalfFloat(containerReader),
                                ReadHalfFloat(containerReader)));
                            break;
                        case ModelAnimDecoderType.CCompressedAnimQuaternion:
                        case ModelAnimDecoderType.CCompressedFullQuaternion:
                        case ModelAnimDecoderType.CCompressedStaticQuaternion:
                            outFrame.SetAttribute(boneNames[bone], channelAttribute, ReadQuaternion(containerReader));
                            break;
#if DEBUG
                        default:
                            if (channelAttribute != "data")
                                Console.WriteLine($"Unhandled animation bone decoder type '{decoder}'");
                            break;
#endif
                    }
                }
            }
        }

        /// <summary>
        /// Read a half-precision float from a binary reader.
        /// </summary>
        /// <param name="r">Binary ready.</param>
        /// <returns>float.</returns>
        static float ReadHalfFloat(BinaryReader r) => HalfPrecConverter.ToSingle(r.ReadUInt16());

        /// <summary>
        /// Read and decode encoded quaternion.
        /// </summary>
        /// <param name="r">Binary reader.</param>
        /// <returns>Quaternion.</returns>
        private static Quaternion ReadQuaternion(BinaryReader r)
        {
            var bytes = r.ReadBytes(6);

            // Values
            var i1 = bytes[0] + ((bytes[1] & 63) << 8);
            var i2 = bytes[2] + ((bytes[3] & 63) << 8);
            var i3 = bytes[4] + ((bytes[5] & 63) << 8);

            // Signs
            var s1 = bytes[1] & 128;
            var s2 = bytes[3] & 128;
            var s3 = bytes[5] & 128;

            var c = (float)Math.Sin(Math.PI / 4.0f) / 16384.0f;
            var t1 = (float)Math.Sin(Math.PI / 4.0f);
            var x = (bytes[1] & 64) == 0 ? c * (i1 - 16384) : c * i1;
            var y = (bytes[3] & 64) == 0 ? c * (i2 - 16384) : c * i2;
            var z = (bytes[5] & 64) == 0 ? c * (i3 - 16384) : c * i3;

            var w = (float)Math.Sqrt(1 - (x * x) - (y * y) - (z * z));

            // Apply sign 3
            if (s3 == 128)
                w *= -1;

            // Apply sign 1 and 2
            return s1 == 128
                ? s2 == 128 ? new Quaternion(y, z, w, x) : new Quaternion(z, w, x, y)
                : s2 == 128 ? new Quaternion(w, x, y, z) : new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Transform the decoder array to a mapping of index to type ID.
        /// </summary>
        static ModelAnimDecoderType[] MakeDecoderArray(IDictionary<string, object>[] decoderArray)
        {
            var array = new ModelAnimDecoderType[decoderArray.Length];
            for (var i = 0; i < decoderArray.Length; i++)
            {
                var decoder = decoderArray[i];
                array[i] = ModelAnimDecoder.FromString(decoder.Get<string>("m_szName"));
            }
            return array;
        }
        
        public override string ToString() => Name;
    }
}
