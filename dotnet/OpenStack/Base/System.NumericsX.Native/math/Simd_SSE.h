#ifndef __SIMD_SSE_H__
#define __SIMD_SSE_H__

#include "Simd_MMX.h"

#if defined(__GNUC__) && defined(__SSE__)
const char* VPCALL SIMD_SSE_GetName(void) const;
void VPCALL SIMD_SSE_Dot(float* dst, const Plane& constant, const DrawVert* src, const int count);
void VPCALL SIMD_SSE_MinMax(Vector3& min, Vector3& max, const DrawVert* src, const int* indexes, const int count);
void VPCALL SIMD_SSE_Dot(float* dst, const Vector3& constant, const Plane* src, const int count);

#elif defined(_MSC_VER)
const char* VPCALL SIMD_SSE_GetName(void);

void VPCALL SIMD_SSE_Add(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_SSE_Add(float* dst, const float* src0, const float* src1, const int count);
void VPCALL SIMD_SSE_Sub(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_SSE_Sub(float* dst, const float* src0, const float* src1, const int count);
void VPCALL SIMD_SSE_Mul(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_SSE_Mul(float* dst, const float* src0, const float* src1, const int count);
void VPCALL SIMD_SSE_Div(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_SSE_Div(float* dst, const float* src0, const float* src1, const int count);
void VPCALL SIMD_SSE_MulAdd(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_SSE_MulAdd(float* dst, const float* src0, const float* src1, const int count);
void VPCALL SIMD_SSE_MulSub(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_SSE_MulSub(float* dst, const float* src0, const float* src1, const int count);

void VPCALL SIMD_SSE_Dot(float* dst, const Vector3& constant, const Vector3* src, const int count);
void VPCALL SIMD_SSE_Dot(float* dst, const Vector3& constant, const Plane* src, const int count);
void VPCALL SIMD_SSE_Dot(float* dst, const Vector3& constant, const DrawVert* src, const int count);
void VPCALL SIMD_SSE_Dot(float* dst, const Plane& constant, const Vector3* src, const int count);
void VPCALL SIMD_SSE_Dot(float* dst, const Plane& constant, const Plane* src, const int count);
void VPCALL SIMD_SSE_Dot(float* dst, const Plane& constant, const DrawVert* src, const int count);
void VPCALL SIMD_SSE_Dot(float* dst, const Vector3* src0, const Vector3* src1, const int count);
void VPCALL SIMD_SSE_Dot(float& dot, const float* src1, const float* src2, const int count);

void VPCALL SIMD_SSE_CmpGT(byte* dst, const float* src0, const float constant, const int count);
void VPCALL SIMD_SSE_CmpGT(byte* dst, const byte bitNum, const float* src0, const float constant, const int count);
void VPCALL SIMD_SSE_CmpGE(byte* dst, const float* src0, const float constant, const int count);
void VPCALL SIMD_SSE_CmpGE(byte* dst, const byte bitNum, const float* src0, const float constant, const int count);
void VPCALL SIMD_SSE_CmpLT(byte* dst, const float* src0, const float constant, const int count);
void VPCALL SIMD_SSE_CmpLT(byte* dst, const byte bitNum, const float* src0, const float constant, const int count);
void VPCALL SIMD_SSE_CmpLE(byte* dst, const float* src0, const float constant, const int count);
void VPCALL SIMD_SSE_CmpLE(byte* dst, const byte bitNum, const float* src0, const float constant, const int count);

void VPCALL SIMD_SSE_MinMax(float& min, float& max, const float* src, const int count);
void VPCALL SIMD_SSE_MinMax(Vector2& min, Vector2& max, const Vector2* src, const int count);
void VPCALL SIMD_SSE_MinMax(Vector3& min, Vector3& max, const Vector3* src, const int count);
void VPCALL SIMD_SSE_MinMax(Vector3& min, Vector3& max, const DrawVert* src, const int count);
void VPCALL SIMD_SSE_MinMax(Vector3& min, Vector3& max, const DrawVert* src, const int* indexes, const int count);

void VPCALL SIMD_SSE_Clamp(float* dst, const float* src, const float min, const float max, const int count);
void VPCALL SIMD_SSE_ClampMin(float* dst, const float* src, const float min, const int count);
void VPCALL SIMD_SSE_ClampMax(float* dst, const float* src, const float max, const int count);

void VPCALL SIMD_SSE_Zero16(float* dst, const int count);
void VPCALL SIMD_SSE_Negate16(float* dst, const int count);
void VPCALL SIMD_SSE_Copy16(float* dst, const float* src, const int count);
void VPCALL SIMD_SSE_Add16(float* dst, const float* src1, const float* src2, const int count);
void VPCALL SIMD_SSE_Sub16(float* dst, const float* src1, const float* src2, const int count);
void VPCALL SIMD_SSE_Mul16(float* dst, const float* src1, const float constant, const int count);
void VPCALL SIMD_SSE_AddAssign16(float* dst, const float* src, const int count);
void VPCALL SIMD_SSE_SubAssign16(float* dst, const float* src, const int count);
void VPCALL SIMD_SSE_MulAssign16(float* dst, const float constant, const int count);

void VPCALL SIMD_SSE_MatX_MultiplyVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_SSE_MatX_MultiplyAddVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_SSE_MatX_MultiplySubVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_SSE_MatX_TransposeMultiplyVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_SSE_MatX_TransposeMultiplyAddVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_SSE_MatX_TransposeMultiplySubVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_SSE_MatX_MultiplyMatX(MatrixX& dst, const MatrixX& m1, const MatrixX& m2);
void VPCALL SIMD_SSE_MatX_TransposeMultiplyMatX(MatrixX& dst, const MatrixX& m1, const MatrixX& m2);
void VPCALL SIMD_SSE_MatX_LowerTriangularSolve(const MatrixX& L, float* x, const float* b, const int n, int skip = 0);
void VPCALL SIMD_SSE_MatX_LowerTriangularSolveTranspose(const MatrixX& L, float* x, const float* b, const int n);
bool VPCALL SIMD_SSE_MatX_LDLTFactor(MatrixX& mat, VectorX& invDiag, const int n);

void VPCALL SIMD_SSE_BlendJoints(JointQuat* joints, const JointQuat* blendJoints, const float lerp, const int* index, const int numJoints);
void VPCALL SIMD_SSE_ConvertJointQuatsToJointMats(JointMat* jointMats, const JointQuat* jointQuats, const int numJoints);
void VPCALL SIMD_SSE_ConvertJointMatsToJointQuats(JointQuat* jointQuats, const JointMat* jointMats, const int numJoints);
void VPCALL SIMD_SSE_TransformJoints(JointMat* jointMats, const int* parents, const int firstJoint, const int lastJoint);
void VPCALL SIMD_SSE_UntransformJoints(JointMat* jointMats, const int* parents, const int firstJoint, const int lastJoint);
void VPCALL SIMD_SSE_TransformVerts(DrawVert* verts, const int numVerts, const JointMat* joints, const Vector4* weights, const int* index, const int numWeights);
void VPCALL SIMD_SSE_TracePointCull(byte* cullBits, byte& totalOr, const float radius, const Plane* planes, const DrawVert* verts, const int numVerts);
void VPCALL SIMD_SSE_DecalPointCull(byte* cullBits, const Plane* planes, const DrawVert* verts, const int numVerts);
void VPCALL SIMD_SSE_OverlayPointCull(byte* cullBits, Vector2* texCoords, const Plane* planes, const DrawVert* verts, const int numVerts);
void VPCALL SIMD_SSE_DeriveTriPlanes(Plane* planes, const DrawVert* verts, const int numVerts, const int* indexes, const int numIndexes);
void VPCALL SIMD_SSE_DeriveTangents(Plane* planes, DrawVert* verts, const int numVerts, const int* indexes, const int numIndexes);
void VPCALL SIMD_SSE_DeriveUnsmoothedTangents(DrawVert* verts, const DominantTri* dominantTris, const int numVerts);
void VPCALL SIMD_SSE_NormalizeTangents(DrawVert* verts, const int numVerts);
void VPCALL SIMD_SSE_CreateTextureSpaceLightVectors(Vector3* lightVectors, const Vector3& lightOrigin, const DrawVert* verts, const int numVerts, const int* indexes, const int numIndexes);
void VPCALL SIMD_SSE_CreateSpecularTextureCoords(Vector4* texCoords, const Vector3& lightOrigin, const Vector3& viewOrigin, const DrawVert* verts, const int numVerts, const int* indexes, const int numIndexes);
int  VPCALL SIMD_SSE_CreateShadowCache(Vector4* vertexCache, int* vertRemap, const Vector3& lightOrigin, const DrawVert* verts, const int numVerts);
int  VPCALL SIMD_SSE_CreateVertexProgramShadowCache(Vector4* vertexCache, const DrawVert* verts, const int numVerts);

void VPCALL SIMD_SSE_UpSamplePCMTo44kHz(float* dest, const short* pcm, const int numSamples, const int kHz, const int numChannels);
void VPCALL SIMD_SSE_UpSampleOGGTo44kHz(float* dest, const float* const* ogg, const int numSamples, const int kHz, const int numChannels);
void VPCALL SIMD_SSE_MixSoundTwoSpeakerMono(float* mixBuffer, const float* samples, const int numSamples, const float lastV[2], const float currentV[2]);
void VPCALL SIMD_SSE_MixSoundTwoSpeakerStereo(float* mixBuffer, const float* samples, const int numSamples, const float lastV[2], const float currentV[2]);
void VPCALL SIMD_SSE_MixSoundSixSpeakerMono(float* mixBuffer, const float* samples, const int numSamples, const float lastV[6], const float currentV[6]);
void VPCALL SIMD_SSE_MixSoundSixSpeakerStereo(float* mixBuffer, const float* samples, const int numSamples, const float lastV[6], const float currentV[6]);
void VPCALL SIMD_SSE_MixedSoundToSamples(short* samples, const float* mixBuffer, const int numSamples);

#endif

#endif
