namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class ObjectDictionary : UnityDictionary<UnityEngine.Object>
    {
        public List<ObjectKvp> values;

        protected override void SetKeyValuePair(string k, UnityEngine.Object v)
        {
            int index = this.values.FindIndex(delegate (ObjectKvp x) {
                return x.Key == k;
            });
            if (index != -1)
            {
                if (v == null)
                {
                    this.values.RemoveAt(index);
                }
                else
                {
                    this.values[index] = new ObjectKvp(k, v);
                }
            }
            else
            {
                this.values.Add(new ObjectKvp(k, v));
            }
        }

        protected override List<UnityKeyValuePair<string, UnityEngine.Object>> KeyValuePairs
        {
            get
            {
                return this.values.ConvertAll<UnityKeyValuePair<string, UnityEngine.Object>>(delegate (ObjectKvp x) {
                    return x;
                });
            }
            set
            {
                if (value == null)
                {
                    this.values = new List<ObjectKvp>();
                }
                else
                {
                    this.values = value.ConvertAll<ObjectKvp>(delegate (UnityKeyValuePair<string, UnityEngine.Object> x) {
                        return new ObjectKvp(x.Key, x.Value);
                    });
                }
            }
        }
    }
}

