namespace GameDraw
{
    using Poly2Tri;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [Serializable]
    public class MeshUtil
    {
        public static void applyOffset(ref Mesh mesh, float xOffset, float yOffset, float zOffset)
        {
            Vector3[] vectorArray = new Vector3[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                Vector3 vector = mesh.vertices[i];
                vector.x += xOffset;
                vector.y += yOffset;
                vector.z += zOffset;
                vectorArray[i] = vector;
            }
            mesh.vertices = vectorArray;
            mesh.RecalculateBounds();
        }

        private static void CalculateTangents(Vector3[] vertex, Vector3[] normal, Vector2[] texcoord, int[] triangle, out Vector4[] tangents)
        {
            tangents = new Vector4[vertex.Length];
            if ((texcoord != null) && (texcoord.Length == vertex.Length))
            {
                long num = triangle.Length / 3;
                int length = vertex.Length;
                tangents = new Vector4[length];
                Vector3[] vectorArray = new Vector3[length];
                Vector3[] vectorArray2 = new Vector3[length];
                for (long i = 0L; i < num; i += 1L)
                {
                    long num4 = triangle[(int) ((IntPtr) (i * 3L))];
                    long num5 = triangle[(int) ((IntPtr) ((i * 3L) + 1L))];
                    long num6 = triangle[(int) ((IntPtr) ((i * 3L) + 2L))];
                    Vector3 vector = vertex[(int) ((IntPtr) num4)];
                    Vector3 vector2 = vertex[(int) ((IntPtr) num5)];
                    Vector3 vector3 = vertex[(int) ((IntPtr) num6)];
                    Vector2 vector4 = texcoord[(int) ((IntPtr) num4)];
                    Vector2 vector5 = texcoord[(int) ((IntPtr) num5)];
                    Vector2 vector6 = texcoord[(int) ((IntPtr) num6)];
                    float num7 = vector2.x - vector.x;
                    float num8 = vector3.x - vector.x;
                    float num9 = vector2.y - vector.y;
                    float num10 = vector3.y - vector.y;
                    float num11 = vector2.z - vector.z;
                    float num12 = vector3.z - vector.z;
                    float num13 = vector5.x - vector4.x;
                    float num14 = vector6.x - vector4.x;
                    float num15 = vector5.y - vector4.y;
                    float num16 = vector6.y - vector4.y;
                    float num17 = 1f / ((num13 * num16) - (num14 * num15));
                    Vector3 vector7 = new Vector3(((num16 * num7) - (num15 * num8)) * num17, ((num16 * num9) - (num15 * num10)) * num17, ((num16 * num11) - (num15 * num12)) * num17);
                    Vector3 vector8 = new Vector3(((num13 * num8) - (num14 * num7)) * num17, ((num13 * num10) - (num14 * num9)) * num17, ((num13 * num12) - (num14 * num11)) * num17);
                    vectorArray[(int) ((IntPtr) num4)] += vector7;
                    vectorArray[(int) ((IntPtr) num5)] += vector7;
                    vectorArray[(int) ((IntPtr) num6)] += vector7;
                    vectorArray2[(int) ((IntPtr) num4)] += vector8;
                    vectorArray2[(int) ((IntPtr) num5)] += vector8;
                    vectorArray2[(int) ((IntPtr) num6)] += vector8;
                }
                for (long j = 0L; j < length; j += 1L)
                {
                    Vector3 lhs = normal[(int) ((IntPtr) j)];
                    Vector3 rhs = vectorArray[(int) ((IntPtr) j)];
                    tangents[(int) ((IntPtr) j)] = (Vector4) (rhs - (lhs * Vector3.Dot(lhs, rhs)));
                    tangents[(int) ((IntPtr) j)].Normalize();
                    tangents[(int) ((IntPtr) j)].w = (Vector3.Dot(Vector3.Cross(lhs, rhs), vectorArray2[(int) ((IntPtr) j)]) < 0f) ? -1f : 1f;
                }
            }
        }

        public static Mesh CloneMesh(Mesh mesh)
        {
            Mesh mesh2 = new Mesh();
            mesh2.vertices = mesh.vertices;
            mesh2.normals = mesh.normals;
            mesh2.tangents = mesh.tangents;
            mesh2.triangles = mesh.triangles;
            mesh2.uv = mesh.uv;
            mesh2.uv2 = mesh.uv2;
            mesh2.uv2 = mesh.uv2;
            mesh2.bindposes = mesh.bindposes;
            mesh2.boneWeights = mesh.boneWeights;
            mesh2.bounds = mesh.bounds;
            mesh2.colors = mesh.colors;
            mesh2.name = mesh.name;
            mesh2.subMeshCount = mesh.subMeshCount;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                mesh2.SetTriangles(mesh.GetTriangles(i), i);
            }
            return mesh2;
        }

        public static Vector3[] CreateNormalsForTriangleList(Vector3[] vertices, int[] indices)
        {
            Vector3[] vectorArray = new Vector3[vertices.Length];
            for (int i = 0; i < indices.Length; i += 3)
            {
                int index = indices[i];
                int num3 = indices[i + 1];
                int num4 = indices[i + 2];
                Vector3 rhs = vertices[num3] - vertices[index];
                Vector3 lhs = vertices[num4] - vertices[index];
                Vector3 vector3 = vertices[index] - vertices[num3];
                Vector3 vector4 = vertices[num4] - vertices[num3];
                Vector3 vector5 = vertices[index] - vertices[num4];
                Vector3 vector6 = vertices[num3] - vertices[num4];
                Vector3 introduced12 = Vector3.Cross(lhs, rhs);
                vectorArray[index] += (Vector3) (introduced12 / (rhs.sqrMagnitude * lhs.sqrMagnitude));
                Vector3 introduced13 = Vector3.Cross(vector3, vector4);
                vectorArray[num3] += (Vector3) (introduced13 / (vector4.sqrMagnitude * vector3.sqrMagnitude));
                Vector3 introduced14 = Vector3.Cross(vector6, vector5);
                vectorArray[num4] += (Vector3) (introduced14 / (vector5.sqrMagnitude * vector6.sqrMagnitude));
            }
            for (int j = 0; j < vertices.Length; j++)
            {
                vectorArray[j].Normalize();
                vectorArray[j] = (Vector3) (vectorArray[j] * -1f);
            }
            return vectorArray;
        }

        public static List<int> findAdjacentNeighborIndexes(Vector3[] v, int[] t, Vector3 vertex)
        {
            List<int> list = new List<int>();
            List<Vector3> adjacentV = new List<Vector3>();
            List<int> list3 = new List<int>();
            int num = 0;
            for (int i = 0; i < v.Length; i++)
            {
                if ((Mathf.Approximately(vertex.x, v[i].x) && Mathf.Approximately(vertex.y, v[i].y)) && Mathf.Approximately(vertex.z, v[i].z))
                {
                    int index = 0;
                    int num4 = 0;
                    bool flag = false;
                    for (int j = 0; j < t.Length; j += 3)
                    {
                        if (!list3.Contains(j))
                        {
                            index = 0;
                            num4 = 0;
                            flag = false;
                            if (i == t[j])
                            {
                                index = t[j + 1];
                                num4 = t[j + 2];
                                flag = true;
                            }
                            if (i == t[j + 1])
                            {
                                index = t[j];
                                num4 = t[j + 2];
                                flag = true;
                            }
                            if (i == t[j + 2])
                            {
                                index = t[j];
                                num4 = t[j + 1];
                                flag = true;
                            }
                            num++;
                            if (flag)
                            {
                                list3.Add(j);
                                if (!isVertexExist(adjacentV, v[index]))
                                {
                                    adjacentV.Add(v[index]);
                                    list.Add(index);
                                }
                                if (!isVertexExist(adjacentV, v[num4]))
                                {
                                    adjacentV.Add(v[num4]);
                                    list.Add(num4);
                                }
                                flag = false;
                            }
                        }
                    }
                }
            }
            return list;
        }

        public static List<Vector3> findAdjacentNeighbors(Vector3[] v, int[] t, Vector3 vertex)
        {
            List<Vector3> adjacentV = new List<Vector3>();
            List<int> list2 = new List<int>();
            int num = 0;
            for (int i = 0; i < v.Length; i++)
            {
                if ((Mathf.Approximately(vertex.x, v[i].x) && Mathf.Approximately(vertex.y, v[i].y)) && Mathf.Approximately(vertex.z, v[i].z))
                {
                    int index = 0;
                    int num4 = 0;
                    bool flag = false;
                    for (int j = 0; j < t.Length; j += 3)
                    {
                        if (!list2.Contains(j))
                        {
                            index = 0;
                            num4 = 0;
                            flag = false;
                            if (i == t[j])
                            {
                                index = t[j + 1];
                                num4 = t[j + 2];
                                flag = true;
                            }
                            if (i == t[j + 1])
                            {
                                index = t[j];
                                num4 = t[j + 2];
                                flag = true;
                            }
                            if (i == t[j + 2])
                            {
                                index = t[j];
                                num4 = t[j + 1];
                                flag = true;
                            }
                            num++;
                            if (flag)
                            {
                                list2.Add(j);
                                if (!isVertexExist(adjacentV, v[index]))
                                {
                                    adjacentV.Add(v[index]);
                                }
                                if (!isVertexExist(adjacentV, v[num4]))
                                {
                                    adjacentV.Add(v[num4]);
                                }
                                flag = false;
                            }
                        }
                    }
                }
            }
            return adjacentV;
        }

        public static Mesh FlipMesh(Mesh mesh, Vector3 direction)
        {
            Mesh mesh2 = CloneMesh(mesh);
            Vector3[] vectorArray = new Vector3[mesh2.vertexCount];
            Vector3[] vectorArray2 = new Vector3[mesh2.vertexCount];
            for (int i = 0; i < mesh2.vertexCount; i++)
            {
                vectorArray[i] = new Vector3(mesh2.vertices[i].x * direction.x, mesh2.vertices[i].y * direction.y, mesh2.vertices[i].z * direction.z);
                vectorArray2[i] = new Vector3(mesh2.normals[i].x * direction.x, mesh2.normals[i].y * direction.y, mesh2.normals[i].z * direction.z);
            }
            List<List<int>> list = new List<List<int>>();
            for (int j = 0; j < mesh.subMeshCount; j++)
            {
                list.Add(new List<int>(mesh.GetTriangles(j)));
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
            mesh2.subMeshCount = list.Count;
            foreach (List<int> list3 in list)
            {
                mesh2.SetTriangles(list3.ToArray(), submesh);
                submesh++;
            }
            mesh2.vertices = vectorArray;
            mesh2.name = mesh.name;
            mesh2.RecalculateNormals();
            mesh2.RecalculateBounds();
            return mesh2;
        }

        public static void FlipNormals(ref Mesh mesh)
        {
            Vector3[] vectorArray = new Vector3[mesh.normals.Length];
            for (int i = 0; i < mesh.normals.Length; i++)
            {
                Vector3 axis = Vector3.Cross(mesh.normals[i], Vector3.up);
                mesh.normals[i] = (Vector3) (Quaternion.AngleAxis(180f, axis) * mesh.normals[i]);
            }
            mesh.normals = vectorArray;
        }

        private static bool isVertexExist(List<Vector3> adjacentV, Vector3 v)
        {
            foreach (Vector3 vector in adjacentV)
            {
                if ((Mathf.Approximately(vector.x, v.x) && Mathf.Approximately(vector.y, v.y)) && Mathf.Approximately(vector.z, v.z))
                {
                    return true;
                }
            }
            return false;
        }

        public static void MakeVerticesUnique(ref Mesh mesh)
        {
            ArrayList list = new ArrayList(mesh.triangles);
            ArrayList list2 = new ArrayList(mesh.vertices);
            for (int i = 0; i < list.Count; i++)
            {
                if (list.IndexOf(list[i], 0, i) != i)
                {
                    list[i] = list2.Add(list2[int.Parse(list[i].ToString())]);
                }
            }
            mesh.vertices = (Vector3[]) list2.ToArray(typeof(Vector3));
            mesh.triangles = (int[]) list.ToArray(typeof(int));
            mesh.RecalculateBounds();
        }

        public static Mesh Merge(Transform trans, out Material[] materials)
        {
            CombineInstance[] combine = new CombineInstance[trans.childCount + 1];
            ArrayList list = new ArrayList();
            MeshFilter component = trans.GetComponent<MeshFilter>();
            if ((component != null) && (component.sharedMesh != null))
            {
                combine[0].mesh = component.sharedMesh;
                combine[0].transform = trans.localToWorldMatrix;
                trans.gameObject.active = false;
                MeshRenderer renderer = trans.GetComponent<MeshRenderer>();
                if ((renderer != null) && (renderer.sharedMaterials != null))
                {
                    list.AddRange(renderer.sharedMaterials);
                }
                for (int i = 0; i < trans.childCount; i++)
                {
                    Transform child = trans.GetChild(i);
                    if (child != null)
                    {
                        component = child.GetComponent<MeshFilter>();
                        if ((component != null) && (component.sharedMesh != null))
                        {
                            MeshRenderer renderer2 = child.GetComponent<MeshRenderer>();
                            if ((renderer2 != null) && (renderer2.sharedMaterials != null))
                            {
                                list.AddRange(renderer2.sharedMaterials);
                            }
                            combine[i + 1].mesh = component.sharedMesh;
                            combine[i + 1].transform = child.localToWorldMatrix;
                            child.gameObject.active = false;
                        }
                    }
                }
            }
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine);
            materials = (Material[]) list.ToArray(typeof(Material));
            trans.gameObject.SetActiveRecursively(true);
            return mesh;
        }

        public static void RecalculateUV(TextureUVType buildingUV, ref Mesh mesh)
        {
            RecalculateUV(buildingUV, ref mesh, false);
        }

        public static void RecalculateUV(TextureUVType buildingUV, ref Mesh mesh, bool isWall)
        {
            int index = 0;
            if (buildingUV == TextureUVType.Planar)
            {
                Vector2[] vectorArray = new Vector2[mesh.vertexCount];
                while (index < vectorArray.Length)
                {
                    if (!isWall)
                    {
                        vectorArray[index] = new Vector2(mesh.vertices[index].x, mesh.vertices[index].z);
                    }
                    else
                    {
                        vectorArray[index] = new Vector2(mesh.vertices[index].x, mesh.vertices[index].y);
                    }
                    index++;
                }
                mesh.uv = vectorArray;
            }
        }

        public static void SmoothNormals(ref Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < vertices.Length; i++)
            {
                for (int j = 0; j < vertices.Length; j++)
                {
                    if (vertices[i] == vertices[j])
                    {
                        normals[i] = (Vector3) ((normals[i] + normals[j]) / 2f);
                        normals[j] = (Vector3) ((normals[i] + normals[j]) / 2f);
                    }
                }
            }
            mesh.normals = normals;
        }

        public static void SmoothNormalsByAngle(Mesh m, int angle)
        {
            int num = 0;
            List<List<int>> list = new List<List<int>>(m.subMeshCount);
            for (int i = 0; i < m.subMeshCount; i++)
            {
                list.Add(new List<int>(m.GetTriangles(i)));
                num += list[i].Count;
            }
            int vertexCount = m.vertexCount;
            List<Vector3> list2 = new List<Vector3>(vertexCount);
            List<Vector2> list3 = new List<Vector2>(vertexCount);
            List<Vector3> list4 = new List<Vector3>(vertexCount);
            new List<Vector3>(vertexCount);
            new List<Vector3>(vertexCount);
            for (int j = 0; j < list.Count; j++)
            {
                List<int> list5 = list[j];
                for (int n = 0; n < list5.Count; n++)
                {
                    Vector3 item = m.vertices[list5[n]];
                    Vector2 vector2 = m.uv[list5[n]];
                    Vector3 rhs = m.normals[list5[n]];
                    int count = list2.Count;
                    int num4 = -1;
                    for (int num8 = 0; num8 < count; num8++)
                    {
                        if (((list2[num8] == item) && (list3[num8] == vector2)) && (Vector3.Dot(list4[num8], rhs) > angle))
                        {
                            num4 = num8;
                            break;
                        }
                    }
                    if (num4 != -1)
                    {
                        list5[n] = num4;
                    }
                    else
                    {
                        list5[n] = count;
                        list2.Add(item);
                        list3.Add(vector2);
                        list4.Add(rhs);
                    }
                }
            }
            m.vertices = list2.ToArray();
            m.normals = list4.ToArray();
            m.uv = list3.ToArray();
            m.subMeshCount = 0;
            m.subMeshCount = list.Count;
            for (int k = 0; k < list.Count; k++)
            {
                m.SetTriangles(list[k].ToArray(), k);
            }
            m.RecalculateNormals();
            m.RecalculateBounds();
        }

        public static int[] SubdivideByTriangleSize(Mesh mesh, float maxTriangleSize)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            List<int> list = new List<int>();
            for (int i = 0; i < (triangles.Length - 2); i += 3)
            {
                if (triangleSize(mesh.vertices[triangles[i]], mesh.vertices[triangles[i + 1]], mesh.vertices[triangles[i + 2]]) > maxTriangleSize)
                {
                    list.Add(i + 2);
                }
            }
            return list.ToArray();
        }

        public static float triangleSize(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            float num = Vector3.Distance(v2, v3);
            float num2 = Vector3.Distance(v3, v1);
            float num3 = Vector3.Distance(v1, v2);
            float num4 = ((num + num3) + num2) / 2f;
            return Mathf.Sqrt(((num4 * (num4 - num)) * (num4 - num3)) * (num4 - num2));
        }

        public static void Triangulate(ArrayList vectors, ref Mesh mesh, bool ceiling, bool twoSided)
        {
            if (vectors.Count >= 3)
            {
                PolygonPoint[] c = new PolygonPoint[vectors.Count];
                Vector3[] vectorArray = new Vector3[vectors.Count];
                Vector2[] vectorArray2 = new Vector2[vectors.Count];
                int[] numArray = null;
                if (twoSided)
                {
                    numArray = new int[vectors.Count * 6];
                }
                else
                {
                    numArray = new int[vectors.Count * 3];
                }
                for (int i = 0; i < vectors.Count; i++)
                {
                    Vector2 vector = (Vector2) vectors[i];
                    c[i] = new PolygonPoint((double) vector.x, (double) vector.y);
                    vectorArray[i] = new Vector3(vector.x, 0f, vector.y);
                    vectorArray2[i] = new Vector2(vector.x, vector.y);
                }
                ArrayList list = new ArrayList(c);
                Polygon p = new Polygon(c);
                P2T.Triangulate(p);
                for (int j = 0; j < p.Triangles.Count; j++)
                {
                    if (twoSided)
                    {
                        numArray[j * 6] = list.IndexOf(p.Triangles[j].Points._2);
                        numArray[(j * 6) + 1] = list.IndexOf(p.Triangles[j].Points._1);
                        numArray[(j * 6) + 2] = list.IndexOf(p.Triangles[j].Points._0);
                        numArray[(j * 6) + 3] = list.IndexOf(p.Triangles[j].Points._0);
                        numArray[(j * 6) + 4] = list.IndexOf(p.Triangles[j].Points._1);
                        numArray[(j * 6) + 5] = list.IndexOf(p.Triangles[j].Points._2);
                    }
                    else if (!ceiling)
                    {
                        numArray[j * 3] = list.IndexOf(p.Triangles[j].Points._2);
                        numArray[(j * 3) + 1] = list.IndexOf(p.Triangles[j].Points._1);
                        numArray[(j * 3) + 2] = list.IndexOf(p.Triangles[j].Points._0);
                    }
                    else
                    {
                        numArray[j * 3] = list.IndexOf(p.Triangles[j].Points._0);
                        numArray[(j * 3) + 1] = list.IndexOf(p.Triangles[j].Points._1);
                        numArray[(j * 3) + 2] = list.IndexOf(p.Triangles[j].Points._2);
                    }
                }
                mesh.vertices = vectorArray;
                mesh.triangles = numArray;
                mesh.uv = vectorArray2;
                mesh.RecalculateBounds();
            }
        }

        public static void TriangulateWithHoles(ArrayList vectors, List<ArrayList> holes, ref Mesh mesh, bool ceiling, bool twoSided)
        {
            TriangulateWithHoles(vectors, holes, ref mesh, ceiling, twoSided, 0f);
        }

        public static void TriangulateWithHoles(ArrayList vectors, List<ArrayList> holes, ref Mesh mesh, bool ceiling, bool twoSided, float y)
        {
            if (vectors.Count >= 3)
            {
                PolygonPoint[] c = new PolygonPoint[vectors.Count];
                List<Vector3> list = new List<Vector3>();
                Vector2[] vectorArray = new Vector2[vectors.Count];
                List<int> list2 = null;
                for (int i = 0; i < vectors.Count; i++)
                {
                    Vector2 vector = (Vector2) vectors[i];
                    c[i] = new PolygonPoint((double) vector.x, (double) vector.y);
                    list.Add(new Vector3(vector.x, y, vector.y));
                    vectorArray[i] = new Vector2(vector.x, vector.y);
                }
                ArrayList list3 = new ArrayList(c);
                Polygon p = new Polygon(c);
                foreach (ArrayList list4 in holes)
                {
                    List<PolygonPoint> list5 = new List<PolygonPoint>();
                    for (int k = 0; k < list4.Count; k++)
                    {
                        Vector2 vector2 = (Vector2) list4[k];
                        list5.Add(new PolygonPoint((double) vector2.x, (double) vector2.y));
                        list.Add(new Vector3(vector2.x, y, vector2.y));
                    }
                    list3.AddRange(list5);
                    p.AddHole(new Polygon(list5));
                }
                P2T.Triangulate(p);
                list2 = new List<int>();
                for (int j = 0; j < p.Triangles.Count; j++)
                {
                    if (twoSided)
                    {
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._2));
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._1));
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._0));
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._0));
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._1));
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._2));
                    }
                    else if (!ceiling)
                    {
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._2));
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._1));
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._0));
                    }
                    else
                    {
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._0));
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._1));
                        list2.Add(list3.IndexOf(p.Triangles[j].Points._2));
                    }
                }
                list2.TrimExcess();
                list.TrimExcess();
                mesh.vertices = list.ToArray();
                mesh.triangles = list2.ToArray();
                mesh.RecalculateBounds();
            }
        }

        public static void UpdateMeshTangents(Mesh mesh)
        {
            Vector4[] vectorArray;
            CalculateTangents(mesh.vertices, mesh.normals, mesh.uv, mesh.triangles, out vectorArray);
            mesh.tangents = vectorArray;
        }

        public enum TextureUVType
        {
            Planar,
            Boxed,
            PerWall,
            PerWallFace
        }
    }
}

