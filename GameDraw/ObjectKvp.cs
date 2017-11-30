namespace GameDraw
{
    using System;
    using UnityEngine;

    [Serializable]
    public sealed class ObjectKvp : UnityNameValuePair<UnityEngine.Object>
    {
        public UnityEngine.Object value;

        public ObjectKvp(string key, UnityEngine.Object value) : base(key, value)
        {
        }

        public override UnityEngine.Object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

