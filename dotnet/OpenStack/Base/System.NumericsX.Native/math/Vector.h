#ifndef __VECTOR_H__
#define __VECTOR_H__

#include "MathX.h"

class Vector2 {
public:
	float x;
	float y;

	float operator[](int index) const;
	float& operator[](int index);
};

ID_INLINE float Vector2::operator[](int index) const { return (&x)[index]; }
ID_INLINE float& Vector2::operator[](int index) { return (&x)[index]; }

class Vector3 {
public:
	float x;
	float y;
	float z;

	Vector3(void);
	explicit Vector3(const float x, const float y, const float z);

	float operator[](int index) const;
	float& operator[](int index);
	Vector3 operator-(const Vector3& a) const;
	float operator*(const Vector3& a) const;
	Vector3 operator*(const float a) const;
	Vector3 operator+(const Vector3& a) const;
	Vector3& operator+=(const Vector3& a);
	Vector3& operator-=(const Vector3& a);

	const float* ToFloatPtr(void) const;
	float* ToFloatPtr(void);

	void Lerp(const Vector3& v1, const Vector3& v2, const float l);
};

ID_INLINE Vector3::Vector3(void) { }
ID_INLINE Vector3::Vector3(const float x, const float y, const float z) { this->x = x; this->y = y; this->z = z; }

ID_INLINE float Vector3::operator[](int index) const { return (&x)[index]; }
ID_INLINE float& Vector3::operator[](int index) { return (&x)[index]; }
ID_INLINE Vector3 Vector3::operator-(const Vector3& a) const { return Vector3(x - a.x, y - a.y, z - a.z); }
ID_INLINE float Vector3::operator*(const Vector3& a) const { return x * a.x + y * a.y + z * a.z; }
ID_INLINE Vector3 Vector3::operator*(const float a) const { return Vector3(x * a, y * a, z * a); }
ID_INLINE Vector3 operator*(const float a, const Vector3 b) { return Vector3(b.x * a, b.y * a, b.z * a); }
ID_INLINE Vector3 Vector3::operator+(const Vector3& a) const { return Vector3(x + a.x, y + a.y, z + a.z); }
ID_INLINE Vector3& Vector3::operator+=(const Vector3& a) { x += a.x; y += a.y; z += a.z; return *this; }
ID_INLINE Vector3& Vector3::operator-=(const Vector3& a) { x -= a.x; y -= a.y; z -= a.z; return *this; }

ID_INLINE const float* Vector3::ToFloatPtr(void) const { return &x; }
ID_INLINE float* Vector3::ToFloatPtr(void) { return &x; }

class Vector4 {
public:
	float x;
	float y;
	float z;
	float w;

	float operator[](int index) const;
	float& operator[](int index);
};

ID_INLINE float Vector4::operator[](int index) const { return (&x)[index]; }
ID_INLINE float& Vector4::operator[](int index) { return (&x)[index]; }


//class Vector5 {
//public:
//	float x;
//	float y;
//	float z;
//	float s;
//	float t;
//};

//class Vector6 {
//public:
//	float p[6];
//};

class VectorX {
public:
	int size;		// size of the vector
	int alloced;	// if -1 p points to data set with SetData
	float* p;		// memory the vector is stored

	float operator[](int index) const;
	float& operator[](int index);

	int GetSize(void) const { return size; }

	const float* ToFloatPtr(void) const;
	float* ToFloatPtr(void);
};

ID_INLINE float VectorX::operator[](const int index) const { assert(index >= 0 && index < size); return p[index]; }
ID_INLINE float& VectorX::operator[](const int index) { assert(index >= 0 && index < size); return p[index]; }

ID_INLINE const float* VectorX::ToFloatPtr(void) const { return p; }
ID_INLINE float* VectorX::ToFloatPtr(void) { return p; }

#endif
