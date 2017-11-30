namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class Subdivide
    {
        public static void subdivide(GDController controller, bool center,  bool selectionOnly=false,  bool smooth=false)
        {
            int[] collection = null;
            Vector3 vector31;
            List<int> list10;
            int num31;
            if (selectionOnly)
            {
                collection = new int[controller.selection.selectedTriangles.Count];
                int index = 0;
                foreach (int num2 in controller.selection.selectedTriangles)
                {
                    collection[index] = (num2 * 3) + 2;
                    index++;
                }
            }
            Mesh mesh = controller.mesh;
            List<int> list = null;
            if (collection != null)
            {
                list = new List<int>(collection);
            }
            List<int> list2 = new List<int>();
            List<int> list3 = new List<int>();
            Vector3[] vertices = mesh.vertices;
            List<List<int>> list4 = new List<List<int>>();
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                list4.Add(new List<int>(mesh.GetTriangles(i)));
                list2.Add(0);
                list3.Add(list4[i].Count);
            }
            int[] triangles = mesh.triangles;
            Vector2[] uv = mesh.uv;
            Vector3[] normals = mesh.normals;
            List<Vector3> list5 = new List<Vector3>(vertices);
            List<int> list6 = new List<int>(triangles);
            List<Vector2> list7 = new List<Vector2>(uv);
            List<Vector3> list8 = new List<Vector3>(normals);
            if (!center)
            {
                for (int j = 2; j < triangles.Length; j += 3)
                {
                    int num5 = 0;
                    int num6 = 0;
                    int num7 = 0;
                    for (int k = 0; k < list3.Count; k++)
                    {
                        if ((j - 2) < (list3[k] + num7))
                        {
                            num5 = k;
                            num6 = (j - 2) - num7;
                            break;
                        }
                        num7 += list3[k];
                    }
                    if ((list == null) || list.Contains(j))
                    {
                        int num9 = triangles[j - 2];
                        int num10 = triangles[j - 1];
                        int num11 = triangles[j];
                        int num12 = j - 2;
                        int num13 = j - 1;
                        int num14 = j;
                        Vector3 vector = vertices[num9];
                        Vector3 vector2 = vertices[num10];
                        Vector3 vector3 = vertices[num11];
                        Vector3 vector4 = normals[num9];
                        Vector3 vector5 = normals[num10];
                        Vector3 vector6 = normals[num11];
                        Vector2 vector7 = uv[num9];
                        Vector2 vector8 = uv[num10];
                        Vector2 vector9 = uv[num11];
                        Vector3 item = (Vector3) ((vector + vector2) / 2f);
                        Vector3 vector11 = (Vector3) ((vector2 + vector3) / 2f);
                        Vector3 vector12 = (Vector3) ((vector + vector3) / 2f);
                        vector31 = (Vector3) ((vector4 + vector5) / 2f);
                        Vector3 normalized = vector31.normalized;
                        vector31 = (Vector3) ((vector5 + vector6) / 2f);
                        Vector3 vector14 = vector31.normalized;
                        vector31 = (Vector3) ((vector4 + vector6) / 2f);
                        Vector3 vector15 = vector31.normalized;
                        Vector2 vector16 = (Vector2) ((vector7 + vector8) / 2f);
                        Vector2 vector17 = (Vector2) ((vector8 + vector9) / 2f);
                        Vector2 vector18 = (Vector2) ((vector7 + vector9) / 2f);
                        int count = list5.Count;
                        int num16 = list5.Count + 1;
                        int num17 = list5.Count + 2;
                        list5.Add(item);
                        list5.Add(vector11);
                        list5.Add(vector12);
                        list8.Add(normalized);
                        list8.Add(vector14);
                        list8.Add(vector15);
                        list7.Add(vector16);
                        list7.Add(vector17);
                        list7.Add(vector18);
                        list6[num12] = num9;
                        list6[num13] = count;
                        list6[num14] = num17;
                        list6.Add(count);
                        list6.Add(num16);
                        list6.Add(num17);
                        list6.Add(count);
                        list6.Add(num10);
                        list6.Add(num16);
                        list6.Add(num17);
                        list6.Add(num16);
                        list6.Add(num11);
                        list4[num5][num6] = num9;
                        list4[num5][num6 + 1] = count;
                        list4[num5][num6 + 2] = num17;
                        list4[num5].Add(count);
                        list4[num5].Add(num16);
                        list4[num5].Add(num17);
                        list4[num5].Add(count);
                        list4[num5].Add(num10);
                        list4[num5].Add(num16);
                        list4[num5].Add(num17);
                        list4[num5].Add(num16);
                        list4[num5].Add(num11);
                        (list10 = list2)[num31 = num5] = list10[num31] + 3;
                    }
                }
            }
            else
            {
                for (int m = 2; m < triangles.Length; m += 3)
                {
                    int num19 = 0;
                    int num20 = 0;
                    int num21 = 0;
                    for (int n = 0; n < list3.Count; n++)
                    {
                        if ((m - 2) < (list3[n] + num21))
                        {
                            num19 = n;
                            num20 = (m - 2) - num21;
                            break;
                        }
                        num21 += list3[n];
                    }
                    if ((list == null) || list.Contains(m))
                    {
                        int num23 = triangles[m - 2];
                        int num24 = triangles[m - 1];
                        int num25 = triangles[m];
                        int num26 = m - 2;
                        int num27 = m - 1;
                        int num28 = m;
                        Vector3 vector19 = vertices[num23];
                        Vector3 vector20 = vertices[num24];
                        Vector3 vector21 = vertices[num25];
                        Vector3 vector22 = normals[num23];
                        Vector3 vector23 = normals[num24];
                        Vector3 vector24 = normals[num25];
                        Vector2 vector25 = uv[num23];
                        Vector2 vector26 = uv[num24];
                        Vector2 vector27 = uv[num25];
                        Vector3 vector28 = (Vector3) (((vector19 + vector20) + vector21) / 3f);
                        vector31 = (Vector3) (((vector22 + vector23) + vector24) / 3f);
                        Vector3 vector29 = vector31.normalized;
                        Vector2 vector30 = (Vector2) (((vector25 + vector26) + vector27) / 3f);
                        int num29 = list5.Count;
                        list5.Add(vector28);
                        list8.Add(vector29);
                        list7.Add(vector30);
                        list6[num26] = num23;
                        list6[num27] = num24;
                        list6[num28] = num29;
                        list6.Add(num29);
                        list6.Add(num24);
                        list6.Add(num25);
                        list6.Add(num23);
                        list6.Add(num29);
                        list6.Add(num25);
                        list4[num19][num20] = num23;
                        list4[num19][num20 + 1] = num24;
                        list4[num19][num20 + 2] = num29;
                        list4[num19].Add(num29);
                        list4[num19].Add(num24);
                        list4[num19].Add(num25);
                        list4[num19].Add(num23);
                        list4[num19].Add(num29);
                        list4[num19].Add(num25);
                        (list10 = list2)[num31 = num19] = list10[num31] + 2;
                    }
                }
            }
            vertices = list5.ToArray();
            normals = list8.ToArray();
            uv = list7.ToArray();
            triangles = list6.ToArray();
            if (vertices.Length <= 0xfde8)
            {
                controller.mesh.vertices = vertices;
                controller.mesh.uv = uv;
                controller.mesh.normals = normals;
                controller.mesh.subMeshCount = list4.Count;
                int submesh = 0;
                foreach (List<int> list9 in list4)
                {
                    controller.mesh.SetTriangles(list9.ToArray(), submesh);
                    submesh++;
                }
                controller.mesh.RecalculateBounds();
                controller._gdmesh = MeshBuildUtil.Build(controller.mesh, null);
                controller.selection.Clear();
            }
        }

        public static void subdivideSelection(GDController controller, bool center)
        {
            subdivide(controller, center, true, false);
        }

        public static void subdivideSelection(GDController controller, bool center, bool smooth)
        {
            subdivide(controller, center, true, smooth);
        }
    }
}

