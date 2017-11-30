namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    [Serializable]
    public class State
    {
        public string Description;
        public string Name;
        public float PartStateValue;
        public List<string> Tags;
        public List<Vector3> VERTEX_VALUES;

        public State(List<Vector3> VERTEX_VALUES)
        {
            this.VERTEX_VALUES = VERTEX_VALUES;
            this.Tags = new List<string>();
        }

        public State(List<Vector3> VERTEX_VALUES, params string[] Tag)
        {
            this.VERTEX_VALUES = VERTEX_VALUES;
            this.Tags = new List<string>();
            foreach (string str in Tag)
            {
                this.Tags.Add(str);
            }
        }

        public List<Vector3> VertexValues
        {
            get
            {
                return this.VERTEX_VALUES;
            }
            set
            {
                this.VERTEX_VALUES = value;
            }
        }
    }
}

