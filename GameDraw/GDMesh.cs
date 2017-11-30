namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;

    [Serializable]
    public class GDMesh
    {
        public List<Matrix4x4> bindposes;
        [NonSerialized]
        public List<Edge> edges;
        [NonSerialized]
        public List<Element> elements;
        private object elock = new object();
        [NonSerialized]
        public List<Face> faces;
        private object flock = new object();
        public bool isBuilt;
        private List<List<int>> orginalSubmeshes;
        private List<Vector3> orginalVerts;
        private Dictionary<int, int> potential;
        [NonSerialized]
        public SerializableDictionary<int, IndexBuffer> relatedVertices;
        public List<int> RVKeys;
        public List<IndexBuffer> RVVals;
        public static StringBuilder sb = new StringBuilder();
        private List<Vector2> uv;
        public int vertexCount;
        public List<Vertex> vertices;
        private List<Vector3> verts;
        private object vlock = new object();

        public GDMesh(Mesh m)
        {
            this.edges = new List<Edge>(m.vertexCount);
            this.vertices = new List<Vertex>(m.vertexCount);
            this.faces = new List<Face>(m.triangles.Length);
            this.bindposes = new List<Matrix4x4>(m.bindposes.Length);
            this.elements = new List<Element>(m.subMeshCount);
            this.relatedVertices = new SerializableDictionary<int, IndexBuffer>(m.vertexCount);
            this.RVKeys = new List<int>(m.vertexCount);
            this.RVVals = new List<IndexBuffer>(m.vertexCount);
        }

        public Edge AddEdge(Vertex v1, Vertex v2, out int orientation)
        {
            bool flag = false;
            orientation = 0;
            for (int i = 0; i < v1.edges.Count; i++)
            {
                Edge edge = v1.edges[i];
                if ((edge.Vertex0 == v2) || (edge.Vertex1 == v2))
                {
                    flag = true;
                    orientation++;
                    return edge;
                }
            }
            if (flag)
            {
                return null;
            }
            Edge item = new Edge();
            item.Vertex0 = v1;
            item.Vertex1 = v2;
            item.Index = this.edges.Count;
            lock (v1.elSynRoot)
            {
                v1.edges.Add(item);
            }
            lock (v2.elSynRoot)
            {
                v2.edges.Add(item);
            }
            if (this.elock == null)
            {
                this.elock = new object();
            }
            lock (this.elock)
            {
                this.edges.Add(item);
            }
            return item;
        }

        public Face AddTriangle(FaceTrait vt, Vertex v1, Vertex v2, Vertex v3)
        {
            Face item = new Face(vt);
            lock (item.vlSynRoot)
            {
                item.vertices.Add(v1);
            }
            lock (v1.flSynRoot)
            {
                v1.faces.Add(item);
            }
            lock (item.vlSynRoot)
            {
                item.vertices.Add(v2);
            }
            lock (v2.flSynRoot)
            {
                v2.faces.Add(item);
            }
            lock (item.vlSynRoot)
            {
                item.vertices.Add(v3);
            }
            lock (v3.flSynRoot)
            {
                v3.faces.Add(item);
            }
            v1.AddNeighbour(v2);
            v1.AddNeighbour(v3);
            v2.AddNeighbour(v3);
            int orientation = 0;
            Edge edge = this.AddEdge(v1, v2, out orientation);
            if (edge != null)
            {
                lock (item.elSynRoot)
                {
                    item.edges.Add(edge);
                }
                lock (item.eolSynRoot)
                {
                    item.edgeOrientation.Add(orientation);
                }
            }
            Edge edge2 = this.AddEdge(v2, v3, out orientation);
            if (edge2 != null)
            {
                lock (item.elSynRoot)
                {
                    item.edges.Add(edge2);
                }
                lock (item.eolSynRoot)
                {
                    item.edgeOrientation.Add(orientation);
                }
            }
            Edge edge3 = this.AddEdge(v3, v1, out orientation);
            if (edge3 != null)
            {
                lock (item.elSynRoot)
                {
                    item.edges.Add(edge3);
                }
                lock (item.eolSynRoot)
                {
                    item.edgeOrientation.Add(orientation);
                }
            }
            if (edge != null)
            {
                lock (edge.flSynRoot)
                {
                    edge.faces.Add(item);
                }
            }
            if (edge2 != null)
            {
                lock (edge2.flSynRoot)
                {
                    edge2.faces.Add(item);
                }
            }
            if (edge3 != null)
            {
                lock (edge3.flSynRoot)
                {
                    edge3.faces.Add(item);
                }
            }
            if (this.flock == null)
            {
                this.flock = new object();
            }
            lock (this.flock)
            {
                this.faces.Add(item);
            }
            return item;
        }

        public void Cap(GDController controller,  int materialID=-1)
        {
            if (this.potential == null)
            {
                this.potential = new Dictionary<int, int>();
            }
            else
            {
                this.potential.Clear();
            }
            if (this.verts == null)
            {
                this.verts = new List<Vector3>(controller.mesh.vertices);
            }
            else
            {
                this.verts.Clear();
            }
            if (this.uv == null)
            {
                this.uv = new List<Vector2>(controller.mesh.uv);
            }
            else
            {
                this.uv.Clear();
            }
            if (controller.selection.selectedVertices.Count == 3)
            {
                List<List<int>> list = new List<List<int>>(controller.mesh.subMeshCount);
                this.orginalSubmeshes = new List<List<int>>(controller.mesh.subMeshCount);
                for (int i = 0; i < controller.mesh.subMeshCount; i++)
                {
                    list.Add(new List<int>(controller.mesh.GetTriangles(i)));
                    this.orginalSubmeshes.Add(new List<int>(controller.mesh.GetTriangles(i)));
                }
                Vertex vert = controller._gdmesh.relatedVertices[controller.selection.selectedVertices[0]].vert;
                Vertex vertex2 = controller._gdmesh.relatedVertices[controller.selection.selectedVertices[1]].vert;
                Vertex vertex3 = controller._gdmesh.relatedVertices[controller.selection.selectedVertices[2]].vert;
                int key = -1;
                if (materialID == -1)
                {
                    foreach (Face face in vert.faces)
                    {
                        if (this.potential.ContainsKey(face.Traits.subMeshIndex))
                        {
                            Dictionary<int, int> dictionary;
                            int num7;
                            (dictionary = this.potential)[num7 = face.Traits.subMeshIndex] = dictionary[num7] + 1;
                        }
                        else
                        {
                            this.potential.Add(face.Traits.subMeshIndex, 1);
                        }
                    }
                    foreach (Face face2 in vertex2.faces)
                    {
                        if (this.potential.ContainsKey(face2.Traits.subMeshIndex))
                        {
                            Dictionary<int, int> dictionary2;
                            int num8;
                            (dictionary2 = this.potential)[num8 = face2.Traits.subMeshIndex] = dictionary2[num8] + 1;
                        }
                        else
                        {
                            this.potential.Add(face2.Traits.subMeshIndex, 1);
                        }
                    }
                    foreach (Face face3 in vertex3.faces)
                    {
                        if (this.potential.ContainsKey(face3.Traits.subMeshIndex))
                        {
                            Dictionary<int, int> dictionary3;
                            int num9;
                            (dictionary3 = this.potential)[num9 = face3.Traits.subMeshIndex] = dictionary3[num9] + 1;
                        }
                        else
                        {
                            this.potential.Add(face3.Traits.subMeshIndex, 1);
                        }
                    }
                    foreach (KeyValuePair<int, int> pair in this.potential)
                    {
                        if (key == -1)
                        {
                            key = pair.Key;
                        }
                        else if (pair.Value >= this.potential[key])
                        {
                            key = pair.Key;
                        }
                    }
                    if (key == -1)
                    {
                        key = 0;
                    }
                }
                else
                {
                    key = materialID;
                }
                int num3 = controller._gdmesh.relatedVertices[controller.selection.selectedVertices[0]][0];
                int num4 = controller._gdmesh.relatedVertices[controller.selection.selectedVertices[1]][0];
                int num5 = controller._gdmesh.relatedVertices[controller.selection.selectedVertices[2]][0];
                this.verts.Add(this.verts[num3]);
                this.verts.Add(this.verts[num4]);
                this.verts.Add(this.verts[num5]);
                this.uv.Add(this.uv[num3]);
                this.uv.Add(this.uv[num4]);
                this.uv.Add(this.uv[num5]);
                list[key].Add(this.verts.Count - 3);
                list[key].Add(this.verts.Count - 2);
                list[key].Add(this.verts.Count - 1);
                controller.mesh.vertices = this.verts.ToArray();
                controller.mesh.uv = this.uv.ToArray();
                controller.mesh.triangles = null;
                controller.mesh.subMeshCount = 0;
                controller.mesh.subMeshCount = list.Count;
                int submesh = 0;
                foreach (List<int> list2 in list)
                {
                    controller.mesh.SetTriangles(list2.ToArray(), submesh);
                    submesh++;
                }
                controller.mesh.RecalculateBounds();
                controller.mesh.RecalculateNormals();
                controller.UpdateCollider();
            }
        }

        public bool Cut(Face f, Vector3 p1, Vector3 p2, GDController controller)
        {
            List<Vector3> list = new List<Vector3>(controller.mesh.vertices);
            List<List<int>> list2 = new List<List<int>>(controller.mesh.subMeshCount);
            new List<Face>();
            for (int i = 0; i < controller.mesh.subMeshCount; i++)
            {
                list2.Add(new List<int>(controller.mesh.GetTriangles(i)));
            }
            Edge item = f.NearestEdge(p1);
            Edge edge2 = f.NearestEdge(p2);
            if (item == edge2)
            {
                return false;
            }
            Mathfx.NearestPoint(item.Vertex0.Traits.position, item.Vertex1.Traits.position, p1);
            Mathfx.NearestPoint(edge2.Vertex0.Traits.position, edge2.Vertex1.Traits.position, p2);
            int index = f.Vertices.IndexOf(item.Vertex1);
            int num3 = f.Edges.IndexOf(item);
            Vertex vertex = f.OpositeToEdge(item);
            list2[f.Traits.subMeshIndex][f.Traits.subMeshTriID + index] = list.Count - 1;
            if ((f.EdgeOrientation[num3] == 0) || (f.EdgeOrientation[num3] > 1))
            {
                list2[f.Traits.subMeshIndex].Add(controller._gdmesh.relatedVertices[vertex.Traits.hashCode][0]);
                list2[f.Traits.subMeshIndex].Add(list.Count - 1);
                list2[f.Traits.subMeshIndex].Add(controller._gdmesh.relatedVertices[item.Vertex1.Traits.hashCode][0]);
            }
            else if (f.EdgeOrientation[num3] == 1)
            {
                list2[f.Traits.subMeshIndex].Add(controller._gdmesh.relatedVertices[item.Vertex1.Traits.hashCode][0]);
                list2[f.Traits.subMeshIndex].Add(list.Count - 1);
                list2[f.Traits.subMeshIndex].Add(controller._gdmesh.relatedVertices[vertex.Traits.hashCode][0]);
            }
            controller.mesh.triangles = null;
            controller.mesh.subMeshCount = 0;
            controller.mesh.vertices = list.ToArray();
            controller.mesh.subMeshCount = list2.Count;
            int submesh = 0;
            foreach (List<int> list3 in list2)
            {
                controller.mesh.SetTriangles(list3.ToArray(), submesh);
                submesh++;
            }
            controller.mesh.RecalculateBounds();
            controller._gdmesh = MeshBuildUtil.Build(controller.mesh, null);
            return true;
        }

        public void FillDictionary()
        {
            if (((this.RVKeys != null) && (this.RVVals != null)) && ((this.RVKeys.Count == this.RVVals.Count) && (this.RVKeys.Count > 0)))
            {
                if (this.relatedVertices == null)
                {
                    this.relatedVertices = new SerializableDictionary<int, IndexBuffer>();
                }
                else
                {
                    this.relatedVertices.Clear();
                }
                for (int i = 0; i < this.RVKeys.Count; i++)
                {
                    this.relatedVertices.Add(this.RVKeys[i], this.RVVals[i]);
                }
            }
        }

        public static int getHashforVector3(Vector3 vec)
        {
            return (vec.x.ToString() + vec.y.ToString() + vec.z.ToString()).GetHashCode();
        }

        public void Slice(ref List<Edge> SliceEdges, ref List<Vector3> points, GDController controller)
        {
            List<Vector3> verts = new List<Vector3>(controller.mesh.vertices);
            List<Vector2> uvs = new List<Vector2>(controller.mesh.uv);
            List<List<int>> submeshes = new List<List<int>>(controller.mesh.subMeshCount);
            this.orginalVerts = new List<Vector3>(controller.mesh.vertices);
            this.orginalSubmeshes = new List<List<int>>(controller.mesh.subMeshCount);
            new List<Face>();
            for (int i = 0; i < controller.mesh.subMeshCount; i++)
            {
                submeshes.Add(new List<int>(controller.mesh.GetTriangles(i)));
                this.orginalSubmeshes.Add(new List<int>(controller.mesh.GetTriangles(i)));
            }
            for (int j = 0; j < this.faces.Count; j++)
            {
                Face f = this.faces[j];
                List<Edge> list4 = new List<Edge>();
                List<int> list5 = new List<int>();
                foreach (Edge edge in f.Edges)
                {
                    int index = SliceEdges.IndexOf(edge);
                    if (index != -1)
                    {
                        list4.Add(edge);
                        list5.Add(index);
                    }
                }
                if (list4.Count == 2)
                {
                    this.SplitEdge(list4[0], list4[1], f, points[list5[0]], points[list5[1]], ref submeshes, ref verts, ref uvs);
                }
            }
            controller.mesh.triangles = null;
            controller.mesh.subMeshCount = 0;
            controller.mesh.vertices = verts.ToArray();
            controller.mesh.uv = uvs.ToArray();
            controller.mesh.subMeshCount = submeshes.Count;
            int submesh = 0;
            foreach (List<int> list6 in submeshes)
            {
                controller.mesh.SetTriangles(list6.ToArray(), submesh);
                submesh++;
            }
            controller.mesh.RecalculateBounds();
        }

        public void SplitEdge(List<int> el, GDController controller)
        {
            List<Vector3> list = new List<Vector3>(controller.mesh.vertices);
            List<List<int>> list2 = new List<List<int>>(controller.mesh.subMeshCount);
            List<Face> list3 = new List<Face>();
            foreach (int num in el)
            {
                Edge item = controller.GDMesh.Edges[num];
                for (int i = 0; i < controller.mesh.subMeshCount; i++)
                {
                    list2.Add(new List<int>(controller.mesh.GetTriangles(i)));
                }
                VertexTrait trait = new VertexTrait(this.vertices.Count);
                trait.color = (Color) ((item.Vertex0.Traits.color + item.Vertex0.Traits.color) / 2f);
                trait.boneWeight = item.Vertex0.Traits.boneWeight;
                trait.position = (Vector3) ((item.Vertex0.Traits.position + item.Vertex1.Traits.position) / 2f);
                trait.hashCode = getHashforVector3(trait.position);
                Vector3 vector = (Vector3) ((item.Vertex0.Traits.Normal + item.Vertex1.Traits.Normal) / 2f);
                trait.Normal = vector.normalized;
                trait.selectionWeight = (item.Vertex0.Traits.selectionWeight + item.Vertex1.Traits.selectionWeight) / 2f;
                trait.tangent = (Vector4) ((item.Vertex0.Traits.tangent + item.Vertex1.Traits.tangent) / 2f);
                trait.uv = (Vector2) ((item.Vertex0.Traits.uv + item.Vertex1.Traits.uv) / 2f);
                trait.uv1 = (Vector2) ((item.Vertex0.Traits.uv1 + item.Vertex1.Traits.uv1) / 2f);
                trait.uv2 = (Vector2) ((item.Vertex0.Traits.uv2 + item.Vertex1.Traits.uv2) / 2f);
                trait.ID = list.Count;
                list.Add(trait.position);
                new Vertex(trait);
                Debug.Log(item.faces.Count);
                for (int j = 0; j < item.Faces.Count; j++)
                {
                    if (!list3.Contains(item.Faces[j]))
                    {
                        int index = item.Faces[j].Vertices.IndexOf(item.Vertex1);
                        int num5 = item.Faces[j].Edges.IndexOf(item);
                        Vertex vertex = item.Faces[j].OpositeToEdge(item);
                        list2[item.Faces[j].Traits.subMeshIndex][item.Faces[j].Traits.subMeshTriID + index] = list.Count - 1;
                        if ((item.Faces[j].EdgeOrientation[num5] == 0) || (item.Faces[j].EdgeOrientation[num5] > 1))
                        {
                            list2[item.Faces[j].Traits.subMeshIndex].Add(controller._gdmesh.relatedVertices[vertex.Traits.hashCode][0]);
                            list2[item.Faces[j].Traits.subMeshIndex].Add(list.Count - 1);
                            list2[item.Faces[j].Traits.subMeshIndex].Add(controller._gdmesh.relatedVertices[item.Vertex1.Traits.hashCode][0]);
                        }
                        else if (item.Faces[j].EdgeOrientation[num5] == 1)
                        {
                            list2[item.Faces[j].Traits.subMeshIndex].Add(controller._gdmesh.relatedVertices[item.Vertex1.Traits.hashCode][0]);
                            list2[item.Faces[j].Traits.subMeshIndex].Add(list.Count - 1);
                            list2[item.Faces[j].Traits.subMeshIndex].Add(controller._gdmesh.relatedVertices[vertex.Traits.hashCode][0]);
                        }
                        list3.Add(item.Faces[j]);
                    }
                }
            }
            controller.mesh.triangles = null;
            controller.mesh.subMeshCount = 0;
            controller.mesh.vertices = list.ToArray();
            controller.mesh.subMeshCount = list2.Count;
            int submesh = 0;
            foreach (List<int> list4 in list2)
            {
                controller.mesh.SetTriangles(list4.ToArray(), submesh);
                submesh++;
            }
            controller.mesh.RecalculateBounds();
            controller._gdmesh = MeshBuildUtil.Build(controller.mesh, null);
        }

        private void SplitEdge(Edge e1, Face f, ref List<List<int>> submeshes, ref List<Vector3> verts, ref List<Vector2> uvs)
        {
            this.SplitEdge(e1, null, f, Vector3.zero, Vector3.zero, ref submeshes, ref verts, ref uvs);
        }

        private void SplitEdge(Edge e1, Edge e2, Face f, ref List<List<int>> submeshes, ref List<Vector3> verts, ref List<Vector2> uvs)
        {
            this.SplitEdge(e1, e2, f, Vector3.zero, Vector3.zero, ref submeshes, ref verts, ref uvs);
        }

        private void SplitEdge(Edge e1, Face f, Vector3 ep1, List<List<int>> submeshes, List<Vector3> verts, List<Vector2> uvs)
        {
            this.SplitEdge(e1, null, f, ep1, Vector3.zero, ref submeshes, ref verts, ref uvs);
        }

        private void SplitEdge(Edge e1, Edge e2, Face f, Vector3 ep1, ref List<List<int>> submeshes, ref List<Vector3> verts, ref List<Vector2> uvs)
        {
            this.SplitEdge(e1, e2, f, ep1, Vector3.zero, ref submeshes, ref verts, ref uvs);
        }

        private void SplitEdge(Edge e1, Edge e2, Face f, Vector3 ep1, Vector3 ep2, ref List<List<int>> submeshes, ref List<Vector3> verts, ref List<Vector2> uvs)
        {
            if ((e1 != null) && (e1 != e2))
            {
                if (e2 == null)
                {
                    if (ep1 == Vector3.zero)
                    {
                        verts.Add((Vector3) ((e1.Vertex0.Traits.position + e1.Vertex1.Traits.position) / 2f));
                        uvs.Add(uvs[e1.Vertex0.Traits.ID] + ((Vector2) (uvs[e1.Vertex1.Traits.ID] / 2f)));
                    }
                    else
                    {
                        verts.Add(ep1);
                        uvs.Add(Vector2.Lerp(uvs[e1.Vertex0.Traits.ID], uvs[e1.Vertex1.Traits.ID], Vector3.Distance(e1.Vertex0.Traits.pos, ep1) / Vector3.Distance(e1.Vertex0.Traits.pos, e1.Vertex1.Traits.pos)));
                    }
                    int index = f.vertices.IndexOf(e1.Vertex1);
                    int num2 = f.Edges.IndexOf(e1);
                    Vertex vertex = f.OpositeToEdge(e1);
                    submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID + index] = verts.Count - 1;
                    if ((f.EdgeOrientation[num2] == 0) || (f.EdgeOrientation[num2] > 1))
                    {
                        submeshes[f.Traits.subMeshIndex].Add(this.relatedVertices[vertex.Traits.hashCode][0]);
                        submeshes[f.Traits.subMeshIndex].Add(verts.Count - 1);
                        submeshes[f.Traits.subMeshIndex].Add(this.relatedVertices[e1.Vertex1.Traits.hashCode][0]);
                    }
                    else if (f.EdgeOrientation[num2] == 1)
                    {
                        submeshes[f.Traits.subMeshIndex].Add(this.relatedVertices[e1.Vertex1.Traits.hashCode][0]);
                        submeshes[f.Traits.subMeshIndex].Add(verts.Count - 1);
                        submeshes[f.Traits.subMeshIndex].Add(this.relatedVertices[vertex.Traits.hashCode][0]);
                    }
                }
                else
                {
                    if (ep1 == Vector3.zero)
                    {
                        verts.Add((Vector3) ((e1.Vertex0.Traits.position + e1.Vertex1.Traits.position) / 2f));
                        uvs.Add(uvs[e1.Vertex0.Traits.ID] + ((Vector2) (uvs[e1.Vertex1.Traits.ID] / 2f)));
                    }
                    else
                    {
                        verts.Add(ep1);
                        uvs.Add(Vector2.Lerp(uvs[e1.Vertex0.Traits.ID], uvs[e1.Vertex1.Traits.ID], Vector3.Distance(e1.Vertex0.Traits.pos, ep1) / Vector3.Distance(e1.Vertex0.Traits.pos, e1.Vertex1.Traits.pos)));
                    }
                    if (ep2 == Vector3.zero)
                    {
                        verts.Add((Vector3) ((e2.Vertex0.Traits.position + e2.Vertex1.Traits.position) / 2f));
                        uvs.Add(uvs[e2.Vertex0.Traits.ID] + ((Vector2) (uvs[e2.Vertex1.Traits.ID] / 2f)));
                    }
                    else
                    {
                        verts.Add(ep2);
                        uvs.Add(Vector2.Lerp(uvs[e2.Vertex0.Traits.ID], uvs[e2.Vertex1.Traits.ID], Vector3.Distance(e2.Vertex0.Traits.pos, ep2) / Vector3.Distance(e2.Vertex0.Traits.pos, e2.Vertex1.Traits.pos)));
                    }
                    Edge item = null;
                    foreach (Edge edge2 in f.Edges)
                    {
                        if ((edge2 != e1) && (edge2 != e2))
                        {
                            item = edge2;
                            break;
                        }
                    }
                    f.Vertices.IndexOf(item.Vertex0);
                    f.Vertices.IndexOf(item.Vertex1);
                    f.Edges.IndexOf(item);
                    Vertex vertex2 = f.OpositeToEdge(item);
                    int num3 = f.Vertices.IndexOf(vertex2);
                    int num4 = submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID];
                    int num5 = submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID + 1];
                    int num6 = submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID + 2];
                    switch (num3)
                    {
                        case 0:
                            submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID] = num5;
                            submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID + 1] = num6;
                            submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID + 2] = verts.Count - 2;
                            submeshes[f.Traits.subMeshIndex].Add(num6);
                            submeshes[f.Traits.subMeshIndex].Add(verts.Count - 1);
                            submeshes[f.Traits.subMeshIndex].Add(verts.Count - 2);
                            submeshes[f.Traits.subMeshIndex].Add(num4);
                            submeshes[f.Traits.subMeshIndex].Add(verts.Count - 2);
                            submeshes[f.Traits.subMeshIndex].Add(verts.Count - 1);
                            return;

                        case 1:
                            submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID + 2] = verts.Count - 2;
                            submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID] = num5;
                            submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID + 1] = verts.Count - 1;
                            submeshes[f.Traits.subMeshIndex].Add(verts.Count - 1);
                            submeshes[f.Traits.subMeshIndex].Add(num6);
                            submeshes[f.Traits.subMeshIndex].Add(verts.Count - 2);
                            submeshes[f.Traits.subMeshIndex].Add(num6);
                            submeshes[f.Traits.subMeshIndex].Add(num4);
                            submeshes[f.Traits.subMeshIndex].Add(verts.Count - 2);
                            return;

                        case 2:
                            submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID + 2] = verts.Count - 2;
                            submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID] = num6;
                            submeshes[f.Traits.subMeshIndex][f.Traits.subMeshTriID + 1] = verts.Count - 1;
                            submeshes[f.Traits.subMeshIndex].Add(verts.Count - 1);
                            submeshes[f.Traits.subMeshIndex].Add(num5);
                            submeshes[f.Traits.subMeshIndex].Add(verts.Count - 2);
                            submeshes[f.Traits.subMeshIndex].Add(num5);
                            submeshes[f.Traits.subMeshIndex].Add(verts.Count - 1);
                            submeshes[f.Traits.subMeshIndex].Add(num4);
                            break;
                    }
                }
            }
        }

        public void VerifyTopology()
        {
            throw new NotImplementedException();
        }

        public List<Edge> Edges
        {
            get
            {
                return this.edges;
            }
            set
            {
                this.edges = value;
            }
        }

        public List<Element> Elements
        {
            get
            {
                return this.elements;
            }
            set
            {
                this.elements = value;
            }
        }

        public List<Face> Faces
        {
            get
            {
                return this.faces;
            }
            set
            {
                this.faces = value;
            }
        }

        public List<Vertex> Vertices
        {
            get
            {
                return this.vertices;
            }
            set
            {
                this.vertices = value;
            }
        }

        [Serializable]
        public class Edge
        {
            [NonSerialized]
            public List<GDMesh.Face> faces;
            public object flSynRoot;
            public int Index;
            public EdgeTrait Traits;
            public GDMesh.Vertex Vertex0;
            public GDMesh.Vertex Vertex1;

            public Edge()
            {
                this.flSynRoot = new object();
                this.faces = new List<GDMesh.Face>(2);
            }

            public Edge(EdgeTrait trait)
            {
                this.flSynRoot = new object();
                this.faces = new List<GDMesh.Face>(2);
                this.Traits = trait;
            }

            public bool isContained(GDController controller)
            {
                int num = 0;
                List<int> selectedTriangles = controller.selection.selectedTriangles;
                Vector3 vector = controller.mesh.vertices[controller._gdmesh.relatedVertices[this.Vertex0.Traits.hashCode][0]];
                Vector3 vector2 = controller.mesh.vertices[controller._gdmesh.relatedVertices[this.Vertex1.Traits.hashCode][0]];
                foreach (int num2 in selectedTriangles)
                {
                    bool flag = false;
                    bool flag2 = false;
                    GDMesh.Face face = controller._gdmesh.faces[num2];
                    foreach (GDMesh.Vertex vertex in face.vertices)
                    {
                        Vector3 vector3 = controller.mesh.vertices[controller._gdmesh.relatedVertices[vertex.Traits.hashCode][0]];
                        if (vector3 == vector)
                        {
                            flag = true;
                        }
                        else if (vector3 == vector2)
                        {
                            flag2 = true;
                        }
                    }
                    if (flag && flag2)
                    {
                        num++;
                    }
                    if (num >= 2)
                    {
                        return true;
                    }
                }
                return false;
            }

            public GDMesh.Face shareFace(GDMesh.Edge e)
            {
                int index = -1;
                foreach (GDMesh.Face face in this.faces)
                {
                    index = e.faces.IndexOf(face);
                    if (index != -1)
                    {
                        return e.faces[index];
                    }
                }
                return null;
            }

            public Vector3 Center
            {
                get
                {
                    return (Vector3) ((this.Vertex0.Traits.pos + this.Vertex1.Traits.pos) / 2f);
                }
            }

            public List<GDMesh.Face> Faces
            {
                get
                {
                    return this.faces;
                }
                set
                {
                    this.faces = value;
                }
            }

            public bool isBoundary
            {
                get
                {
                    return (this.faces.Count < 2);
                }
            }
        }

        [Serializable]
        public class Element
        {
            [NonSerialized]
            public List<GDMesh.Face> faces;
            public object flSynRoot;
            public int Index;
            public ElementTrait Traits;

            public Element()
            {
                this.faces = new List<GDMesh.Face>();
                this.flSynRoot = new object();
                this.faces = new List<GDMesh.Face>();
            }

            public Element(ElementTrait trait)
            {
                this.faces = new List<GDMesh.Face>();
                this.flSynRoot = new object();
                this.Traits = trait;
            }

            public Element(int count)
            {
                this.faces = new List<GDMesh.Face>();
                this.flSynRoot = new object();
                this.faces = new List<GDMesh.Face>(count);
            }

            public List<GDMesh.Face> Faces
            {
                get
                {
                    return this.faces;
                }
                set
                {
                    this.faces = value;
                }
            }
        }

        [Serializable]
        public class Face
        {
            [NonSerialized]
            public List<int> edgeOrientation = new List<int>(3);
            [NonSerialized]
            public List<GDMesh.Edge> edges = new List<GDMesh.Edge>(3);
            public GameDraw.GDMesh.Element element;
            public object elSynRoot = new object();
            public object eolSynRoot = new object();
            public int quadMatch = -1;
            public FaceTrait Traits;
            public GDMesh.FaceType type;
            [NonSerialized]
            public List<GDMesh.Vertex> vertices = new List<GDMesh.Vertex>(3);
            public object vlSynRoot = new object();

            public Face(FaceTrait trait)
            {
                this.Traits = trait;
            }

            public GDMesh.Edge GetEdge(int index)
            {
                if ((this.edges != null) && (this.edges.Count > index))
                {
                    return this.edges[index];
                }
                return null;
            }

            public GDMesh.Edge NearestEdge(Vector3 point)
            {
                float num = Vector3.Distance(Mathfx.NearestPoint(this.edges[0].Vertex0.Traits.position, this.edges[0].Vertex1.Traits.position, point), point);
                float num2 = Vector3.Distance(Mathfx.NearestPoint(this.edges[1].Vertex0.Traits.position, this.edges[1].Vertex1.Traits.position, point), point);
                float num3 = Vector3.Distance(Mathfx.NearestPoint(this.edges[2].Vertex0.Traits.position, this.edges[2].Vertex1.Traits.position, point), point);
                if (num < num2)
                {
                    if (num < num3)
                    {
                        return this.edges[0];
                    }
                    return this.edges[2];
                }
                if (num2 < num3)
                {
                    return this.edges[1];
                }
                return this.edges[2];
            }

            public GDMesh.Vertex OpositeToEdge(GDMesh.Edge e)
            {
                foreach (GDMesh.Vertex vertex in this.vertices)
                {
                    if ((e.Vertex0 != vertex) && (e.Vertex1 != vertex))
                    {
                        return vertex;
                    }
                }
                return this.vertices[0];
            }

            public Vector3 Center
            {
                get
                {
                    return (Vector3) (((this.vertices[0].Traits.pos + this.vertices[1].Traits.pos) + this.vertices[2].Traits.pos) / 3f);
                }
            }

            public List<int> EdgeOrientation
            {
                get
                {
                    return this.edgeOrientation;
                }
                set
                {
                    this.edgeOrientation = value;
                }
            }

            public List<GDMesh.Edge> Edges
            {
                get
                {
                    return this.edges;
                }
                set
                {
                    this.edges = value;
                }
            }

            public GameDraw.GDMesh.Element Element
            {
                get
                {
                    return this.element;
                }
                set
                {
                    this.element = value;
                    if (!this.element.Faces.Contains(this))
                    {
                        lock (this.element.flSynRoot)
                        {
                            this.element.Faces.Add(this);
                        }
                    }
                }
            }

            public Vector3 Normal
            {
                get
                {
                    return (Vector3) (((this.vertices[0].Traits.Normal + this.vertices[1].Traits.Normal) + this.vertices[2].Traits.Normal) / 3f);
                }
            }

            public bool OnBoundary
            {
                get
                {
                    foreach (GDMesh.Edge edge in this.edges)
                    {
                        if (edge.isBoundary)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            public List<GDMesh.Vertex> Vertices
            {
                get
                {
                    return this.vertices;
                }
                set
                {
                    this.vertices = value;
                }
            }
        }

        public enum FaceType
        {
            Triangle,
            Quad
        }

        public class GDMeshUtils
        {
            private static List<BoneWeight> boneWeights;
            private static List<GDMesh.Edge> borderEdges;
            private static List<GDMesh.Edge> boundaryEdges;
            private static List<int> boundaryOffset;
            private static List<Color> colors;
            private static List<int> faceOffset;
            private static List<GDMesh.Face> faces;
            private static List<GDMesh.Edge> internalEdges;
            private static List<int> newSelection;
            private static List<Vector3> normals;
            private static List<int> oldSelection;
            private static List<List<int>> submeshes;
            private static List<int> submeshIndices;
            private static List<int> submeshOffset;
            private static List<Vector4> tangents;
            private static List<Vector2> uv;
            private static List<Vector2> uv1;
            private static List<Vector2> uv2;
            private static List<Vector3> vertices;

            public static void AssignMaterial(GDController controller, List<int> selectedFaces, Material mat)
            {
                GDMesh.Element item = new GDMesh.Element();
                List<Material> list = new List<Material>(controller.referencedGO.GetComponent<Renderer>().sharedMaterials);
                if (list.Contains(mat))
                {
                    int index = list.IndexOf(mat);
                    if (controller._gdmesh.elements.Count > index)
                    {
                        item = controller._gdmesh.elements[index];
                    }
                    else
                    {
                        list.Add(mat);
                        item.Index = controller._gdmesh.elements.Count;
                        controller._gdmesh.elements.Add(item);
                    }
                }
                else
                {
                    list.Add(mat);
                    item.Index = controller._gdmesh.elements.Count;
                    controller._gdmesh.elements.Add(item);
                }
                foreach (int num2 in selectedFaces)
                {
                    GDMesh.Element element = controller._gdmesh.Faces[num2].element;
                    element.Faces.Remove(controller._gdmesh.Faces[num2]);
                    controller._gdmesh.Faces[num2].element = item;
                    item.Faces.Add(controller._gdmesh.Faces[num2]);
                    if (element.Faces.Count == 0)
                    {
                        int num3 = controller._gdmesh.elements.IndexOf(element);
                        controller._gdmesh.elements.RemoveAt(num3);
                        list.RemoveAt(num3);
                        item.Index--;
                    }
                }
                List<List<int>> list2 = new List<List<int>>();
                foreach (GDMesh.Element element3 in controller._gdmesh.elements)
                {
                    List<int> list3 = new List<int>();
                    foreach (GDMesh.Face face in element3.Faces)
                    {
                        list3.Add(controller.mesh.triangles[face.Traits.ID * 3]);
                        list3.Add(controller.mesh.triangles[(face.Traits.ID * 3) + 1]);
                        list3.Add(controller.mesh.triangles[(face.Traits.ID * 3) + 2]);
                    }
                    list2.Add(list3);
                }
                controller.mesh.triangles = null;
                controller.mesh.subMeshCount = 0;
                controller.mesh.subMeshCount = list2.Count;
                int submesh = 0;
                foreach (List<int> list4 in list2)
                {
                    controller.mesh.SetTriangles(list4.ToArray(), submesh);
                    submesh++;
                }
                if (list.Count > controller._gdmesh.elements.Count)
                {
                    list.RemoveAt(0);
                }
                controller.referencedGO.GetComponent<Renderer>().sharedMaterials = list.ToArray();
                controller._gdmesh = MeshBuildUtil.Build(controller.mesh, null);
                controller.UpdateCollider();
                controller.selection.selectedTriangles.Clear();
            }

            public static void Clone(GDController controller)
            {
                Clone(controller, Vector3.zero, false, false);
            }

            public static void Clone(GDController controller, Vector3 direction,  bool mirror=false,  bool flip=false)
            {
                if (controller.selection.selectionType == MeshSelection.SelectionType.Element)
                {
                    controller.selection.selectedTriangles.Clear();
                    if (controller.selection.selectedElements.Count == 0)
                    {
                        foreach (GDMesh.Element element in controller.GDMesh.elements)
                        {
                            foreach (GDMesh.Face face in element.Faces)
                            {
                                controller.selection.selectedTriangles.Add(face.Traits.ID);
                            }
                        }
                    }
                    else
                    {
                        foreach (int num in controller.selection.selectedElements)
                        {
                            GDMesh.Element element2 = controller.GDMesh.elements[num];
                            foreach (GDMesh.Face face2 in element2.Faces)
                            {
                                controller.selection.selectedTriangles.Add(face2.Traits.ID);
                            }
                        }
                    }
                }
                else if ((controller.selection.selectionType == MeshSelection.SelectionType.Triangle) && (controller.selection.selectedTriangles.Count == 0))
                {
                    foreach (GDMesh.Face face3 in controller.GDMesh.faces)
                    {
                        controller.selection.selectedTriangles.Add(face3.Traits.ID);
                    }
                }
                if ((controller.selection.selectionType == MeshSelection.SelectionType.Triangle) || (controller.selection.selectionType == MeshSelection.SelectionType.Element))
                {
                    Mesh mesh = new Mesh();
                    List<Vector3> list = new List<Vector3>();
                    List<List<int>> list2 = new List<List<int>>();
                    for (int i = 0; i < controller.GDMesh.Elements.Count; i++)
                    {
                        list2.Add(new List<int>());
                    }
                    List<Color> list3 = new List<Color>();
                    List<Vector3> list4 = new List<Vector3>();
                    List<Vector4> list5 = new List<Vector4>();
                    List<Vector2> list6 = new List<Vector2>();
                    List<Vector2> list7 = new List<Vector2>();
                    List<Vector2> list8 = new List<Vector2>();
                    List<BoneWeight> list9 = new List<BoneWeight>();
                    if (controller.selection.selectedTriangles != null)
                    {
                        foreach (int num3 in controller.selection.selectedTriangles)
                        {
                            GDMesh.Face face4 = controller.GDMesh.Faces[num3];
                            Vector3 item = controller.mesh.vertices[controller.mesh.triangles[face4.Traits.ID * 3]];
                            Vector3 vector2 = controller.mesh.vertices[controller.mesh.triangles[(face4.Traits.ID * 3) + 1]];
                            Vector3 vector3 = controller.mesh.vertices[controller.mesh.triangles[(face4.Traits.ID * 3) + 2]];
                            if (!flip)
                            {
                                if (mirror)
                                {
                                    list.Add(new Vector3(item.x * direction.x, item.y * direction.y, item.z * direction.z));
                                }
                                else
                                {
                                    list.Add(item);
                                }
                                list2[face4.element.Index].Add(list.Count - 1);
                                if (mirror)
                                {
                                    list.Add(new Vector3(vector2.x * direction.x, vector2.y * direction.y, vector2.z * direction.z));
                                }
                                else
                                {
                                    list.Add(vector2);
                                }
                                list2[face4.element.Index].Add(list.Count - 1);
                                if (mirror)
                                {
                                    list.Add(new Vector3(vector3.x * direction.x, vector3.y * direction.y, vector3.z * direction.z));
                                }
                                else
                                {
                                    list.Add(vector3);
                                }
                                list2[face4.element.Index].Add(list.Count - 1);
                            }
                            else
                            {
                                if (mirror)
                                {
                                    list.Add(new Vector3(vector3.x * direction.x, vector3.y * direction.y, vector3.z * direction.z));
                                }
                                else
                                {
                                    list.Add(vector3);
                                }
                                list2[face4.element.Index].Add(list.Count - 1);
                                if (mirror)
                                {
                                    list.Add(new Vector3(vector2.x * direction.x, vector2.y * direction.y, vector2.z * direction.z));
                                }
                                else
                                {
                                    list.Add(vector2);
                                }
                                list2[face4.element.Index].Add(list.Count - 1);
                                if (mirror)
                                {
                                    list.Add(new Vector3(item.x * direction.x, item.y * direction.y, item.z * direction.z));
                                }
                                else
                                {
                                    list.Add(item);
                                }
                                list2[face4.element.Index].Add(list.Count - 1);
                            }
                            if ((controller.mesh.uv != null) && (controller.mesh.uv.Length == controller.mesh.vertexCount))
                            {
                                Vector2 vector4 = controller.mesh.uv[controller.mesh.triangles[face4.Traits.ID * 3]];
                                list6.Add(vector4);
                                Vector2 vector5 = controller.mesh.uv[controller.mesh.triangles[(face4.Traits.ID * 3) + 1]];
                                list6.Add(vector5);
                                Vector2 vector6 = controller.mesh.uv[controller.mesh.triangles[(face4.Traits.ID * 3) + 2]];
                                list6.Add(vector6);
                            }
                            if ((controller.mesh.uv2 != null) && (controller.mesh.uv2.Length == controller.mesh.vertexCount))
                            {
                                Vector2 vector7 = controller.mesh.uv2[controller.mesh.triangles[face4.Traits.ID * 3]];
                                list7.Add(vector7);
                                Vector2 vector8 = controller.mesh.uv2[controller.mesh.triangles[(face4.Traits.ID * 3) + 1]];
                                list7.Add(vector8);
                                Vector2 vector9 = controller.mesh.uv2[controller.mesh.triangles[(face4.Traits.ID * 3) + 2]];
                                list7.Add(vector9);
                            }
                            if ((controller.mesh.uv2 != null) && (controller.mesh.uv2.Length == controller.mesh.vertexCount))
                            {
                                Vector2 vector10 = controller.mesh.uv2[controller.mesh.triangles[face4.Traits.ID * 3]];
                                list8.Add(vector10);
                                Vector2 vector11 = controller.mesh.uv2[controller.mesh.triangles[(face4.Traits.ID * 3) + 1]];
                                list8.Add(vector11);
                                Vector2 vector12 = controller.mesh.uv2[controller.mesh.triangles[(face4.Traits.ID * 3) + 2]];
                                list8.Add(vector12);
                            }
                            if ((controller.mesh.colors != null) && (controller.mesh.colors.Length == controller.mesh.vertexCount))
                            {
                                Color color = controller.mesh.colors[controller.mesh.triangles[face4.Traits.ID * 3]];
                                list3.Add(color);
                                Color color2 = controller.mesh.colors[controller.mesh.triangles[(face4.Traits.ID * 3) + 1]];
                                list3.Add(color2);
                                Color color3 = controller.mesh.colors[controller.mesh.triangles[(face4.Traits.ID * 3) + 2]];
                                list3.Add(color3);
                            }
                            if ((controller.mesh.normals != null) && (controller.mesh.normals.Length == controller.mesh.vertexCount))
                            {
                                Vector3 vector13 = controller.mesh.normals[controller.mesh.triangles[face4.Traits.ID * 3]];
                                list4.Add(vector13);
                                Vector3 vector14 = controller.mesh.normals[controller.mesh.triangles[(face4.Traits.ID * 3) + 1]];
                                list4.Add(vector14);
                                Vector3 vector15 = controller.mesh.normals[controller.mesh.triangles[(face4.Traits.ID * 3) + 2]];
                                list4.Add(vector15);
                            }
                            if ((controller.mesh.tangents != null) && (controller.mesh.tangents.Length == controller.mesh.vertexCount))
                            {
                                Vector4 vector16 = controller.mesh.tangents[controller.mesh.triangles[face4.Traits.ID * 3]];
                                list5.Add(vector16);
                                Vector4 vector17 = controller.mesh.tangents[controller.mesh.triangles[(face4.Traits.ID * 3) + 1]];
                                list5.Add(vector17);
                                Vector4 vector18 = controller.mesh.tangents[controller.mesh.triangles[(face4.Traits.ID * 3) + 2]];
                                list5.Add(vector18);
                            }
                            if ((controller.mesh.boneWeights != null) && (controller.mesh.boneWeights.Length == controller.mesh.vertexCount))
                            {
                                BoneWeight weight = controller.mesh.boneWeights[controller.mesh.triangles[face4.Traits.ID * 3]];
                                list9.Add(weight);
                                BoneWeight weight2 = controller.mesh.boneWeights[controller.mesh.triangles[(face4.Traits.ID * 3) + 1]];
                                list9.Add(weight2);
                                BoneWeight weight3 = controller.mesh.boneWeights[controller.mesh.triangles[(face4.Traits.ID * 3) + 2]];
                                list9.Add(weight3);
                            }
                        }
                        mesh.triangles = null;
                        mesh.subMeshCount = 0;
                        mesh.vertices = list.ToArray();
                        mesh.normals = list4.ToArray();
                        mesh.tangents = list5.ToArray();
                        mesh.colors = list3.ToArray();
                        mesh.uv = list6.ToArray();
                        mesh.uv2 = list7.ToArray();
                        mesh.uv2 = list8.ToArray();
                        mesh.boneWeights = list9.ToArray();
                        mesh.bindposes = controller.mesh.bindposes;
                        List<Material> list10 = new List<Material>(controller.renderer.sharedMaterials);
                        mesh.subMeshCount = list2.Count;
                        mesh.name = controller.mesh.name;
                        int submesh = 0;
                        foreach (List<int> list11 in list2)
                        {
                            if (list11.Count > 0)
                            {
                                mesh.SetTriangles(list11.ToArray(), submesh);
                                submesh++;
                            }
                            else
                            {
                                list10.RemoveAt(submesh);
                            }
                        }
                        mesh.subMeshCount = submesh;
                        mesh.RecalculateBounds();
                        GameObject obj2 = new GameObject(controller.mesh.name, new System.Type[] { typeof(MeshFilter), typeof(MeshRenderer) });
                        obj2.transform.position = controller.transform.position;
                        obj2.transform.rotation = controller.transform.rotation;
                        obj2.transform.localScale = controller.transform.localScale;
                        obj2.transform.parent = controller.transform.parent;
                        obj2.GetComponent<MeshFilter>().sharedMesh = mesh;
                        obj2.GetComponent<MeshRenderer>().sharedMaterials = list10.ToArray();
                    }
                }
            }

            public static GDMesh DeleteFaces(GDController controller, List<int> selection)
            {
                GDMesh mesh = controller._gdmesh;
                Mesh mesh2 = controller.mesh;
                Renderer component = controller.referencedGO.GetComponent<Renderer>();
                bool flag = false;
                List<int> list = new List<int>(selection.Count);
                List<List<int>> list2 = new List<List<int>>(mesh2.subMeshCount);
                for (int i = 0; i < mesh2.subMeshCount; i++)
                {
                    list2.Add(new List<int>(mesh2.GetTriangles(i)));
                }
                int num2 = 0;
                for (int j = 0; j < list2.Count; j++)
                {
                    List<int> item = list2[j];
                    for (int n = 0; n < item.Count; n += 3)
                    {
                        for (int num5 = 0; num5 < selection.Count; num5++)
                        {
                            if (((num2 == selection[num5]) && (mesh.Faces.Count > selection[num5])) && (mesh.Faces[selection[num5]] != null))
                            {
                                GDMesh.Face face = mesh.Faces[selection[num5]];
                                item.RemoveAt(n);
                                item.RemoveAt(n);
                                item.RemoveAt(n);
                                n -= 3;
                                list.Add(face.Traits.ID);
                                flag = true;
                                break;
                            }
                        }
                        if (item.Count == 0)
                        {
                            List<Material> list4 = new List<Material>(component.sharedMaterials);
                            list2.Remove(item);
                            list4.RemoveAt(j);
                            component.sharedMaterials = list4.ToArray();
                            j--;
                            num2++;
                            break;
                        }
                        num2++;
                    }
                }
                if (!flag)
                {
                    return mesh;
                }
                List<Vector3> list5 = new List<Vector3>(mesh2.vertices);
                List<Vector3> list6 = new List<Vector3>(mesh2.normals);
                List<Vector2> list7 = new List<Vector2>(mesh2.uv);
                List<Vector2> list8 = new List<Vector2>(mesh2.uv2);
                List<int> list9 = new List<int>(mesh2.vertexCount);
                int num6 = 0;
                int index = 0;
                for (int k = 0; k < mesh2.vertices.Length; k++)
                {
                    bool flag2 = false;
                    foreach (List<int> list10 in list2)
                    {
                        if (list10.Contains(k))
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    if (flag2)
                    {
                        list9.Add(k - num6);
                    }
                    else
                    {
                        list5.RemoveAt(index);
                        if (index < list6.Count)
                        {
                            list6.RemoveAt(index);
                        }
                        if (index < list7.Count)
                        {
                            list7.RemoveAt(index);
                        }
                        if (index < list8.Count)
                        {
                            list8.RemoveAt(index);
                        }
                        index--;
                        num6++;
                        list9.Add(-1);
                    }
                    index++;
                }
                for (int m = 0; m < list2.Count; m++)
                {
                    for (int num10 = 0; num10 < list2[m].Count; num10++)
                    {
                        list2[m][num10] = list9[list2[m][num10]];
                    }
                }
                mesh2.triangles = null;
                mesh2.subMeshCount = 0;
                mesh2.vertices = list5.ToArray();
                mesh2.normals = list6.ToArray();
                mesh2.uv = list7.ToArray();
                mesh2.uv2 = list8.ToArray();
                mesh2.subMeshCount = list2.Count;
                int submesh = 0;
                foreach (List<int> list11 in list2)
                {
                    mesh2.SetTriangles(list11.ToArray(), submesh);
                    submesh++;
                }
                mesh2.RecalculateBounds();
                controller.mesh = mesh2;
                controller.built = false;
                controller.CheckAndBuild(false);
                return controller._gdmesh;
            }

            public static GDMesh DeleteVertices(GDController controller, List<int> selection)
            {
                List<int> list = new List<int>();
                foreach (int num in selection)
                {
                    int item = 0;
                    foreach (GDMesh.Face face in controller._gdmesh.Faces)
                    {
                        foreach (GDMesh.Vertex vertex in face.Vertices)
                        {
                            if ((controller.GDMesh.relatedVertices.ContainsKey(num) && (vertex.Traits.hashCode == controller.GDMesh.relatedVertices[num].vert.Traits.hashCode)) && !list.Contains(item))
                            {
                                list.Add(item);
                            }
                        }
                        item++;
                    }
                }
                return DeleteFaces(controller, list);
            }

            public static void detach(GDController controller)
            {
                Clone(controller);
                if (controller.selection.selectedTriangles.Count > 0)
                {
                    controller.selection.selectedVertices.Clear();
                    controller.selection.selectedEdges.Clear();
                    controller.selection.selectedElements.Clear();
                    DeleteFaces(controller, controller.selection.selectedTriangles);
                    controller.selection.selectedTriangles.Clear();
                    controller.selection.Clear();
                    GDManager.manager.aMesh.Clear();
                }
            }

            public static GDMesh extrude(GDController controller, List<int> selectedFaces)
            {
                if (internalEdges == null)
                {
                    internalEdges = new List<GDMesh.Edge>(selectedFaces.Count * 3);
                }
                else
                {
                    internalEdges.Clear();
                    internalEdges.Capacity = selectedFaces.Count * 3;
                }
                if (boundaryEdges == null)
                {
                    boundaryEdges = new List<GDMesh.Edge>(selectedFaces.Count * 3);
                }
                else
                {
                    boundaryEdges.Clear();
                    boundaryEdges.Capacity = selectedFaces.Count * 3;
                }
                if (borderEdges == null)
                {
                    borderEdges = new List<GDMesh.Edge>(selectedFaces.Count * 3);
                }
                else
                {
                    borderEdges.Clear();
                    borderEdges.Capacity = selectedFaces.Count * 3;
                }
                if (submeshIndices == null)
                {
                    submeshIndices = new List<int>(selectedFaces.Count * 3);
                }
                else
                {
                    submeshIndices.Clear();
                }
                if (submeshOffset == null)
                {
                    submeshOffset = new List<int>(selectedFaces.Count * 3);
                }
                else
                {
                    submeshOffset.Clear();
                }
                if (boundaryOffset == null)
                {
                    boundaryOffset = new List<int>(selectedFaces.Count * 3);
                }
                else
                {
                    boundaryOffset.Clear();
                }
                if (faceOffset == null)
                {
                    faceOffset = new List<int>(selectedFaces.Count * 3);
                }
                else
                {
                    faceOffset.Clear();
                }
                if (faces == null)
                {
                    faces = new List<GDMesh.Face>(selectedFaces.Count * 2);
                }
                else
                {
                    faces.Clear();
                    faces.Capacity = selectedFaces.Count * 2;
                }
                if (submeshes == null)
                {
                    submeshes = new List<List<int>>();
                }
                else
                {
                    submeshes.Clear();
                }
                if (newSelection == null)
                {
                    newSelection = new List<int>(selectedFaces.Count);
                }
                else
                {
                    newSelection.Clear();
                    newSelection.Capacity = selectedFaces.Count;
                }
                if (oldSelection == null)
                {
                    oldSelection = new List<int>(selectedFaces.Count);
                }
                else
                {
                    oldSelection.Clear();
                    oldSelection.Capacity = selectedFaces.Count;
                }
                for (int i = 0; i < controller.mesh.subMeshCount; i++)
                {
                    submeshes.Add(new List<int>(controller.mesh.GetTriangles(i)));
                    submeshOffset.Add(0);
                    boundaryOffset.Add(0);
                    faceOffset.Add(0);
                }
                foreach (int num2 in selectedFaces)
                {
                    foreach (GDMesh.Edge edge in controller._gdmesh.Faces[num2].Edges)
                    {
                        if (edge.isBoundary && !edge.isContained(controller))
                        {
                            if (!boundaryEdges.Contains(edge))
                            {
                                boundaryEdges.Add(edge);
                                if (!borderEdges.Contains(edge))
                                {
                                    borderEdges.Add(edge);
                                    submeshIndices.Add(edge.Faces[0].element.Index);
                                }
                            }
                        }
                        else if (!internalEdges.Contains(edge))
                        {
                            internalEdges.Add(edge);
                            foreach (GDMesh.Face face in edge.Faces)
                            {
                                if (!selectedFaces.Contains(face.Traits.ID) && !borderEdges.Contains(edge))
                                {
                                    borderEdges.Add(edge);
                                    controller.selection.selectedEdges.Add(edge.Index);
                                    submeshIndices.Add(face.element.Index);
                                    break;
                                }
                            }
                        }
                    }
                    faces.Add(controller.GDMesh.Faces[num2]);
                }
                if (vertices == null)
                {
                    vertices = new List<Vector3>(controller.mesh.vertices);
                }
                else
                {
                    vertices.Clear();
                    if (controller.mesh.vertices != null)
                    {
                        vertices.AddRange(controller.mesh.vertices);
                    }
                }
                if (normals == null)
                {
                    normals = new List<Vector3>(controller.mesh.normals);
                }
                else
                {
                    normals.Clear();
                    if (controller.mesh.normals != null)
                    {
                        normals.AddRange(controller.mesh.normals);
                    }
                }
                if (tangents == null)
                {
                    tangents = new List<Vector4>(controller.mesh.tangents);
                }
                else
                {
                    tangents.Clear();
                    if (controller.mesh.tangents != null)
                    {
                        tangents.AddRange(controller.mesh.tangents);
                    }
                }
                if (colors == null)
                {
                    colors = new List<Color>(controller.mesh.colors);
                }
                else
                {
                    colors.Clear();
                    if (controller.mesh.colors != null)
                    {
                        colors.AddRange(controller.mesh.colors);
                    }
                }
                if (uv == null)
                {
                    uv = new List<Vector2>(controller.mesh.uv);
                }
                else
                {
                    uv.Clear();
                    if (controller.mesh.uv != null)
                    {
                        uv.AddRange(controller.mesh.uv);
                    }
                }
                if (uv1 == null)
                {
                    uv1 = new List<Vector2>(controller.mesh.uv2);
                }
                else
                {
                    uv1.Clear();
                    if (controller.mesh.uv2 != null)
                    {
                        uv1.AddRange(controller.mesh.uv2);
                    }
                }
                if (uv2 == null)
                {
                    uv2 = new List<Vector2>(controller.mesh.uv2);
                }
                else
                {
                    uv2.Clear();
                    if (controller.mesh.uv2 != null)
                    {
                        uv2.AddRange(controller.mesh.uv2);
                    }
                }
                if (boneWeights == null)
                {
                    boneWeights = new List<BoneWeight>(controller.mesh.boneWeights);
                }
                else
                {
                    boneWeights.Clear();
                    if (controller.mesh.boneWeights != null)
                    {
                        boneWeights.AddRange(controller.mesh.boneWeights);
                    }
                }
                int num3 = 0;
                foreach (GDMesh.Edge edge2 in borderEdges)
                {
                    List<int> list2;
                    int num20;
                    List<int> list3;
                    int num21;
                    int num4 = -1;
                    foreach (GDMesh.Face face2 in faces)
                    {
                        int index = face2.Edges.IndexOf(edge2);
                        if (index != -1)
                        {
                            num4 = face2.EdgeOrientation[index];
                        }
                        if (num4 != -1)
                        {
                            break;
                        }
                    }
                    Vector3 vector = controller.mesh.vertices[controller._gdmesh.relatedVertices[edge2.Vertex0.Traits.hashCode][0]];
                    Vector3 item = new Vector3(vector.x, vector.y, vector.z);
                    vertices.Add(item);
                    Vector3 vector3 = controller.mesh.vertices[controller._gdmesh.relatedVertices[edge2.Vertex1.Traits.hashCode][0]];
                    Vector3 vector4 = new Vector3(vector3.x, vector3.y, vector3.z);
                    vertices.Add(vector4);
                    Vector3 vector5 = new Vector3(vector.x + 1E-05f, vector.y + 1E-05f, vector.z + 1E-05f);
                    vertices.Add(vector5);
                    Vector3 vector6 = new Vector3(vector3.x + 1E-05f, vector3.y + 1E-05f, vector3.z + 1E-05f);
                    vertices.Add(vector6);
                    if (normals.Count > 0)
                    {
                        normals.Add(edge2.Vertex0.Traits.Normal);
                        normals.Add(edge2.Vertex1.Traits.Normal);
                        normals.Add(edge2.Vertex0.Traits.Normal);
                        normals.Add(edge2.Vertex1.Traits.Normal);
                    }
                    if (tangents.Count > 0)
                    {
                        tangents.Add(edge2.Vertex0.Traits.tangent);
                        tangents.Add(edge2.Vertex1.Traits.tangent);
                        tangents.Add(edge2.Vertex0.Traits.tangent);
                        tangents.Add(edge2.Vertex1.Traits.tangent);
                    }
                    if (colors.Count > 0)
                    {
                        colors.Add(edge2.Vertex0.Traits.color);
                        colors.Add(edge2.Vertex1.Traits.color);
                        colors.Add(edge2.Vertex0.Traits.color);
                        colors.Add(edge2.Vertex1.Traits.color);
                    }
                    if (uv.Count > 0)
                    {
                        uv.Add(edge2.Vertex0.Traits.uv);
                        uv.Add(edge2.Vertex1.Traits.uv);
                        uv.Add(edge2.Vertex0.Traits.uv);
                        uv.Add(edge2.Vertex1.Traits.uv);
                    }
                    if (uv1.Count > 0)
                    {
                        uv1.Add(edge2.Vertex0.Traits.uv1);
                        uv1.Add(edge2.Vertex1.Traits.uv1);
                        uv1.Add(edge2.Vertex0.Traits.uv1);
                        uv1.Add(edge2.Vertex1.Traits.uv1);
                    }
                    if (uv2.Count > 0)
                    {
                        uv2.Add(edge2.Vertex0.Traits.uv2);
                        uv2.Add(edge2.Vertex1.Traits.uv2);
                        uv2.Add(edge2.Vertex0.Traits.uv2);
                        uv2.Add(edge2.Vertex1.Traits.uv2);
                    }
                    if (boneWeights.Count > 0)
                    {
                        boneWeights.Add(edge2.Vertex0.Traits.boneWeight);
                        boneWeights.Add(edge2.Vertex1.Traits.boneWeight);
                        boneWeights.Add(edge2.Vertex0.Traits.boneWeight);
                        boneWeights.Add(edge2.Vertex1.Traits.boneWeight);
                    }
                    int num6 = vertices.Count - 4;
                    int num7 = vertices.Count - 3;
                    int num8 = vertices.Count - 2;
                    int num9 = vertices.Count - 1;
                    if ((num4 == 0) || (num4 > 1))
                    {
                        submeshes[submeshIndices[num3]].Add(num7);
                        submeshes[submeshIndices[num3]].Add(num9);
                        submeshes[submeshIndices[num3]].Add(num6);
                        submeshes[submeshIndices[num3]].Add(num6);
                        submeshes[submeshIndices[num3]].Add(num9);
                        submeshes[submeshIndices[num3]].Add(num8);
                    }
                    else if (num4 == 1)
                    {
                        submeshes[submeshIndices[num3]].Add(num6);
                        submeshes[submeshIndices[num3]].Add(num8);
                        submeshes[submeshIndices[num3]].Add(num7);
                        submeshes[submeshIndices[num3]].Add(num8);
                        submeshes[submeshIndices[num3]].Add(num9);
                        submeshes[submeshIndices[num3]].Add(num7);
                    }
                    (list2 = boundaryOffset)[num20 = submeshIndices[num3]] = list2[num20] + 2;
                    (list3 = submeshOffset)[num21 = submeshIndices[num3]] = list3[num21] + 2;
                    num3++;
                }
                foreach (GDMesh.Face face3 in faces)
                {
                    List<int> list4;
                    int num22;
                    List<int> list5;
                    int num23;
                    Vector3 vector7 = controller.mesh.vertices[controller._gdmesh.relatedVertices[face3.Vertices[0].Traits.hashCode][0]];
                    Vector3 vector8 = new Vector3(vector7.x + 1E-05f, vector7.y + 1E-05f, vector7.z + 1E-05f);
                    vertices.Add(vector8);
                    Vector3 vector9 = controller.mesh.vertices[controller._gdmesh.relatedVertices[face3.Vertices[1].Traits.hashCode][0]];
                    Vector3 vector10 = new Vector3(vector9.x + 1E-05f, vector9.y + 1E-05f, vector9.z + 1E-05f);
                    vertices.Add(vector10);
                    Vector3 vector11 = controller.mesh.vertices[controller._gdmesh.relatedVertices[face3.Vertices[2].Traits.hashCode][0]];
                    Vector3 vector12 = new Vector3(vector11.x + 1E-05f, vector11.y + 1E-05f, vector11.z + 1E-05f);
                    vertices.Add(vector12);
                    int num10 = vertices.Count - 3;
                    int num11 = vertices.Count - 2;
                    int num12 = vertices.Count - 1;
                    submeshes[face3.Element.Index].Add(num10);
                    submeshes[face3.Element.Index].Add(num11);
                    submeshes[face3.Element.Index].Add(num12);
                    (list4 = submeshOffset)[num22 = face3.element.Index] = list4[num22] + 1;
                    (list5 = faceOffset)[num23 = face3.element.Index] = list5[num23] + 1;
                    int num13 = 0;
                    for (int j = 0; j <= face3.element.Index; j++)
                    {
                        if (controller.GDMesh.elements.Count == 1)
                        {
                            num13 += submeshes[j].Count;
                        }
                        else if (j == face3.element.Index)
                        {
                            num13 += submeshes[j].Count;
                        }
                    }
                    newSelection.Add(num13 / 3);
                    if (normals.Count > 0)
                    {
                        normals.Add(face3.Vertices[0].Traits.Normal);
                        normals.Add(face3.Vertices[1].Traits.Normal);
                        normals.Add(face3.Vertices[2].Traits.Normal);
                    }
                    if (tangents.Count > 0)
                    {
                        tangents.Add(face3.Vertices[0].Traits.tangent);
                        tangents.Add(face3.Vertices[1].Traits.tangent);
                        tangents.Add(face3.Vertices[2].Traits.tangent);
                    }
                    if (colors.Count > 0)
                    {
                        colors.Add(face3.Vertices[0].Traits.color);
                        colors.Add(face3.Vertices[1].Traits.color);
                        colors.Add(face3.Vertices[2].Traits.color);
                    }
                    if (uv.Count > 0)
                    {
                        uv.Add(face3.Vertices[0].Traits.uv);
                        uv.Add(face3.Vertices[1].Traits.uv);
                        uv.Add(face3.Vertices[2].Traits.uv);
                    }
                    if (uv1.Count > 0)
                    {
                        uv1.Add(face3.Vertices[0].Traits.uv1);
                        uv1.Add(face3.Vertices[1].Traits.uv1);
                        uv1.Add(face3.Vertices[2].Traits.uv1);
                    }
                    if (uv2.Count > 0)
                    {
                        uv2.Add(face3.Vertices[0].Traits.uv2);
                        uv2.Add(face3.Vertices[1].Traits.uv2);
                        uv2.Add(face3.Vertices[2].Traits.uv2);
                    }
                    if (boneWeights.Count > 0)
                    {
                        boneWeights.Add(face3.Vertices[0].Traits.boneWeight);
                        boneWeights.Add(face3.Vertices[1].Traits.boneWeight);
                        boneWeights.Add(face3.Vertices[2].Traits.boneWeight);
                    }
                }
                int submesh = 0;
                foreach (GDMesh.Face face4 in faces)
                {
                    List<int> list6;
                    int num24;
                    int num16 = 0;
                    for (int k = 0; k <= face4.element.Index; k++)
                    {
                        if (controller.GDMesh.elements.Count == 1)
                        {
                            num16 -= faceOffset[k] * 3;
                            num16 -= 3;
                        }
                        else if (k < face4.element.Index)
                        {
                            num16 += submeshes[k].Count - (faceOffset[k] * 3);
                        }
                        else if (k == face4.element.Index)
                        {
                            num16 -= faceOffset[k] * 3;
                            num16 -= 3;
                        }
                    }
                    (list6 = newSelection)[num24 = submesh] = list6[num24] + (num16 / 3);
                    submesh++;
                }
                foreach (int num18 in selectedFaces)
                {
                    GDMesh.Face face5 = controller.GDMesh.Faces[num18];
                    oldSelection.Add(num18);
                    for (int m = 0; m <= face5.element.Index; m++)
                    {
                        if (controller.GDMesh.elements.Count == 1)
                        {
                            break;
                        }
                        if (m > 0)
                        {
                            List<int> list7;
                            int num25;
                            (list7 = oldSelection)[num25 = oldSelection.Count - 1] = list7[num25] + submeshOffset[m - 1];
                        }
                    }
                }
                controller.mesh.triangles = null;
                controller.mesh.subMeshCount = 0;
                controller.mesh.vertices = vertices.ToArray();
                controller.mesh.normals = normals.ToArray();
                controller.mesh.tangents = tangents.ToArray();
                controller.mesh.colors = colors.ToArray();
                controller.mesh.uv = uv.ToArray();
                controller.mesh.uv2 = uv1.ToArray();
                controller.mesh.uv2 = uv2.ToArray();
                controller.mesh.boneWeights = boneWeights.ToArray();
                controller.mesh.subMeshCount = submeshes.Count;
                submesh = 0;
                foreach (List<int> list in submeshes)
                {
                    controller.mesh.SetTriangles(list.ToArray(), submesh);
                    submesh++;
                }
                controller.mesh.RecalculateBounds();
                controller.UpdateCollider();
                controller.built = false;
                controller.CheckAndBuild(false);
                GDMesh mesh = DeleteFaces(controller, oldSelection);
                controller.selection.selectedTriangles.Clear();
                controller.selection.selectedTriangles.AddRange(newSelection);
                return mesh;
            }

            public static void laplacianFilter(GDController controller)
            {
                SmoothFilter.laplacianFilter(controller);
            }

            public static void Mirror(GDController controller, Vector3 direction)
            {
                Clone(controller, direction, true, true);
            }

            public static void MirrorMesh(GDController controller, Vector3 direction)
            {
                Mesh mesh = MeshUtil.CloneMesh(controller.mesh);
                Vector3[] vectorArray = new Vector3[mesh.vertexCount];
                Vector3[] vectorArray2 = new Vector3[mesh.vertexCount];
                for (int i = 0; i < mesh.vertexCount; i++)
                {
                    vectorArray[i] = new Vector3(mesh.vertices[i].x * direction.x, mesh.vertices[i].y * direction.y, mesh.vertices[i].z * direction.z);
                    vectorArray2[i] = new Vector3(mesh.normals[i].x * direction.x, mesh.normals[i].y * direction.y, mesh.normals[i].z * direction.z);
                }
                List<List<int>> list = new List<List<int>>();
                for (int j = 0; j < controller.mesh.subMeshCount; j++)
                {
                    list.Add(new List<int>(controller.mesh.GetTriangles(j)));
                }
                for (int k = 0; k < list.Count; k++)
                {
                    List<int> list2 = list[k];
                    for (int m = 0; m < list2.Count; m += 3)
                    {
                        int num5 = list2[m];
                        list2[m] = list2[m + 2];
                        list2[m + 2] = num5;
                    }
                    list[k] = list2;
                }
                int submesh = 0;
                mesh.subMeshCount = list.Count;
                foreach (List<int> list3 in list)
                {
                    mesh.SetTriangles(list3.ToArray(), submesh);
                    submesh++;
                }
                mesh.vertices = vectorArray;
                mesh.name = controller.mesh.name + "(m)";
                mesh.RecalculateBounds();
                GameObject obj2 = new GameObject(controller.mesh.name, new System.Type[] { typeof(MeshFilter), typeof(MeshRenderer) });
                obj2.transform.position = controller.transform.position;
                obj2.transform.rotation = controller.transform.rotation;
                obj2.transform.localScale = controller.transform.localScale;
                obj2.transform.parent = controller.transform.parent;
                List<Material> list4 = new List<Material>(controller.renderer.sharedMaterials);
                obj2.GetComponent<MeshFilter>().sharedMesh = mesh;
                obj2.GetComponent<MeshRenderer>().sharedMaterials = list4.ToArray();
            }

            public static void RefreshTriangleList(GDController controller)
            {
                Mesh mesh = controller.mesh;
                if (mesh != null)
                {
                    List<List<int>> list = new List<List<int>>(mesh.subMeshCount);
                    for (int i = 0; i < mesh.subMeshCount; i++)
                    {
                        list.Add(new List<int>(mesh.GetTriangles(i)));
                    }
                    mesh.triangles = null;
                    mesh.subMeshCount = 0;
                    mesh.subMeshCount = list.Count;
                    int submesh = 0;
                    foreach (List<int> list2 in list)
                    {
                        mesh.SetTriangles(list2.ToArray(), submesh);
                        submesh++;
                    }
                    mesh.RecalculateBounds();
                }
            }

            public static void SmoothNormals(GDController controller)
            {
                Vector3[] normals = controller.mesh.normals;
                for (int i = 0; i < controller.GDMesh.vertices.Count; i++)
                {
                    List<GDMesh.Face> faces = controller._gdmesh.vertices[i].Faces;
                    Vector3 zero = Vector3.zero;
                    for (int j = 0; j < faces.Count; j++)
                    {
                        zero += faces[j].Normal;
                    }
                    zero = (Vector3) (zero / ((float) faces.Count));
                    for (int k = 0; k < controller._gdmesh.relatedVertices[controller._gdmesh.vertices[i].Traits.hashCode].buffer.Count; k++)
                    {
                        normals[controller._gdmesh.relatedVertices[controller._gdmesh.vertices[i].Traits.hashCode].buffer[k]] = zero.normalized;
                    }
                }
                controller.mesh.normals = normals;
            }

            public static void Weld(GDController controller, float threashold, Vector3 centerPoint, /*[Optional, DefaultParameterValue(2)]*/ WeldType type=WeldType.Destination,  bool center=false)
            {
                List<GDMesh.Vertex> list = new List<GDMesh.Vertex>();
                List<GDMesh.Vertex> list2 = new List<GDMesh.Vertex>();
                foreach (int num in controller.selection.selectedVertices)
                {
                    list.Add(controller.GDMesh.relatedVertices[num].vert);
                }
                GDMesh gDMesh = controller.GDMesh;
                bool flag = false;
                List<Vector3> list3 = new List<Vector3>(controller.mesh.vertices);
                List<List<int>> list4 = new List<List<int>>();
                for (int i = 0; i < controller.mesh.subMeshCount; i++)
                {
                    list4.Add(new List<int>(controller.mesh.GetTriangles(i)));
                }
                for (int j = 0; j < list.Count; j++)
                {
                    GDMesh.Vertex item = list[j];
                    if (item != null)
                    {
                        for (int k = 0; k < list.Count; k++)
                        {
                            GDMesh.Vertex vertex2 = list[k];
                            if ((!list2.Contains(vertex2) && (j != k)) && ((vertex2 != item) && (vertex2 != null)))
                            {
                                switch (type)
                                {
                                    case WeldType.Destination:
                                        if (threashold <= 0f)
                                        {
                                            goto Label_0315;
                                        }
                                        if (Vector3.Distance(vertex2.Traits.position, item.Traits.position) >= threashold)
                                        {
                                            break;
                                        }
                                        item.Neighbours.Remove(vertex2);
                                        vertex2.Neighbours.Remove(item);
                                        if (center)
                                        {
                                            goto Label_0189;
                                        }
                                        item.Traits.position = vertex2.Traits.position;
                                        goto Label_0196;
                                }
                            }
                            continue;
                        Label_0189:
                            item.Traits.position = centerPoint;
                        Label_0196:
                            if (gDMesh.vertices.Contains(vertex2))
                            {
                                if (gDMesh.relatedVertices[item.Traits.hashCode] != null)
                                {
                                    for (int m = 0; m < gDMesh.relatedVertices[item.Traits.hashCode].buffer.Count; m++)
                                    {
                                        list3[gDMesh.relatedVertices[item.Traits.hashCode][m]] = item.Traits.position;
                                    }
                                }
                                if (gDMesh.relatedVertices[vertex2.Traits.hashCode] != null)
                                {
                                    for (int n = 0; n < gDMesh.relatedVertices[vertex2.Traits.hashCode].buffer.Count; n++)
                                    {
                                        int num7 = 0;
                                        for (int num8 = 0; num8 < list4.Count; num8++)
                                        {
                                            List<int> list5 = list4[num8];
                                            for (int num9 = 0; num9 < list5.Count; num9++)
                                            {
                                                int num10 = list5[num9];
                                                if (num10 == gDMesh.relatedVertices[vertex2.Traits.hashCode][n])
                                                {
                                                    list5[num9] = gDMesh.relatedVertices[item.Traits.hashCode][0];
                                                }
                                                num7++;
                                            }
                                        }
                                    }
                                }
                            }
                            flag = true;
                            continue;
                        Label_0315:
                            item.Neighbours.Remove(vertex2);
                            vertex2.Neighbours.Remove(item);
                            if (!center)
                            {
                                item.Traits.position = vertex2.Traits.position;
                            }
                            else
                            {
                                item.Traits.position = centerPoint;
                            }
                            if (gDMesh.vertices.Contains(vertex2))
                            {
                                if (gDMesh.relatedVertices[item.Traits.hashCode] != null)
                                {
                                    for (int num11 = 0; num11 < gDMesh.relatedVertices[item.Traits.hashCode].buffer.Count; num11++)
                                    {
                                        list3[gDMesh.relatedVertices[item.Traits.hashCode][num11]] = item.Traits.position;
                                    }
                                }
                                if (gDMesh.relatedVertices[vertex2.Traits.hashCode] != null)
                                {
                                    for (int num12 = 0; num12 < gDMesh.relatedVertices[vertex2.Traits.hashCode].buffer.Count; num12++)
                                    {
                                        int num13 = 0;
                                        for (int num14 = 0; num14 < list4.Count; num14++)
                                        {
                                            List<int> list6 = list4[num14];
                                            for (int num15 = 0; num15 < list6.Count; num15++)
                                            {
                                                int num16 = list6[num15];
                                                if (num16 == gDMesh.relatedVertices[vertex2.Traits.hashCode][num12])
                                                {
                                                    list6[num15] = gDMesh.relatedVertices[item.Traits.hashCode][0];
                                                }
                                                num13++;
                                            }
                                        }
                                    }
                                }
                            }
                            flag = true;
                        }
                    }
                }
                if (flag)
                {
                    list3.TrimExcess();
                    controller.mesh.vertices = list3.ToArray();
                    controller.mesh.subMeshCount = list4.Count;
                    int submesh = 0;
                    foreach (List<int> list7 in list4)
                    {
                        controller.mesh.SetTriangles(list7.ToArray(), submesh);
                        submesh++;
                    }
                    controller.mesh.RecalculateBounds();
                    controller.selection.Clear();
                }
            }

            public enum WeldType
            {
                Average,
                Target,
                Destination
            }
        }

        [Serializable]
        public class Vertex
        {
            [NonSerialized]
            public List<GDMesh.Edge> edges;
            public object elSynRoot;
            [NonSerialized]
            public List<GDMesh.Face> faces;
            public object flSynRoot;
            [NonSerialized]
            public List<GDMesh.Vertex> neighbours;
            public object nlSynRoot;
            public VertexTrait Traits;

            public Vertex()
            {
                this.flSynRoot = new object();
                this.elSynRoot = new object();
                this.nlSynRoot = new object();
                this.faces = new List<GDMesh.Face>(10);
                this.edges = new List<GDMesh.Edge>(10);
                this.neighbours = new List<GDMesh.Vertex>(10);
            }

            public Vertex(VertexTrait trait)
            {
                this.flSynRoot = new object();
                this.elSynRoot = new object();
                this.nlSynRoot = new object();
                this.faces = new List<GDMesh.Face>(10);
                this.edges = new List<GDMesh.Edge>(10);
                this.neighbours = new List<GDMesh.Vertex>(10);
                this.Traits = trait;
            }

            public void AddNeighbour(GDMesh.Vertex vn)
            {
                if (!this.neighbours.Contains(vn))
                {
                    lock (this.nlSynRoot)
                    {
                        this.neighbours.Add(vn);
                    }
                    lock (vn.nlSynRoot)
                    {
                        vn.neighbours.Add(this);
                    }
                }
            }

            public List<GDMesh.Edge> Edges
            {
                get
                {
                    return this.edges;
                }
                set
                {
                    this.edges = value;
                }
            }

            public List<GDMesh.Face> Faces
            {
                get
                {
                    return this.faces;
                }
                set
                {
                    this.faces = value;
                }
            }

            public List<GDMesh.Vertex> Neighbours
            {
                get
                {
                    return this.neighbours;
                }
                set
                {
                    this.neighbours = value;
                }
            }

            public bool OnBoundary
            {
                get
                {
                    foreach (GDMesh.Edge edge in this.edges)
                    {
                        if (edge.isBoundary)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
        }
    }
}

