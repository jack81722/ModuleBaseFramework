using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased.Rx
{
    public class CompositeDisposable : IDisposable, ICollection<IDisposable>
    {
        private List<IDisposable> _children = new List<IDisposable>();

        public int Count => _children.Count;

        public bool IsReadOnly => false;

        public void Add(IDisposable item)
        {
            _children.Add(item);
        }

        public void Clear()
        {
            _children.Clear();
        }

        public bool Contains(IDisposable item)
        {
            return _children.Contains(item);
        }

        public void CopyTo(IDisposable[] array, int arrayIndex)
        {
            _children.CopyTo(array, arrayIndex);
        }

        public void Dispose()
        {
            foreach(var child in _children)
            {
                child.Dispose();
            }
        }

        public IEnumerator<IDisposable> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        public bool Remove(IDisposable item)
        {
            return _children.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }
    }
}
