using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WindowView : MonoBehaviour, IWindow { 
    protected Subject<WindowView> subject;

    public void ClickOpen()
    {
        Open();
    }


    public IObservable<WindowView> Open()
    {
        try
        {
            subject = new Subject<WindowView>();
            return subject;
        }
        finally
        {
            OnOpen();
        }
    }

    public void Close()
    {   
        try
        {
            subject?.OnNext(this);
            subject?.OnCompleted();
        }
        finally
        {
            OnClose();
        }
    }

    protected virtual void OnOpen() { }

    protected virtual void OnClose() { }
}

public interface IWindow
{
    IObservable<WindowView> Open();

}
