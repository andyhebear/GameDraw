namespace GameDraw
{
    using System;
    using System.Collections;
    using UnityEngine;

    public class DecimatedMeshBuffer
    {
        public bool initialized;
        public bool isCalculated;
        public bool isPreview;
        public float ratio = 0.5f;
        public ArrayList selectedPos = new ArrayList();
        public Mesh sMesh;
        public float smoothAngle = 45f;
        public Mesh tmpMesh;
        public string tmpMeshName = "";
    }
}

