using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama
{
    public interface IDramaAction : IObservable, IDisposable, IPlayable
    {
        float TimeScale { get; set; }

        bool IsPause();

        bool IsCompleted();

        void Complete();

    }
}
