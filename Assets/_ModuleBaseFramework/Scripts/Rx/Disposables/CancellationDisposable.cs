using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ModuleBased.Rx
{
    public class CancellationDisposable : IDisposable
    {
        private CancellationTokenSource _cts;

        public CancellationDisposable()
        {
            _cts = new CancellationTokenSource();
        }

        public CancellationDisposable(CancellationTokenSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Source is null.");
            }
            _cts = source;
        }

        public void Dispose()
        {
            _cts.Cancel();
        }

        public CancellationToken Token => _cts.Token;
        public bool IsDisposed => _cts.IsCancellationRequested;
    }


}
