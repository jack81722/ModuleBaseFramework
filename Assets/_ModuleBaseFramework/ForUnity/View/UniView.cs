using ModuleBased.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModuleBased.ForUnity
{
    public class UniView : MonoBehaviour, IGameView, INode
    {
        #region -- Exceptions --
        protected static readonly Exception ErrNotInit = new InvalidOperationException("The view is not initialized.");
        protected static readonly Exception ErrNodeNull = new ArgumentNullException("The node is null.");
        protected static readonly Exception ErrLoopView = new InvalidOperationException("The node wanted to add must be not making loop.");
        #endregion

        protected bool IsInit { get; private set; }

        protected void Awake()
        {
            ReTransform();
        }

        protected void OnDestroy()
        {
            try
            {
                OnDestroyView();
            }
            catch (Exception e)
            {
                Logger.LogWarning(e.Message);
            }
            finally
            {
                if (Parent != null)
                    Parent.RemoveNode(this);
            }
        }

        public void ApplyView()
        {
            ReTransform();
            // initialize added child
            _childList = _childList.Union(_addTemp).ToList();
            if (IsInit)
            {
                foreach (var node in _addTemp)
                {
                    var view = node as IGameView;
                    if (view == null)
                        continue;
                    if (!node.Under(this))
                        continue;
                    view.InitializeView();
                }
            }
            _addTemp.Clear();
            // refresh removed children
            foreach (var node in _removeTemp)
            {
                (node as IGameView)?.ApplyView();
            }
            _removeTemp.Clear();
        }

        #region -- IGameView --
        public ILogger Logger { get; set; }

        public IGameModuleCollection Modules { get; set; }

        public void InitializeView()
        {
            if (IsInit)
                return;
            _childList = new List<INode>();
            _addTemp = new List<INode>();
            _removeTemp = new List<INode>();
            IsInit = true;
            AssignRequiredModules(this);
            try
            {
                OnBeginInitializeView();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            ApplyView();
            foreach (var child in _childList)
            {
                var childView = child as IGameView;
                childView.Logger = Logger;
                childView.Modules = Modules;
                childView.InitializeView();
            }
            try
            {
                OnEndInitializeView();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }

        }

        /// <summary>
        /// Begin phase of initialization
        /// </summary>
        /// <remarks>
        /// May instantiate or add children view in this phase.
        /// </remarks>
        protected virtual void OnBeginInitializeView() { }

        /// <summary>
        /// End phase of initialization
        /// </summary>
        /// <remarks>
        /// The view almost complete the initialization, may get access of parent or children
        /// </remarks>
        protected virtual void OnEndInitializeView() { }

        /// <summary>
        /// Destroy view
        /// </summary>
        protected virtual void OnDestroyView() { }
        #endregion

        #region -- INode --
        private INode _parent;
        public INode Parent {
            get => _parent;
            set
            {
                _parent = value;
                var mono = _parent as MonoBehaviour;
                if (mono == null)
                    return;
                transform.SetParent(mono.transform);
            }
        }

        /// <summary>
        /// The original name of view
        /// </summary>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// The name of view in path
        /// </summary>
        public virtual string PathName {
            get
            {
                if (Parent == null)
                    return Name;
                return Name + $"_{Parent.IndexOf(this)}";
            }
        }

        public string NodePath {
            get
            {
                if (Parent == null)
                    return Name;
                return Parent.ToString() + NodeUtils.PathSepChar + PathName;
            }
        }

        public int IndexOf(INode node)
        {
            if (!IsInit)
                throw ErrNotInit;
            return _childList.IndexOf(node);
        }


        /// <summary>
        /// Add node under this node
        /// </summary>
        /// <exception cref="ArgumentNullException">The node is null.</exception>
        /// <exception cref="InvalidOperationException">"The view is not initialized."</exception>
        /// <exception cref="InvalidOperationException">The node wanted to add must be not making loop.</exception>
        public void AddNode(INode node)
        {
            if (node == null)
                throw ErrNodeNull;
            if (!IsInit)
                throw ErrNotInit;
            // check if the node is not the parent
            if (this.Under(node))
                throw ErrLoopView;
            // check if has existed
            if (ReferenceEquals(node.Parent, this))
                return;
            node.Parent = this;
            _addTemp.Add(node);
        }

        public bool RemoveNode(INode node)
        {
            var i = IndexOf(node);
            if (i < 0)
            {
                return false;
            }
            _removeTemp.Add(_childList[i]);
            _childList.RemoveAt(i);
            return true;
        }

        private void ReTransform()
        {
            if (Parent == null)
                return;
            Component comp = Parent as Component;
            if (comp == null)
                return;
            transform.SetParent(comp.transform);
        }
        #endregion

        #region -- Child view methods --
        private List<INode> _childList;
        private List<INode> _addTemp;
        private List<INode> _removeTemp;

        public bool TryGetView<T>(string pathName, out T view) where T : class
        {
            bool result = TryGetChild(pathName, out INode node);
            view = node as T;
            return result;
        }


        public bool TryGetChild(string pathName, out INode node)
        {
            int index = _childList.FindIndex(v =>
            {
                var child = v as UniView;
                if (child == null)
                {
                    return false;
                }
                return child.PathName == pathName;
            });
            if (index < 0)
            {
                node = null;
                return false;
            }
            node = _childList[index];
            return true;
        }


        public bool RemoveChild(string pathName)
        {
            bool result = TryGetChild(pathName, out INode node);
            if (result)
                _childList.Remove(node);
            return result;
        }

        public IEnumerable<INode> GetChildren()
        {
            if (_childList == null)
                return new INode[0];
            return (IReadOnlyList<INode>)_childList;
        }
        #endregion

        #region -- DI methods --
        private void AssignRequiredModules(IGameView instance)
        {
            Type objType = instance.GetType();
            MemberInfo[] members = objType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var member in members)
            {
                if (member.IsDefined(typeof(RequireModuleAttribute)))
                {
                    Type reqType;
                    switch (member.MemberType)
                    {
                        case MemberTypes.Field:

                            FieldInfo field = (FieldInfo)member;
                            reqType = field.FieldType;
                            field.SetValue(instance, Modules.GetModule(reqType));
                            break;
                        case MemberTypes.Property:
                            PropertyInfo prop = (PropertyInfo)member;
                            reqType = prop.PropertyType;
                            prop.SetValue(instance, Modules.GetModule(reqType));
                            break;
                    }
                }
            }
        }
        #endregion

        #region -- Unity UI convert operator --
        /*
         * UniView will convert to unity components automatically
         */

        /// <summary>
        /// Unity ui component cache
        /// </summary>
        private MonoBehaviour _comp;

        public T Convert<T>() where T : MonoBehaviour
        {
            T comp = _comp as T;
            if (comp == null)
                _comp = comp = GetComponent<T>();
            return comp;
        }

        public static explicit operator Button(UniView view)
        {
            Button btn = view._comp as Button;
            if (btn == null)
                view._comp = btn = view.GetComponent<Button>();
            return btn;
        }

        public static explicit operator Image(UniView view)
        {
            Image img = view._comp as Image;
            if (img == null)
                view._comp = img = view.GetComponent<Image>();
            return img;
        }

        public static explicit operator Text(UniView view)
        {
            Text txt = view._comp as Text;
            if (txt == null)
                view._comp = txt = view.GetComponent<Text>();
            return txt;
        }
        #endregion

        public override string ToString()
        {
            return NodePath;
        }
    }


}