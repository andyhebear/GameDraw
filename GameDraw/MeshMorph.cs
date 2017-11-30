namespace GameDraw
{
    using System;
    using UnityEngine;

    public class MeshMorph : ScriptableObject
    {
        public Mesh mesh;
        public Part root;

        public static void Init()
        {
            ScriptableObject.CreateInstance<MeshMorph>();
        }
    }
}

