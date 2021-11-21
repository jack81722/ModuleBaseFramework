using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace ModuleBased.Injection
{
    public interface IContainer
    {
        Contraction Contract(Type contractType, object identity = null);

        Contraction Contract<T>(object identity = null) where T : class;
    }


    public struct BindingInfo
    {
        private Type _contractType;
        private object _identity;

        public BindingInfo(Type contractType, object identity)
        {
            _contractType = contractType;
            _identity = identity;
        }
    }

    public class Container : IContainer, IEnumerable<Contraction>
    {
        Dictionary<BindingInfo, Contraction> _injectionDict = new Dictionary<BindingInfo, Contraction>();

        public Contraction Contract(Type contractType, object identity = null)
        {
            Contraction contraction = new Contraction();
            contraction.ContractType = contractType;
            contraction.Identity = identity;
            _injectionDict.Add(new BindingInfo(contractType, identity), contraction);
            return contraction;
        }


        public Contraction Contract<T>(object identity = null) where T : class
        {
            return Contract(typeof(T), identity);
        }

        public void BreakContract(Type contractType, object identity = null)
        {
            BindingInfo bind = new BindingInfo(contractType, identity);
            _injectionDict.Remove(bind);
        }

        public void BreakContract<T>(object identity = null)
        {
            BreakContract(typeof(T), identity);
        }

        public Contraction FindContract(Type contractType, object identity = null)
        {
            BindingInfo bind = new BindingInfo(contractType, identity);
            if (_injectionDict.TryGetValue(bind, out Contraction contraction))
            {
                return contraction;
            }
            return null;
        }


        public Contraction FindContract<T>(object identity = null)
        {
            return FindContract(typeof(T), identity);
        }

        public void DestroyConcrete(Type contractType, object identity = null)
        {
            var contraction = FindContract(contractType, identity);
            contraction.DestroyConcrete();
        }

        public void DestroyConcrete<T>(object identity = null)
        {
            DestroyConcrete(typeof(T), identity);
        }

        #region -- IEnumerable --
        public IEnumerator<Contraction> GetEnumerator()
        {
            return _injectionDict.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _injectionDict.Values.GetEnumerator();
        }
        #endregion
    }

    public static class ContractionExtension
    {
        public static Contraction Concrete<T>(this Contraction contraction)
        {
            Type concreteType = typeof(T);
            if (!Assert.MatchContract(contraction, concreteType))
                throw Errors.ErrContractNotMatched;
            contraction.ConcreteType = concreteType;
            return contraction;
        }

        public static Contraction Concrete<T>(this Contraction contraction, T concrete)
        {
            Type concreteType = typeof(T);
            if (!Assert.MatchContract(contraction, concreteType))
                throw Errors.ErrContractNotMatched;
            contraction.ConcreteType = concreteType;
            contraction.ConcreteInstance = concrete;
            return contraction;
        }

        public static Contraction Concrete(this Contraction contraction, Type concreteType, object concrete)
        {
            if (!Assert.MatchContract(contraction, concreteType))
                throw Errors.ErrContractNotMatched;
            contraction.ConcreteType = concreteType;
            contraction.ConcreteInstance = concrete;
            return contraction;
        }

        public static Contraction SetScope(this Contraction contraction, EContractScope scope)
        {
            contraction.ContractScope = scope;
            return contraction;
        }

        public static Contraction AsSingleton(this Contraction contraction)
        {
            return contraction.SetScope(EContractScope.Singleton);
        }

        public static Contraction AsTransient(this Contraction contraction)
        {
            return contraction.SetScope(EContractScope.Transient);
        }

        public static Contraction AsScoped(this Contraction contraction)
        {
            return contraction.SetScope(EContractScope.Scoped);
        }

        public static Contraction From(this Contraction contraction, object instance)
        {
            contraction.ConcreteInstance = instance;
            return contraction;
        }

        public static Contraction FromFunc(this Contraction contraction, Func<object> spawn)
        {
            return contraction.FromFactory(new SpawnFuncFactory(spawn));
        }

        public static Contraction FromFactory(this Contraction contraction, IFactory factory)
        {
            contraction.Factory = factory;
            return contraction;
        }

        public static Contraction WithArgs(this Contraction contraction, params object[] args)
        {
            contraction.Args = args;
            return contraction;
        }
    }

    public enum EContractScope
    {
        Singleton,
        Transient,
        Scoped
    }

    public class Contraction
    {
        internal object Identity;
        internal Type ContractType;
        internal EContractScope ContractScope = EContractScope.Singleton;
        internal Type ConcreteType;
        internal object ConcreteInstance;
        internal IFactory Factory;
        internal object[] Args;
        internal IDisposable Disposable;
        private bool _isWrapped;
        internal List<Type> InterceptorTypes = new List<Type>();

        #region -- Dirty pattern
        private bool _isDirty = true;

        public bool IsDirty()
        {
            var dirty = _isDirty;
            return dirty;
        }


        public void ResetDirty()
        {
            _isDirty = false;
        }
        #endregion

        public Contraction()
        {
            _isDirty = true;
        }

        private object instantiate()
        {
            if (ContractScope == EContractScope.Singleton)
            {
                if (ConcreteInstance == null)
                {
                    ConcreteInstance = Spawn();
                    Disposable = ConcreteInstance as IDisposable;
                }
                return ConcreteInstance;
            }
            if (ContractScope == EContractScope.Transient)
            {
                var concrete = Spawn();
                _isWrapped = false;
                Disposable = concrete as IDisposable;
                return concrete;
            }
            // scoped not implemented
            if (ConcreteInstance == null)
            {
                ConcreteInstance = Spawn();
                Disposable = ConcreteInstance as IDisposable;
            }
            _isDirty = true;
            return ConcreteInstance;
        }

        private object wrapInterceptor(object concrete)
        {
            if (_isWrapped)
                return concrete;
            object wrapped = concrete;
            foreach (var interceptorType in InterceptorTypes)
            {
                wrapped = Activator.CreateInstance(interceptorType, wrapped);
            }
            ConcreteInstance = wrapped;
            _isWrapped = true;
            return wrapped;
        }

        public object Instantiate()
        {
            var concrete = wrapInterceptor(instantiate());
            return concrete;
        }


        private object Spawn()
        {
            if (Factory != null)
            {
                return Factory.Create(Args);
            }
            return Activator.CreateInstance(ConcreteType, Args);
        }

        internal void DestroyConcrete()
        {
            if (Disposable != null)
                Disposable.Dispose();
        }
    }

    internal class Errors
    {
        public static readonly Exception ErrContractEmpty = new Exception("Contract types are empty.");
        public static readonly Exception ErrContractNotMatched = new Exception("Concrete type is not the subclass or implement of contract.");
    }

    public class SpawnFuncFactory : IFactory
    {
        private Func<object> _spawner;

        public SpawnFuncFactory(Func<object> spawner)
        {
            if (_spawner == null)
                throw new ArgumentNullException();
            _spawner = spawner;
        }

        public object Create(object args)
        {
            return _spawner.Invoke();
        }
    }

    internal static class Assert
    {

        public static bool MatchContract(Contraction contraction, Type concreteType)
        {
            return concreteType.IsAssignableFrom(concreteType) ||
                   concreteType.IsSubclassOf(contraction.ContractType) ||
                   contraction.ContractType == concreteType;
        }
    }

    public interface IFactory
    {
        object Create(object args);
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
        public object Identity { get; }

        public InjectAttribute(object identity = null)
        {
            Identity = identity;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FactoryAttribute : Attribute
    {
        public Type FactoryType { get; }

        public FactoryAttribute(Type factoryType)
        {
            FactoryType = factoryType;
        }
    }
}