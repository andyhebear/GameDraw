namespace GameDraw
{
    using System;
    using System.Collections.Generic;

    public class GDDictionary<K, V> : UnityDictionary<K, V>
    {
        protected override void SetKeyValuePair(K k, V v)
        {
            this.KeyValuePairs.Add(new UnityKeyValuePair<K, V>(k, v));
        }

        protected override List<UnityKeyValuePair<K, V>> KeyValuePairs
        {
            get
            {
                List<UnityKeyValuePair<K, V>> list = new List<UnityKeyValuePair<K, V>>();
                foreach (K local in base.Keys)
                {
                    list.Add(new UnityKeyValuePair<K, V>(local, this[local]));
                }
                return list;
            }
            set
            {
                foreach (UnityKeyValuePair<K, V> pair in value)
                {
                    base.Add(pair.Key, pair.Value);
                }
            }
        }
    }
}

