using ModuleBased.Node;
using System;
using System.Collections;
using System.Collections.Generic;
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
            Transform parent = transform.parent;
            if (parent == null)
                return;
            // register self under the parent
            UniView parentView = parent.GetComponent<UniView>();
            if (parentView != null)
            {
                parentView.AddNode(this);
            }
        }

        #region -- IGameView --
        public ILogger Logger { get; set; }

        public IGameModuleCollection Modules { get; set; }

        public void InitializeView()
        {
            foreach(var child in childList)
            {
                var childView = child as UniView;
                childView.InitializeView();
            }
            try
            {
                OnInitializeView();
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
        protected virtual void OnInitializeView() { }
        #endregion

        #region -- INode --
        private INode _parent;
        public INode Parent
        {
            get => _parent; set
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
        public virtual string PathName
        {
            get
            {
                if (Parent == null)
                    return Name;
                return Name + $"_{Parent.IndexOf(this)}";
            }
        }

        public string NodePath
        {
            get
            {
                if (Parent == null)
                    return Name;
                return Parent.NodePath + NodeUtils.PathSepChar + PathName;
            }
        }

        public int IndexOf(INode node)
        {
            int at = childList.IndexOf(node);
            if (at < 0)
                throw new InvalidOperationException("The view is not in collection.");
            // if name of node is same, index will plus
            // such as (image_0, image_1, ...)
            int index = 0;
            for (int i = 0; i < at; i++)
            {
                if (childList[i].Name == node.Name)
                    index++;
            }

            return index;
        }

        public bool TryGetNode(string pathName, out INode node)
        {
            int index = childList.FindIndex(v => v.PathName == pathName);
            if (index < 0)
            {
                node = null;
                return false;
            }
            node = childList[index];
            return true;
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
            childList.Add(node);

            // try initialize
            IGameView view = node as IGameView;
            if (view != null)
            {
                view.Logger = Logger;
                AssignRequiredModules(view);
                if (IsInit)
                {
                    view.InitializeView();
                }
            }
        }

        public bool RemoveNode(string pathName)
        {
            bool result = TryGetNode(pathName, out INode node);
            if (result)
                childList.Remove(node);
            return result;
        }

        public bool RemoveNode(INode node)
        {
            return RemoveNode(node.PathName);
        }
        #endregion

        #region -- Child view methods --
        private List<INode> _childList;
        private List<INode> childList
        {
            get
            {
                if (_childList == null)
                {
                    _childList = new List<INode>();
                }
                return _childList;
            }
        }

        /// <summary>
        /// Get view under this view with specific type and path
        /// </summary>
        public T GetView<T>(string path) where T : MonoBehaviour
        {
            INode node = this.GetNode(path);
            UniView view = node as UniView;
            if (view == null)
                return null;
            return view.Convert<T>();
        }

        public bool TryGetView<T>(string pathName, out T view) where T : class
        {
            bool result = TryGetNode(pathName, out INode node);
            view = node as T;
            return result;
        }

        public IEnumerable<INode> GetChildren()
        {
            if (childList == null)
                return new INode[0];
            return (IReadOnlyList<INode>)childList;
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
                    if (member.MemberType == MemberTypes.Field)
                    {
                        FieldInfo field = (FieldInfo)member;
                        Type reqType = field.FieldType;
                        field.SetValue(instance, Modules.GetModule(reqType));
                    }
                    if (member.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo prop = (PropertyInfo)member;
                        Type reqType = prop.PropertyType;
                        prop.SetValue(instance, Modules.GetModule(reqType));
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
    }
}