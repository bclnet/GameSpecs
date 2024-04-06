#ifndef __PLANE_H__
#define __PLANE_H__

#include "Vector.h"
#include "Matrix.h"

class Plane {
public:
	float a;
	float b;
	float c;
	float d;

	float operator[](int index) const;
	float& operator[](int index);

	void SetNormal(const Vector3& normal);		// sets the normal
	const Vector3& Normal(void) const;					// reference to const normal
	Vector3& Normal(void);							// reference to normal

	void FitThroughPoint(const Vector3& p);	// assumes normal is valid

	float Distance(const Vector3& v) const;
};

ID_INLINE float Plane::operator[](int index) const { return (&a)[index]; }
ID_INLINE float& Plane::operator[](int index) { return (&a)[index]; }

ID_INLINE void Plane::SetNormal(const Vector3& normal) { a = normal.x; b = normal.y; c = normal.z; }
ID_INLINE const Vector3& Plane::Normal(void) const { return *reinterpret_cast<const Vector3*>(&a); }
ID_INLINE Vector3& Plane::Normal(void) { return *reinterpret_cast<Vector3*>(&a); }

ID_INLINE void Plane::FitThroughPoint(const Vector3& p) { d = -(Normal() * p); }

ID_INLINE float Plane::Distance(const Vector3& v) const { return a * v.x + b * v.y + c * v.z + d; }

#endif
