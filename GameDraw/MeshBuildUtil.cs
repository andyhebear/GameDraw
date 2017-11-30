namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public static class MeshBuildUtil
    {
        public static void AddRelatedVertex(GDMesh.Vertex v, GDMesh GDMesh,  bool flag=true)
        {
            if (flag)
            {
                if (!GDMesh.relatedVertices[v.Traits.hashCode].Contains(v.Traits.ID))
                {
                    GDMesh.relatedVertices[v.Traits.hashCode].Add(v.Traits.ID);
                }
            }
            else
            {
                List<int> indices = new List<int>(5);
                indices.Add(v.Traits.ID);
                IndexBuffer buffer = new IndexBuffer(v, indices);
                GDMesh.relatedVertices.Add(v.Traits.hashCode, buffer);
                GDMesh.RVKeys.Add(v.Traits.hashCode);
                GDMesh.RVVals.Add(buffer);
            }
        }

        public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return (Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * 57.29578f);
        }

        public static GDMesh Build(Mesh m,  GDMesh GDMesh=null)
        {
            bool flag = false;
            bool flag2 = (m.normals != null) && (m.normals.Length == m.vertexCount);
            bool flag3 = (m.tangents != null) && (m.tangents.Length == m.vertexCount);
            bool flag4 = (m.colors != null) && (m.colors.Length == m.vertexCount);
            bool flag5 = (m.uv != null) && (m.uv.Length == m.vertexCount);
            if (m.uv2 != null)
            {
                Vector2[] vectorArray1 = m.uv2;
                int vertexCount = m.vertexCount;
            }
            bool flag6 = (m.uv2 != null) && (m.uv2.Length == m.vertexCount);
            bool flag7 = (m.boneWeights != null) && (m.boneWeights.Length == m.vertexCount);
            bool flag8 = (m.bindposes != null) && (m.bindposes.Length > 0);
            Vector3[] vertices = m.vertices;
            Vector3[] normals = m.normals;
            Vector4[] tangents = m.tangents;
            Color[] colors = m.colors;
            Vector2[] uv = m.uv;
            Vector2[] vectorArray6 = m.uv2;
            Vector2[] vectorArray5 = m.uv2;
            BoneWeight[] boneWeights = m.boneWeights;
            int[] triangles = m.triangles;
            if (GDMesh == null)
            {
                flag = true;
                GDMesh = new GDMesh(m);
            }
            GDMesh.vertexCount = m.vertexCount;
            GDMesh.Vertex[] collection = new GDMesh.Vertex[m.vertexCount];
            SerializableDictionary<int, IndexBuffer> relatedVertices = GDMesh.relatedVertices;
            List<GDMesh.Vertex> list = GDMesh.Vertices;
            if (flag)
            {
                int num = m.vertexCount;
                for (int i = 0; i < num; i++)
                {
                    VertexTrait trait = new VertexTrait(i);
                    trait.position = vertices[i];
                    trait.hashCode = GDMesh.getHashforVector3(m.vertices[i]);
                    trait.ID = i;
                    if (flag2)
                    {
                        trait.Normal = normals[i];
                    }
                    if (flag3)
                    {
                        trait.tangent = tangents[i];
                    }
                    if (flag4)
                    {
                        trait.color = colors[i];
                    }
                    if (flag5)
                    {
                        trait.uv = uv[i];
                    }
                    if (flag6)
                    {
                        trait.uv2 = vectorArray5[i];
                    }
                    if (flag7)
                    {
                        trait.boneWeight = boneWeights[i];
                    }
                    GDMesh.Vertex item = new GDMesh.Vertex();
                    item.Traits = trait;
                    if (!relatedVertices.ContainsKey(trait.hashCode))
                    {
                        list.Add(item);
                        AddRelatedVertex(item, GDMesh, false);
                    }
                    else
                    {
                        AddRelatedVertex(item, GDMesh, true);
                    }
                    collection[i] = item;
                }
            }
            else
            {
                if (GDController.vertexCount > 0)
                {
                    collection = new GDMesh.Vertex[GDController.vertexCount];
                }
                int count = list.Count;
                for (int j = 0; j < count; j++)
                {
                    IndexBuffer buffer = relatedVertices[list[j].Traits.hashCode];
                    foreach (int num5 in buffer)
                    {
                        collection[num5] = list[j];
                    }
                }
            }
            if (!flag)
            {
                GDMesh.vertices = new List<GDMesh.Vertex>(collection);
                if (GDMesh.faces == null)
                {
                    GDMesh.faces = new List<GDMesh.Face>();
                }
                GDMesh.faces.Clear();
                if (GDMesh.edges == null)
                {
                    GDMesh.edges = new List<GDMesh.Edge>();
                }
                GDMesh.edges.Clear();
                if (GDMesh.elements == null)
                {
                    GDMesh.elements = new List<GDMesh.Element>();
                }
                GDMesh.elements.Clear();
            }
            if (m.subMeshCount == 0)
            {
                GDMesh.Element element = new GDMesh.Element();
                int length = m.triangles.Length;
                for (int k = 0; k < length; k += 3)
                {
                    FaceTrait vt = new FaceTrait(k);
                    vt.ID = k;
                    vt.subMeshIndex = 0;
                    GDMesh.AddTriangle(vt, relatedVertices[collection[triangles[k]].Traits.hashCode].vert, GDMesh.relatedVertices[collection[triangles[k + 1]].Traits.hashCode].vert, GDMesh.relatedVertices[collection[triangles[k + 2]].Traits.hashCode].vert).Element = element;
                }
                element.Index = 0;
                GDMesh.Elements.Add(element);
            }
            else
            {
                int id = 0;
                int subMeshCount = m.subMeshCount;
                for (int n = 0; n < subMeshCount; n++)
                {
                    GDMesh.Element element2 = new GDMesh.Element();
                    int[] numArray2 = m.GetTriangles(n);
                    int num11 = numArray2.Length;
                    for (int num12 = 0; num12 < num11; num12 += 3)
                    {
                        FaceTrait trait3 = new FaceTrait(id);
                        trait3.ID = id;
                        trait3.subMeshIndex = n;
                        trait3.subMeshTriID = num12;
                        GDMesh.AddTriangle(trait3, relatedVertices[collection[numArray2[num12]].Traits.hashCode].vert, GDMesh.relatedVertices[collection[numArray2[num12 + 1]].Traits.hashCode].vert, GDMesh.relatedVertices[collection[numArray2[num12 + 2]].Traits.hashCode].vert).Element = element2;
                        id++;
                    }
                    element2.Index = n;
                    GDMesh.Elements.Add(element2);
                }
            }
            if (flag8)
            {
                GDMesh.bindposes = new List<Matrix4x4>(m.bindposes);
            }
            Quadrangulate(GDMesh, 90f, 5f, 5f, true, false);
            GDMesh.isBuilt = true;
            return GDMesh;
        }

        public static GDMesh BuildThreaded(Mesh m,  GDMesh GDMesh=null)
        {
            MeshBuildThreaded threaded = new MeshBuildThreaded();
            if (GDMesh != null)
            {
                threaded._gdmesh = GDMesh;
            }
            else
            {
                threaded._gdmesh = null;
            }
            threaded.m = m;
            return threaded.BuildThreaded();
        }

        public static GDMesh Quadrangulate(GDMesh m, float angle=60f,  float maxCorner=10f,  float maxPlanes=10f,  bool rebuild=true, bool extrude=false)
        {
            float num = Mathf.Abs(angle);
            List<int> list = new List<int>(m.faces.Count);
            float num2 = Mathf.Abs((float) (maxPlanes / 360f));
            foreach (GDMesh.Face face in m.faces)
            {
                if ((face.quadMatch == -1) || (rebuild && !list.Contains(face.Traits.ID)))
                {
                    foreach (GDMesh.Edge edge in face.edges)
                    {
                        bool flag2 = false;
                        GDMesh.Vertex vertex = face.OpositeToEdge(edge);
                        Plane plane = new Plane(edge.Vertex0.Traits.pos, edge.Vertex1.Traits.pos, vertex.Traits.pos);
                        foreach (GDMesh.Face face2 in edge.faces)
                        {
                            if ((face2 != face) && (face2.quadMatch == -1))
                            {
                                GDMesh.Vertex vertex2 = face2.OpositeToEdge(edge);
                                Plane plane2 = new Plane(vertex2.Traits.pos, edge.Vertex1.Traits.pos, edge.Vertex0.Traits.pos);
                                float num3 = Vector3.Dot(plane.normal, plane2.normal);
                                float num4 = Mathf.Abs((float) (AngleSigned(vertex.Traits.pos, vertex2.Traits.pos, plane.normal) % 90f));
                                float num5 = 180f - num4;
                                float f = Vector3.Distance(edge.Vertex0.Traits.pos, edge.Vertex1.Traits.pos);
                                float num7 = Vector3.Distance(edge.Vertex0.Traits.pos, vertex.Traits.pos);
                                float num8 = Vector3.Distance(edge.Vertex1.Traits.pos, vertex.Traits.pos);
                                float num9 = 57.29578f * Mathf.Acos(((Mathf.Pow(num8, 2f) + Mathf.Pow(num7, 2f)) - Mathf.Pow(f, 2f)) / ((2f * num8) * num7));
                                float num10 = 57.29578f * Mathf.Acos(((Mathf.Pow(num8, 2f) + Mathf.Pow(f, 2f)) - Mathf.Pow(num7, 2f)) / ((2f * f) * num8));
                                float num11 = 57.29578f * Mathf.Acos(((Mathf.Pow(f, 2f) + Mathf.Pow(num7, 2f)) - Mathf.Pow(num8, 2f)) / ((2f * f) * num7));
                                float num12 = Vector3.Distance(edge.Vertex0.Traits.pos, edge.Vertex1.Traits.pos);
                                float num13 = Vector3.Distance(edge.Vertex0.Traits.pos, vertex2.Traits.pos);
                                float num14 = Vector3.Distance(edge.Vertex1.Traits.pos, vertex2.Traits.pos);
                                float num15 = 57.29578f * Mathf.Acos(((Mathf.Pow(num14, 2f) + Mathf.Pow(num13, 2f)) - Mathf.Pow(num12, 2f)) / ((2f * num14) * num13));
                                float num16 = 57.29578f * Mathf.Acos(((Mathf.Pow(num14, 2f) + Mathf.Pow(num12, 2f)) - Mathf.Pow(num13, 2f)) / ((2f * num12) * num14));
                                float num17 = 57.29578f * Mathf.Acos(((Mathf.Pow(num12, 2f) + Mathf.Pow(num13, 2f)) - Mathf.Pow(num14, 2f)) / ((2f * num12) * num13));
                                bool flag3 = (((num11 + num17) - 90f) < maxCorner) || (((num10 + num16) - 90f) < maxCorner);
                                float num18 = (num16 == 0f) ? 1f : (num11 / num16);
                                bool flag4 = (num18 <= 4f) && (num18 >= 0.25f);
                                float num19 = (num17 == 0f) ? 1f : (num10 / num17);
                                bool flag5 = (num19 <= 4f) && (num19 >= 0.25f);
                                bool flag6 = (((num11 + num17) - 270f) > -maxCorner) || (((num10 + num16) - 270f) > -maxCorner);
                                if (((((num3 >= num2) && (((extrude ? (angle > 0f) : (num9 > 70f)) || (num18 == 1f)) || (num19 == 1f))) && (flag4 && flag5)) && (((num4 <= num) && (num4 >= -num)) || ((num5 <= num) && (num5 >= -num)))) && (flag3 || flag6))
                                {
                                    face.quadMatch = face2.Traits.ID;
                                    face2.quadMatch = face.Traits.ID;
                                    list.Add(face.Traits.ID);
                                    list.Add(face2.Traits.ID);
                                    flag2 = true;
                                    break;
                                }
                            }
                        }
                        if (flag2)
                        {
                            break;
                        }
                    }
                }
            }
            return m;
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
    }
}

