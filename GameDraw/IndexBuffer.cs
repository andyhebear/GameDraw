namespace GameDraw
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    [Serializable]
    public class IndexBuffer
    {
        public List<int> buffer;
        public GDMesh.Vertex vert;

        public IndexBuffer(GDMesh.Vertex v)
        {
            this.buffer = new List<int>();
            this.vert = v;
            this.buffer = new List<int>();
        }

        public IndexBuffer(GDMesh.Vertex v, List<int> indices)
        {
            this.buffer = new List<int>();
            this.vert = v;
            this.buffer = indices;
        }

        public void Add(int val)
        {
            this.buffer.Add(val);
        }

        public bool Contains(int val)
        {
            return this.buffer.Contains(val);
        }

        public IEnumerator GetEnumerator()
        {
            return this.buffer.GetEnumerator();
        }

        public int this[int i]
        {
            get
            {
                return this.buffer[i];
            }
            set
            {
                this.buffer[i] = value;
            }
        }
    }
}

