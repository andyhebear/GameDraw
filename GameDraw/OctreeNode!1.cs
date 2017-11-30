namespace GameDraw
{
    using System;

    public class OctreeNode<T>
    {
        public OctreeNode<T>[] children;
        public T value;

        public OctreeNode()
        {
            this.children = null;
        }
    }
}

