// Inspiration -> https://www.dotnetperls.com/multimap
using System.Collections.Immutable;
using System.Linq;

namespace System.Collections.Generic
{
    public sealed class MultiMap<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    {
        private readonly SortedDictionary<T1, ImmutableList<T2>> _dictionary =
            new SortedDictionary<T1, ImmutableList<T2>>();

        public void Add(T1 key, T2 value)
        {
            if (_dictionary.TryGetValue(key, out var list))
            {
                _dictionary[key] = list.ToImmutableList().Add(value);
            }
            else
            {
                _dictionary[key] = ImmutableList<T2>.Empty.Add(value);
            }
        }

        public void Remove(T1 key)
        {
            _dictionary.Remove(key);
        }

        public void Erase(T2 item)
        {
            T1 keyWhereToRemove = default(T1);
            var foundIt = false;
            foreach (var kvp in _dictionary.ToArray())
            {
                if (!foundIt)
                {
                    foreach (var innerItem in kvp.Value)
                    {
                        if (Equals(item, innerItem))
                        {
                            keyWhereToRemove = kvp.Key;
                            foundIt = true;
                            break;
                        }
                    }
                }
            }

            if (foundIt)
            {
                _dictionary[keyWhereToRemove] = _dictionary[keyWhereToRemove].Remove(item);
            }
        }

        public IEnumerable<T1> Keys => _dictionary.Keys;

        public ImmutableList<T2> this[T1 key]
        {
            get
            {
                if (!_dictionary.TryGetValue(key, out var list))
                {
                    list = ImmutableList<T2>.Empty;
                    _dictionary[key] = list;
                }
                return list;
            }
            set
            {
                foreach (var val in value)
                {
                    Add(key, val);
                }
            }
        }
        
        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            foreach (var kvp in _dictionary.ToList())
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