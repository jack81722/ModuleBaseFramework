using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModuleBased.Injection
{
    public class Container
    {
        List<Registration> _regList = new List<Registration>();

        public Registration Contract<T>()
        {
            Registration reg = new Registration();
            _regList.Add(reg);
            reg.ContractTypes.Add(typeof(T));
            return reg;
        }

        public Registration Contract<T1, T2>()
        {
            Registration reg = new Registration();
            _regList.Add(reg);
            reg.ContractTypes.Add(typeof(T1));
            reg.ContractTypes.Add(typeof(T2));
            return reg;
        }

        public Registration Contract<T1, T2, T3>()
        {
            Registration reg = new Registration();
            _regList.Add(reg);
            reg.ContractTypes.Add(typeof(T1));
            reg.ContractTypes.Add(typeof(T2));
            reg.ContractTypes.Add(typeof(T3));
            return reg;
        }

    }

    public static class ContainerExtension
    {
        public static Registration Contract<T>(this Registration reg)
        {
            reg.ContractTypes.Add(typeof(T));
            return reg;
        }

        public static Registration Contract<T1, T2>(this Registration reg)
        {
            reg.ContractTypes.Add(typeof(T1));
            reg.ContractTypes.Add(typeof(T2));
            return reg;
        }

        public static Registration Contract<T1, T2, T3>(this Registration reg)
        {
            reg.ContractTypes.Add(typeof(T1));
            reg.ContractTypes.Add(typeof(T2));
            reg.ContractTypes.Add(typeof(T3));
            return reg;
        }

        public static Registration Concrete<T>(this Registration reg)
        {
            reg.ConcreteType = typeof(T);
            return reg;
        }

        public static Registration AsSingleton(this Registration reg)
        {
            reg.ContractScope = EContractScope.Singleton;
            return reg;
        }

        public static Registration AsTransient(this Registration reg)
        {
            reg.ContractScope = EContractScope.Transient;
            return reg;
        }

        public static Registration AsScoped(this Registration reg)
        {
            reg.ContractScope = EContractScope.Scoped;
            return reg;
        }

        public static Registration From(this Registration reg, object instance)
        {
            reg.ConcreteInstance = instance;
            return reg;
        }

        public static Registration FromFactory(this Registration reg, IFactory factory)
        {
            reg.Factory = factory;
            return reg;
        }

        public static Registration WithArgs(this Registration reg, params object[] args)
        {
            reg.Args = args;
            return reg;
        }
    }

    public enum EContractScope
    {
        Singleton,
        Transient,
        Scoped
    }

    public class Registration
    {
        internal List<Type> ContractTypes;
        internal EContractScope ContractScope = EContractScope.Singleton;
        internal Type ConcreteType;
        internal object ConcreteInstance;
        internal IFactory Factory;
        internal object[] Args;

        public Registration()
        {
            ContractTypes = new List<Type>();
        }
    }

    public interface IFactory
    {
        object Create(object args);
    }

    public abstract class ContractProviderBase
    {
        protected Registration reg;

        public ContractProviderBase(Registration reg)
        {
            this.reg = reg;
        }

        public abstract void Inject(object target, MethodInfo method);

        protected object InstantiateConcreteType()
        {
            object instance;
            if (reg.Factory != null)
            {
                instance = reg.Factory.Create(reg.Args);
            }
            else
            {
                instance = Activator.CreateInstance(reg.ConcreteType, reg.Args);
            }
            return instance;
        }
    }

    public class TransientProvider : ContractProviderBase
    {
        public TransientProvider(Registration reg) : base(reg) { }

        public override void Inject(object target, MethodInfo method)
        {
            var instance = InstantiateConcreteType();
            method.Invoke(target, new object[] { instance });
        }
    }

    public class SingletonProvider : ContractProviderBase
    {
        public SingletonProvider(Registration reg) : base(reg) { }

        public override void Inject(object target, MethodInfo method)
        {
            if (reg.ConcreteInstance == null)
            {
                reg.ConcreteInstance = InstantiateConcreteType();
            }
            method.Invoke(target, new object[] { reg.ConcreteInstance });
        }
    }

    public class Injector
    {
        private HashSet<object> _injectables = new HashSet<object>();
        private Container _container;

        public void BindProvider()
        {

        }

        public void AnalyseReflection()
        {

        }
        
    }
}
