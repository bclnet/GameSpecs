using static System.Diagnostics.Debug;

namespace System.Collections.Generic
{
    /// <summary>
    /// A reference to a range of elements in a non-null array.
    /// </summary>
    public struct ArrayRange<T> : IEnumerable<T>
    {
        #region Enumerator

        /// <summary>
        /// An enumerator for the elements in an array range.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            ArrayRange<T> _arrayRange;
            int _currentIndex;

            public Enumerator(ArrayRange<T> arrayRange)
            {
                _arrayRange = arrayRange;
                _currentIndex = arrayRange.offset - 1; // Enumerators start positioned before the first element.
            }

            public void Dispose()
            {
                _arrayRange = new ArrayRange<T>();
                _currentIndex = -1;
            }

            public T Current => _arrayRange.array[_currentIndex];
            object IEnumerator.Current => Current;

            public bool MoveNext() => ++_currentIndex < (_arrayRange.offset + _arrayRange.length);

            public void Reset() => _currentIndex = _arrayRange.offset - 1; // Enumerators start positioned before the first element.
        }

        #endregion

        /// <summary>
        /// Constructs an ArrayRange referring to an entire array.
        /// </summary>
        /// <param name="array">A non-null array.</param>
        public ArrayRange(T[] array)
        {
            Assert(array != null);
            this.array = array;
            offset = 0;
            length = array.Length;
        }
        /// <summary>
        /// Constructs an ArrayRange referring to a portion of an array.
        /// </summary>
        /// <param name="array">A non-null array.</param>
        /// <param name="offset">A nonnegative offset.</param>
        /// <param name="length">A nonnegative length.</param>
        public ArrayRange(T[] array, int offset, int length)
        {
            Assert(array != null && offset >= 0 && length >= 0 && (offset + length) <= array.Length);
            this.array = array;
            this.offset = offset;
            this.length = length;
        }

        public T[] array { get; }
        public int offset { get; }
        public int length { get; }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}