// Inspiration -> https://www.dotnetperls.com/multimap
namespace System.Collections.Generic
{
    public sealed class MultiMap<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    {
        private readonly SortedDictionary<T1, List<T2>> _dictionary =
            new SortedDictionary<T1, List<T2>>();

        public void Add(T1 key, T2 value)
        {
            if (this._dictionary.TryGetValue(key, out var list))
            {
                list.Add(value);
            }
            else
            {
                list = new List<T2> {value};
                _dictionary[key] = list;
            }
        }

        public void Remove(T1 key)
        {
            _dictionary.Remove(key);
        }

        public void Erase(T2 item)
        {
            var removed = false;
            foreach (var kvp in _dictionary)
            {
                foreach (var innerItem in kvp.Value)
                {
                    if(removed) return;
                    if (Equals(item, innerItem))
                    {
                        kvp.Value.Remove(innerItem);
                        removed = true;
                    }
                }
            }
        }

        public IEnumerable<T1> Keys => _dictionary.Keys;

        public List<T2> this[T1 key]
        {
            get
            {
                if (!_dictionary.TryGetValue(key, out var list))
                {
                    list = new List<T2>();
                    _dictionary[key] = list;
                }
                return list;
            }
        }
        
        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            foreach (var kvp in _dictionary)
            {
                foreach (var val in kvp.Value)
                {
                    yield return new KeyValuePair<T1, T2>(kvp.Key, val);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}