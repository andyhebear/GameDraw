namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [Serializable]
    public class MeshSelection
    {
        public Dictionary<int, bool> edgeCheck;
        public Dictionary<int, bool> elementCheck;
        public float endX;
        public float endY;
        public List<int> halfEdges;
        public List<int> selectedEdges;
        public List<int> selectedElements;
        public List<int> selectedTriangles;
        public List<int> selectedVertices;
        public List<int> selectedVerticesIDs;
        public Texture2D selectionRect;
        public SelectionType selectionType;
        public float startX;
        public float startY;
        public Dictionary<int, bool> triCheck;
        public Dictionary<int, bool> vertCheck;

        public MeshSelection(MeshSelection original)
        {
            this.selectedVertices = new List<int>(original.selectedVertices);
            if (original.vertCheck != null)
            {
                this.vertCheck = new Dictionary<int, bool>(original.vertCheck);
            }
            else
            {
                this.vertCheck = new Dictionary<int, bool>();
            }
            this.selectedTriangles = new List<int>(original.selectedTriangles);
            if (original.triCheck != null)
            {
                this.triCheck = new Dictionary<int, bool>(original.triCheck);
            }
            else
            {
                this.triCheck = new Dictionary<int, bool>();
            }
            this.selectedEdges = new List<int>(original.selectedEdges);
            if (original.edgeCheck != null)
            {
                this.edgeCheck = new Dictionary<int, bool>(original.edgeCheck);
            }
            else
            {
                this.edgeCheck = new Dictionary<int, bool>();
            }
            this.halfEdges = new List<int>(original.halfEdges);
            this.selectedVerticesIDs = new List<int>(original.selectedVerticesIDs);
            this.selectedElements = new List<int>(original.selectedElements);
            if (original.elementCheck != null)
            {
                this.elementCheck = new Dictionary<int, bool>(original.elementCheck);
            }
            else
            {
                this.elementCheck = new Dictionary<int, bool>();
            }
            this.selectionType = original.selectionType;
        }

        public MeshSelection( Mesh m=null)
        {
            if (m == null)
            {
                this.selectedVertices = new List<int>();
                this.selectedTriangles = new List<int>();
                this.selectedEdges = new List<int>();
                this.vertCheck = new Dictionary<int, bool>();
                this.triCheck = new Dictionary<int, bool>();
                this.edgeCheck = new Dictionary<int, bool>();
                this.elementCheck = new Dictionary<int, bool>();
                this.halfEdges = new List<int>();
                this.selectedVerticesIDs = new List<int>();
                this.selectedElements = new List<int>();
            }
            else
            {
                this.selectedVertices = new List<int>(m.vertexCount);
                this.vertCheck = new Dictionary<int, bool>(m.vertexCount);
                this.selectedTriangles = new List<int>(m.triangles.Length);
                this.triCheck = new Dictionary<int, bool>(m.triangles.Length);
                this.selectedEdges = new List<int>(m.triangles.Length);
                this.edgeCheck = new Dictionary<int, bool>(m.triangles.Length);
                this.halfEdges = new List<int>(m.triangles.Length);
                this.selectedVerticesIDs = new List<int>(m.vertexCount);
                this.selectedElements = new List<int>(m.subMeshCount);
                this.elementCheck = new Dictionary<int, bool>(m.subMeshCount);
            }
            this.selectionType = SelectionType.Vertex;
        }

        public void Clear()
        {
            this.selectedVertices.Clear();
            this.selectedTriangles.Clear();
            this.selectedEdges.Clear();
            this.halfEdges.Clear();
            this.selectedElements.Clear();
            this.selectedVerticesIDs.Clear();
        }

        public void ClearCheckers()
        {
            if (this.vertCheck != null)
            {
                this.vertCheck.Clear();
            }
            else
            {
                this.vertCheck = new Dictionary<int, bool>(this.selectedVertices.Capacity);
            }
            if (this.triCheck != null)
            {
                this.triCheck.Clear();
            }
            else
            {
                this.triCheck = new Dictionary<int, bool>(this.selectedTriangles.Capacity);
            }
            if (this.edgeCheck != null)
            {
                this.edgeCheck.Clear();
            }
            else
            {
                this.edgeCheck = new Dictionary<int, bool>(this.selectedEdges.Capacity);
            }
            if (this.elementCheck != null)
            {
                this.elementCheck.Clear();
            }
            else
            {
                this.elementCheck = new Dictionary<int, bool>(this.selectedElements.Capacity);
            }
        }

        public void Dragging(Vector2 mousePosition)
        {
            this.endX = mousePosition.x;
            this.endY = mousePosition.y;
        }

        public void DrawSelectionBox()
        {
            if (this.selectionRect == null)
            {
                this.selectionRect = new Texture2D(1, 1);
                this.selectionRect.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.5f, 0.5f));
                this.selectionRect.Apply();
            }
            if (this.startX < this.endX)
            {
                if (this.startY < this.endY)
                {
                    GUI.DrawTexture(new Rect(this.startX, this.startY, Mathf.Abs((float) (this.endX - this.startX)), Mathf.Abs((float) (this.endY - this.startY))), this.selectionRect);
                }
                else
                {
                    GUI.DrawTexture(new Rect(this.startX, this.endY, Mathf.Abs((float) (this.endX - this.startX)), Mathf.Abs((float) (this.endY - this.startY))), this.selectionRect);
                }
            }
            else if (this.startY < this.endY)
            {
                GUI.DrawTexture(new Rect(this.endX, this.startY, Mathf.Abs((float) (this.endX - this.startX)), Mathf.Abs((float) (this.endY - this.startY))), this.selectionRect);
            }
            else
            {
                GUI.DrawTexture(new Rect(this.endX, this.endY, Mathf.Abs((float) (this.endX - this.startX)), Mathf.Abs((float) (this.endY - this.startY))), this.selectionRect);
            }
        }

        public bool EndDrag(Vector2 mousePosition, out Vector3[] Frustum, Ray[] Rays)
        {
            if ((Mathf.Abs((float) (this.endX - this.startX)) > 1f) && (Mathf.Abs((float) (this.endY - this.startY)) > 1f))
            {
                if (this.endX < this.startX)
                {
                    this.SwapPosition(ref this.endX, ref this.startX);
                }
                if (this.endY < this.startY)
                {
                    this.SwapPosition(ref this.endY, ref this.startY);
                }
                Ray[] r = new Ray[4];
                Vector3[] frustum = new Vector3[4];
                r = Rays;
                frustum = this.GetFrustum(r);
                Frustum = frustum;
                this.startX = this.startY = this.endX = this.endY = -1f;
                return true;
            }
            Frustum = null;
            Rays = null;
            return false;
        }

        private Vector3[] GetFrustum(Ray[] r)
        {
            Vector3[] vectorArray = new Vector3[4];
            if (Camera.current.orthographic)
            {
                Vector3 vector = r[1].origin - r[0].origin;
                vectorArray[0] = Vector3.Cross(vector.normalized, r[0].direction).normalized;
                Vector3 vector3 = r[3].origin - r[0].origin;
                vectorArray[1] = Vector3.Cross(vector3.normalized, r[0].direction).normalized;
                Vector3 vector5 = r[3].origin - r[2].origin;
                vectorArray[2] = Vector3.Cross(vector5.normalized, r[2].direction).normalized;
                Vector3 vector7 = r[1].origin - r[2].origin;
                vectorArray[3] = Vector3.Cross(vector7.normalized, r[2].direction).normalized;
                return vectorArray;
            }
            vectorArray[0] = Vector3.Cross(r[1].direction, r[0].direction).normalized;
            vectorArray[1] = Vector3.Cross(r[2].direction, r[1].direction).normalized;
            vectorArray[2] = Vector3.Cross(r[3].direction, r[2].direction).normalized;
            vectorArray[3] = Vector3.Cross(r[0].direction, r[3].direction).normalized;
            return vectorArray;
        }

        public Ray[] GetRays()
        {
            return new Ray[] { Camera.current.ScreenPointToRay((Vector3) new Vector2(this.startX, this.startY)), Camera.current.ScreenPointToRay((Vector3) new Vector2(this.startX, this.endY)), Camera.current.ScreenPointToRay((Vector3) new Vector2(this.endX, this.endY)), Camera.current.ScreenPointToRay((Vector3) new Vector2(this.endX, this.startY)) };
        }

        public void StartDrag(Vector2 mousePosition)
        {
            this.startX = mousePosition.x;
            this.startY = mousePosition.y;
            this.endX = mousePosition.x;
            this.endY = mousePosition.y;
        }

        public void SwapPosition(ref float x, ref float y)
        {
            float num = x;
            x = y;
            y = num;
        }

        public void UpdateTables()
        {
            if (this.vertCheck == null)
            {
                this.vertCheck = new Dictionary<int, bool>(this.selectedVertices.Capacity);
            }
            if (this.edgeCheck == null)
            {
                this.edgeCheck = new Dictionary<int, bool>(this.selectedEdges.Capacity);
            }
            if (this.triCheck == null)
            {
                this.triCheck = new Dictionary<int, bool>(this.selectedTriangles.Capacity);
            }
            if (this.elementCheck == null)
            {
                this.elementCheck = new Dictionary<int, bool>(this.selectedElements.Capacity);
            }
        }

        public enum SelectionType
        {
            Vertex,
            Edge,
            Triangle,
            Element
        }
    }
}

