//using System;
//using System.Threading;

//namespace StereoKit.Maui.Platform
//{
//    class ActionDisposable : IDisposable
//    {
//        volatile Action? _action;
//        public ActionDisposable(Action action) => _action = action;
//        public void Dispose() => Interlocked.Exchange(ref _action, null)?.Invoke();
//    }
//}
