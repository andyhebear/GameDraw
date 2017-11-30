namespace GameDraw
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public abstract class UnityDictionary<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable
    {
        protected UnityDictionary()
        {
        }

        public void Add(KeyValuePair<K, V> kvp)
        {
            this[kvp.Key] = kvp.Value;
        }

        public void Add(K key, V value)
        {
            this[key] = value;
        }

        public void Clear()
        {
            List<UnityKeyValuePair<K, V>> keyValuePairs = this.KeyValuePairs;
            keyValuePairs.Clear();
            this.KeyValuePairs = keyValuePairs;
        }

        public bool Contains(KeyValuePair<K, V> kvp)
        {
            V local = this[kvp.Key];
            return local.Equals(kvp.Value);
        }

        public bool ContainsKey(K key)
        {
            return (this.KeyValuePairs.FindIndex(delegate (UnityKeyValuePair<K, V> x) {
                return x.Key.Equals(key);
            }) != -1);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int index)
        {
            this.KeyValuePairs.ConvertAll<KeyValuePair<K, V>>(delegate (UnityKeyValuePair<K, V> x) {
                return new KeyValuePair<K, V>(x.Key, x.Value);
            }).CopyTo(array, index);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return this.GetEnumerator();//new UnityDictionaryEnumerator<K, V>((UnityDictionary<K, V>) this);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            return this.Remove(item.Key);
        }

        public bool Remove(K key)
        {
            List<UnityKeyValuePair<K, V>> keyValuePairs = this.KeyValuePairs;
            int index = keyValuePairs.FindIndex(delegate (UnityKeyValuePair<K, V> x) {
                return x.Key.Equals(key);
            });
            if (index == -1)
            {
                return false;
            }
            keyValuePairs.RemoveAt(index);
            this.KeyValuePairs = keyValuePairs;
            return true;
        }

        protected abstract void SetKeyValuePair(K k, V v);
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(K key, out V value)
        {
            if (!this.ContainsKey(key))
            {
                value = default(V);
                return false;
            }
            value = this[key];
            return true;
        }

        public int Count
        {
            get
            {
                return this.KeyValuePairs.Count;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual V this[K key]
        {
            get
            {
                UnityKeyValuePair<K, V> pair = this.KeyValuePairs.Find(delegate (UnityKeyValuePair<K, V> x) {
                    return x.Key.Equals(key);
                });
                if (pair == null)
                {
                    return default(V);
                }
                return pair.Value;
            }
            set
            {
                if (key != null)
                {
                    this.SetKeyValuePair(key, value);
                }
            }
        }

        public ICollection<KeyValuePair<K, V>> Items
        {
            get
            {
                return this.KeyValuePairs.ConvertAll<KeyValuePair<K, V>>(delegate (UnityKeyValuePair<K, V> x) {
                    return new KeyValuePair<K, V>(x.Key, x.Value);
                });
            }
        }

        public ICollection<K> Keys
        {
            get
            {
                return this.KeyValuePairs.ConvertAll<K>(delegate (UnityKeyValuePair<K, V> x) {
                    return x.Key;
                });
            }
        }

        protected abstract List<UnityKeyValuePair<K, V>> KeyValuePairs { get; set; }

        public V SyncRoot
        {
            get
            {
                return default(V);
            }
        }

        public ICollection<V> Values
        {
            get
            {
                return (ICollection<V>) this.KeyValuePairs.ConvertAll<V>(delegate (UnityKeyValuePair<K, V> x) {
                    return x.Value;
                });
            }
        }

        internal sealed class UnityDictionaryEnumerator : IEnumerator<KeyValuePair<K, V>>, IDisposable, IEnumerator
        {
            private int index;
            private KeyValuePair<K, V>[] items;

            internal UnityDictionaryEnumerator()
            {
                this.index = -1;
            }

            internal UnityDictionaryEnumerator(UnityDictionary<K, V> ud)
            {
                this.index = -1;
                this.items = new KeyValuePair<K, V>[ud.Count];
                ud.CopyTo(this.items, 0);
            }

            public void Dispose()
            {
                this.index = -1;
                this.items = null;
            }

            public bool MoveNext()
            {
                if (this.index < (this.items.Length - 1))
                {
                    this.index++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                this.index = -1;
            }

            private void ValidateIndex()
            {
                if ((this.index < 0) || (this.index >= this.items.Length))
                {
                    throw new InvalidOperationException("Enumerator is before or after the collection.");
                }
            }

            public KeyValuePair<K, V> Current
            {
                get
                {
                    this.ValidateIndex();
                    return this.items[this.index];
                }
            }

            public KeyValuePair<K, V> Entry
            {
                get
                {
                    return this.Current;
                }
            }

            public K Key
            {
                get
                {
                    this.ValidateIndex();
                    return this.items[this.index].Key;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public V Value
            {
                get
                {
                    this.ValidateIndex();
                    return this.items[this.index].Value;
                }
            }
        }
    }
}

