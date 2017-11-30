namespace GameDraw
{
    using System;
    using UnityEngine;

    [Serializable]
    public class VertexTrait
    {
        public BoneWeight boneWeight;
        public Color color;
        public int hashCode;
        public int ID;
        public object locker;
        public Vector3 Normal;
        public Vector3 pos;
        public float selectionWeight;
        public Vector4 tangent;
        public Vector2 uv;
        public Vector2 uv1;
        public Vector2 uv2;

        public VertexTrait()
        {
            this.locker = new object();
            this.selectionWeight = 1f;
            this.ID = 0;
            this.Normal = Vector3.forward;
            this.uv = Vector2.zero;
            this.uv1 = Vector2.zero;
            this.uv2 = Vector2.zero;
            this.color = Color.white;
            this.tangent = Vector4.one;
            this.boneWeight = new BoneWeight();
            this.position = Vector3.zero;
            this.hashCode = 0;
        }

        public VertexTrait(int id)
        {
            this.locker = new object();
            this.selectionWeight = 1f;
            this.ID = id;
            this.Normal = Vector3.forward;
            this.uv = Vector2.zero;
            this.uv1 = Vector2.zero;
            this.uv2 = Vector2.zero;
            this.color = Color.white;
            this.tangent = Vector4.one;
            this.boneWeight = new BoneWeight();
            this.position = Vector3.zero;
            this.hashCode = 0;
        }

        public VertexTrait(int id, Vector3 pos, Vector3 Normal, Vector2 uv, Vector2 uv1, Vector2 uv2, Color c, Vector4 t, BoneWeight weight)
        {
            this.locker = new object();
            this.selectionWeight = 1f;
            this.ID = id;
            this.Normal = Normal;
            this.uv = uv;
            this.uv1 = uv1;
            this.uv2 = uv2;
            this.color = c;
            this.tangent = t;
            this.boneWeight = weight;
            this.position = pos;
            this.hashCode = 0;
        }

        public override string ToString()
        {
            return this.ID.ToString();
        }

        public Vector3 position
        {
            get
            {
                return this.pos;
            }
            set
            {
                lock (this.locker)
                {
                    this.pos = value;
                }
            }
        }
    }
}

