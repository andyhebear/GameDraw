namespace GameDraw
{
    using System;
    using System.Runtime.CompilerServices;

    public class UnityKeyValuePair<K, V>
    {
        //[CompilerGenerated]
        //private K <Key>k__BackingField;
        //[CompilerGenerated]
        //private V <Value>k__BackingField;

        public UnityKeyValuePair()
        {
            this.Key = default(K);
            this.Value = default(V);
        }

        public UnityKeyValuePair(K key, V value)
        {
            this.Key = key;
            this.Value = value;
        }

        public virtual K Key {
            get;
            set;
        }

        public virtual V Value {
            get;
            set;
        }
    }
}

