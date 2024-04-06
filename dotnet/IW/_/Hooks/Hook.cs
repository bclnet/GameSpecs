using System;
using System.Collections.Generic;

namespace GameX.IW.Formats
{
    public unsafe class Hook
    {
        bool initialized;
        bool installed;
        IntPtr place;
        object stub;
        object original;
        byte[] buffer;
        bool useJump;
        ushort protection;
        object stateMutex;

        public class Signature
        {
            IntPtr start;
            int length;
            List<Container> signatures = new List<Container>();

            public struct Container
            {
                public string signature;
                public string mask;
                public Action<IntPtr> callback;
            }

            public Signature() : this(0x400000, 0x800000) { }
            public Signature(int start, int length) : this(new IntPtr(start), length) { }
            public Signature(IntPtr start, int length)
            {
                this.start = start;
                this.length = length;
            }

            public void process()
            {
                if (signatures.Count == 0) return;

                var _start = (char*)start;

                var sigCount = signatures.Count;
                var containers = signatures;

                for (var i = 0; i < length; ++i)
                {
                    var address = _start + i;
                    for (var k = 0; k < sigCount; ++k)
                    {
                        var container = containers[k];
                        int j;
                        for (j = 0; j < container.mask.Length; ++j)
                            if (container.mask[j] != '?' && container.signature[j] != address[j])
                                break;
                        if (j == container.mask.Length)
                            container.callback((IntPtr)address);
                    }
                }
            }

            public void add(ref Container container)
                => signatures.Add(container);
        }

        public class Interceptor
        {
            static Dictionary<IntPtr, object> IReturn = new Dictionary<IntPtr, object>();
            static Dictionary<IntPtr, Action> ICallbacks = new Dictionary<IntPtr, Action>();

            
            public static void Install(ref IntPtr place, Action stub)
            {
                IReturn[place] = place;
                ICallbacks[place] = stub;
                //place = InterceptionStub();
            }
            public static void Install(ref IntPtr place, object stub) => Install(ref place, (Action)stub);
            //public static void Install(IntPtr place, Action stub) => Install(reinterpret_cast<void**>(place), stub);

            //static void InterceptionStub()
            //{
            //    RunCallback();
            //}

            static object RunCallback(IntPtr place)
            {
                if (ICallbacks.TryGetValue(place, out var callback))
                {
                    callback();
                    ICallbacks.Remove(place);
                }
                object retVal = null;
                if (IReturn.TryGetValue(place, out var ret))
                {
                    retVal = ret;
                    IReturn.Remove(place);
                }
                return retVal;
            }
        }

        public Hook() { }
        public Hook(IntPtr place, object stub, bool useJump = true) { initialize(place, stub, useJump); }
        public Hook(IntPtr place, Action stub, bool useJump = true) : this(place, (object)stub, useJump) { }

        public Hook(int place, object stub, bool useJump = true) : this(new IntPtr(place), stub, useJump) { }
        public Hook(int place, int stub, bool useJump = true) : this(new IntPtr(place), (object)stub, useJump) { }
        public Hook(int place, Action stub, bool useJump = true) : this(new IntPtr(place), (object)stub, useJump) { }

        public Hook initialize(IntPtr place, object stub, bool useJump = true)
        {
            if (initialized) return this;
            initialized = true;

            this.useJump = useJump;
            this.place = place;
            this.stub = stub;

            //original = static_cast<char*>(this.place) + 5 + *reinterpret_cast<DWORD*>((static_cast<char*>(this.place) + 1));

            return this;
        }
        public Hook initialize(int place, object stub, bool useJump = true) => initialize(new IntPtr(place), stub, useJump);
        public Hook initialize(int place, Action stub, bool useJump = true) => initialize(new IntPtr(place), (object)stub, useJump); // For lambdas

        public Hook install(bool unprotect = true, bool keepUnprotected = false)
        {
            lock (stateMutex)
            {
                if (!initialized || installed)
                    return this;
                installed = true;

                //if (unprotect) VirtualProtect(this->place, sizeof(this->buffer), PAGE_EXECUTE_READWRITE, &this->protection);
                //std::memcpy(this->buffer, this->place, sizeof(this->buffer));

                //char* code = static_cast<char*>(this->place);

                //*code = static_cast<char>(this->useJump ? 0xE9 : 0xE8);

                //*reinterpret_cast<size_t*>(code + 1) = reinterpret_cast<size_t>(this->stub) - (reinterpret_cast<size_t>(this->place) + 5);

                //if (unprotect && !keepUnprotected) VirtualProtect(this->place, sizeof(this->buffer), this->protection, &this->protection);

                //FlushInstructionCache(GetCurrentProcess(), this->place, sizeof(this->buffer));

                return this;
            }
        }

        public void quick()
        {
            if (installed)
                installed = false;
        }

        public Hook uninstall(bool unprotect = true)
        {
            lock (stateMutex)
            {
                if (!initialized || !installed)
                    return this;
                installed = false;

                //if (unprotect) VirtualProtect(this->place, sizeof(this->buffer), PAGE_EXECUTE_READWRITE, &this->protection);

                //std::memcpy(this->place, this->buffer, sizeof(this->buffer));

                //if (unprotect) VirtualProtect(this->place, sizeof(this->buffer), this->protection, &this->protection);

                //FlushInstructionCache(GetCurrentProcess(), this->place, sizeof(this->buffer));

                return this;
            }
        }

        public void Dispose()
        {
            if (initialized)
                uninstall();
        }

        public IntPtr Address => place;

        public static Func<T> Call<T>(int function)
        {
            return null;
            //Func<T>(reinterpret_cast<T*>(function));
        }
        //public static Func<T> Call<T>(FARPROC function) => Call<T>(reinterpret_cast<int>(function));
        //public static Func<T> Call<T>(void* function) => Call<T>(reinterpret_cast<int>(function));

        public static void Nop(IntPtr place, int length)
        {
            //DWORD oldProtect;
            //VirtualProtect(place, length, PAGE_EXECUTE_READWRITE, &oldProtect);

            //memset(place, 0x90, length);

            //VirtualProtect(place, length, oldProtect, &oldProtect);
            //FlushInstructionCache(GetCurrentProcess(), place, length);
        }
        public static void Nop(int place, int length) => Nop(new IntPtr(place), length);

        public static void SetString(IntPtr place, string s, int length)
        {
            //DWORD oldProtect;
            //VirtualProtect(place, length + 1, PAGE_EXECUTE_READWRITE, &oldProtect);

            //strncpy_s(static_cast<char*>(place), length, string, length);

            //VirtualProtect(place, length + 1, oldProtect, &oldProtect);
        }
        public static void SetString(int place, string s, int length) => SetString(new IntPtr(place), s, length);
        public static void SetString(IntPtr place, string s) => SetString(place, s, s.Length);
        public static void SetString(int place, string s) => SetString(new IntPtr(place), s, s.Length);

        public static void RedirectJump(IntPtr place, object stub)
        {
            //char* operandPtr = static_cast<char*>(place) + 2;
            //int newOperand = reinterpret_cast<int>(stub) - (reinterpret_cast<int>(place) + 6);
            //Set<int>(operandPtr, newOperand);
        }
        public static void RedirectJump(int place, object stub) => RedirectJump(new IntPtr(place), stub);

        public static void Set<T>(IntPtr place, T value)
        {
            //int oldProtect;
            //VirtualProtect(place, sizeof(T), PAGE_EXECUTE_READWRITE, &oldProtect);

            //*static_cast<T*>(place) = value;

            //VirtualProtect(place, sizeof(T), oldProtect, &oldProtect);
            //FlushInstructionCache(GetCurrentProcess(), place, sizeof(T));
        }
        public static void Set<T>(int place, T value) => Set<T>(new IntPtr(place), value);

        public static void Xor<T>(IntPtr place, T value)
        {
            //int oldProtect;
            //VirtualProtect(place, sizeof(T), PAGE_EXECUTE_READWRITE, &oldProtect);

            //*static_cast<T*>(place) ^= value;

            //VirtualProtect(place, sizeof(T), oldProtect, &oldProtect);
            //FlushInstructionCache(GetCurrentProcess(), place, sizeof(T));
        }
        public static void Xor<T>(int place, T value) => Xor<T>(new IntPtr(place), value);

        public static void Or<T>(IntPtr place, T value)
        {
            //int oldProtect;
            //VirtualProtect(place, sizeof(T), PAGE_EXECUTE_READWRITE, &oldProtect);

            //*static_cast<T*>(place) |= value;

            //VirtualProtect(place, sizeof(T), oldProtect, &oldProtect);
            //FlushInstructionCache(GetCurrentProcess(), place, sizeof(T));
        }
        public static void Or<T>(int place, T value) => Or<T>(new IntPtr(place), value);

        public static void And<T>(IntPtr place, T value)
        {
            //int oldProtect;
            //VirtualProtect(place, sizeof(T), PAGE_EXECUTE_READWRITE, &oldProtect);

            //*static_cast<T*>(place) &= value;

            //VirtualProtect(place, sizeof(T), oldProtect, &oldProtect);
            //FlushInstructionCache(GetCurrentProcess(), place, sizeof(T));
        }
        public static void And<T>(int place, T value) => And<T>(new IntPtr(place), value);

        public static T Get<T>(IntPtr place)
            => default; //*static_cast<T*>(place);
        public static T Get<T>(int place) => Get<T>(new IntPtr(place));
    }
}