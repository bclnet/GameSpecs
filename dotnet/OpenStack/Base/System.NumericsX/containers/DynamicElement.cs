using System.Runtime.InteropServices;

namespace System.NumericsX
{
    public class DynamicElement<T>
    {
        public DynamicElement() { }
        public DynamicElement(T[] value)
            => Value = value;
        public T[] Value;
        public GCHandle ValueHandle;
        //public Memory<T> Memory;
    }
}