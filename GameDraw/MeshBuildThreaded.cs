namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using UnityEngine;

    public class MeshBuildThreaded
    {
        public GDMesh _gdmesh;
        private Matrix4x4[] binds;
        private BoneWeight[] bones;
        private bool checkBinds;
        private bool checkBones;
        private bool checkColors;
        private bool checkNormals;
        private bool checkTangents;
        private bool checkUV;
        private bool checkUV1;
        private bool checkUV2;
        private Color[] colors;
        private int end;
        private int[] ends;
        private Exception ex;
        private GDMesh.Face[] faces;
        private int finishedCount;
        private ReaderWriterLockSlim locker;
        public Mesh m;
        private Mutex mutex = new Mutex();
        private Vector3[] normals;
        private object RVlocker = new object();
        private object RVlocker2 = new object();
        private int start;
        private int[] starts;
        private int subMeshCount;
        private List<int[]> subTriangles;
        private Vector4[] tangents;
        private Thread[] threads;
        private int[] triangles;
        private Vector2[] uv;
        private Vector2[] uv1;
        private Vector2[] uv2;
        private GDMesh.Vertex[] vertices;
        private Vector3[] verts;
        private object Vlocker = new object();
        private WaitHandle[] waitHandles;

        public void AddRelatedVertex(GDMesh.Vertex v,  bool flag=true)
        {
            if (flag)
            {
                if (this._gdmesh.relatedVertices[v.Traits.hashCode].Contains(v.Traits.ID))
                {
                    return;
                }
                lock (this.RVlocker)
                {
                    this._gdmesh.relatedVertices[v.Traits.hashCode].Add(v.Traits.ID);
                    return;
                }
            }
            List<int> indices = new List<int>(6);
            indices.Add(v.Traits.ID);
            IndexBuffer buffer = new IndexBuffer(v, indices);
            lock (this._gdmesh.relatedVertices)
            {
                if (!this._gdmesh.relatedVertices.ContainsKey(v.Traits.hashCode))
                {
                    lock (this.RVlocker2)
                    {
                        this._gdmesh.relatedVertices.Add(v.Traits.hashCode, buffer);
                        this._gdmesh.RVKeys.Add(v.Traits.hashCode);
                        this._gdmesh.RVVals.Add(buffer);
                        return;
                    }
                }
                if (!this._gdmesh.relatedVertices[v.Traits.hashCode].Contains(v.Traits.ID))
                {
                    lock (this.RVlocker)
                    {
                        this._gdmesh.relatedVertices[v.Traits.hashCode].Add(v.Traits.ID);
                    }
                }
            }
        }

        public void BuildDataStructure(object num)
        {
            int num2 = 0;
            if ((this.subMeshCount == 1) || (this.subMeshCount == 0))
            {
                int num3 = Mathf.Min(SystemInfo.processorCount * 30, this.triangles.Length);
                if (num3 <= 0)
                {
                    num3 = 1;
                }
                int num4 = (this.triangles.Length / num3) - ((this.triangles.Length / num3) % 3);
                this.threads = new Thread[num3];
                GDMesh.Element item = new GDMesh.Element();
                int index = 0;
                while (index < (num3 - 1))
                {
                    ThreadData data = new ThreadData(num4 * index, num4 * (index + 1));
                    data.custom = item;
                    data._gdmesh = this._gdmesh;
                    data.custom2 = this.triangles;
                    data.custom3 = 0;
                    data.custom4 = 0;
                    this.threads[index] = new Thread(new ParameterizedThreadStart(this.ProcessMesh));
                    this.threads[index].Priority = System.Threading.ThreadPriority.Highest;
                    this.threads[index].Start(data);
                    index++;
                }
                ThreadData parameter = new ThreadData(num4 * index, this.triangles.Length);
                parameter.custom = item;
                parameter._gdmesh = this._gdmesh;
                parameter.custom2 = this.triangles;
                parameter.custom3 = 0;
                parameter.custom4 = num2;
                this.threads[index] = new Thread(new ParameterizedThreadStart(this.ProcessMesh));
                this.threads[index].Start(parameter);
                this.threads[index].Priority = System.Threading.ThreadPriority.Highest;
                foreach (Thread thread in this.threads)
                {
                    thread.Join();
                }
                this._gdmesh.faces.Sort(delegate (GDMesh.Face x, GDMesh.Face y) {
                    if (x.Traits.ID < y.Traits.ID)
                    {
                        return -1;
                    }
                    if (x.Traits.ID > y.Traits.ID)
                    {
                        return 1;
                    }
                    return 0;
                });
                item.Index = 0;
                this._gdmesh.Elements.Add(item);
            }
            else
            {
                for (int i = 0; i < this.subMeshCount; i++)
                {
                    int num7 = Mathf.Min(SystemInfo.processorCount * 30, this.subTriangles[i].Length);
                    if (num7 <= 0)
                    {
                        num7 = 1;
                    }
                    int num8 = (this.subTriangles[i].Length / num7) - ((this.subTriangles[i].Length / num7) % 3);
                    this.threads = new Thread[num7];
                    GDMesh.Element element2 = new GDMesh.Element();
                    int num9 = 0;
                    while (num9 < (num7 - 1))
                    {
                        ThreadData data3 = new ThreadData(num8 * num9, num8 * (num9 + 1));
                        data3.custom = element2;
                        data3._gdmesh = this._gdmesh;
                        data3.custom2 = this.subTriangles[i];
                        data3.custom3 = i;
                        data3.custom4 = num2;
                        this.threads[num9] = new Thread(new ParameterizedThreadStart(this.ProcessMesh));
                        this.threads[num9].Priority = System.Threading.ThreadPriority.Highest;
                        this.threads[num9].Start(data3);
                        num9++;
                    }
                    ThreadData data4 = new ThreadData(num8 * num9, this.subTriangles[i].Length);
                    data4.custom = element2;
                    data4._gdmesh = this._gdmesh;
                    data4.custom2 = this.subTriangles[i];
                    data4.custom3 = i;
                    data4.custom4 = num2;
                    this.threads[num9] = new Thread(new ParameterizedThreadStart(this.ProcessMesh));
                    this.threads[num9].Start(data4);
                    this.threads[num9].Priority = System.Threading.ThreadPriority.Highest;
                    foreach (Thread thread2 in this.threads)
                    {
                        thread2.Join();
                    }
                    num2 += this.subTriangles[i].Length / 3;
                    element2.Index = i;
                    this._gdmesh.Elements.Add(element2);
                }
                this._gdmesh.faces.Sort(delegate (GDMesh.Face x, GDMesh.Face y) {
                    if (x.Traits.ID < y.Traits.ID)
                    {
                        return -1;
                    }
                    if (x.Traits.ID > y.Traits.ID)
                    {
                        return 1;
                    }
                    return 0;
                });
            }
            this.finishedCount++;
        }

        public GDMesh BuildThreaded()
        {
            DateTime now = DateTime.Now;
            bool flag = false;
            this.checkNormals = (this.m.normals != null) && (this.m.normals.Length == this.m.vertexCount);
            this.checkTangents = (this.m.tangents != null) && (this.m.tangents.Length == this.m.vertexCount);
            this.checkColors = (this.m.colors != null) && (this.m.colors.Length == this.m.vertexCount);
            this.checkUV = (this.m.uv != null) && (this.m.uv.Length == this.m.vertexCount);
            this.checkUV1 = (this.m.uv2 != null) && (this.m.uv2.Length == this.m.vertexCount);
            this.checkUV2 = (this.m.uv2 != null) && (this.m.uv2.Length == this.m.vertexCount);
            this.checkBones = (this.m.boneWeights != null) && (this.m.boneWeights.Length == this.m.vertexCount);
            this.checkBinds = (this.m.bindposes != null) && (this.m.bindposes.Length > 0);
            this.locker = new ReaderWriterLockSlim();
            this.verts = this.m.vertices;
            this.normals = this.m.normals;
            this.tangents = this.m.tangents;
            this.colors = this.m.colors;
            this.uv = this.m.uv;
            this.uv1 = this.m.uv2;
            this.uv2 = this.m.uv2;
            this.bones = this.m.boneWeights;
            this.binds = this.m.bindposes;
            if (this.mutex == null)
            {
                this.mutex = new Mutex(false);
            }
            if (this._gdmesh == null)
            {
                flag = true;
                this._gdmesh = new GDMesh(this.m);
            }
            this._gdmesh.vertexCount = this.m.vertexCount;
            this.vertices = new GDMesh.Vertex[this.m.vertexCount];
            int num = Mathf.Min(SystemInfo.processorCount * 30, this.m.vertexCount);
            if (num <= 0)
            {
                num = 1;
            }
            int num2 = this.m.vertexCount / num;
            this.threads = new Thread[num];
            if (flag)
            {
                try
                {
                    int index = 0;
                    while (index < (num - 1))
                    {
                        ThreadData data = new ThreadData(num2 * index, num2 * (index + 1));
                        this.threads[index] = new Thread(new ParameterizedThreadStart(this.HandleVertices));
                        this.threads[index].Priority = System.Threading.ThreadPriority.Highest;
                        this.threads[index].Start(data);
                        index++;
                    }
                    ThreadData parameter = new ThreadData(num2 * index, this.m.vertexCount);
                    this.threads[index] = new Thread(new ParameterizedThreadStart(this.HandleVertices));
                    this.threads[index].Start(parameter);
                    this.threads[index].Priority = System.Threading.ThreadPriority.Highest;
                    foreach (Thread thread in this.threads)
                    {
                        thread.Join();
                    }
                    this._gdmesh.vertices.Sort(delegate (GDMesh.Vertex x, GDMesh.Vertex y) {
                        if (x.Traits.ID < y.Traits.ID)
                        {
                            return -1;
                        }
                        if (x.Traits.ID > y.Traits.ID)
                        {
                            return 1;
                        }
                        return 0;
                    });
                }
                catch (Exception)
                {
                }
            }
            else
            {
                for (int j = 0; j < this._gdmesh.vertices.Count; j++)
                {
                    IndexBuffer buffer = this._gdmesh.relatedVertices[this._gdmesh.vertices[j].Traits.hashCode];
                    foreach (int num5 in buffer)
                    {
                        this.vertices[num5] = this._gdmesh.vertices[j];
                    }
                }
            }
            if (!flag)
            {
                if (this._gdmesh == null)
                {
                    this._gdmesh = new GDMesh(this.m);
                }
                if (this._gdmesh.faces == null)
                {
                    this._gdmesh.faces = new List<GDMesh.Face>(this.m.triangles.Length / 3);
                }
                if (this._gdmesh.edges == null)
                {
                    this._gdmesh.edges = new List<GDMesh.Edge>(this.m.triangles.Length);
                }
                if (this._gdmesh.elements == null)
                {
                    this._gdmesh.elements = new List<GDMesh.Element>(this.m.subMeshCount);
                }
                this._gdmesh.faces.Clear();
                this._gdmesh.edges.Clear();
                this._gdmesh.elements.Clear();
            }
            this.finishedCount = 0;
            DateTime time2 = DateTime.Now;
            this.subMeshCount = this.m.subMeshCount;
            this.triangles = this.m.triangles;
            this.subTriangles = new List<int[]>(this.subMeshCount);
            for (int i = 0; i < this.subMeshCount; i++)
            {
                this.subTriangles.Add(this.m.GetTriangles(i));
            }
            this.BuildDataStructure(0);
            if (this.checkBinds)
            {
                this._gdmesh.bindposes = new List<Matrix4x4>(this.m.bindposes);
            }
            this._gdmesh.isBuilt = true;
            GDManager.activeController.UpdateCollider();
            return this._gdmesh;
        }

        public void HandleVertices(object num)
        {
            ThreadData data = (ThreadData) num;
            int start = data.start;
            int end = data.end;
            for (int i = start; i < end; i++)
            {
                VertexTrait trait = new VertexTrait(i);
                trait.position = this.verts[i];
                trait.hashCode = GDMesh.getHashforVector3(this.verts[i]);
                trait.ID = i;
                if (this.checkNormals)
                {
                    trait.Normal = this.normals[i];
                }
                if (this.checkTangents)
                {
                    trait.tangent = this.tangents[i];
                }
                if (this.checkColors)
                {
                    trait.color = this.colors[i];
                }
                if (this.checkUV)
                {
                    trait.uv = this.uv[i];
                }
                if (this.checkUV1)
                {
                    trait.uv1 = this.uv1[i];
                }
                if (this.checkUV2)
                {
                    trait.uv2 = this.uv2[i];
                }
                if (this.checkBones)
                {
                    trait.boneWeight = this.bones[i];
                }
                GDMesh.Vertex item = new GDMesh.Vertex();
                item.Traits = trait;
                if (!this._gdmesh.relatedVertices.ContainsKey(item.Traits.hashCode))
                {
                    lock (this.Vlocker)
                    {
                        this._gdmesh.Vertices.Add(item);
                    }
                    this.AddRelatedVertex(item, false);
                }
                else
                {
                    this.AddRelatedVertex(item, true);
                }
                this.vertices[i] = item;
            }
            this.mutex.WaitOne();
            this.finishedCount++;
            this.mutex.ReleaseMutex();
        }

        public void ProcessMesh(object obj)
        {
            ThreadData data = (ThreadData) obj;
            GDMesh.Element custom = data.custom as GDMesh.Element;
            GDMesh mesh = data._gdmesh as GDMesh;
            int[] numArray = data.custom2 as int[];
            int num = (int) data.custom3;
            int num2 = (int) data.custom4;
            int start = data.start;
            int end = data.end;
            try
            {
                for (int i = start; i < end; i += 3)
                {
                    if ((i + 2) < numArray.Length)
                    {
                        FaceTrait vt = new FaceTrait(num2 + (i / 3));
                        vt.ID = num2 + (i / 3);
                        vt.subMeshIndex = num;
                        vt.subMeshTriID = i;
                        mesh.AddTriangle(vt, this.vertices[numArray[i]], this.vertices[numArray[i + 1]], this.vertices[numArray[i + 2]]).Element = custom;
                    }
                }
                this.mutex.WaitOne();
                this.finishedCount++;
                this.mutex.ReleaseMutex();
            }
            catch (Exception exception)
            {
                this.ex = exception;
                this.finishedCount = -1;
            }
        }

        public static void RemapAllocation(GDMesh m)
        {
            int num = 0;
            foreach (IndexBuffer buffer in m.RVVals)
            {
                buffer.vert.Traits.hashCode = m.RVKeys[num];
                num++;
            }
        }

        public class ThreadData
        {
            public object _gdmesh;
            public object custom;
            public object custom2;
            public object custom3;
            public object custom4;
            public int end;
            public int start;

            public ThreadData(int s, int e)
            {
                this.start = s;
                this.end = e;
            }
        }
    }
}

