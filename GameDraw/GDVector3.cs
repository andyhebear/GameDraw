namespace GameDraw
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public class GDVector3
    {
        //[CompilerGenerated]
        //private float <x>k__BackingField;
        //[CompilerGenerated]
        //private float <y>k__BackingField;
        //[CompilerGenerated]
        //private float <z>k__BackingField;

        public GDVector3()
        {
            this.x = 0f;
            this.y = 0f;
            this.z = 0f;
        }

        public GDVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static GDVector3 operator +(GDVector3 v, GDVector3 v1)
        {
            return new GDVector3(v.x + v1.x, v.y + v1.y, v.z + v1.z);
        }

        public static GDVector3 operator +(GDVector3 v, Vector3 v1)
        {
            return new GDVector3(v.x + v1.x, v.y + v1.y, v.z + v1.z);
        }

        public static GDVector3 operator +(Vector3 v, GDVector3 v1)
        {
            return new GDVector3(v.x + v1.x, v.y + v1.y, v.z + v1.z);
        }

        public static GDVector3 operator /(GDVector3 v, int val)
        {
            return new GDVector3(v.x / ((float) val), v.y / ((float) val), v.z / ((float) val));
        }

        public static implicit operator Vector3(GDVector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator GDVector3(Vector3 v)
        {
            return new GDVector3(v.x, v.y, v.z);
        }

        public static GDVector3 operator -(GDVector3 v, GDVector3 v1)
        {
            return new GDVector3(v.x - v1.x, v.y - v1.y, v.z - v1.z);
        }

        public static GDVector3 operator -(GDVector3 v, Vector3 v1)
        {
            return new GDVector3(v.x - v1.x, v.y - v1.y, v.z - v1.z);
        }

        public static GDVector3 operator -(Vector3 v, GDVector3 v1)
        {
            return new GDVector3(v.x - v1.x, v.y - v1.y, v.z - v1.z);
        }

        public GDVector3 normalized
        {
            get
            {
                Vector3 vector = new Vector3(this.x, this.y, this.z);
                return vector.normalized;
            }
        }

        public float x
        {
           get;set;
        }

        public float y
        {
           get;set;
        }

        public float z
        {
           get;set;
        }
    }
}

