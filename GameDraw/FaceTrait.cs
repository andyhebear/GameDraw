namespace GameDraw
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [Serializable]
    public class FaceTrait
    {
        public int ID;
        public Vector3 Normal;
        public int subMeshIndex;
        public int subMeshTriID;

        public FaceTrait(int id)
        {
            this.ID = id;
            this.Normal = Vector3.forward.normalized;
            this.subMeshIndex = 0;
            this.subMeshTriID = 0;
        }

        public FaceTrait(int id, Vector3 normal, int index, int subMeshTriID=0)
        {
            this.ID = id;
            this.Normal = normal;
            this.subMeshIndex = index;
            this.subMeshTriID = subMeshTriID;
        }

        public override string ToString()
        {
            return this.ID.ToString();
        }
    }
}

