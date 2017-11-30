namespace GameDraw
{
    using System;
    using UnityEngine;

    public class Octree<T>
    {
        public Vector3 center;
        public int maxLevel;
        public OctreeNode<T> root;
        public Vector3 size;

        public Octree()
        {
            this.maxLevel = 5;
            this.size = Vector3.one;
            this.root = new OctreeNode<T>();
            this.center = Vector3.zero;
        }

        public Octree(Vector3 size)
        {
            this.maxLevel = 5;
            this.size = Vector3.one;
            this.root = new OctreeNode<T>();
            this.size = size;
        }

        public Octree(Vector3 center, Vector3 size)
        {
            this.maxLevel = 5;
            this.size = Vector3.one;
            this.root = new OctreeNode<T>();
            this.center = center;
            this.size = size;
        }

        public OctreeNode<T> Add(Vector3 p, T val)
        {
            OctreeNode<T> root = this.root;
            int num = 0;
            int index = 0;
            do
            {
                if (root.children == null)
                {
                    root.children = new OctreeNode<T>[8];
                }
                if (p.x > this.center.x)
                {
                    index |= 1;
                }
                if (p.y > this.center.y)
                {
                    index |= 2;
                }
                if (p.z > this.center.z)
                {
                    index |= 4;
                }
                num++;
                if (root.children[index] == null)
                {
                    root.children[index] = new OctreeNode<T>();
                    if (num == this.maxLevel)
                    {
                        root.children[index].value = val;
                        return root.children[index];
                    }
                }
                root = root.children[index];
            }
            while (num < this.maxLevel);
            return null;
        }

        public OctreeNode<T> FindLeaf(Vector3 p)
        {
            bool flag = false;
            OctreeNode<T> root = this.root;
            do
            {
                if (root.children == null)
                {
                    flag = true;
                    return root;
                }
                int index = 0;
                if (p.x > this.center.x)
                {
                    index |= 1;
                }
                if (p.y > this.center.y)
                {
                    index |= 2;
                }
                if (p.z > this.center.z)
                {
                    index |= 4;
                }
                root = root.children[index];
            }
            while (!flag);
            return root;
        }
    }
}

