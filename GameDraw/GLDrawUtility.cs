namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class GLDrawUtility
    {
        protected static Rect clippingBounds;
        protected static bool clippingEnabled;
        public static Material Highlight;
        public static Material lineMaterial = null;
        private static Material mat;
        public static Matrix4x4 matrix;
        public static Material TriHighlight = null;

        public static void BeginGroup(Rect position)
        {
            clippingEnabled = true;
            clippingBounds = new Rect(0f, 0f, position.width, position.height);
            GUI.BeginGroup(position);
        }

        protected static bool clip_test(float p, float q, ref float u1, ref float u2)
        {
            float num;
            bool flag = true;
            if (p < 0.0)
            {
                num = q / p;
                if (num > u2)
                {
                    return false;
                }
                if (num > u1)
                {
                    u1 = num;
                }
                return flag;
            }
            if (p > 0.0)
            {
                num = q / p;
                if (num < u1)
                {
                    return false;
                }
                if (num < u2)
                {
                    u2 = num;
                }
                return flag;
            }
            if (q < 0.0)
            {
                flag = false;
            }
            return flag;
        }

        public static void CreateMaterial()
        {
            if (lineMaterial == null)
            {
                lineMaterial = new Material("Shader \"Lines/Colored Blended\" {SubShader { Pass {     Blend SrcAlpha OneMinusSrcAlpha     ZWrite Off Cull Off Fog { Mode Off }     BindChannels {      Bind \"vertex\", vertex Bind \"color\", color }} } }");
            }
        }

        private static Vector2 CubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
        {
            float num = 1f - t;
            float num2 = num * t;
            return (Vector2) ((((((num * num) * num) * s) + (((3f * num) * num2) * st)) + (((3f * num2) * t) * et)) + (((t * t) * t) * e));
        }

        public static void DrawBezier(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width)
        {
            Vector2 vector = start - end;
            int segments = Mathf.FloorToInt(vector.magnitude / 20f) * 3;
            DrawBezier(start, startTangent, end, endTangent, color, width, segments);
        }

        public static void DrawBezier(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, int segments)
        {
            Vector2 vector = CubeBezier(start, startTangent, end, endTangent, 0f);
            for (int i = 1; i < segments; i++)
            {
                Vector2 vector2 = CubeBezier(start, startTangent, end, endTangent, ((float) i) / ((float) segments));
                DrawLine(vector, vector2, color, width);
                vector = vector2;
            }
        }

        public static void DrawBox(Rect box, Color color, float width)
        {
            Vector2 start = new Vector2(box.xMin, box.yMin);
            Vector2 end = new Vector2(box.xMax, box.yMin);
            Vector2 vector3 = new Vector2(box.xMax, box.yMax);
            Vector2 vector4 = new Vector2(box.xMin, box.yMax);
            DrawLine(start, end, color, width);
            DrawLine(end, vector3, color, width);
            DrawLine(vector3, vector4, color, width);
            DrawLine(vector4, start, color, width);
        }

        public static void DrawBox(Vector2 topLeftCorner, Vector2 bottomRightCorner, Color color, float width)
        {
            Rect box = new Rect(topLeftCorner.x, topLeftCorner.y, bottomRightCorner.x - topLeftCorner.x, bottomRightCorner.y - topLeftCorner.y);
            DrawBox(box, color, width);
        }

        public static void DrawGrid(Color color, Color Emission, Color Ambient, Transform target,  Material mat=null)
        {
            Camera current = Camera.current;
            float num = 1f;
            GL.PushMatrix();
            if (mat == null)
            {
                mat = new Material(Shader.Find("GameDraw/Highlight"));
                mat.color = color;
                mat.SetColor("_Color", color);
                mat.SetColor("_Emission", Emission);
                mat.SetColor("_Ambient", Ambient);
            }
            mat.SetPass(0);
            GL.Begin(1);
            for (float i = -100f; i < 100f; i += num)
            {
                Vector3[] vectorArray = new Vector3[] { new Vector3(i, 0f, -100f), new Vector3(i, 0f, 100f) };
                GL.Vertex3(vectorArray[0].x, vectorArray[0].y, vectorArray[0].z);
                GL.Vertex3(vectorArray[1].x, vectorArray[1].y, vectorArray[1].z);
            }
            for (float j = -100f; j < 100f; j += num)
            {
                Vector3[] vectorArray2 = new Vector3[] { new Vector3(-100f, 0f, j), new Vector3(100f, 0f, j) };
                GL.Vertex3(vectorArray2[0].x, vectorArray2[0].y, vectorArray2[0].z);
                GL.Vertex3(vectorArray2[1].x, vectorArray2[1].y, vectorArray2[1].z);
            }
            GL.End();
            GL.PopMatrix();
            GL.InvalidateState();
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
        {
            if (((Event.current != null) && (Event.current.type == EventType.Repaint)) && (!clippingEnabled || segment_rect_intersection(clippingBounds, ref start, ref end)))
            {
                Vector3 vector;
                Vector3 vector2;
                CreateMaterial();
                lineMaterial.SetPass(0);
                GL.Color(color);
                if (width == 1f)
                {
                    GL.Begin(1);
                    vector = new Vector3(start.x, start.y, 0f);
                    vector2 = new Vector3(end.x, end.y, 0f);
                    GL.Vertex(vector);
                    GL.Vertex(vector2);
                }
                else
                {
                    GL.Begin(7);
                    vector = new Vector3(end.y, start.x, 0f);
                    vector2 = new Vector3(start.y, end.x, 0f);
                    Vector3 vector6 = vector - vector2;
                    Vector3 vector3 = (Vector3) (vector6.normalized * width);
                    Vector3 vector4 = new Vector3(start.x, start.y, 0f);
                    Vector3 vector5 = new Vector3(end.x, end.y, 0f);
                    GL.Vertex(vector4 - vector3);
                    GL.Vertex(vector4 + vector3);
                    GL.Vertex(vector5 + vector3);
                    GL.Vertex(vector5 - vector3);
                }
                GL.End();
            }
        }

        public static void DrawLine(Color color, Color Emission, Color Ambient, Vector3[] linePoints, bool linear=false)
        {
            if (linePoints.Length >= 2)
            {
                GL.PushMatrix();
                if (mat == null)
                {
                    mat = new Material(Shader.Find("GameDraw/Highlight"));
                    mat.color = color;
                    mat.SetColor("_Color", color);
                    mat.SetColor("_Emission", Emission);
                    mat.SetColor("_Ambient", Ambient);
                    mat.SetPass(0);
                }
                GL.Begin(1);
                if (!linear)
                {
                    for (int i = 0; i < linePoints.Length; i += 2)
                    {
                        Vector3[] vectorArray = new Vector3[] { linePoints[i], linePoints[i + 1] };
                        GL.Vertex3(vectorArray[0].x, vectorArray[0].y, vectorArray[0].z);
                        GL.Vertex3(vectorArray[1].x, vectorArray[1].y, vectorArray[1].z);
                    }
                }
                else
                {
                    for (int j = 0; j < (linePoints.Length - 1); j++)
                    {
                        Vector3[] vectorArray2 = new Vector3[] { linePoints[j], linePoints[j + 1] };
                        GL.Vertex3(vectorArray2[0].x, vectorArray2[0].y, vectorArray2[0].z);
                        GL.Vertex3(vectorArray2[1].x, vectorArray2[1].y, vectorArray2[1].z);
                    }
                }
                GL.End();
                GL.PopMatrix();
                GL.InvalidateState();
            }
        }

        public static void DrawQuad(Color color, Color Emission, Color Ambient, Vector3 vec1, Vector3 vec2, Vector3 vec3, Vector3 vec4)
        {
            GL.PushMatrix();
            if (mat == null)
            {
                mat = new Material(Shader.Find("GameDraw/Highlight"));
                mat.color = color;
                mat.SetColor("_Color", color);
                mat.SetColor("_Emission", Emission);
                mat.SetColor("_Ambient", Ambient);
            }
            GL.Begin(7);
            GL.Vertex3(vec1.x, vec1.y, vec1.z);
            GL.Vertex3(vec2.x, vec2.y, vec2.z);
            GL.Vertex3(vec3.x, vec3.y, vec3.z);
            GL.Vertex3(vec4.x, vec4.y, vec4.z);
            GL.End();
            GL.PopMatrix();
            GL.InvalidateState();
        }

        public static void DrawRoundedBox(Rect box, float radius, Color color, float width)
        {
            Vector2 start = new Vector2(box.xMin + radius, box.yMin);
            Vector2 end = new Vector2(box.xMax - radius, box.yMin);
            Vector2 vector3 = new Vector2(box.xMax, box.yMin + radius);
            Vector2 vector4 = new Vector2(box.xMax, box.yMax - radius);
            Vector2 vector5 = new Vector2(box.xMax - radius, box.yMax);
            Vector2 vector6 = new Vector2(box.xMin + radius, box.yMax);
            Vector2 vector7 = new Vector2(box.xMin, box.yMax - radius);
            Vector2 vector8 = new Vector2(box.xMin, box.yMin + radius);
            DrawLine(start, end, color, width);
            DrawLine(vector3, vector4, color, width);
            DrawLine(vector5, vector6, color, width);
            DrawLine(vector7, vector8, color, width);
            float num = radius / 2f;
            Vector2 startTangent = new Vector2(vector8.x, vector8.y + num);
            Vector2 endTangent = new Vector2(start.x - num, start.y);
            DrawBezier(vector8, startTangent, start, endTangent, color, width);
            startTangent = new Vector2(end.x + num, end.y);
            endTangent = new Vector2(vector3.x, vector3.y - num);
            DrawBezier(end, startTangent, vector3, endTangent, color, width);
            startTangent = new Vector2(vector4.x, vector4.y + num);
            endTangent = new Vector2(vector5.x + num, vector5.y);
            DrawBezier(vector4, startTangent, vector5, endTangent, color, width);
            startTangent = new Vector2(vector6.x - num, vector6.y);
            endTangent = new Vector2(vector7.x, vector7.y + num);
            DrawBezier(vector6, startTangent, vector7, endTangent, color, width);
        }

        public static void DrawTriangle(Color color, Color Emission, Color Ambient, params Vector3[] triangles)
        {
            if (triangles.Length >= 3)
            {
                GL.PushMatrix();
                if (mat == null)
                {
                    mat = new Material(Shader.Find("GameDraw/HighlightPlusNoCull"));
                    mat.color = color;
                    mat.SetColor("_Color", color);
                    mat.SetColor("_Emission", Emission);
                    mat.SetColor("_Ambient", Ambient);
                }
                else
                {
                    mat.SetColor("_Color", color);
                }
                mat.SetPass(0);
                GL.Begin(4);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3[] vectorArray = new Vector3[] { triangles[i], triangles[i + 1], triangles[i + 2] };
                    GL.Vertex3(vectorArray[0].x, vectorArray[0].y, vectorArray[0].z);
                    GL.Vertex3(vectorArray[1].x, vectorArray[1].y, vectorArray[1].z);
                    GL.Vertex3(vectorArray[2].x, vectorArray[2].y, vectorArray[2].z);
                }
                GL.End();
                GL.PopMatrix();
                GL.InvalidateState();
            }
        }

        public static void DrawTriangle(Color color, Color Emission, Color Ambient, List<int> triangles, GDController controller)
        {
            if (triangles.Count >= 3)
            {
                GL.PushMatrix();
                if (TriHighlight == null)
                {
                    TriHighlight = new Material(Shader.Find("GameDraw/Highlight"));
                    TriHighlight.color = color;
                    TriHighlight.SetColor("_Color", color);
                    TriHighlight.SetColor("_Emission", Emission);
                    TriHighlight.SetColor("_Ambient", Ambient);
                }
                else
                {
                    TriHighlight.SetColor("_Color", color);
                }
                TriHighlight.SetPass(0);
                GL.modelview = matrix;
                GL.Begin(4);
                for (int i = 0; i < triangles.Count; i++)
                {
                    foreach (GDMesh.Vertex vertex in controller._gdmesh.faces[triangles[i]].Vertices)
                    {
                        Vector3 vector = controller.mesh.vertices[controller._gdmesh.relatedVertices[vertex.Traits.hashCode][0]];
                        GL.Vertex3(vector.x, vector.y, vector.z);
                    }
                }
                GL.End();
                GL.PopMatrix();
                GL.InvalidateState();
            }
        }

        public static void DrawTriangle(Color color, Color Emission, Color Ambient, Vector3 vec1, Vector3 vec2, Vector3 vec3)
        {
            GL.PushMatrix();
            if (mat == null)
            {
                mat = new Material(Shader.Find("GameDraw/Highlight"));
                mat.color = color;
                mat.SetColor("_Color", color);
                mat.SetColor("_Emission", Emission);
                mat.SetColor("_Ambient", Ambient);
            }
            else
            {
                mat.SetColor("_Color", color);
            }
            mat.SetPass(0);
            GL.Begin(4);
            GL.Vertex3(vec1.x, vec1.y, vec1.z);
            GL.Vertex3(vec2.x, vec2.y, vec2.z);
            GL.Vertex3(vec2.x, vec2.y, vec2.z);
            GL.End();
            GL.PopMatrix();
            GL.InvalidateState();
        }

        public static void EndGroup()
        {
            GUI.EndGroup();
            clippingBounds = new Rect(0f, 0f, (float) Screen.width, (float) Screen.height);
            clippingEnabled = false;
        }

        protected static bool segment_rect_intersection(Rect bounds, ref Vector2 p1, ref Vector2 p2)
        {
            float num = 0f;
            float num2 = 1f;
            float p = p2.x - p1.x;
            if (clip_test(-p, p1.x - bounds.xMin, ref num, ref num2) && clip_test(p, bounds.xMax - p1.x, ref num, ref num2))
            {
                float num4 = p2.y - p1.y;
                if (clip_test(-num4, p1.y - bounds.yMin, ref num, ref num2) && clip_test(num4, bounds.yMax - p1.y, ref num, ref num2))
                {
                    if (num2 < 1.0)
                    {
                        p2.x = p1.x + (num2 * p);
                        p2.y = p1.y + (num2 * num4);
                    }
                    if (num > 0.0)
                    {
                        p1.x += num * p;
                        p1.y += num * num4;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}

