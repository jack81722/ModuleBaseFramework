using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
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

        internal Registration[] SelectContract(Type contractType)
        {
            return _regList.FindAll((reg) => reg.ContractTypes.Exists((contract) => contract == contractType)).ToArray();
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
            Type concreteType = typeof(T);
            if (!Assert.HasContract(reg))
                throw Errors.ErrContractEmpty;
            if (!Assert.MatchContract(reg, concreteType))
                throw Errors.ErrContractNotMatched;
            reg.ConcreteType = concreteType;
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

        public static Registration Identity(this Registration reg, object identity)
        {
            reg.Identity = identity;
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
        internal object Identity;
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

    internal class Errors
    {
        public static readonly Exception ErrContractEmpty = new Exception("Contract types are empty.");
        public static readonly Exception ErrContractNotMatched = new Exception("Concrete type is not the subclass or implement of contract.");

    }



    internal static class Assert
    {
        public static bool HasContract(Registration reg)
        {
            return reg.ContractTypes != null && reg.ContractTypes.Count > 0;
        }

        public static bool MatchContract(Registration reg, Type concreteType)
        {
            return reg.ContractTypes.TrueForAll((t) =>
            {
                return
                   t.IsAssignableFrom(concreteType) ||
                   concreteType.IsSubclassOf(t) ||
                   t == concreteType;
            });
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

        //public abstract void Inject(object target, FieldInfo field);

        //public abstract void Inject(object target, PropertyInfo field);

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
        private static readonly Type InjectAttrType = typeof(InjectAttribute);

        private Container _container;


        public void Inject(object target)
        {
            var targetType = target.GetType();
            var members = targetType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var member in members)
            {
                if (member.IsDefined(InjectAttrType))
                {
                    var attr = (InjectAttribute)member.GetCustomAttribute(InjectAttrType);
                    if (member.MemberType == MemberTypes.Field)
                    {
                        var field = (FieldInfo)member;
                        var contractType = field.FieldType;
                        var candidates = _container.SelectContract(contractType);
                        if (candidates.Length <= 0)
                            continue;
                        if (candidates.Length > 1)
                        {
                            candidates = candidates.Where((candidate) => candidate.Identity == attr.Identity).ToArray();
                            if (candidates.Length > 1)
                            {
                                throw new Exception("Inject candidate too much.");
                            }
                        }
                        var reg = candidates[0];
                        switch (reg.ContractScope)
                        {
                            case EContractScope.Singleton:
                                if (reg.ConcreteInstance == null)
                                    reg.ConcreteInstance = InstantiateConcreteType(reg);
                                field.SetValue(target, reg.ConcreteInstance);
                                break;
                            case EContractScope.Transient:
                                field.SetValue(target, InstantiateConcreteType(reg));
                                break;
                        }
                    }
                }
            }
        }

        protected object InstantiateConcreteType(Registration reg)
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

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
        public object Identity { get; }

        public InjectAttribute(object identity)
        {
            Identity = identity;
        }
    }
}
