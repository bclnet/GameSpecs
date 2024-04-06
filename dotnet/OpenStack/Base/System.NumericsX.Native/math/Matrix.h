#ifndef __MATRIX_H__
#define __MATRIX_H__

#include "Vector.h"

class Matrix3x3 {
public:
	const Vector3& operator[](int index) const;
	Vector3& operator[](int index);

	const float* ToFloatPtr(void) const;
	float* ToFloatPtr(void);

private:
	Vector3			mat[3];
};

ID_INLINE const Vector3& Matrix3x3::operator[](int index) const { return mat[index]; }
ID_INLINE Vector3& Matrix3x3::operator[](int index) { return mat[index]; }

ID_INLINE const float* Matrix3x3::ToFloatPtr(void) const { return mat[0].ToFloatPtr(); }
ID_INLINE float* Matrix3x3::ToFloatPtr(void) { return mat[0].ToFloatPtr(); }

class MatrixX {
public:
	int	numRows;		// number of rows
	int	numColumns;		// number of columns
	int	alloced;		// floats allocated, if -1 then mat points to data set with SetData
	float* mat;			// memory the matrix is stored

	const float* operator[](int index) const;
	float* operator[](int index);

	int GetNumRows(void) const { return numRows; }					// get the number of rows
	int GetNumColumns(void) const { return numColumns; }				// get the number of columns

	const float* ToFloatPtr(void) const;
	float* ToFloatPtr(void);
};

ID_INLINE const float* MatrixX::operator[](int index) const { assert((index >= 0) && (index < numRows)); return mat + index * numColumns; }
ID_INLINE float* MatrixX::operator[](int index) { assert((index >= 0) && (index < numRows)); return mat + index * numColumns; }

ID_INLINE const float* MatrixX::ToFloatPtr(void) const { return mat; }
ID_INLINE float* MatrixX::ToFloatPtr(void) { return mat; }

#endif
