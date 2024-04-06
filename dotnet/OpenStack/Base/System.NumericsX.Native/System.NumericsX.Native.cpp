#include "System.NumericsX.Native.h"
#include "math/Simd_3DNow.h"
#include "math/Simd_Generic.h"
#include "math/Simd_MMX.h"

using namespace std;

int main()
{
	cout << "SIMD: " << GetProcessorId() << endl;
	//cout << "SIMD: " << SIMD_3DNow_GetName() << endl; 
	cout << "SIMD: " << SIMD_Generic_GetName() << endl;
	cout << "SIMD: " << SIMD_MMX_GetName() << endl;
	return 0;
}
