using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased
{
    public class DefaultGameViewCollection : IGameViewCollection
    {
        private Dictionary<Type, IGameView> _views;

        public DefaultGameViewCollection()
        {
            _views = new Dictionary<Type, IGameView>();
        }

        public void AddView(IGameView view)
        {
            Type type = view.GetType();
            if (!_views.ContainsKey(type))
                _views.Add(type, view);
        }

        public IEnumerator<IGameView> GetEnumerator()
        {
            return _views.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _views.Values.GetEnumerator();
        }
    }
}