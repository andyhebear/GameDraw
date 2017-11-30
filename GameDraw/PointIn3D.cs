namespace GameDraw
{
    using System;
    using UnityEngine;

    public class PointIn3D
    {
        public Vector3 normal;
        public Plane plane = new Plane();
        public Vector3 position;
        public Quaternion rotation;

        public Plane getPlane
        {
            get
            {
                this.plane = new Plane(this.RotationToNormal, this.position);
                return this.plane;
            }
        }

        public Vector3 RotationToNormal
        {
            get
            {
                Vector3 vector = (Vector3) (this.rotation * Vector3.forward);
                this.normal = vector.normalized;
                return this.normal;
            }
        }
    }
}

