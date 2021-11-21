using System;

namespace ModuleBased.Proxy.AOP
{
    public interface IAOPHandler
    {
        void OnInvoke(object sender, AOPEventArgs args);

    }

}
