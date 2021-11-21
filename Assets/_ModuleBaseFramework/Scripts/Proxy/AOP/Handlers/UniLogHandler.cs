using ModuleBased.Utils;
using System;
using UnityEngine;

namespace ModuleBased.Proxy.AOP.Handlers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UniLogAttribute : AOPAttribute
    {
        public UniLogAttribute(EAOPStatus usage) : base(usage, typeof(UniLogHandler)) { }
    }

    public class UniLogHandler : IAOPHandler
    {
        public void OnInvoke(object sender, AOPEventArgs args)
        {
            try
            {
                switch (args.Status)
                {
                    case EAOPStatus.Before:
                        Debug.Log($"Before call [{args.Method.Name}] (params:{args.Args.ToArrayString()})");
                        break;
                    case EAOPStatus.After:
                        Debug.Log($"After call [{args.Method.Name}] (params:{args.Args.ToArrayString()}, result:{args.Result})");
                        break;
                    case EAOPStatus.Around:
                        Debug.Log($"Around call [{args.Method.Name}] (params:{args.Args.ToArrayString()}, result:{args.Result})");
                        break;
                    case EAOPStatus.Error:
                        Debug.Log($"Failed call [{args.Method.Name}] (params:{args.Args.ToArrayString()}, error:{args.Error})");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to invoke AOP handler:{e}");
            }
        }
    }
}
