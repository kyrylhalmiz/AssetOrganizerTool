using System;

namespace Editor.AssetsOrganizer.ViewModel.Observables
{
    public class Observable<T>
    {
        private T _value;

        public event Action<T> OnChanged;

        public Observable(T initialValue = default)
        {
            _value = initialValue;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    OnChanged?.Invoke(_value);
                }
            }
        }

        public void ForceNotify()
        {
            OnChanged?.Invoke(_value);
        }
    }
}