namespace GD.Meshes.Generic
{
    using GameDraw;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading;
    using UnityEngine;

    [Serializable]
    public class GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>
    {
        public List<Matrix4x4> bindposes;
        public MeshBuffer buffer;
        //public List<Edge<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>> edges;
        //public readonly EdgeCollection<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Edges;
        //public List<Element<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>> elements;
        //public List<Face<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>> faces;
        //public readonly FaceCollection<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Faces;
        //public List<Halfedge<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>> halfedges;
        //public readonly HalfedgeCollection<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Halfedges;
        public List<Edge> edges;
        public readonly EdgeCollection Edges;
        public List<Element> elements;
        public List<Face> faces;
        public readonly FaceCollection Faces;
        public List<Halfedge> halfedges;
        public readonly HalfedgeCollection Halfedges;
        public SerializableDictionary<int, IndexBuffer> relatedVertices;
        public List<int> RVKeys;
        public List<IndexBuffer> RVVals;
        [SerializeField]
        protected bool trianglesOnly;
        //public List<Vertex<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>> vertices;
        //public readonly VertexCollection<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Vertices;
        public List<Vertex> vertices;
        public readonly VertexCollection Vertices;
        public GD_Mesh()
        {
            //this.edges = new List<Edge<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>>();
            //this.faces = new List<Face<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>>();
            //this.halfedges = new List<Halfedge<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>>();
            //this.vertices = new List<Vertex<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>>();
            this.edges = new List<Edge>();
            this.faces = new List<Face>();
            this.halfedges = new List<Halfedge>();
            this.vertices = new List<Vertex>();
            this.bindposes = new List<Matrix4x4>();
            this.elements = new List<Element>();
            this.relatedVertices = new SerializableDictionary<int, IndexBuffer>();
            this.RVKeys = new List<int>();
            this.RVVals = new List<IndexBuffer>();
            this.buffer = new MeshBuffer();
            this.Edges = new EdgeCollection((GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>) this);
            this.Faces = new FaceCollection((GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>) this);
            this.Halfedges = new HalfedgeCollection((GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>) this);
            this.Vertices = new VertexCollection((GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>) this);
            this.elements = new List<Element>();
            this.relatedVertices = new SerializableDictionary<int, IndexBuffer>();
            this.RVKeys = new List<int>();
            this.RVVals = new List<IndexBuffer>();
            this.buffer = new MeshBuffer();
        }

        protected void AppendToEdgeList(Edge edge)
        {
            edge.Index = this.edges.Count;
            this.edges.Add(edge);
        }

        protected void AppendToFaceList(Face face)
        {
            face.Index = this.faces.Count;
            this.faces.Add(face);
        }

        protected void AppendToHalfedgeList(Halfedge halfedge)
        {
            halfedge.Index = this.halfedges.Count;
            this.halfedges.Add(halfedge);
        }

        protected void AppendToVertexList(Vertex vertex)
        {
            vertex.Index = this.vertices.Count;
            vertex.Mesh = (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>) this;
            this.vertices.Add(vertex);
        }

        protected virtual void Clear()
        {
            this.edges.Clear();
            this.faces.Clear();
            this.halfedges.Clear();
            this.vertices.Clear();
        }

        public static bool FacesShareEdge(Face faceA, Face faceB)
        {
            foreach (Face face in faceA.Faces)
            {
                if (face == faceB)
                {
                    return true;
                }
            }
            return false;
        }

        public void FillDictionary()
        {
            if (((this.RVKeys != null) && (this.RVVals != null)) && ((this.RVKeys.Count == this.RVVals.Count) && (this.RVKeys.Count > 0)))
            {
                if (this.relatedVertices == null)
                {
                    this.relatedVertices = new SerializableDictionary<int, IndexBuffer>();
                }
                for (int i = 0; i < this.RVKeys.Count; i++)
                {
                    this.relatedVertices.Add(this.RVKeys[i], this.RVVals[i]);
                }
            }
        }

        public static GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> FromFile(string fileName)
        {
            using (Stream stream = File.OpenRead(fileName))
            {
                return GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.FromStream(stream);
            }
        }

        public static GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> FromStream(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            return (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>) formatter.Deserialize(stream);
        }

        public static int getHashforVector3(Vector3 vec) {
         return   (vec.x.ToString() + vec.y.ToString() + vec.z.ToString()).GetHashCode();
        }

        public void Save(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
        }

        public void Save(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (Stream stream = File.OpenWrite(fileName))
            {
                this.Save(stream);
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Face face in this.Faces)
            {
                foreach (Vertex vertex in face.Vertices)
                {
                    builder.Append(vertex.Traits.ToString());
                    builder.Append(" -> ");
                }
                builder.Append(Environment.NewLine);
            }
            return builder.ToString();
        }

        public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> TriangularCopy()
        {
            GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh = new GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>();
            Dictionary<Vertex, Vertex> dictionary = new Dictionary<Vertex, Vertex>();
            foreach (Vertex vertex in this.Vertices)
            {
                dictionary[vertex] = mesh.Vertices.Add(vertex.Traits);
            }
            foreach (Face face in this.Faces)
            {
                Vertex[] faceVertices = new Vertex[face.VertexCount];
                int index = 0;
                foreach (Vertex vertex2 in face.Vertices)
                {
                    faceVertices[index] = dictionary[vertex2];
                    index++;
                }
                mesh.Faces.AddTriangles(face.Traits, faceVertices);
            }
            return mesh;
        }

        public void TrimExcess()
        {
            this.edges.TrimExcess();
            this.faces.TrimExcess();
            this.halfedges.TrimExcess();
            this.vertices.TrimExcess();
        }

        public void VerifyTopology()
        {
            foreach (Halfedge halfedge in this.halfedges)
            {
                if (halfedge.Previous.Next != halfedge)
                {
                    throw new BadTopologyException("A halfedge's previous next is not itself.");
                }
                if (halfedge.Next.Previous != halfedge)
                {
                    throw new BadTopologyException("A halfedge's next previous is not itself.");
                }
                if (halfedge.Next.Face != halfedge.Face)
                {
                    throw new BadTopologyException("Adjacent halfedges do not belong to the same face.");
                }
                bool flag = false;
                foreach (Halfedge halfedge2 in halfedge.FromVertex.Halfedges)
                {
                    if (halfedge2 == halfedge)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    throw new BadTopologyException("A halfedge is not reachable from the vertex it originates from.");
                }
            }
        }

        [Serializable]
        public class Edge
        {
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge;
            [SerializeField]
            private int index;
            public TEdgeTraits Traits;

            internal Edge()
            {
            }

            internal Edge(TEdgeTraits edgeTraits)
            {
                this.Traits = edgeTraits;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face Face0 {
              get{return   this.halfedge.Face;}
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face Face1 {
              get {return  this.halfedge.Opposite.Face;}
            }
            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge Halfedge0
            {
                get {
                 return   this.halfedge;
                }
                internal set
                {
                    this.halfedge = value;
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge Halfedge1
            {
                get {
                 return   this.halfedge.Opposite;
                }
                internal set
                {
                    this.halfedge.Opposite = value;
                }
            }

            public int Index
            {
                get {
                 return   this.index;
                }
                internal set
                {
                    this.index = value;
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Mesh {
             get{return   this.halfedge.Mesh;}
            }
            public bool OnBoundary
            {
                get
                {
                    if (!this.halfedge.OnBoundary)
                    {
                        return this.halfedge.Opposite.OnBoundary;
                    }
                    return true;
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex Vertex0 {
             get{return   this.halfedge.ToVertex;}
            
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex Vertex1 {
              get{return   this.halfedge.Opposite.ToVertex;}
            }
        }

        [Serializable]
        public class EdgeCollection : IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge>, IEnumerable
        {
            [SerializeField]
            private readonly GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh;

            internal EdgeCollection(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> m)
            {
                this.mesh = m;
            }

            public IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge> GetEnumerator()
            {
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge iteratorVariable0 in this.mesh.edges)
                {
                    yield return iteratorVariable0;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
              return   this.GetEnumerator();
            }

            public int Count {
              get{return   this.mesh.edges.Count;}
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge this[int index]{
              get{ return   this.mesh.edges[index];}
            }

          
        }

        [Serializable]
        public class EdgeDynamicTrait<TraitType>
        {
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh;
            [SerializeField]
            private TraitType[] trait;

            public EdgeDynamicTrait(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh)
            {
                this.mesh = mesh;
                this.trait = new TraitType[mesh.Edges.Count];
            }

            public TraitType this[GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge edge]
            {
                get
                {
                    TraitType local;
                    if (edge.Mesh != this.mesh)
                    {
                        throw new MismatchedMeshException("The dynamic trait is not assigned to the mesh that the edge belongs to.");
                    }
                    try
                    {
                        local = this.trait[edge.Index];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentOutOfRangeException("Cannot access dynamic trait of an edge that wasn't present when the dynamic trait was created.");
                    }
                    return local;
                }
                set
                {
                    if (edge.Mesh != this.mesh)
                    {
                        throw new MismatchedMeshException("The dynamic trait is not assigned to the mesh that the edge belongs to.");
                    }
                    try
                    {
                        this.trait[edge.Index] = value;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentOutOfRangeException("Cannot access dynamic trait of an edge that wasn't present when the dynamic trait was created.");
                    }
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Mesh {
             get{return   this.mesh;}
            }
        }

        [Serializable]
        public class Element
        {
            [SerializeField]
            private List<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face> faces;
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge;
            [SerializeField]
            private int index;

            internal Element()
            {
                this.faces = new List<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face>();
            }

            internal Element(List<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face> faceList)
            {
                this.faces = faceList;
            }

            public List<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face> BoundaryFaces
            {
                get
                {
                    List<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face> list = new List<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face>();
                    foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face face in this.faces)
                    {
                        foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge in face.Halfedges)
                        {
                            if (halfedge.Opposite.OnBoundary)
                            {
                                list.Add(face);
                            }
                        }
                    }
                    return list;
                }
            }

            public int FaceCount{
               get {return   this.faces.Count;}
            }

            public List<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face> Faces
            {
                get {
                 return   this.faces;
                }
                set
                {
                    this.faces = value;
                }
            }

            public int Index
            {
                get {
                 return   this.index;
                }
                internal set
                {
                    this.index = value;
                }
            }
        }

        [Serializable]
        public class ElementCollection : IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Element>, IEnumerable
        {
            [SerializeField]
            private readonly GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh;

            internal ElementCollection(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> m)
            {
                this.mesh = m;
            }

            public IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Element> GetEnumerator()
            {
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Element iteratorVariable0 in this.mesh.elements)
                {
                    yield return iteratorVariable0;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
             return   this.GetEnumerator();
            }

            public int Count {
             get{return   this.mesh.elements.Count;}
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Element this[int index] {
             get{return   this.mesh.elements[index];}
            }

           
        }

        [Serializable]
        public class Face
        {
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Element element;
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge;
            [SerializeField]
            private int index;
            public TFaceTraits Traits;

            internal Face()
            {
            }

            internal Face(TFaceTraits faceTraits)
            {
                this.Traits = faceTraits;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge FindEdgeTo(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face face)
            {
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge in this.Halfedges)
                {
                    if (halfedge.Opposite.Face == face)
                    {
                        return halfedge.Edge;
                    }
                }
                return null;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge FindHalfedgeTo(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex vertex)
            {
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge in this.Halfedges)
                {
                    if (halfedge.ToVertex == vertex)
                    {
                        return halfedge;
                    }
                }
                return null;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge GetEdge(int index)
            {
                int num = 0;
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge edge in this.Edges)
                {
                    if (num == index)
                    {
                        return edge;
                    }
                    num++;
                }
                throw new ArgumentOutOfRangeException("index");
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face GetFace(int index)
            {
                int num = 0;
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face face in this.Faces)
                {
                    if (num == index)
                    {
                        return face;
                    }
                    num++;
                }
                throw new ArgumentOutOfRangeException("index");
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge GetHalfedge(int index)
            {
                int num = 0;
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge in this.Halfedges)
                {
                    if (num == index)
                    {
                        return halfedge;
                    }
                    num++;
                }
                throw new ArgumentOutOfRangeException("index");
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex GetVertex(int index)
            {
                int num = 0;
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex vertex in this.Vertices)
                {
                    if (num == index)
                    {
                        return vertex;
                    }
                    num++;
                }
                throw new ArgumentOutOfRangeException("index");
            }

            public int EdgeCount
            {
                get
                {
                    int num = 0;
                    using (IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge> enumerator = this.Edges.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge current = enumerator.Current;
                            num++;
                        }
                    }
                    return num;
                }
            }

            public IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge> Edges
            {
                get
                {
                    foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge iteratorVariable0 in this.Halfedges)
                    {
                        yield return iteratorVariable0.Edge;
                    }
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Element Element
            {
                get {
                 return    this.element;
                }
                internal set
                {
                    this.element = value;
                    if (value == null)
                    {
                        this.element.Faces.Remove((GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face) this);
                    }
                    else if (!this.element.Faces.Contains((GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face) this))
                    {
                        this.element.Faces.Add((GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face) this);
                    }
                }
            }

            public int FaceCount
            {
                get
                {
                    int num = 0;
                    using (IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face> enumerator = this.Faces.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face current = enumerator.Current;
                            num++;
                        }
                    }
                    return num;
                }
            }

            public IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face> Faces
            {
                get
                {
                    foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge iteratorVariable0 in this.Halfedges)
                    {
                        yield return iteratorVariable0.Opposite.Face;
                    }
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge Halfedge
            {
                get {
                 return   this.halfedge;
                }
                internal set
                {
                    this.halfedge = value;
                }
            }

            public int HalfedgeCount
            {
                get
                {
                    int num = 0;
                    using (IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge> enumerator = this.Halfedges.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge current = enumerator.Current;
                            num++;
                        }
                    }
                    return num;
                }
            }

            public IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge> Halfedges
            {
                get
                {
                    GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge = this.halfedge;
                    do
                    {
                        yield return halfedge;
                        halfedge = halfedge.Next;
                    }
                    while (halfedge != this.halfedge);
                }
            }

            public int Index
            {
                get {
                 return   this.index;
                }
                internal set
                {
                    this.index = value;
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Mesh {
              get{ return   this.halfedge.Mesh;}
            }

            public bool OnBoundary
            {
                get
                {
                    foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge in this.Halfedges)
                    {
                        if (halfedge.Opposite.OnBoundary)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            public int VertexCount
            {
                get
                {
                    int num = 0;
                    using (IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex> enumerator = this.Vertices.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex current = enumerator.Current;
                            num++;
                        }
                    }
                    return num;
                }
            }

            public IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex> Vertices
            {
                get
                {
                    foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge iteratorVariable0 in this.Halfedges)
                    {
                        yield return iteratorVariable0.ToVertex;
                    }
                }
            }

           

          

          

          
        }

        [Serializable]
        public class FaceCollection : IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face>, IEnumerable
        {
            [SerializeField]
            private readonly GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh;

            internal FaceCollection(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> m)
            {
                this.mesh = m;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face Add(params GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex[] faceVertices) { 
              return  this.Add(default(TFaceTraits), faceVertices);
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face Add(TFaceTraits faceTraits, params GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex[] faceVertices)
            {
                if (this.mesh.trianglesOnly)
                {
                    return this.AddTriangles(faceTraits, faceVertices)[0];
                }
                return this.CreateFace(faceTraits, faceVertices);
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face[] AddTriangles(params GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex[] faceVertices) {
              return  this.AddTriangles(default(TFaceTraits), faceVertices);
            }
            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face[] AddTriangles(TFaceTraits faceTraits, params GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex[] faceVertices)
            {
                int length = faceVertices.Length;
                if (length < 3)
                {
                    throw new BadTopologyException("Cannot create a polygon with fewer than three vertices.");
                }
                GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face[] faceArray = new GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face[length - 2];
                for (int i = 0; i < (length - 2); i++)
                {
                    faceArray[i] = this.CreateFace(faceTraits, new GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex[] { faceVertices[0], faceVertices[i + 1], faceVertices[i + 2] });
                }
                return faceArray;
            }

            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face CreateFace(TFaceTraits faceTraits, params GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex[] faceVertices)
            {
                int length = faceVertices.Length;
                if (length < 3)
                {
                    throw new BadTopologyException("Cannot create a polygon with fewer than three vertices.");
                }
                GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge[] halfedgeArray = new GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge[length];
                bool[] flagArray = new bool[length];
                bool[] flagArray2 = new bool[length];
                for (int i = 0; i < length; i++)
                {
                    int index = (i + 1) % length;
                    if (faceVertices[i] == null)
                    {
                        throw new ArgumentNullException("Can't add a null vertex to a face.");
                    }
                    if (!faceVertices[i].OnBoundary)
                    {
                        throw new BadTopologyException("Can't add an edge to a vertex on the interior of a mesh.");
                    }
                    halfedgeArray[i] = faceVertices[i].FindHalfedgeTo(faceVertices[index]);
                    flagArray[i] = halfedgeArray[i] == null;
                    flagArray2[i] = faceVertices[i].Halfedge != null;
                    if (!flagArray[i] && !halfedgeArray[i].OnBoundary)
                    {
                        throw new BadTopologyException("Can't add more than two faces to an edge.");
                    }
                }
                GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face face = new GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face(faceTraits);
                this.mesh.AppendToFaceList(face);
                for (int j = 0; j < length; j++)
                {
                    int num5 = (j + 1) % length;
                    if (flagArray[j])
                    {
                        GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge edge = new GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge();
                        this.mesh.AppendToEdgeList(edge);
                        halfedgeArray[j] = new GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge();
                        this.mesh.AppendToHalfedgeList(halfedgeArray[j]);
                        halfedgeArray[j].Opposite = new GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge();
                        this.mesh.AppendToHalfedgeList(halfedgeArray[j].Opposite);
                        halfedgeArray[j].Opposite.Opposite = halfedgeArray[j];
                        edge.Halfedge0 = halfedgeArray[j];
                        halfedgeArray[j].Edge = edge;
                        halfedgeArray[j].Opposite.Edge = edge;
                        halfedgeArray[j].ToVertex = faceVertices[num5];
                        halfedgeArray[j].Opposite.ToVertex = faceVertices[j];
                        if (faceVertices[j].Halfedge == null)
                        {
                            faceVertices[j].Halfedge = halfedgeArray[j];
                        }
                    }
                    if (halfedgeArray[j].Face != null)
                    {
                        throw new BadTopologyException("An inner halfedge already has a face assigned to it.");
                    }
                    halfedgeArray[j].Face = face;
                }
                for (int k = 0; k < length; k++)
                {
                    int num7 = (k + 1) % length;
                    if ((flagArray[k] && flagArray[num7]) && flagArray2[num7])
                    {
                        GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge = null;
                        foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge2 in faceVertices[num7].Halfedges)
                        {
                            if (halfedge2.Face == null)
                            {
                                halfedge = halfedge2;
                                break;
                            }
                        }
                        GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge previous = halfedge.Previous;
                        halfedgeArray[k].Opposite.Previous = previous;
                        previous.Next = halfedgeArray[k].Opposite;
                        halfedgeArray[num7].Opposite.Next = halfedge;
                        halfedge.Previous = halfedgeArray[num7].Opposite;
                    }
                    else if (flagArray[k] && flagArray[num7])
                    {
                        halfedgeArray[k].Opposite.Previous = halfedgeArray[num7].Opposite;
                        halfedgeArray[num7].Opposite.Next = halfedgeArray[k].Opposite;
                    }
                    else if (flagArray[k] && !flagArray[num7])
                    {
                        halfedgeArray[k].Opposite.Previous = halfedgeArray[num7].Previous;
                        halfedgeArray[num7].Previous.Next = halfedgeArray[k].Opposite;
                    }
                    else if (!flagArray[k] && flagArray[num7])
                    {
                        halfedgeArray[k].Next.Previous = halfedgeArray[num7].Opposite;
                        halfedgeArray[num7].Opposite.Next = halfedgeArray[k].Next;
                    }
                    else if ((!flagArray[k] && !flagArray[num7]) && (halfedgeArray[k].Next != halfedgeArray[num7]))
                    {
                        GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge opposite = halfedgeArray[k].Opposite;
                        do
                        {
                            opposite = opposite.Previous.Opposite;
                        }
                        while (((opposite.Face != null) && (opposite != halfedgeArray[num7])) && (opposite != halfedgeArray[k].Opposite));
                        if ((opposite == halfedgeArray[num7]) || (opposite == halfedgeArray[k].Opposite))
                        {
                            throw new BadTopologyException("Unable to find an opening to relink an existing face.");
                        }
                        GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge5 = opposite.Previous;
                        halfedge5.Next = halfedgeArray[k].Next;
                        halfedgeArray[k].Next.Previous = halfedge5;
                        halfedgeArray[num7].Previous.Next = opposite;
                        opposite.Previous = halfedgeArray[num7].Previous;
                    }
                    halfedgeArray[k].Next = halfedgeArray[num7];
                    halfedgeArray[num7].Previous = halfedgeArray[k];
                }
                face.Halfedge = halfedgeArray[0];
                return face;
            }

            public IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face> GetEnumerator()
            {
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face iteratorVariable0 in this.mesh.faces)
                {
                    yield return iteratorVariable0;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
             
               return this.GetEnumerator();
            }

            public int Count{
            
                get {return this.mesh.faces.Count;}
            }
            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face this[int index] {
             get{return   this.mesh.faces[index];}
            }

            
        }

        [Serializable]
        public class FaceDynamicTrait<TraitType>
        {
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh;
            [SerializeField]
            private TraitType[] trait;

            public FaceDynamicTrait(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh)
            {
                this.mesh = mesh;
                this.trait = new TraitType[mesh.Faces.Count];
            }

            public TraitType this[GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face face]
            {
                get
                {
                    TraitType local;
                    if (face.Mesh != this.mesh)
                    {
                        throw new MismatchedMeshException("The dynamic trait is not assigned to the mesh that the face belongs to.");
                    }
                    try
                    {
                        local = this.trait[face.Index];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentOutOfRangeException("Cannot access dynamic trait of a face that wasn't present when the dynamic trait was created.");
                    }
                    return local;
                }
                set
                {
                    if (face.Mesh != this.mesh)
                    {
                        throw new MismatchedMeshException("The dynamic trait is not assigned to the mesh that the face belongs to.");
                    }
                    try
                    {
                        this.trait[face.Index] = value;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentOutOfRangeException("Cannot access dynamic trait of a face that wasn't present when the dynamic trait was created.");
                    }
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Mesh {
             get{return   this.mesh;}
            }
        }

        [Serializable]
        public class Halfedge
        {
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge edge;
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face face;
            [SerializeField]
            private int index;
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge nextHalfedge;
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge oppositeHalfedge;
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge previousHalfedge;
            public THalfedgeTraits Traits;
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex vertex;

            internal Halfedge()
            {
            }

            internal Halfedge(THalfedgeTraits halfedgeTraits)
            {
                this.Traits = halfedgeTraits;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge Edge
            {
                get {
                 return   this.edge;
                }
                internal set
                {
                    this.edge = value;
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face Face
            {
                get {
                 return   this.face;
                }
                internal set
                {
                    this.face = value;
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex FromVertex {
             get{return   this.Opposite.ToVertex;}
            }
            public int Index
            {
                get {
                 return   this.index;
                }
                internal set
                {
                    this.index = value;
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Mesh {
              get{  return this.vertex.Mesh;}
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge Next
            {
                get {
                 return   this.nextHalfedge;
                }
                internal set
                {
                    this.nextHalfedge = value;
                }
            }

            public bool OnBoundary {
             get {return   (this.face == null);}
            }
            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge Opposite
            {
                get {
                 return   this.oppositeHalfedge;
                }
                internal set
                {
                    this.oppositeHalfedge = value;
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge Previous
            {
                get {
                 return   this.previousHalfedge;
                }
                internal set
                {
                    this.previousHalfedge = value;
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex ToVertex
            {
                get {
                 return   this.vertex;
                }
                internal set
                {
                    this.vertex = value;
                }
            }
        }

        [Serializable]
        public class HalfedgeCollection : IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge>, IEnumerable
        {
            [SerializeField]
            private readonly GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh;

            internal HalfedgeCollection(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> m)
            {
                this.mesh = m;
            }

            public IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge> GetEnumerator()
            {
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge iteratorVariable0 in this.mesh.halfedges)
                {
                    yield return iteratorVariable0;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
              return   this.GetEnumerator();
            }

            public int Count{
             
                get {return this.mesh.halfedges.Count;}
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge this[int index] {
              get{return   this.mesh.halfedges[index];}
            }

            
        }

        [Serializable]
        public class HalfedgeDynamicTrait<TraitType>
        {
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh;
            [SerializeField]
            private TraitType[] trait;

            public HalfedgeDynamicTrait(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh)
            {
                this.mesh = mesh;
                this.trait = new TraitType[mesh.Halfedges.Count];
            }

            public TraitType this[GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge]
            {
                get
                {
                    TraitType local;
                    if (halfedge.Mesh != this.mesh)
                    {
                        throw new MismatchedMeshException("The dynamic trait is not assigned to the mesh that the halfedge belongs to.");
                    }
                    try
                    {
                        local = this.trait[halfedge.Index];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentOutOfRangeException("Cannot access dynamic trait of a halfedge that wasn't present when the dynamic trait was created.");
                    }
                    return local;
                }
                set
                {
                    if (halfedge.Mesh != this.mesh)
                    {
                        throw new MismatchedMeshException("The dynamic trait is not assigned to the mesh that the halfedge belongs to.");
                    }
                    try
                    {
                        this.trait[halfedge.Index] = value;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentOutOfRangeException("Cannot access dynamic trait of a halfedge that wasn't present when the dynamic trait was created.");
                    }
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Mesh {
              get {return  this.mesh;}
            }
        }

        [Serializable]
        public class Vertex
        {
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge;
            [SerializeField]
            private int index;
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh;
            public TVertexTraits Traits;

            internal Vertex()
            {
            }

            internal Vertex(TVertexTraits vertexTraits)
            {
                this.Traits = vertexTraits;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge FindEdgeTo(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex vertex)
            {
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge in this.Halfedges)
                {
                    if (halfedge.ToVertex == vertex)
                    {
                        return halfedge.Edge;
                    }
                }
                return null;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge FindHalfedgeTo(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face face)
            {
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge in this.Halfedges)
                {
                    if (halfedge.Face == face)
                    {
                        return halfedge;
                    }
                }
                return null;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge FindHalfedgeTo(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex vertex)
            {
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge in this.Halfedges)
                {
                    if (halfedge.ToVertex == vertex)
                    {
                        return halfedge;
                    }
                }
                return null;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge GetEdge(int index)
            {
                int num = 0;
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge edge in this.Edges)
                {
                    if (num == index)
                    {
                        return edge;
                    }
                    num++;
                }
                throw new ArgumentOutOfRangeException("index");
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face GetFace(int index)
            {
                int num = 0;
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face face in this.Faces)
                {
                    if (num == index)
                    {
                        return face;
                    }
                    num++;
                }
                throw new ArgumentOutOfRangeException("index");
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge GetHalfedge(int index)
            {
                int num = 0;
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge in this.Halfedges)
                {
                    if (num == index)
                    {
                        return halfedge;
                    }
                    num++;
                }
                throw new ArgumentOutOfRangeException("index");
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex GetVertex(int index)
            {
                int num = 0;
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex vertex in this.Vertices)
                {
                    if (num == index)
                    {
                        return vertex;
                    }
                    num++;
                }
                throw new ArgumentOutOfRangeException("index");
            }

            public int EdgeCount
            {
                get
                {
                    int num = 0;
                    using (IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge> enumerator = this.Edges.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge current = enumerator.Current;
                            num++;
                        }
                    }
                    return num;
                }
            }

            public IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge> Edges
            {
                get
                {
                    foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge iteratorVariable0 in this.Halfedges)
                    {
                        yield return iteratorVariable0.Edge;
                    }
                }
            }

            public int FaceCount
            {
                get
                {
                    int num = 0;
                    using (IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face> enumerator = this.Faces.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face current = enumerator.Current;
                            num++;
                        }
                    }
                    return num;
                }
            }

            public IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Face> Faces
            {
                get
                {
                    foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge iteratorVariable0 in this.Halfedges)
                    {
                        if (iteratorVariable0.Face != null)
                        {
                            yield return iteratorVariable0.Face;
                        }
                    }
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge Halfedge
            {
                get {
                 return   this.halfedge;
                }
                internal set
                {
                    this.halfedge = value;
                }
            }

            public int HalfedgeCount
            {
                get
                {
                    int num = 0;
                    using (IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge> enumerator = this.Halfedges.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge current = enumerator.Current;
                            num++;
                        }
                    }
                    return num;
                }
            }

            public IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge> Halfedges
            {
                get
                {
                    GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge = this.halfedge;
                    if (halfedge != null)
                    {
                        do
                        {
                            yield return halfedge;
                            halfedge = halfedge.Opposite.Next;
                        }
                        while (halfedge != this.halfedge);
                    }
                }
            }

            public int Index
            {
                get {
                 return   this.index;
                }
                internal set
                {
                    this.index = value;
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Mesh
            {
                get {
                 return   this.mesh;
                }
                internal set
                {
                    this.mesh = value;
                }
            }

            public bool OnBoundary
            {
                get
                {
                    if (this.halfedge == null)
                    {
                        return true;
                    }
                    foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge halfedge in this.Halfedges)
                    {
                        if (halfedge.OnBoundary)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            public int VertexCount
            {
                get
                {
                    int num = 0;
                    using (IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex> enumerator = this.Vertices.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex current = enumerator.Current;
                            num++;
                        }
                    }
                    return num;
                }
            }

            public IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex> Vertices
            {
                get
                {
                    foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Halfedge iteratorVariable0 in this.Halfedges)
                    {
                        yield return iteratorVariable0.ToVertex;
                    }
                }
            }

         

           
        }

        [Serializable]
        public class VertexCollection : IEnumerable<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex>, IEnumerable
        {
            [SerializeField]
            private readonly GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh;

            internal VertexCollection(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> m)
            {
                this.mesh = m;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex Add()
            {
                GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex vertex = new GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex();
                this.mesh.AppendToVertexList(vertex);
                return vertex;
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex Add(TVertexTraits vertexTraits)
            {
                GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex vertex = new GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex(vertexTraits);
                this.mesh.AppendToVertexList(vertex);
                return vertex;
            }

            public IEnumerator<GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex> GetEnumerator()
            {
                foreach (GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex iteratorVariable0 in this.mesh.vertices)
                {
                    yield return iteratorVariable0;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
             return   this.GetEnumerator();
            }
            public int Count{
              get {return   this.mesh.vertices.Count;}
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex this[int index]
            {
                get
                {
                    if (index < this.mesh.vertices.Count)
                    {
                        return this.mesh.vertices[index];
                    }
                    return null;
                }
            }

            
        }

        [Serializable]
        public class VertexDynamicTrait<TraitType>
        {
            [SerializeField]
            private GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh;
            [SerializeField]
            private TraitType[] trait;

            public VertexDynamicTrait(GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> mesh)
            {
                this.mesh = mesh;
                this.trait = new TraitType[mesh.Vertices.Count];
            }

            public TraitType this[GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Vertex vertex]
            {
                get
                {
                    TraitType local;
                    if (vertex.Mesh != this.mesh)
                    {
                        throw new MismatchedMeshException("The dynamic trait is not assigned to the mesh that the vertex belongs to.");
                    }
                    try
                    {
                        local = this.trait[vertex.Index];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentOutOfRangeException("Cannot access dynamic trait of a vertex that wasn't present when the dynamic trait was created.");
                    }
                    return local;
                }
                set
                {
                    if (vertex.Mesh != this.mesh)
                    {
                        throw new MismatchedMeshException("The dynamic trait is not assigned to the mesh that the vertex belongs to.");
                    }
                    try
                    {
                        this.trait[vertex.Index] = value;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentOutOfRangeException("Cannot access dynamic trait of a vertex that wasn't present when the dynamic trait was created.");
                    }
                }
            }

            public GD_Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> Mesh {
              get{return   this.mesh;}
            }
        }
    }
}
