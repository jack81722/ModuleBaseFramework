using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased
{
    public class SerialDisposable : IDisposable
    {
        private IDisposable _current;

        public IDisposable Disposable
        {
            get => _current;
            set
            {
                _current = value;
            }
        }

        public void Dispose()
        {
            _current.Dispose();
        }
    }
}
