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
        protected bool IsInit { get; private set; }

        protected void Awake()
        {
            RefreshParent();
        }

        protected void ApplyView()
        {
            RefreshParent();
            // initialize added child
            _childList = _childList.Union(_addTemp).ToList();
            if (IsInit)
            {
                foreach (var add in _addTemp)
                {
                    var view = add as UniView;
                    if (view == null)
                        continue;
                    if (!view.IsInit)
                        view.InitializeView();
                    view.ApplyView();
                }
            }
            _addTemp.Clear();
            // refresh removed children
            foreach (var rm in _removeTemp)
            {
                (rm as UniView)?.ApplyView();
            }
            _removeTemp.Clear();
        }

        #region -- IGameView --
        public ILogger Logger { get; set; }

        public IGameModuleCollection Modules { get; set; }

        public void InitializeView()
        {
            _childList = new List<INode>();
            _addTemp = new List<INode>();
            _removeTemp = new List<INode>();
            RefreshParent();
            try
            {
                OnBeginInitializeView();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
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

            IsInit = true;
        }



        /// <summary>
        /// Custom initialize
        /// </summary>
        protected virtual void OnBeginInitializeView() { }

        protected virtual void OnEndInitializeView() { }
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
            return _childList.IndexOf(node);
        }


        /// <summary>
        /// Add node under this node
        /// </summary>
        /// <exception cref="InvalidOperationException">The node wanted to add must be not making loop.</exception>
        public void AddNode(INode node)
        {
            // check if the node is not the parent
            if (this.Under(node))
                throw new InvalidOperationException("The node wanted to add must be not making loop.");
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

        private void RefreshParent()
        {
            Transform parent = transform.parent;
            if (parent == null)
                return;
            // register self under the parent
            INode parentView = parent.GetComponent<INode>();
            if (parentView != null)
            {
                parentView.AddNode(this);
            }
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
        private void AssignRequiredModules(object instance)
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