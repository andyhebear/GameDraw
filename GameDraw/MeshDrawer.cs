namespace GameDraw
{
    using System;
    using UnityEngine;

    [ExecuteInEditMode]
    public class MeshDrawer : MonoBehaviour
    {
        public Mesh aMesh;
        public bool draw;
        public Material mat;
        public Matrix4x4 matrix;

        private void Update()
        {
            if (this.draw)
            {
                this.mat.SetPass(0);
                Graphics.DrawMeshNow(this.aMesh, this.matrix);
            }
        }
    }
}

