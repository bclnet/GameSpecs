#include "system/Platform.h"
#include "Vector.h"

// Linearly inperpolates one vector to another.
void Vector3::Lerp(const Vector3& v1, const Vector3& v2, const float l) {
	if (l <= 0.0f) (*this) = v1;
	else if (l >= 1.0f) (*this) = v2;
	else (*this) = v1 + l * (v2 - v1);
}