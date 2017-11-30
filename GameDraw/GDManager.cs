namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [ExecuteInEditMode]
    public class GDManager : MonoBehaviour
    {
        public static GDController activeController;
        public Mesh aMesh;
        public List<GDController> controllers;
        public bool drawElements;
        public bool drawFlipped;
        public bool drawTriangles;
        public string email = "";
        public Material flippedMat;
        public Mesh flippedMesh;
        private int frame;
        [NonSerialized]
        public static GDManager manager;
        public Material mat;
        public Material mat2;
        public Matrix4x4 matrix;
        public int maxObjectCount = 3;
        public bool messageRead;
        [HideInInspector]
        public List<GameObject> objects;
        public static bool showGDManager = false;

        public void buildFlippedMesh(Mesh mesh)
        {
            this.flippedMesh = UnityEngine.Object.Instantiate(mesh) as Mesh;
            List<List<int>> list = new List<List<int>>(this.flippedMesh.subMeshCount);
            for (int i = 0; i < this.flippedMesh.subMeshCount; i++)
            {
                list.Add(new List<int>(this.flippedMesh.GetTriangles(i)));
            }
            int[] triangles = this.flippedMesh.triangles;
            Vector3[] vertices = this.flippedMesh.vertices;
            for (int j = 0; j < triangles.Length; j += 3)
            {
                int num3 = 0;
                int num4 = 0;
                int num5 = 0;
                int num6 = j;
                for (int k = 0; k < list.Count; k++)
                {
                    if (num6 < (list[k].Count + num3))
                    {
                        num4 = k;
                        num5 = num6 - num3;
                        break;
                    }
                    num3 += list[k].Count;
                }
                int num8 = list[num4][num5];
                list[num4][num5] = list[num4][num5 + 2];
                list[num4][num5 + 2] = num8;
                int num9 = triangles[j];
                triangles[j] = triangles[j + 2];
                triangles[j + 2] = num9;
            }
            this.flippedMesh.subMeshCount = list.Count;
            int submesh = 0;
            foreach (List<int> list2 in list)
            {
                this.flippedMesh.SetTriangles(list2.ToArray(), submesh);
                submesh++;
            }
        }

        public static GDManager forceGetManager()
        {
            manager = (GDManager) UnityEngine.Object.FindObjectOfType(typeof(GDManager));
            return manager;
        }

        public GDController getController(GameObject go,  bool refresh=false)
        {
            if (this.objects == null)
            {
                this.objects = new List<GameObject>();
            }
            if (this.controllers == null)
            {
                this.controllers = new List<GDController>();
            }
            if (!this.objects.Contains(go))
            {
                if (this.objects.Count < this.maxObjectCount)
                {
                    this.objects.Add(go);
                    GDController item = new GDController(go);
                    this.controllers.Add(item);
                    return item;
                }
                this.objects[0] = go;
                GDController controller2 = new GDController(go);
                this.controllers[0] = controller2;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return controller2;
            }
            int index = this.objects.IndexOf(go);
            if ((this.controllers.Count < this.objects.Count) || (this.controllers[index] == null))
            {
                this.objects.Clear();
                this.controllers.Clear();
                this.objects.Add(go);
                GDController controller3 = new GDController(go);
                this.controllers.Add(controller3);
                return controller3;
            }
            if (refresh)
            {
                this.controllers[index] = new GDController(go);
            }
            return this.controllers[index];
        }

        public static GDManager getManager()
        {
            if (manager == null)
            {
                manager = (GDManager) UnityEngine.Object.FindObjectOfType(typeof(GDManager));
            }
            if (manager == null)
            {
                GameObject obj2 = new GameObject("GameDrawManager", new System.Type[] { typeof(GDManager), typeof(MeshCollider) });
                obj2.hideFlags = HideFlags.HideInHierarchy;
                return obj2.GetComponent<GDManager>();
            }
            return manager;
        }

        public void OnRenderObject()
        {
            if (this.mat == null)
            {
                Color color = new Color(1f, 0f, 0f, 0.6f);
                this.mat = new Material(Shader.Find("GameDraw/Highlight"));
                this.mat.color = color;
                this.mat.SetColor("_Color", color);
                this.mat.SetColor("_Emission", color);
                this.mat.SetColor("_Ambient", color);
            }
            if (this.mat2 == null)
            {
                Color color2 = new Color(1f, 0f, 0f, 0.6f);
                this.mat2 = new Material(Shader.Find("GameDraw/HighlightPlusNoCull"));
                this.mat2.color = color2;
                this.mat2.SetColor("_Color", color2);
                this.mat2.SetColor("_Emission", color2);
                this.mat2.SetColor("_Ambient", color2);
            }
            if (this.flippedMat == null)
            {
                Color color3 = new Color(1f, 0f, 1f, 0.6f);
                this.flippedMat = new Material(Shader.Find("VertexLit"));
                this.flippedMat.color = color3;
                this.flippedMat.SetColor("_Color", color3);
                this.flippedMat.SetColor("_Emission", color3);
                this.flippedMat.SetColor("_Ambient", color3);
            }
            if (((activeController != null) && (activeController.selection != null)) && ((activeController.gui.mainSelectionNum == 1) || (activeController.gui.mainSelectionNum == 3)))
            {
                if (activeController.selection.selectionType == MeshSelection.SelectionType.Triangle)
                {
                    if (this.drawTriangles && this.drawFlipped)
                    {
                        this.flippedMat.SetPass(0);
                        Graphics.DrawMeshNow(this.flippedMesh, base.transform.localToWorldMatrix);
                        this.mat2.SetPass(0);
                        Graphics.DrawMeshNow(this.aMesh, base.transform.localToWorldMatrix);
                    }
                    else if (this.drawTriangles)
                    {
                        this.mat.SetPass(0);
                        Graphics.DrawMeshNow(this.aMesh, base.transform.localToWorldMatrix);
                    }
                }
                else if ((activeController.selection.selectionType == MeshSelection.SelectionType.Element) && this.drawElements)
                {
                    this.mat.SetPass(0);
                    foreach (int num in activeController.selection.selectedElements)
                    {
                        Graphics.DrawMeshNow(activeController.mesh, base.transform.localToWorldMatrix, num);
                    }
                }
            }
        }

        private void Start()
        {
            base.GetComponent<Collider>().enabled = false;
        }

        private void Update()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                base.GetComponent<Collider>().enabled = true;
            }
            if ((activeController != null) && (activeController.transform != null))
            {
                if (base.transform.position != activeController.transform.position)
                {
                    base.transform.position = activeController.transform.position;
                }
                if (base.transform.rotation != activeController.transform.rotation)
                {
                    base.transform.rotation = activeController.transform.rotation;
                }
                if (base.transform.localScale != activeController.transform.lossyScale)
                {
                    base.transform.localScale = activeController.transform.lossyScale;
                }
            }
        }
    }
}

