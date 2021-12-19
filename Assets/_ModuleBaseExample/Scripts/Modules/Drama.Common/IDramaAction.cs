using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama
{
    public interface IDramaAction : IObservable<object>, IDisposable
    {
        bool IsPause();

        bool IsFinished();

        void ModifySpeed(float speed);

        void Play();

        void Pause();

        void Resume();
    }
}
