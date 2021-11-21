using ModuleBased.DAO;
using ModuleBased.Injection;
using ModuleBased.Models;
using ModuleBased.Rx;
using ModuleBased.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased
{
    public class GameCore : IGameCore
    {
        private Container _container;
        private GameCoreFsm _fsm;

        public GameCore()
        {
            _fsm = new GameCoreFsm(this);
            _container = new Container();
            _container.Contract<IGameCore>()
                .AsSingleton()
                .Concrete(this);
            _container.Contract<IFsm<IGameCore>>()
                .AsSingleton()
                .Concrete(_fsm);
        }

        public void Launch(IFsmState<IGameCore> state = null)
        {
            _fsm.Start(state);
        }

        public IEnumerator InitializeAll()
        {
            List<object> instances = new List<object>();
            foreach (var contraction in _container)
            {
                if (contraction.ContractScope == EContractScope.Singleton)
                {
                    var instance = contraction.Instantiate();
                    instances.Add(instance);
                }
            }
            foreach (var instance in instances)
            {
                try
                {
                    Inject(instance);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e);
                }
            }
            yield return null;

        }

        public void Initialize(IEnumerable<Contraction> contractions)
        {
            List<object> instances = new List<object>();
            foreach (var contraction in contractions)
            {
                if (contraction.ContractScope == EContractScope.Singleton)
                {
                    var instance = contraction.Instantiate();
                    instances.Add(instance);
                }
            }
            foreach (var instance in instances)
            {
                try
                {
                    Inject(instance);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e);
                }
            }
        }

        public void Inject(object target)
        {
            var type = target.GetType();
            type.Members(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).
                WhereAttr<InjectAttribute>(true).
                IfFieldThen(fieldInfo =>
                {
                    var contraction = _container.FindContract(fieldInfo.FieldType);
                    if (contraction == null)
                    {
                        UnityEngine.Debug.LogError(fieldInfo.FieldType);
                    }
                    fieldInfo.SetValue(target, contraction.Instantiate());
                }).
                IfPropertyThen(propInfo =>
                {
                    var contraction = _container.FindContract(propInfo.PropertyType);
                    if (contraction == null)
                    {
                        UnityEngine.Debug.LogError(propInfo.PropertyType);
                    }
                    propInfo.SetValue(target, contraction.Instantiate());
                }).
                IfMethodThen(methodInfo =>
                {
                    var ps = methodInfo.GetParameters();
                    var args = new object[ps.Length];
                    int i = 0;
                    foreach (var p in ps)
                    {
                        var contraction = _container.FindContract(p.ParameterType);
                        if (contraction == null)
                        {
                            UnityEngine.Debug.LogError(p.ParameterType.Name);
                        }
                        args[i++] = contraction.Instantiate();
                    }
                    methodInfo.Invoke(target, args);
                });
            (target as IEventInject)?.OnInject();
        }

        public Contraction Add(Type contractType, object identity = null)
        {
            return _container.Contract(contractType, identity);
        }

        public void Remove(Type contractType, object identity = null)
        {
            _container.BreakContract(contractType, identity);
        }

        public object Get(Type contractType, object identity = null)
        {
            Contraction contraction = _container.FindContract(contractType, identity);
            if (contraction == null)
            {
                throw new InvalidOperationException($"Contract[{contractType.Name}] not found.");
            }
            object concrete;
            bool isDirty = contraction.IsDirty();
            contraction.ResetDirty();
            switch (contraction.ContractScope)
            {
                case EContractScope.Singleton:
                    concrete = contraction.Instantiate();
                    break;
                case EContractScope.Scoped:
                    concrete = contraction.Instantiate();
                    break;
                case EContractScope.Transient:
                    concrete = contraction.Instantiate();
                    break;
                default:
                    throw new InvalidOperationException("Unexpected scope.");
            }
            if (isDirty)
                Inject(concrete);
            return concrete;
        }


        public void Destroy(Type contractType, object identity = null)
        {
            _container.DestroyConcrete(contractType, identity);
        }

    }

    public static class GameCoreExtention
    {
        public static Contraction Add<T>(this IGameCore core, object identity = null) where T : class
        {
            return core.Add(typeof(T), identity);
        }

        public static bool TryAdd(this IGameCore core, Type contractType, out Contraction contraction, object identity = null)
        {
            try
            {
                contraction = core.Add(contractType, identity);
            }
            catch
            {
                contraction = null;
                return false;
            }
            return true;
        }

        public static bool TryAdd<T>(this IGameCore core, out Contraction contraction, object identity = null) where T : class
        {
            return core.TryAdd(typeof(T), out contraction, identity);
        }

        public static void Remove<T>(this IGameCore core, object identity = null)
        {
            core.Remove(typeof(T), identity);
        }

        public static T Get<T>(this IGameCore core, object identity = null) where T : class
        {
            return (T)core.Get(typeof(T), identity);
        }

        public static bool TryGet(this IGameCore core, Type contractType, out object concrete, object identity = null)
        {
            try
            {
                concrete = core.Get(contractType, identity);
                return true;
            }
            catch
            {
                concrete = null;
                return false;
            }
        }

        public static bool TryGet<T>(this IGameCore core, out T concrete, object identity = null) where T : class
        {
            bool result = core.TryGet(typeof(T), out object _concrete, identity);
            if (result)
                concrete = (T)_concrete;
            else
                concrete = default(T);
            return result;
        }

        public static void Destroy<T>(this IGameCore core, object identity = null)
        {
            core.Destroy(typeof(T), identity);
        }
    }

    public class ProgressSubject<T> : IObservable<T>, IProgress<T>
    {
        List<IObserver<T>> _observers;

        public ProgressSubject()
        {
            _observers = new List<IObserver<T>>();
        }

        public void Report(T value)
        {
            foreach (var observer in _observers)
            {
                try
                {
                    observer.OnNext(value);
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            }
            foreach (var observer in _observers)
            {
                try
                {
                    observer.OnCompleted();
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return null;
        }
    }

    public interface IFsm<T>
    {
        T Target { get; }

        IFsmState<T> Current { get; }

        void SetState(IFsmState<T> concreteState);

        void OnError(Exception ex);

        bool IsState<TState>(bool inherit = false) where TState : IFsmState<T>;
    }

    public static class FsmExtension
    {
        public static void SetState<T, TState>(this IFsm<T> fsm) where TState : IFsmState<T>
        {
            var state = Activator.CreateInstance<TState>();
            fsm.SetState(state);
        }

        public static void SetState<T>(this IFsm<T> fsm, Type stateType)
        {
            var state = Activator.CreateInstance(stateType);
            fsm.SetState((IFsmState<T>)state);
        }
    }

    public interface IFsmState<T>
    {
        void OnEnter(IFsm<T> fsm);

        void OnUpdate(IFsm<T> fsm);

        void OnExit(IFsm<T> fms);
    }

    public class GameCoreFsm : IFsm<IGameCore>
    {
        #region -- Errors --
        public static readonly Exception ErrArgNull = new ArgumentNullException();
        #endregion

        private IGameCore _core;
        private IFsmState<IGameCore> _currentState;

        public IGameCore Target => _core;

        public IFsmState<IGameCore> Current => _currentState;

        public GameCoreFsm(IGameCore core)
        {
            _core = core;
            _currentState = new RootState<IGameCore>();
        }

        public void Start(IFsmState<IGameCore> defaultState = null)
        {
            if (defaultState == null)
            {
                defaultState = new DefaultState();
            }
            InitializeState initState = new InitializeState(defaultState);
            SetState(initState);
        }

        public void OnError(Exception ex)
        {
            UnityEngine.Debug.LogError(ex);
        }

        public void SetState<TState>()
            where TState : IFsmState<IGameCore>
        {
            SetState(typeof(TState));
        }

        public void SetState(Type stateType)
        {
            if (stateType == null)
                throw ErrArgNull;
            var state = (IFsmState<IGameCore>)Activator.CreateInstance(stateType);
            try
            {
                if (_currentState != null)
                    _currentState.OnExit(this);
            }
            catch (Exception e)
            {
                OnError(e);
            }
            _currentState = state;
            try
            {
                state.OnEnter(this);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public void SetState(IFsmState<IGameCore> concreteState)
        {
            if (concreteState == null)
                throw ErrArgNull;
            try
            {
                if (_currentState != null)
                    _currentState.OnExit(this);
            }
            catch (Exception e)
            {
                OnError(e);
            }
            _currentState = concreteState;
            try
            {
                concreteState.OnEnter(this);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public bool IsState<TState>(bool inherit = false) where TState : IFsmState<IGameCore>
        {
            Type actual = _currentState.GetType();
            Type expected = typeof(TState);
            return actual == expected
                || actual.IsSubclassOf(expected);
        }
    }

    public abstract class FsmStateBase<T> : IFsmState<T>
    {
        public virtual void OnEnter(IFsm<T> fsm) { }

        public virtual void OnExit(IFsm<T> fms) { }

        public virtual void OnUpdate(IFsm<T> fsm) { }
    }

    public sealed class RootState<T> : FsmStateBase<T> { }


    public class InitializeState : FsmStateBase<IGameCore>
    {
        IFsmState<IGameCore> _next;

        public InitializeState(IFsmState<IGameCore> next)
        {
            _next = next;
        }

        public override void OnEnter(IFsm<IGameCore> fsm)
        {
            MainThreadDispatcher.SendStartCoroutine(
                fsm.Target.InitializeAll(),
                () => OnCompletedInitialize(fsm),
                fsm.OnError);
        }

        private void OnCompletedInitialize(IFsm<IGameCore> fsm)
        {
            // next state
            fsm.SetState(_next);
        }

    }

    public class DefaultState : FsmStateBase<IGameCore>
    {
        public override void OnEnter(IFsm<IGameCore> fsm)
        {
            UnityEngine.Debug.Log("Default state!");
        }
    }

    public interface IEventInject
    {
        void OnInject();
    }
}