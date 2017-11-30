namespace GameDraw
{
    using System;
    using UnityEngine;

    public class Gridcell
    {
        public Vector3[] p;
        public float[] val;

        public Gridcell(Vector3[] positions, float[] values)
        {
            this.p = positions;
            this.val = values;
        }
    }
}

