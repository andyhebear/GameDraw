namespace GameDraw
{
    using System;
    using UnityEngine;

    public class FrustumUtil
    {
        public static Vector3[] frustum;
        public static Matrix4x4 localmatrix;
        private static Plane p = new Plane();
        public static Ray[] rays;

        public static void ApplyMatrix(Matrix4x4 matrix, Matrix4x4 lMatrix)
        {
            localmatrix = lMatrix;
        }

        public static bool ElementInFrustum(GDMesh.Element element)
        {
            if ((frustum != null) && (rays != null))
            {
                foreach (GDMesh.Face face in element.faces)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (PointInFrustum(face.vertices[i]))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool LineInFrustum(GDMesh.Edge e)
        {
            return (PointInFrustum(e.Vertex0) && PointInFrustum(e.Vertex1));
        }

        public static bool LineInFrustum(Vector3 p1, Vector3 p2, Vector3[] frustum, Ray[] rays)
        {
            if (!PointInFrustum(p1, frustum, rays))
            {
                return PointInFrustum(p2, frustum, rays);
            }
            return true;
        }

        public static bool PointInFrustum(GDMesh.Vertex v)
        {
            Vector3 vector = localmatrix.MultiplyPoint3x4(v.Traits.pos);
            return (((Vector3.Dot(vector - rays[0].origin, frustum[0]) <= 0f) && (Vector3.Dot(vector - rays[1].origin, frustum[1]) <= 0f)) && ((Vector3.Dot(vector - rays[2].origin, frustum[2]) <= 0f) && (Vector3.Dot(vector - rays[3].origin, frustum[3]) <= 0f)));
        }

        public static bool PointInFrustum(Vector3 point, Vector3[] frustum, Ray[] rays)
        {
            return (((Vector3.Dot(point - rays[0].origin, frustum[0]) <= 0f) && (Vector3.Dot(point - rays[1].origin, frustum[1]) <= 0f)) && ((Vector3.Dot(point - rays[2].origin, frustum[2]) <= 0f) && (Vector3.Dot(point - rays[3].origin, frustum[3]) <= 0f)));
        }

        public static bool TriInFrustum(GDMesh.Face triangle)
        {
            if ((frustum != null) && (rays != null))
            {
                for (int i = 0; i < 3; i++)
                {
                    if (PointInFrustum(triangle.vertices[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TriInFrustum(Vector3[] triangle, Vector3[] frustum, Ray[] rays)
        {
            if ((frustum != null) && (rays != null))
            {
                for (int i = 0; i < 3; i++)
                {
                    if (PointInFrustum(triangle[i], frustum, rays))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

