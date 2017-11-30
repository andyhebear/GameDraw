namespace GameDraw
{
    using System;
    using UnityEngine;

    [Serializable]
    public class TraceLayer
    {
        public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        public CornerType corners;
        public float isoLevel = 0.9f;
        [HideInInspector]
        public TraceMode mode;
        public bool optimize = true;
        public int resolution = 0x40;
        [HideInInspector]
        public TraceSides sides;
        public Texture2D[] sideTextures = new Texture2D[3];
        public float size = 1f;

        [Flags]
        public enum CornerType
        {
            Sharp,
            Corner,
            Smooth
        }
    }
}

