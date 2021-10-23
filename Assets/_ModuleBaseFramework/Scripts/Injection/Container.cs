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

        private Dictionary<Type, List<InjectBinding>> _bindings = new Dictionary<Type, List<InjectBinding>>();

        private Container _container;

        public List<InjectBinding> Analyze(Type targetType)
        {
            var members = targetType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (!_bindings.TryGetValue(targetType, out List<InjectBinding> bindings))
            {
                bindings = new List<InjectBinding>();
                _bindings.Add(targetType, bindings);
            }
            foreach (var member in members)
            {
                if (!member.IsDefined(InjectAttrType))
                    continue;
                var info = new InjectInfo(member);
                ContractProviderBase provider;
                var regs = _container.SelectContract(info.RequiredContractType);
                if (regs.Length < 0)
                    continue;
                if (regs.Length > 1)
                {
                    regs = regs.Where((candidate) => candidate.Identity == info.Identity).ToArray();
                    if (regs.Length > 1)
                    {
                        throw new Exception("Inject candidate too much.");
                    }
                }
                var reg = regs[0];
                switch (reg.ContractScope)
                {
                    case EContractScope.Singleton:
                        provider = new SingletonProvider(reg);
                        break;
                    case EContractScope.Transient:
                        provider = new TransientProvider(reg);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown contract scope.");
                }
                var binding = new InjectBinding(info, reg, provider);
                bindings.Add(binding);
            }
            return bindings;
        }

        public void Inject(object target)
        {
            var targetType = target.GetType();
            if (!_bindings.TryGetValue(targetType, out List<InjectBinding> bindings))
            {
                bindings = Analyze(targetType);
            }
            foreach (var binding in bindings)
            {
                var info = binding.InjectInfo;
                var reg = binding.Registration;
                switch (reg.ContractScope)
                {
                    case EContractScope.Singleton:
                        if (reg.ConcreteInstance == null)
                            reg.ConcreteInstance = InstantiateConcreteType(reg);
                        info.Inject(target, reg.ConcreteInstance);
                        break;
                    case EContractScope.Transient:
                        info.Inject(target, InstantiateConcreteType(reg));
                        break;
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

    public class InjectBinding
    {
        public InjectInfo InjectInfo;
        public Registration Registration;
        public ContractProviderBase Provider;

        public InjectBinding(InjectInfo info, Registration reg, ContractProviderBase provider)
        {
            InjectInfo = info;
            Registration = reg;
            Provider = provider;
        }
    }

    public class InjectInfo
    {
        private static readonly Type InjectAttrType = typeof(InjectAttribute);

        private MemberInfo _member;
        private MemberTypes _memberType;

        public object Identity { get; private set; }
        public Type RequiredContractType { get; private set; }

        public InjectInfo(MemberInfo member)
        {
            if (!member.IsDefined(InjectAttrType))
                throw new Exception("Must be defined inject attribute.");
            var attr = (InjectAttribute)member.GetCustomAttribute(InjectAttrType);
            Identity = attr.Identity;
            _member = member;
            _memberType = member.MemberType;
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    var field = (FieldInfo)member;
                    RequiredContractType = field.FieldType;
                    break;
                case MemberTypes.Property:
                    var prop = (PropertyInfo)member;
                    RequiredContractType = prop.PropertyType;
                    break;
            }

        }

        public void Inject(object target, object concreteObj)
        {
            switch (_memberType)
            {
                case MemberTypes.Field:
                    var field = (FieldInfo)_member;
                    field.SetValue(target, concreteObj);
                    break;
                case MemberTypes.Property:
                    var prop = (PropertyInfo)_member;
                    prop.SetValue(target, concreteObj);
                    break;
            }
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