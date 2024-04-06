#include "system/Platform.h"
#include "Matrix.h"
#include "Quat.h"

Matrix3x3 Quat::ToMat3(void) const {
	Matrix3x3	mat;
	float	wx, wy, wz;
	float	xx, yy, yz;
	float	xy, xz, zz;
	float	x2, y2, z2;

	x2 = x + x;
	y2 = y + y;
	z2 = z + z;

	xx = x * x2;
	xy = x * y2;
	xz = x * z2;

	yy = y * y2;
	yz = y * z2;
	zz = z * z2;

	wx = w * x2;
	wy = w * y2;
	wz = w * z2;

	mat[0][0] = 1.0f - (yy + zz);
	mat[0][1] = xy - wz;
	mat[0][2] = xz + wy;

	mat[1][0] = xy + wz;
	mat[1][1] = 1.0f - (xx + zz);
	mat[1][2] = yz - wx;

	mat[2][0] = xz - wy;
	mat[2][1] = yz + wx;
	mat[2][2] = 1.0f - (xx + yy);

	return mat;
}

// Spherical linear interpolation between two quaternions.
Quat& Quat::Slerp(const Quat& from, const Quat& to, float t) {
	Quat temp;
	float omega, cosom, sinom, scale0, scale1;

	if (t <= 0.0f) { *this = from; return *this; }
	if (t >= 1.0f) { *this = to; return *this; }
	if (from == to) { *this = to; return *this; }

	cosom = from.x * to.x + from.y * to.y + from.z * to.z + from.w * to.w;
	if (cosom < 0.0f) { temp = -to; cosom = -cosom; }
	else { temp = to; }

	if ((1.0f - cosom) > 1e-6f) {
		scale0 = 1.0f - cosom * cosom;
		sinom = MathX::InvSqrt(scale0);
		omega = MathX::ATan16(scale0 * sinom, cosom);
		scale0 = MathX::Sin16((1.0f - t) * omega) * sinom;
		scale1 = MathX::Sin16(t * omega) * sinom;
	}
	else { scale0 = 1.0f - t;		scale1 = t; }

	*this = (scale0 * from) + (scale1 * temp);
	return *this;
}