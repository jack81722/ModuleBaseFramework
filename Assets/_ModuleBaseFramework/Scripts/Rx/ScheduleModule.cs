using ModuleBased.ForUnity;
using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModuleBased
{
    [UniModule(typeof(IScheduleModule))]
    public class ScheduleModule : UniGameModule, IScheduleModule
    {
        // how to instantiate module ?

        Dictionary<Type, List<MethodInfo>> _scheduleMethods;

        protected override void OnStartingModule()
        {
            Debug.Log("Start");
            Test();
            //foreach (var mod in Modules)
            //{
            //    var methods = mod.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            //    foreach (var method in methods)
            //    {
            //        if (method.IsDefined(typeof(ObserverAttribute)))
            //        {
            //            var ps = method.GetParameters();
            //            if (ps.Length <= 0)
            //                continue;
            //            AddScheduleMethod(ps[0].ParameterType, method);
            //        }
            //    }

            //}
        }

        private void AddScheduleMethod(Type schType, MethodInfo method)
        {
            if (!_scheduleMethods.TryGetValue(schType, out List<MethodInfo> methods))
            {
                methods = new List<MethodInfo>();
                _scheduleMethods.Add(schType, methods);
            }
            methods.Add(method);
        }

        private void RemoveScheduleMethodAll(IGameModule module)
        {


        }

        public void Test()
        {
            HttpTrigger.Get("https://www.google.com/")
                .Subscribe(
                    c => Debug.Log(c),
                    e => Debug.LogError(e)
                );
        }
    }

    public interface IScheduleModule
    {
    }

    public class ObserverAttribute : Attribute
    {
    }
}
