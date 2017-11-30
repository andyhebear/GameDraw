namespace GameDraw
{
    using System;
    using UnityEngine;

    public class Grid : MonoBehaviour
    {
        public Color gridColor;
        public Shader shader;
        public Transform target;

        private void OnPostRender()
        {
            GLDrawUtility.DrawGrid(this.gridColor, this.gridColor, this.gridColor, this.target, null);
        }
    }
}

