namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class SmoothFilter
    {
        public static void laplacianFilter(GDController controller)
        {
            Vector3[] vertices = controller.mesh.vertices;
            int[] triangles = controller.mesh.triangles;
            Vector3[] vectorArray2 = new Vector3[vertices.Length];
            List<Vector3> list = new List<Vector3>();
            float num = 0f;
            float num2 = 0f;
            float num3 = 0f;
            for (int i = 0; i < vertices.Length; i++)
            {
                list = MeshUtil.findAdjacentNeighbors(vertices, triangles, vertices[i]);
                if (list.Count != 0)
                {
                    num = 0f;
                    num2 = 0f;
                    num3 = 0f;
                    for (int j = 0; j < list.Count; j++)
                    {
                        num += list[j].x;
                        num2 += list[j].y;
                        num3 += list[j].z;
                    }
                    vectorArray2[i].x = num / ((float) list.Count);
                    vectorArray2[i].y = num2 / ((float) list.Count);
                    vectorArray2[i].z = num3 / ((float) list.Count);
                }
            }
            controller.mesh.vertices = vectorArray2;
            controller.mesh.RecalculateNormals();
            controller.mesh.RecalculateBounds();
            controller.UpdateCollider();
            controller._gdmesh = MeshBuildUtil.Build(controller.mesh, null);
        }
    }
}

