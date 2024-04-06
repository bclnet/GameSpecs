using System.Runtime.CompilerServices;

namespace System.NumericsX
{
    public unsafe static class BinSearch
    {
        // Finds the last array element which is smaller than the given value.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Less<T>(Span<T> array, int arraySize, in T value) where T : IComparable<T>
        {
            int len = arraySize, mid = len, offset = 0;
            while (mid > 0)
            {
                mid = len >> 1;
                if (array[offset + mid].CompareTo(value) < 0) offset += mid;
                len -= mid;
            }
            return offset;
        }

        // Finds the last array element which is smaller than or equal to the given value.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LessEqual<T>(Span<T> array, int arraySize, in T value) where T : IComparable<T>
        {
            int len = arraySize, mid = len, offset = 0;
            while (mid > 0)
            {
                mid = len >> 1;
                if (array[offset + mid].CompareTo(value) <= 0) offset += mid;
                len -= mid;
            }
            return offset;
        }

        // Finds the first array element which is greater than the given value.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Greater<T>(Span<T> array, int arraySize, in T value) where T : IComparable<T>
        {
            int len = arraySize, mid = len, offset = 0, res = 0;
            while (mid > 0)
            {
                mid = len >> 1;
                if (array[offset + mid].CompareTo(value) > 0) res = 0;
                else { offset += mid; res = 1; }
                len -= mid;
            }
            return offset + res;
        }

        // Finds the first array element which is greater than or equal to the given value.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GreaterEqual<T>(Span<T> array, int arraySize, in T value) where T : IComparable<T>
        {
            int len = arraySize, mid = len, offset = 0, res = 0;
            while (mid > 0)
            {
                mid = len >> 1;
                if (array[offset + mid].CompareTo(value) >= 0) res = 0;
                else { offset += mid; res = 1; }
                len -= mid;
            }
            return offset + res;
        }
    }
}