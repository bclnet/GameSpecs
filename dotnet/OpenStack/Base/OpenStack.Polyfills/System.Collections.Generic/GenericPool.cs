using System.Collections.Concurrent;

namespace System.Collections.Generic
{
    public delegate void GenericPoolAction<T>(Action<T> action);
    public delegate TResult GenericPoolFunc<T, TResult>(Func<T, TResult> action);

    public class GenericPool<T> : IDisposable
        where T : IDisposable
    {
        readonly ConcurrentBag<T> _items = new ConcurrentBag<T>();
        public readonly Func<T> Factory;
        public readonly int RetainInPool;

        public GenericPool(Func<T> factory, int retainInPool = 10)
        {
            Factory = factory;
            RetainInPool = retainInPool;
        }

        public void Dispose()
        {
            foreach (var item in _items)
                item.Dispose();
        }

        public void Release(T item)
        {
            if (_items.Count < RetainInPool) _items.Add(item);
            else item.Dispose();
        }

        public T Get() => _items.TryTake(out var item) ? item : Factory();

        public void Action(Action<T> action)
        {
            var item = Get();
            try { action(item); }
            finally { Release(item); }
        }

        public TResult Func<TResult>(Func<T, TResult> action)
        {
            var item = Get();
            try { return action(item); }
            finally { Release(item); }
        }
    }
}