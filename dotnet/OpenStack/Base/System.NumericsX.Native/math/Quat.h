#ifndef __QUAT_H__
#define __QUAT_H__

#include "Vector.h"

class Matrix3x3;

class Quat {
public:
	float x;
	float y;
	float z;
	float w;

	Quat(void);
	Quat(float x, float y, float z, float w);

	float operator[](int index) const;
	float& operator[](int index);
	Quat operator-() const;
	Quat operator+(const Quat& a) const;
	Quat operator*(float a) const;

	friend Quat operator*(const float a, const Quat& b);

	bool Compare(const Quat& a) const;						// exact compare, no epsilon
	bool operator==(const Quat& a) const;					// exact compare, no epsilon

	Matrix3x3 ToMat3(void) const;

	Quat& Slerp(const Quat& from, const Quat& to, float t);

	const float* ToFloatPtr(void) const;
	float* ToFloatPtr(void);
};

ID_INLINE Quat::Quat(void) { }
ID_INLINE Quat::Quat(float x, float y, float z, float w) { this->x = x; this->y = y; this->z = z; this->w = w; }

ID_INLINE float Quat::operator[](int index) const { assert((index >= 0) && (index < 4)); return (&x)[index]; }
ID_INLINE float& Quat::operator[](int index) { assert((index >= 0) && (index < 4)); return (&x)[index]; }
ID_INLINE Quat Quat::operator-() const { return Quat(-x, -y, -z, -w); }
ID_INLINE Quat Quat::operator+(const Quat& a) const { return Quat(x + a.x, y + a.y, z + a.z, w + a.w); }
ID_INLINE Quat Quat::operator*(float a) const { return Quat(x * a, y * a, z * a, w * a); }
ID_INLINE Quat operator*(const float a, const Quat& b) { return b * a; }

ID_INLINE bool Quat::Compare(const Quat& a) const { return ((x == a.x) && (y == a.y) && (z == a.z) && (w == a.w)); }
ID_INLINE bool Quat::operator==(const Quat& a) const { return Compare(a); }

ID_INLINE const float* Quat::ToFloatPtr(void) const { return &x; }
ID_INLINE float* Quat::ToFloatPtr(void) { return &x; }


#endif
