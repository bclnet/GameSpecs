using StereoKit;
using System;
using System.Collections.Generic;

namespace GameX.App.Explorer.Tools
{
    class DemoAnim<T>
    {
        class TimeCompare : IComparer<(float time, T data)>
        {
            public int Compare((float time, T data) a, (float time, T data) b) => a.time.CompareTo(b.time);
        }

        TimeCompare _comparer = new();
        (float time, T data)[] _frames;
        Func<T, T, float, T> _lerp;
        float _startTime;
        float _updated;
        float _speed = 1;
        T _curr;

        public T Current
        {
            get
            {
                var now = Time.Totalf;
                if (now == _updated) return _curr;
                var elapsed = (now - _startTime) * _speed;
                _updated = now;
                _curr = Sample(elapsed);
                return _curr;
            }
        }
        public bool Playing => (Time.Totalf - _startTime) <= _frames[_frames.Length - 1].time;
        public float Duration => _frames[_frames.Length - 1].time;

        public DemoAnim(Func<T, T, float, T> lerp, params (float, T)[] frames)
        {
            _frames = frames;
            _lerp = lerp;
            _startTime = Time.Totalf;
        }

        public void Play(float speed = 1)
        {
            _speed = speed;
            _startTime = Time.Totalf;
        }

        T Sample(float time)
        {
            if (time <= _frames[0].time) return _frames[0].data;
            else if (time >= _frames[_frames.Length - 1].time) return _frames[_frames.Length - 1].data;

            var item = Array.BinarySearch(_frames, (time, default(T)), _comparer);
            if (item > 0) return _frames[item].data;
            else
            {
                item = ~item;
                var p1 = _frames[item - 1];
                var p2 = _frames[item];
                float pct = (time - p1.time) / (p2.time - p1.time);
                return _lerp(p1.data, p2.data, pct);
            }
        }
    }
}