using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased
{
    public interface IObserver<T>
    {
        void OnError(Exception error);
        void OnComplete(T result);
    }

    public interface IObservable<T>
    {
        IDisposable Subscribe(IObserver<T> observable);
    }

    public static class ObservableUtils
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onCompleted, Action<Exception> onError)
        {
            var observer = new Subscribe_<T>(onCompleted, onError);
            return source.Subscribe(observer);
        }
    }
}


