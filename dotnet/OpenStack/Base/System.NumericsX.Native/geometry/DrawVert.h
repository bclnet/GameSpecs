#ifndef __DRAWVERT_H__
#define __DRAWVERT_H__

#include "math/Vector.h"

class DrawVert {
public:
	Vector3 xyz;
	Vector3 st;
	Vector3 normal;
	Vector3 tangents[2];
	byte color[4];
};

#endif
