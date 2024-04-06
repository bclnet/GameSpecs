#ifndef __SIMD_GENERIC_H__
#define __SIMD_GENERIC_H__

#include "Simd.h"

const char* VPCALL SIMD_Generic_GetName(void);

void VPCALL SIMD_Generic_Add(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_Generic_Add(float* dst, const float* src0, const float* src1, const int count);
void VPCALL SIMD_Generic_Sub(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_Generic_Sub(float* dst, const float* src0, const float* src1, const int count);
void VPCALL SIMD_Generic_Mul(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_Generic_Mul(float* dst, const float* src0, const float* src1, const int count);
void VPCALL SIMD_Generic_Div(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_Generic_Div(float* dst, const float* src0, const float* src1, const int count);
void VPCALL SIMD_Generic_MulAdd(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_Generic_MulAdd(float* dst, const float* src0, const float* src1, const int count);
void VPCALL SIMD_Generic_MulSub(float* dst, const float constant, const float* src, const int count);
void VPCALL SIMD_Generic_MulSub(float* dst, const float* src0, const float* src1, const int count);

void VPCALL SIMD_Generic_Dot(float* dst, const Vector3& constant, const Vector3* src, const int count);
void VPCALL SIMD_Generic_Dot(float* dst, const Vector3& constant, const Plane* src, const int count);
void VPCALL SIMD_Generic_Dot(float* dst, const Vector3& constant, const DrawVert* src, const int count);
void VPCALL SIMD_Generic_Dot(float* dst, const Plane& constant, const Vector3* src, const int count);
void VPCALL SIMD_Generic_Dot(float* dst, const Plane& constant, const Plane* src, const int count);
void VPCALL SIMD_Generic_Dot(float* dst, const Plane& constant, const DrawVert* src, const int count);
void VPCALL SIMD_Generic_Dot(float* dst, const Vector3* src0, const Vector3* src1, const int count);
void VPCALL SIMD_Generic_Dot(float& dot, const float* src1, const float* src2, const int count);

void VPCALL SIMD_Generic_CmpGT(byte* dst, const float* src0, const float constant, const int count);
void VPCALL SIMD_Generic_CmpGT(byte* dst, const byte bitNum, const float* src0, const float constant, const int count);
void VPCALL SIMD_Generic_CmpGE(byte* dst, const float* src0, const float constant, const int count);
void VPCALL SIMD_Generic_CmpGE(byte* dst, const byte bitNum, const float* src0, const float constant, const int count);
void VPCALL SIMD_Generic_CmpLT(byte* dst, const float* src0, const float constant, const int count);
void VPCALL SIMD_Generic_CmpLT(byte* dst, const byte bitNum, const float* src0, const float constant, const int count);
void VPCALL SIMD_Generic_CmpLE(byte* dst, const float* src0, const float constant, const int count);
void VPCALL SIMD_Generic_CmpLE(byte* dst, const byte bitNum, const float* src0, const float constant, const int count);

void VPCALL SIMD_Generic_MinMax(float& min, float& max, const float* src, const int count);
void VPCALL SIMD_Generic_MinMax(Vector2& min, Vector2& max, const Vector2* src, const int count);
void VPCALL SIMD_Generic_MinMax(Vector3& min, Vector3& max, const Vector3* src, const int count);
void VPCALL SIMD_Generic_MinMax(Vector3& min, Vector3& max, const DrawVert* src, const int count);
void VPCALL SIMD_Generic_MinMax(Vector3& min, Vector3& max, const DrawVert* src, const int* indexes, const int count);
void VPCALL SIMD_Generic_MinMax(Vector3& min, Vector3& max, const DrawVert* src, const short* indexes, const int count);

void VPCALL SIMD_Generic_Clamp(float* dst, const float* src, const float min, const float max, const int count);
void VPCALL SIMD_Generic_ClampMin(float* dst, const float* src, const float min, const int count);
void VPCALL SIMD_Generic_ClampMax(float* dst, const float* src, const float max, const int count);

void VPCALL SIMD_Generic_Memcpy(void* dst, const void* src, const int count);
void VPCALL SIMD_Generic_Memset(void* dst, const int val, const int count);

void VPCALL SIMD_Generic_Zero16(float* dst, const int count);
void VPCALL SIMD_Generic_Negate16(float* dst, const int count);
void VPCALL SIMD_Generic_Copy16(float* dst, const float* src, const int count);
void VPCALL SIMD_Generic_Add16(float* dst, const float* src1, const float* src2, const int count);
void VPCALL SIMD_Generic_Sub16(float* dst, const float* src1, const float* src2, const int count);
void VPCALL SIMD_Generic_Mul16(float* dst, const float* src1, const float constant, const int count);
void VPCALL SIMD_Generic_AddAssign16(float* dst, const float* src, const int count);
void VPCALL SIMD_Generic_SubAssign16(float* dst, const float* src, const int count);
void VPCALL SIMD_Generic_MulAssign16(float* dst, const float constant, const int count);

void VPCALL SIMD_Generic_MatX_MultiplyVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_Generic_MatX_MultiplyAddVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_Generic_MatX_MultiplySubVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_Generic_MatX_TransposeMultiplyVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_Generic_MatX_TransposeMultiplyAddVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_Generic_MatX_TransposeMultiplySubVecX(VectorX& dst, const MatrixX& mat, const VectorX& vec);
void VPCALL SIMD_Generic_MatX_MultiplyMatX(MatrixX& dst, const MatrixX& m1, const MatrixX& m2);
void VPCALL SIMD_Generic_MatX_TransposeMultiplyMatX(MatrixX& dst, const MatrixX& m1, const MatrixX& m2);
void VPCALL SIMD_Generic_MatX_LowerTriangularSolve(const MatrixX& L, float* x, const float* b, const int n, int skip = 0);
void VPCALL SIMD_Generic_MatX_LowerTriangularSolveTranspose(const MatrixX& L, float* x, const float* b, const int n);
bool VPCALL SIMD_Generic_MatX_LDLTFactor(MatrixX& mat, VectorX& invDiag, const int n);

void VPCALL SIMD_Generic_BlendJoints(JointQuat* joints, const JointQuat* blendJoints, const float lerp, const int* index, const int numJoints);
void VPCALL SIMD_Generic_ConvertJointQuatsToJointMats(JointMat* jointMats, const JointQuat* jointQuats, const int numJoints);
void VPCALL SIMD_Generic_ConvertJointMatsToJointQuats(JointQuat* jointQuats, const JointMat* jointMats, const int numJoints);
void VPCALL SIMD_Generic_TransformJoints(JointMat* jointMats, const int* parents, const int firstJoint, const int lastJoint);
void VPCALL SIMD_Generic_UntransformJoints(JointMat* jointMats, const int* parents, const int firstJoint, const int lastJoint);
void VPCALL SIMD_Generic_TransformVerts(DrawVert* verts, const int numVerts, const JointMat* joints, const Vector4* weights, const int* index, const int numWeights);
void VPCALL SIMD_Generic_TracePointCull(byte* cullBits, byte& totalOr, const float radius, const Plane* planes, const DrawVert* verts, const int numVerts);
void VPCALL SIMD_Generic_DecalPointCull(byte* cullBits, const Plane* planes, const DrawVert* verts, const int numVerts);
void VPCALL SIMD_Generic_OverlayPointCull(byte* cullBits, Vector2* texCoords, const Plane* planes, const DrawVert* verts, const int numVerts);
void VPCALL SIMD_Generic_DeriveTriPlanes(Plane* planes, const DrawVert* verts, const int numVerts, const int* indexes, const int numIndexes);
void VPCALL SIMD_Generic_DeriveTriPlanes(Plane* planes, const DrawVert* verts, const int numVerts, const short* indexes, const int numIndexes);
void VPCALL SIMD_Generic_DeriveTangents(Plane* planes, DrawVert* verts, const int numVerts, const int* indexes, const int numIndexes);
void VPCALL SIMD_Generic_DeriveTangents(Plane* planes, DrawVert* verts, const int numVerts, const short* indexes, const int numIndexes);
void VPCALL SIMD_Generic_DeriveUnsmoothedTangents(DrawVert* verts, const DominantTri* dominantTris, const int numVerts);
void VPCALL SIMD_Generic_NormalizeTangents(DrawVert* verts, const int numVerts);
void VPCALL SIMD_Generic_CreateTextureSpaceLightVectors(Vector3* lightVectors, const Vector3& lightOrigin, const DrawVert* verts, const int numVerts, const int* indexes, const int numIndexes);
void VPCALL SIMD_Generic_CreateSpecularTextureCoords(Vector4* texCoords, const Vector3& lightOrigin, const Vector3& viewOrigin, const DrawVert* verts, const int numVerts, const int* indexes, const int numIndexes);
int  VPCALL SIMD_Generic_CreateShadowCache(Vector4* vertexCache, int* vertRemap, const Vector3& lightOrigin, const DrawVert* verts, const int numVerts);
int  VPCALL SIMD_Generic_CreateVertexProgramShadowCache(Vector4* vertexCache, const DrawVert* verts, const int numVerts);

void VPCALL SIMD_Generic_UpSamplePCMTo44kHz(float* dest, const short* pcm, const int numSamples, const int kHz, const int numChannels);
void VPCALL SIMD_Generic_UpSampleOGGTo44kHz(float* dest, const float* const* ogg, const int numSamples, const int kHz, const int numChannels);
void VPCALL SIMD_Generic_MixSoundTwoSpeakerMono(float* mixBuffer, const float* samples, const int numSamples, const float lastV[2], const float currentV[2]);
void VPCALL SIMD_Generic_MixSoundTwoSpeakerStereo(float* mixBuffer, const float* samples, const int numSamples, const float lastV[2], const float currentV[2]);
void VPCALL SIMD_Generic_MixSoundSixSpeakerMono(float* mixBuffer, const float* samples, const int numSamples, const float lastV[6], const float currentV[6]);
void VPCALL SIMD_Generic_MixSoundSixSpeakerStereo(float* mixBuffer, const float* samples, const int numSamples, const float lastV[6], const float currentV[6]);
void VPCALL SIMD_Generic_MixedSoundToSamples(short* samples, const float* mixBuffer, const int numSamples);

#endif
