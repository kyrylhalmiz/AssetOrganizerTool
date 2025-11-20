using System;
using System.Collections.Generic;

namespace Editor.AssetsOrganizer.ViewModel.Observables
{
    public class ObservableList<T>
    {
        private readonly List<T> _internalList = new();

        public event Action OnListChanged;

        public List<T> Items => _internalList;

        public void Set(IEnumerable<T> items)
        {
            _internalList.Clear();
            _internalList.AddRange(items);
            OnListChanged?.Invoke();
        }

        public void Add(T item)
        {
            _internalList.Add(item);
            OnListChanged?.Invoke();
        }

        public void Clear()
        {
            _internalList.Clear();
            OnListChanged?.Invoke();
        }
    }
}