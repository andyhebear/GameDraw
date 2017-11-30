namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class Visualizer : MonoBehaviour
    {
        public Transform bottom;
        public Transform front;
        public float isoLevel = 0.9f;
        public List<TraceLayer> layers = new List<TraceLayer>();
        public int resolution = 0x20;
        public Transform right;
        public float size = 1f;
        public Transform target;
        private MarchingCubeVisualizer visualizer = new MarchingCubeVisualizer();
        public float[,,] voxels;
        private int xSample;
        public int xSize;
        private int ySample;
        public int ySize;
        private int zSample;
        public int zSize;

        public void BuildFromSides(Texture2D front, Texture2D right, Texture2D bottom)
        {
            Color[] pixels = front.GetPixels();
            Color[] colorArray2 = right.GetPixels();
            int height = front.height;
            int width = front.width;
            int num3 = right.width;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (pixels[(i * width) + j].a == 1f)
                    {
                        for (int k = 0; k < num3; k++)
                        {
                            if (colorArray2[(i * num3) + k].a == 1f)
                            {
                                this.voxels[j, i, k] = 1f;
                            }
                            else
                            {
                                this.voxels[j, i, k] = 0f;
                            }
                        }
                    }
                }
            }
        }

        public void Construct()
        {
            if (this.voxels == null)
            {
                this.voxels = new float[this.xSize, this.ySize, this.zSize];
            }
        }

        public void execute()
        {
            this.voxels = new float[this.xSize, this.ySize, this.zSize];
            this.visualizer.sizeX = this.xSize;
            this.visualizer.sizeY = this.ySize;
            this.visualizer.sizeZ = this.zSize;
            this.visualizer.renderer = this.target.GetComponent<Renderer>();
            DateTime now = DateTime.Now;
            this.GetFromSides();
            Debug.Log(((TimeSpan) (DateTime.Now - now)).ToString());
            now = DateTime.Now;
            this.UpdateMeshBySurfaceNets();
            Debug.Log(((TimeSpan) (DateTime.Now - now)).ToString());
        }

        public void GetFromSides()
        {
            Texture2D mainTexture = (Texture2D) this.front.GetComponent<Renderer>().sharedMaterial.mainTexture;
            Texture2D right = (Texture2D) this.right.GetComponent<Renderer>().sharedMaterial.mainTexture;
            Texture2D bottom = new Texture2D(this.xSize, this.ySize);
            if (this.bottom.GetComponent<Renderer>().material.mainTexture != null)
            {
                bottom = (Texture2D) this.bottom.GetComponent<Renderer>().sharedMaterial.mainTexture;
            }
            this.BuildFromSides(mainTexture, right, bottom);
        }

        public void RandomFill()
        {
            for (int i = 1; i < (this.xSize - 1); i++)
            {
                for (int j = 1; j < (this.ySize - 1); j++)
                {
                    for (int k = 1; k < (this.zSize - 1); k++)
                    {
                        this.voxels[i, j, k] = UnityEngine.Random.Range(0, 1);
                    }
                }
            }
        }

        public void Start()
        {
            this.execute();
        }

        public void Update()
        {
        }

        public void UpdateMesh()
        {
            Voxels voxels = new Voxels();
            voxels.gridSize = this.size;
            voxels.length = this.resolution;
            voxels.Isolevel = this.isoLevel;
            voxels.Load(this.voxels);
            List<Vector3> list = new List<Vector3>(voxels.triangles.Count * 3);
            List<int> list2 = new List<int>(voxels.triangles.Count * 3);
            int num = 0;
            foreach (Triangle triangle in voxels.triangles)
            {
                if ((num < 0xfde8) && (list.Count < 0xfde5))
                {
                    list2.Add(list.Count);
                    list.Add(triangle.pointOne);
                    list2.Add(list.Count);
                    list.Add(triangle.pointTwo);
                    list2.Add(list.Count);
                    list.Add(triangle.pointThree);
                    num++;
                }
                else
                {
                    num = 0;
                    Mesh mesh = new Mesh();
                    mesh.vertices = list.ToArray();
                    mesh.triangles = list2.ToArray();
                    mesh.uv = new Vector2[mesh.vertexCount];
                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();
                    GameObject obj2 = new GameObject("Model", new System.Type[] { typeof(MeshFilter), typeof(MeshRenderer) });
                    obj2.GetComponent<MeshFilter>().sharedMesh = mesh;
                    obj2.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
                    list2.Clear();
                    list.Clear();
                }
            }
            Mesh mesh2 = new Mesh();
            mesh2.vertices = list.ToArray();
            mesh2.triangles = list2.ToArray();
            mesh2.uv = new Vector2[mesh2.vertexCount];
            mesh2.RecalculateBounds();
            mesh2.RecalculateNormals();
            GameObject obj3 = new GameObject("Model", new System.Type[] { typeof(MeshFilter), typeof(MeshRenderer) });
            obj3.GetComponent<MeshFilter>().sharedMesh = mesh2;
            obj3.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
            list2.Clear();
            list.Clear();
        }

        public void UpdateMeshBySurfaceNets()
        {
            NaiveSurfaceNets nets = new NaiveSurfaceNets();
            Mesh mesh = new Mesh();
            nets.UpdateMesh(Vector3.one, this.voxels, new Bounds(Vector3.zero, Vector3.one), mesh, 0);
            mesh.uv = new Vector2[mesh.vertexCount];
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            GameObject obj2 = new GameObject("Model", new System.Type[] { typeof(MeshFilter), typeof(MeshRenderer) });
            obj2.GetComponent<MeshFilter>().sharedMesh = mesh;
            obj2.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
        }
    }
}

