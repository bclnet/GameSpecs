using System.Runtime.InteropServices;
using static System.NumericsX.PlatformNative;

namespace System.NumericsX
{
    public enum SPEAKER
    {
        LEFT = 0,
        RIGHT,
        CENTER,
        LFE,
        BACKLEFT,
        BACKRIGHT
    }

    public unsafe static class Simd
    {
        public static void InitProcessor(string module, bool forceGeneric)
        {
            var cpuid = GetProcessorId();
            if (!forceGeneric)
            {
                if ((cpuid & CPUID.ALTIVEC) != 0) SimdAltiVec.Activate();
                else if ((cpuid & CPUID.MMX) != 0 && (cpuid & CPUID.SSE) != 0 && (cpuid & CPUID.SSE2) != 0 && (cpuid & CPUID.SSE3) != 0) SimdSSE3.Activate();
                else if ((cpuid & CPUID.MMX) != 0 && (cpuid & CPUID.SSE) != 0 && (cpuid & CPUID.SSE2) != 0) SimdSSE2.Activate();
                else if ((cpuid & CPUID.MMX) != 0 && (cpuid & CPUID.SSE) != 0) SimdSSE.Activate();
                else if ((cpuid & CPUID.MMX) != 0 && (cpuid & CPUID._3DNOW) != 0) Simd3DNow.Activate();
                else if ((cpuid & CPUID.MMX) != 0) SimdMMX.Activate();
                else SimdGeneric.Activate();
            }

            Platform.Printf($"{module} using {Name} for SIMD processing\n");

            if ((cpuid & CPUID.SSE) != 0)
            {
                FPU_SetFTZ(true);
                FPU_SetDAZ(true);
            }
        }

        public const int MIXBUFFER_SAMPLES = 4096;

        public static int CpuId = SimdGeneric.CpuId;
        public static string Name = SimdGeneric.Name;

        public static AddDelegate Add = SimdGeneric.Add;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void AddDelegate(float* dst, float constant, float* src, int count);
        public static AddvDelegate Addv = SimdGeneric.Addv;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void AddvDelegate(float* dst, float* src0, float* src1, int count);
        public static SubDelegate Sub = SimdGeneric.Sub;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SubDelegate(float* dst, float constant, float* src, int count);
        public static SubvDelegate Subv = SimdGeneric.Subv;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SubvDelegate(float* dst, float* src0, float* src1, int count);
        public static MulDelegate Mul = SimdGeneric.Mul;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MulDelegate(float* dst, float constant, float* src, int count);
        public static MulvDelegate Mulv = SimdGeneric.Mulv;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MulvDelegate(float* dst, float* src0, float* src1, int count);
        public static DivDelegate Div = SimdGeneric.Div;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DivDelegate(float* dst, float constant, float* src, int count);
        public static DivvDelegate Divv = SimdGeneric.Divv;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DivvDelegate(float* dst, float* src0, float* src1, int count);
        public static MulAddDelegate MulAdd = SimdGeneric.MulAdd;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MulAddDelegate(float* dst, float constant, float* src, int count);
        public static MulAddvDelegate MulAddv = SimdGeneric.MulSubv;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MulAddvDelegate(float* dst, float* src0, float* src1, int count);
        public static MulSubDelegate MulSub = SimdGeneric.MulSub;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MulSubDelegate(float* dst, float constant, float* src, int count);
        public static MulSubvDelegate MulSubv = SimdGeneric.MulSubv;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MulSubvDelegate(float* dst, float* src0, float* src1, int count);

        public static DotcvDelegate Dotcv = SimdGeneric.Dotcv;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DotcvDelegate(float* dst, Vector3 constant, Vector3* src, int count);
        public static DotcpDelegate Dotcp = SimdGeneric.Dotcp;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DotcpDelegate(float* dst, Vector3 constant, Plane* src, int count);
        public static DotcdDelegate Dotcd = SimdGeneric.Dotcd;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DotcdDelegate(float* dst, Vector3 constant, DrawVert* src, int count);
        public static DotpvDelegate Dotpv = SimdGeneric.Dotpv;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DotpvDelegate(float* dst, Plane constant, Vector3* src, int count);
        public static DotppDelegate Dotpp = SimdGeneric.Dotpp;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DotppDelegate(float* dst, Plane constant, Plane* src, int count);
        public static DotpdDelegate Dotpd = SimdGeneric.Dotpd;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DotpdDelegate(float* dst, Plane constant, DrawVert* src, int count);
        public static DotvvDelegate Dotvv = SimdGeneric.Dotvv;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DotvvDelegate(float* dst, Vector3* src0, Vector3* src1, int count);
        public static DotffDelegate Dotff = SimdGeneric.Dotff;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DotffDelegate(out float dot, float* src1, float* src2, int count);

        public static CmpGTDelegate CmpGT = SimdGeneric.CmpGT;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void CmpGTDelegate(byte* dst, float* src0, float constant, int count);
        public static CmpGTbDelegate CmpGTb = SimdGeneric.CmpGTb;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void CmpGTbDelegate(byte* dst, byte bitNum, float* src0, float constant, int count);
        public static CmpGEDelegate CmpGE = SimdGeneric.CmpGE;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void CmpGEDelegate(byte* dst, float* src0, float constant, int count);
        public static CmpGEbDelegate CmpGEb = SimdGeneric.CmpGEb;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void CmpGEbDelegate(byte* dst, byte bitNum, float* src0, float constant, int count);
        public static CmpLTDelegate CmpLT = SimdGeneric.CmpLT;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void CmpLTDelegate(byte* dst, float* src0, float constant, int count);
        public static CmpLTbDelegate CmpLTb = SimdGeneric.CmpLTb;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void CmpLTbDelegate(byte* dst, byte bitNum, float* src0, float constant, int count);
        public static CmpLEDelegate CmpLE = SimdGeneric.CmpLE;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void CmpLEDelegate(byte* dst, float* src0, float constant, int count);
        public static CmpLEbDelegate CmpLEb = SimdGeneric.CmpLEb;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void CmpLEbDelegate(byte* dst, byte bitNum, float* src0, float constant, int count);

        public static MinMaxfDelegate MinMaxf = SimdGeneric.MinMaxf;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MinMaxfDelegate(out float min, out float max, float* src, int count);
        public static MinMax2Delegate MinMax2 = SimdGeneric.MinMax2;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MinMax2Delegate(out Vector2 min, out Vector2 max, Vector2* src, int count);
        public static MinMax3Delegate MinMax3 = SimdGeneric.MinMax3;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MinMax3Delegate(out Vector3 min, out Vector3 max, Vector3* src, int count);
        public static MinMaxdDelegate MinMaxd = SimdGeneric.MinMaxd;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MinMaxdDelegate(out Vector3 min, out Vector3 max, DrawVert* src, int count);
        public static MinMaxdiDelegate MinMaxdi = SimdGeneric.MinMaxdi;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MinMaxdiDelegate(out Vector3 min, out Vector3 max, DrawVert* src, int* indexes, int count);
        public static MinMaxdsDelegate MinMaxds = SimdGeneric.MinMaxds;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MinMaxdsDelegate(out Vector3 min, out Vector3 max, DrawVert* src, short* indexes, int count);

        public static ClampDelegate Clamp = SimdGeneric.Clamp;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void ClampDelegate(float* dst, float* src, float min, float max, int count);
        public static ClampMinDelegate ClampMin = SimdGeneric.ClampMin;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void ClampMinDelegate(float* dst, float* src, float min, int count);
        public static ClampMaxDelegate ClampMax = SimdGeneric.ClampMax;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void ClampMaxDelegate(float* dst, float* src, float max, int count);

        public static MemcpyDelegate Memcpy = SimdGeneric.Memcpy;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MemcpyDelegate(void* dst, void* src, int count);
        public static MemsetDelegate Memset = SimdGeneric.Memset;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MemsetDelegate(void* dst, int val, int count);

        // these assume 16 byte aligned and 16 byte padded memory
        public static Zero16Delegate Zero16 = SimdGeneric.Zero16;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void Zero16Delegate(float* dst, int count);
        public static Negate16Delegate Negate16 = SimdGeneric.Negate16;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void Negate16Delegate(float* dst, int count);
        public static Copy16Delegate Copy16 = SimdGeneric.Copy16;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void Copy16Delegate(float* dst, float* src, int count);
        public static Add16Delegate Add16 = SimdGeneric.Add16;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void Add16Delegate(float* dst, float* src1, float* src2, int count);
        public static Sub16Delegate Sub16 = SimdGeneric.Sub16;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void Sub16Delegate(float* dst, float* src1, float* src2, int count);
        public static Mul16Delegate Mul16 = SimdGeneric.Mul16;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void Mul16Delegate(float* dst, float* src1, float constant, int count);
        public static AddAssign16Delegate AddAssign16 = SimdGeneric.AddAssign16;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void AddAssign16Delegate(float* dst, float* src, int count);
        public static SubAssign16Delegate SubAssign16 = SimdGeneric.SubAssign16;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void SubAssign16Delegate(float* dst, float* src, int count);
        public static MulAssign16Delegate MulAssign16 = SimdGeneric.MulAssign16;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MulAssign16Delegate(float* dst, float constant, int count);

        // MatX operations
        public static MatX_MultiplyVecXDelegate MatX_MultiplyVecX = SimdGeneric.MatX_MultiplyVecX;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MatX_MultiplyVecXDelegate(VectorX dst, MatrixX mat, VectorX vec);
        public static MatX_MultiplyAddVecXDelegate MatX_MultiplyAddVecX = SimdGeneric.MatX_MultiplyAddVecX;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MatX_MultiplyAddVecXDelegate(VectorX dst, MatrixX mat, VectorX vec);
        public static MatX_MultiplySubVecXDelegate MatX_MultiplySubVecX = SimdGeneric.MatX_MultiplySubVecX;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MatX_MultiplySubVecXDelegate(VectorX dst, MatrixX mat, VectorX vec);
        public static MatX_TransposeMultiplyVecXDelegate MatX_TransposeMultiplyVecX = SimdGeneric.MatX_TransposeMultiplyVecX;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MatX_TransposeMultiplyVecXDelegate(VectorX dst, MatrixX mat, VectorX vec);
        public static MatX_TransposeMultiplyAddVecXDelegate MatX_TransposeMultiplyAddVecX = SimdGeneric.MatX_TransposeMultiplyAddVecX;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MatX_TransposeMultiplyAddVecXDelegate(VectorX dst, MatrixX mat, VectorX vec);
        public static MatX_TransposeMultiplySubVecXDelegate MatX_TransposeMultiplySubVecX = SimdGeneric.MatX_TransposeMultiplySubVecX;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MatX_TransposeMultiplySubVecXDelegate(VectorX dst, MatrixX mat, VectorX vec);
        public static MatX_MultiplyMatXDelegate MatX_MultiplyMatX = SimdGeneric.MatX_MultiplyMatX;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MatX_MultiplyMatXDelegate(MatrixX dst, MatrixX m1, MatrixX m2);
        public static MatX_TransposeMultiplyMatXDelegate MatX_TransposeMultiplyMatX = SimdGeneric.MatX_TransposeMultiplyMatX;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MatX_TransposeMultiplyMatXDelegate(MatrixX dst, MatrixX m1, MatrixX m2);
        public static MatX_LowerTriangularSolveDelegate MatX_LowerTriangularSolve = SimdGeneric.MatX_LowerTriangularSolve;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MatX_LowerTriangularSolveDelegate(MatrixX L, float* x, float* b, int n, int skip = 0);
        public static MatX_LowerTriangularSolveTransposeDelegate MatX_LowerTriangularSolveTranspose = SimdGeneric.MatX_LowerTriangularSolveTranspose;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MatX_LowerTriangularSolveTransposeDelegate(MatrixX L, float* x, float* b, int n);
        public static MatX_LDLTFactorDelegate MatX_LDLTFactor = SimdGeneric.MatX_LDLTFactor;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate bool MatX_LDLTFactorDelegate(MatrixX mat, VectorX invDiag, int n);

        // rendering
        public static BlendJointsDelegate BlendJoints = SimdGeneric.BlendJoints;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void BlendJointsDelegate(JointQuat* joints, JointQuat* blendJoints, float lerp, int* index, int numJoints);
        public static ConvertJointQuatsToJointMatsDelegate ConvertJointQuatsToJointMats = SimdGeneric.ConvertJointQuatsToJointMats;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void ConvertJointQuatsToJointMatsDelegate(JointMat* jointMats, JointQuat* jointQuats, int numJoints);
        public static ConvertJointMatsToJointQuatsDelegate ConvertJointMatsToJointQuats = SimdGeneric.ConvertJointMatsToJointQuats;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void ConvertJointMatsToJointQuatsDelegate(JointQuat* jointQuats, JointMat* jointMats, int numJoints);
        public static TransformJointsDelegate TransformJoints = SimdGeneric.TransformJoints;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void TransformJointsDelegate(JointMat* jointMats, int* parents, int firstJoint, int lastJoint);
        public static UntransformJointsDelegate UntransformJoints = SimdGeneric.UntransformJoints;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void UntransformJointsDelegate(JointMat* jointMats, int* parents, int firstJoint, int lastJoint);
        public static TransformVertsDelegate TransformVerts = SimdGeneric.TransformVerts;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void TransformVertsDelegate(DrawVert* verts, int numVerts, JointMat* joints, Vector4* weights, int* index, int numWeights);
        public static TracePointCullDelegate TracePointCull = SimdGeneric.TracePointCull;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void TracePointCullDelegate(byte* cullBits, out byte totalOr, float radius, Plane* planes, DrawVert* verts, int numVerts);
        public static DecalPointCullDelegate DecalPointCull = SimdGeneric.DecalPointCull;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DecalPointCullDelegate(byte* cullBits, Plane* planes, DrawVert* verts, int numVerts);
        public static OverlayPointCullDelegate OverlayPointCull = SimdGeneric.OverlayPointCull;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void OverlayPointCullDelegate(byte* cullBits, Vector2* texCoords, Plane* planes, DrawVert* verts, int numVerts);
        public static DeriveTriPlanesDelegate DeriveTriPlanesi = SimdGeneric.DeriveTriPlanesi;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DeriveTriPlanesDelegate(Plane* planes, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        public static DeriveTriPlanessDelegate DeriveTriPlaness = SimdGeneric.DeriveTriPlaness;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DeriveTriPlanessDelegate(Plane* planes, DrawVert* verts, int numVerts, short* indexes, int numIndexes);
        public static DeriveTangentsiDelegate DeriveTangentsi = SimdGeneric.DeriveTangentsi;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DeriveTangentsiDelegate(Plane* planes, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        public static DeriveTangentssDelegate DeriveTangentss = SimdGeneric.DeriveTangentss;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DeriveTangentssDelegate(Plane* planes, DrawVert* verts, int numVerts, short* indexes, int numIndexes);
        public static DeriveUnsmoothedTangentsDelegate DeriveUnsmoothedTangents = SimdGeneric.DeriveUnsmoothedTangents;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void DeriveUnsmoothedTangentsDelegate(DrawVert* verts, DominantTri* dominantTris, int numVerts);
        public static NormalizeTangentsDelegate NormalizeTangents = SimdGeneric.NormalizeTangents;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void NormalizeTangentsDelegate(DrawVert* verts, int numVerts);
        public static CreateTextureSpaceLightVectorsDelegate CreateTextureSpaceLightVectors = SimdGeneric.CreateTextureSpaceLightVectors;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void CreateTextureSpaceLightVectorsDelegate(Vector3* lightVectors, Vector3 lightOrigin, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        public static CreateSpecularTextureCoordsDelegate CreateSpecularTextureCoords = SimdGeneric.CreateSpecularTextureCoords;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void CreateSpecularTextureCoordsDelegate(Vector4* texCoords, Vector3 lightOrigin, Vector3 viewOrigin, DrawVert* verts, int numVerts, int* indexes, int numIndexes);
        public static CreateShadowCacheDelegate CreateShadowCache = SimdGeneric.CreateShadowCache;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int CreateShadowCacheDelegate(Vector4* vertexCache, int* vertRemap, Vector3 lightOrigin, DrawVert* verts, int numVerts);
        public static CreateVertexProgramShadowCacheDelegate CreateVertexProgramShadowCache = SimdGeneric.CreateVertexProgramShadowCache;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int CreateVertexProgramShadowCacheDelegate(Vector4* vertexCache, DrawVert* verts, int numVerts);

        // sound mixing
        public static UpSamplePCMTo44kHzDelegate UpSamplePCMTo44kHz = SimdGeneric.UpSamplePCMTo44kHz;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void UpSamplePCMTo44kHzDelegate(float* dest, short* pcm, int numSamples, int kHz, int numChannels);
        public static UpSampleOGGTo44kHzDelegate UpSampleOGGTo44kHz = SimdGeneric.UpSampleOGGTo44kHz;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void UpSampleOGGTo44kHzDelegate(float* dest, float** ogg, int numSamples, int kHz, int numChannels);
        public static MixSoundTwoSpeakerMonoDelegate MixSoundTwoSpeakerMono = SimdGeneric.MixSoundTwoSpeakerMono;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MixSoundTwoSpeakerMonoDelegate(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV);
        public static MixSoundTwoSpeakerStereoDelegate MixSoundTwoSpeakerStereo = SimdGeneric.MixSoundTwoSpeakerStereo;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MixSoundTwoSpeakerStereoDelegate(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV);
        public static MixSoundSixSpeakerMonoDelegate MixSoundSixSpeakerMono = SimdGeneric.MixSoundSixSpeakerMono;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MixSoundSixSpeakerMonoDelegate(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV);
        public static MixSoundSixSpeakerStereoDelegate MixSoundSixSpeakerStereo = SimdGeneric.MixSoundSixSpeakerStereo;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MixSoundSixSpeakerStereoDelegate(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV);
        public static MixedSoundToSamplesDelegate MixedSoundToSamples = SimdGeneric.MixedSoundToSamples;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void MixedSoundToSamplesDelegate(short* samples, float* mixBuffer, int numSamples);
    }
}
