namespace GameDraw
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    [Serializable]
    public class RelatedBuffer
    {
        public List<IndexBuffer> buffer = new List<IndexBuffer>();
        public List<int> hashes = new List<int>();

        public RelatedBuffer()
        {
            this.hashes = new List<int>();
            this.buffer = new List<IndexBuffer>();
        }

        public void Add(int hash, IndexBuffer val)
        {
            this.hashes.Add(hash);
            this.buffer.Add(val);
        }

        public bool Contains(int val)
        {
            return this.hashes.Contains(val);
        }

        public IEnumerator GetEnumerator()
        {
            return this.hashes.GetEnumerator();
        }

        public int this[int i]
        {
            get
            {
                return this.hashes[i];
            }
            set
            {
                this.hashes[i] = value;
            }
        }
    }
}

