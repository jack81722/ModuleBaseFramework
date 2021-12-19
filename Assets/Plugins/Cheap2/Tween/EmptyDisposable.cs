using System;

namespace Cheap2.Plugin.TweenExt
{
    public class EmptyDisposable : IDisposable
    {
        private static EmptyDisposable _singleton;
        public static EmptyDisposable Singleton {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new EmptyDisposable();
                }
                return _singleton;
            }
        }

        public void Dispose() { }
    }
}