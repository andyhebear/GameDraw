namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class RuntimeManager : MonoBehaviour
    {
        public Vector3 center = Vector3.zero;
        public GDController controller;
        public GDController Controller;
        [HideInInspector]
        public MeshFilter filter;
        public MeshFilter Filter;
        public Vector3[] frustum = new Vector3[4];
        public Gizmo gizmo;
        public bool isDragSelection;
        public int lastHandleID;
        public GDManager manager;
        public GDManager Manager;
        public bool OverMesh;
        public Vector3 positionOffset;
        private Vector3 positionOffsetTEMP;
        public Ray[] rays = new Ray[4];
        public Quaternion rotationOffset;
        private Quaternion rotationOffsetTEMP;
        public Vector3 scaleOffset = Vector3.one;
        private Vector3 scaleOffsetTEMP;
        public int selected;
        public int selectedType;
        public GUIContent[] selections = new GUIContent[] { new GUIContent("Hand"), new GUIContent("Move"), new GUIContent("Rotate"), new GUIContent("Scale") };
        public MeshFilter target;
        public GDTool tool;
        public bool transformed;
        public GUIContent[] types = new GUIContent[] { new GUIContent("Vertex"), new GUIContent("Face"), new GUIContent("Edge"), new GUIContent("Element") };
        private Vector3[] verticesUntransformed;
        private List<int> visited;
        public bool wrongTopology;

        private void Awake()
        {
            this.UpdateReferences();
            this.SetActiveObject(this.controller, this.manager, this.filter);
        }

        private void CheckDragSelection()
        {
            if ((this.frustum != null) && (this.rays != null))
            {
                if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Vertex)
                {
                    foreach (int num in this.Controller.GDMesh.relatedVertices.Keys)
                    {
                        if (FrustumUtil.PointInFrustum(this.Controller.transform.TransformPoint(this.Controller.GDMesh.Vertices[this.Controller.GDMesh.relatedVertices[num][0]].Traits.position), this.frustum, this.rays))
                        {
                            this.SelectVertex(this.Controller.GDMesh.relatedVertices[num][0]);
                        }
                    }
                }
                else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Triangle)
                {
                    int index = 0;
                    for (int i = 0; i < this.Controller.mesh.triangles.Length; i += 3)
                    {
                        Vector3[] triangle = new Vector3[] { this.Controller.transform.TransformPoint(this.Controller.mesh.vertices[this.Controller.mesh.triangles[i]]), this.Controller.transform.TransformPoint(this.Controller.mesh.vertices[this.Controller.mesh.triangles[i + 1]]), this.Controller.transform.TransformPoint(this.Controller.mesh.vertices[this.Controller.mesh.triangles[i + 2]]) };
                        if (FrustumUtil.TriInFrustum(triangle, this.frustum, this.rays))
                        {
                            this.SelectTriangle(index);
                        }
                        index++;
                    }
                }
                else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Edge)
                {
                    foreach (GDMesh.Edge edge in this.Controller.GDMesh.Edges)
                    {
                        if (FrustumUtil.LineInFrustum(this.Controller.transform.TransformPoint(edge.Vertex0.Traits.position), this.Controller.transform.TransformPoint(edge.Vertex1.Traits.position), this.frustum, this.rays))
                        {
                            this.SelectEdge(edge.Index);
                        }
                    }
                }
                else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Element)
                {
                    foreach (GDMesh.Element element in this.Controller.GDMesh.elements)
                    {
                        foreach (GDMesh.Face face in element.Faces)
                        {
                            List<Vector3> list = new List<Vector3>();
                            foreach (GDMesh.Vertex vertex in face.Vertices)
                            {
                                list.Add(this.Controller.transform.TransformPoint(vertex.Traits.position));
                            }
                            if (FrustumUtil.TriInFrustum(list.ToArray(), this.frustum, this.rays))
                            {
                                this.SelectElement(element.Index);
                            }
                            else
                            {
                                this.Controller.selection.selectedElements.Remove(element.Index);
                                break;
                            }
                        }
                    }
                }
                this.frustum = null;
                this.rays = null;
            }
        }

        public void CleanManager()
        {
            for (int i = 0; i < this.manager.objects.Count; i++)
            {
                if ((this.manager.objects[i] == null) || (this.manager.controllers[i] == null))
                {
                    this.manager.objects.RemoveAt(i);
                    this.manager.controllers.RemoveAt(i);
                }
            }
        }

        public void DragSelection()
        {
            if (!this.gizmo.lockDrag)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    this.Controller.selection.StartDrag(new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 0f));
                }
                else if (Input.GetMouseButton(0))
                {
                    this.Controller.selection.Dragging(new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 0f));
                    if ((Mathf.Abs(Input.GetAxis("Mouse X")) > 1f) || (Mathf.Abs(Input.GetAxis("Mouse Y")) > 1f))
                    {
                        this.isDragSelection = true;
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    this.ValidateClearSelection();
                    this.rays = this.controller.selection.GetRays();
                    this.Controller.selection.EndDrag(new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 0f), out this.frustum, this.rays);
                    this.CheckDragSelection();
                    this.isDragSelection = false;
                    this.OverMesh = false;
                }
                this.Controller.selection.DrawSelectionBox();
            }
        }

        public void DrawEdges()
        {
            List<Vector3> list = new List<Vector3>();
            foreach (int num in this.Controller.selection.selectedEdges)
            {
                GDMesh.Edge edge = this.Controller.GDMesh.Edges[num];
                list.Add(this.Controller.referencedGO.transform.TransformPoint(edge.Vertex0.Traits.position));
                list.Add(this.Controller.referencedGO.transform.TransformPoint(edge.Vertex1.Traits.position));
            }
            GLDrawUtility.DrawLine(Color.red, Color.red, Color.red, list.ToArray(), false);
        }

        public void DrawElements()
        {
            List<Vector3> list = new List<Vector3>();
            foreach (int num in this.Controller.selection.selectedElements)
            {
                int num2 = 0;
                GDMesh.Element element = this.Controller.GDMesh.elements[num];
                foreach (GDMesh.Face face in element.Faces)
                {
                    foreach (GDMesh.Vertex vertex in face.Vertices)
                    {
                        list.Add(this.Controller.referencedGO.transform.TransformPoint(vertex.Traits.position));
                        num2++;
                    }
                }
            }
            GLDrawUtility.DrawTriangle(Color.red, Color.red, Color.red, list.ToArray());
        }

        public void DrawSelection()
        {
            if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Vertex)
            {
                this.DrawVertex();
            }
            else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Triangle)
            {
                this.DrawTriangles();
            }
            else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Edge)
            {
                this.DrawEdges();
            }
            else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Element)
            {
                this.DrawElements();
            }
        }

        public bool drawTransformHandles()
        {
            if (this.tool == GDTool.Move)
            {
                this.positionOffsetTEMP = this.Controller.transform.InverseTransformPoint(this.Controller.transform.TransformPoint(this.gizmo.handle.position));
                if (this.positionOffset != this.positionOffsetTEMP)
                {
                    this.lastHandleID = GUIUtility.hotControl;
                    this.positionOffset = this.positionOffsetTEMP;
                    this.transformed = true;
                }
            }
            else if (this.tool == GDTool.Scale)
            {
                this.scaleOffsetTEMP = Vector3.one;
                if (((this.scaleOffset.x != this.scaleOffsetTEMP.x) || (this.scaleOffset.y != this.scaleOffsetTEMP.y)) || (this.scaleOffset.z != this.scaleOffsetTEMP.z))
                {
                    this.scaleOffset = this.scaleOffsetTEMP;
                    this.transformed = true;
                }
            }
            else if (this.tool == GDTool.Rotate)
            {
                this.rotationOffsetTEMP = Quaternion.identity;
                if (this.rotationOffset != this.rotationOffsetTEMP)
                {
                    this.rotationOffset = this.rotationOffsetTEMP;
                    this.transformed = true;
                }
            }
            this.UpdateCenter();
            return this.transformed;
        }

        public void DrawTriangles()
        {
            List<Vector3> list = new List<Vector3>();
            foreach (int num in this.Controller.selection.selectedTriangles)
            {
                int num2 = 0;
                GDMesh.Face face = this.Controller.GDMesh.Faces[num];
                foreach (GDMesh.Vertex vertex in face.Vertices)
                {
                    list.Add(this.Controller.referencedGO.transform.TransformPoint(vertex.Traits.position));
                    num2++;
                }
            }
            GLDrawUtility.DrawTriangle(Color.red, Color.red, Color.red, list.ToArray());
        }

        public void DrawVertex()
        {
            foreach (int num in this.Controller.selection.selectedVertices)
            {
                Vector3 position = this.Controller.transform.TransformPoint(this.Controller.mesh.vertices[this.Controller.GDMesh.relatedVertices[num][0]]);
                Vector3 vector2 = base.GetComponent<Camera>().WorldToScreenPoint(position);
                GUI.Button(new Rect(vector2.x - 2f, vector2.y - 2f, 4f, 4f), "");
            }
        }

        private void ExtrudeSelection()
        {
            this.controller.locked = true;
            if (this.controller.selection.selectionType == MeshSelection.SelectionType.Triangle)
            {
                this.controller._gdmesh = GDMesh.GDMeshUtils.extrude(this.controller, this.controller.selection.selectedTriangles);
            }
            this.controller.locked = false;
        }

        public int getNearestEdge(RaycastHit hit)
        {
            Vector3 vector2;
            Vector3 vector3;
            Vector3 b = vector2 = vector3 = Vector3.zero;
            int num = 0;
            foreach (GDMesh.Edge edge in this.Controller.GDMesh.Faces[hit.triangleIndex].Edges)
            {
                switch (num)
                {
                    case 0:
                        b = (Vector3) ((edge.Vertex0.Traits.position + edge.Vertex1.Traits.position) / 2f);
                        break;

                    case 1:
                        vector2 = (Vector3) ((edge.Vertex0.Traits.position + edge.Vertex1.Traits.position) / 2f);
                        break;

                    case 2:
                        vector3 = (Vector3) ((edge.Vertex0.Traits.position + edge.Vertex1.Traits.position) / 2f);
                        break;
                }
                num++;
            }
            float num2 = Vector3.Distance(this.Controller.transform.InverseTransformPoint(hit.point), b);
            float num3 = Vector3.Distance(this.Controller.transform.InverseTransformPoint(hit.point), vector2);
            float num4 = Vector3.Distance(this.Controller.transform.InverseTransformPoint(hit.point), vector3);
            int index = 0;
            if ((num2 < num3) && (num2 < num4))
            {
                return this.Controller.GDMesh.Faces[hit.triangleIndex].GetEdge(0).Index;
            }
            if ((num3 < num2) && (num3 < num4))
            {
                return this.Controller.GDMesh.Faces[hit.triangleIndex].GetEdge(1).Index;
            }
            if ((num4 < num2) && (num4 < num3))
            {
                index = this.Controller.GDMesh.Faces[hit.triangleIndex].GetEdge(2).Index;
            }
            return index;
        }

        public int getNearestVertex(RaycastHit hit)
        {
            Vector3 b = this.Controller.mesh.vertices[this.Controller.mesh.triangles[hit.triangleIndex * 3]];
            Vector3 vector2 = this.Controller.mesh.vertices[this.Controller.mesh.triangles[(hit.triangleIndex * 3) + 1]];
            Vector3 vector3 = this.Controller.mesh.vertices[this.Controller.mesh.triangles[(hit.triangleIndex * 3) + 2]];
            float num = Vector3.Distance(this.Controller.transform.InverseTransformPoint(hit.point), b);
            float num2 = Vector3.Distance(this.Controller.transform.InverseTransformPoint(hit.point), vector2);
            float num3 = Vector3.Distance(this.Controller.transform.InverseTransformPoint(hit.point), vector3);
            int num4 = -1;
            if ((num < num2) && (num < num3))
            {
                return this.Controller.mesh.triangles[hit.triangleIndex * 3];
            }
            if ((num2 < num) && (num2 < num3))
            {
                return this.Controller.mesh.triangles[(hit.triangleIndex * 3) + 1];
            }
            if ((num3 < num) && (num3 < num2))
            {
                num4 = this.Controller.mesh.triangles[(hit.triangleIndex * 3) + 2];
            }
            return num4;
        }

        public void HandleSelection()
        {
            if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Vertex)
            {
                this.HighlightVertex(true);
            }
            else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Triangle)
            {
                this.SelectTriangles();
            }
            else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Edge)
            {
                this.SelectEdges();
            }
            else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Element)
            {
                this.SelectElement();
            }
        }

        public void HighlightEdge()
        {
            RaycastHit hit;
            Ray ray = base.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (this.Controller.referencedCollider.Raycast(ray, out hit, float.PositiveInfinity))
            {
                this.HighlightEdge(this.getNearestEdge(hit));
            }
        }

        public void HighlightEdge(int index)
        {
            if (index != -1)
            {
                List<Vector3> list = new List<Vector3>();
                GDMesh.Edge edge = this.Controller.GDMesh.Edges[index];
                list.Add(this.Controller.referencedGO.transform.TransformPoint(edge.Vertex0.Traits.position));
                list.Add(this.Controller.referencedGO.transform.TransformPoint(edge.Vertex1.Traits.position));
                GLDrawUtility.DrawLine(Color.green, Color.green, Color.green, list.ToArray(), false);
            }
        }

        public void HighlightElement()
        {
            RaycastHit hit;
            Ray ray = base.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (this.Controller.referencedCollider.Raycast(ray, out hit, float.PositiveInfinity))
            {
                this.HighlightElement(hit.triangleIndex);
            }
        }

        public void HighlightElement(int index)
        {
            List<Vector3> list = new List<Vector3>();
            foreach (GDMesh.Face face in this.Controller.GDMesh.Faces[index].Element.Faces)
            {
                foreach (GDMesh.Vertex vertex in face.Vertices)
                {
                    list.Add(this.Controller.referencedGO.transform.TransformPoint(vertex.Traits.position));
                }
            }
            GLDrawUtility.DrawTriangle(Color.green, Color.green, Color.green, list.ToArray());
        }

        public void HighlightSelection()
        {
            if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Vertex)
            {
                this.HighlightVertex(false);
            }
            else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Triangle)
            {
                this.HighlightTriangle();
            }
            else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Edge)
            {
                this.HighlightEdge();
            }
            else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Element)
            {
                this.HighlightElement();
            }
        }

        public void HighlightTriangle()
        {
            RaycastHit hit;
            Ray ray = base.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (this.Controller.referencedCollider.Raycast(ray, out hit, float.PositiveInfinity))
            {
                this.HighlightTriangle(hit.triangleIndex);
            }
        }

        public void HighlightTriangle(int index)
        {
            List<Vector3> list = new List<Vector3>();
            GDMesh.Face face = this.Controller.GDMesh.Faces[index];
            foreach (GDMesh.Vertex vertex in face.Vertices)
            {
                list.Add(this.Controller.referencedGO.transform.TransformPoint(vertex.Traits.position));
            }
            GLDrawUtility.DrawTriangle(Color.green, Color.green, Color.green, list.ToArray());
        }

        public void HighlightVertex( bool allowSelection=true)
        {
            RaycastHit hit;
            Ray ray = base.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (this.Controller.referencedCollider.Raycast(ray, out hit, float.PositiveInfinity))
            {
                this.HighlightVertex(this.getNearestVertex(hit), true);
            }
        }

        public void HighlightVertex(int index, bool allowSelection=true)
        {
            Vector3 position = this.Controller.transform.TransformPoint(this.Controller.mesh.vertices[index]);
            base.GetComponent<Camera>().WorldToScreenPoint(position);
            this.OverMesh = true;
        }

        private void OnGUI()
        {
            if ((this.controller != null) && (this.controller.GDMesh != null))
            {
                this.UpdateReferences();
                this.ScreenGUI();
                this.UpdateCenter();
                if ((((this.tool != GDTool.View) && (this.tool != GDTool.None)) || !Input.GetMouseButton(0)) && Input.GetMouseButton(0))
                {
                    this.drawTransformHandles();
                }
                this.tool = (GDTool) this.selected;
            }
        }

        private void ScreenGUI()
        {
            if (Input.GetKeyUp(KeyCode.C))
            {
                this.selected = 0;
            }
            else if (Input.GetKeyUp(KeyCode.M))
            {
                this.selected = 1;
            }
            else if (Input.GetKeyUp(KeyCode.R))
            {
                this.selected = 2;
            }
            else if (Input.GetKeyUp(KeyCode.S))
            {
                this.selected = 3;
            }
            int num = GUI.SelectionGrid(new Rect((float) (Screen.width - 200), 5f, 190f, 20f), this.selected, this.selections, 4);
            if (num != this.selected)
            {
                this.selected = num;
                int selected = this.selected;
            }
            num = GUI.SelectionGrid(new Rect((float) (Screen.width - 200), 25f, 190f, 20f), this.selectedType, this.types, 4);
            if (num != this.selectedType)
            {
                this.selectedType = num;
                if (this.selectedType == 0)
                {
                    this.controller.selection.selectionType = MeshSelection.SelectionType.Vertex;
                }
                else if (this.selectedType == 1)
                {
                    this.controller.selection.selectionType = MeshSelection.SelectionType.Triangle;
                }
                else if (this.selectedType == 2)
                {
                    this.controller.selection.selectionType = MeshSelection.SelectionType.Edge;
                }
                else if (this.selectedType == 3)
                {
                    this.controller.selection.selectionType = MeshSelection.SelectionType.Element;
                }
            }
            if (GUI.Button(new Rect(20f, 10f, 100f, 20f), "Extrude"))
            {
                this.ExtrudeSelection();
            }
        }

        public void SelectEdge(int index)
        {
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                if (!this.Controller.selection.selectedEdges.Contains(index))
                {
                    this.Controller.selection.selectedEdges.Add(index);
                }
            }
            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (!this.Controller.selection.selectedEdges.Contains(index))
                {
                    this.Controller.selection.selectedEdges.Add(index);
                }
                else
                {
                    this.Controller.selection.selectedEdges.Remove(index);
                }
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (this.Controller.selection.selectedEdges.Contains(index))
                {
                    this.Controller.selection.selectedEdges.Remove(index);
                }
            }
            else
            {
                if (!this.isDragSelection)
                {
                    this.Controller.selection.selectedEdges.Clear();
                }
                this.Controller.selection.selectedEdges.Add(index);
            }
        }

        public void SelectEdges()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = base.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                if (this.Controller.referencedCollider.Raycast(ray, out hit, float.PositiveInfinity))
                {
                    this.OverMesh = true;
                    int index = this.getNearestEdge(hit);
                    if (index != -1)
                    {
                        this.SelectEdge(index);
                    }
                }
            }
        }

        public void SelectElement()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = base.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                if (this.Controller.referencedCollider.Raycast(ray, out hit, float.PositiveInfinity))
                {
                    this.OverMesh = true;
                    GDMesh.Face face = this.Controller.GDMesh.Faces[hit.triangleIndex];
                    this.SelectElement(face.Element.Index);
                }
            }
        }

        public void SelectElement(int index)
        {
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                if (!this.Controller.selection.selectedElements.Contains(index))
                {
                    this.Controller.selection.selectedElements.Add(index);
                }
            }
            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (!this.Controller.selection.selectedElements.Contains(index))
                {
                    this.Controller.selection.selectedElements.Add(index);
                }
                else
                {
                    this.Controller.selection.selectedElements.Remove(index);
                }
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (this.Controller.selection.selectedElements.Contains(index))
                {
                    this.Controller.selection.selectedElements.Remove(index);
                }
            }
            else
            {
                if (!this.isDragSelection)
                {
                    this.Controller.selection.selectedElements.Clear();
                }
                this.Controller.selection.selectedElements.Add(index);
            }
        }

        public void SelectTriangle(int index)
        {
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                if (!this.Controller.selection.selectedTriangles.Contains(index))
                {
                    this.Controller.selection.selectedTriangles.Add(index);
                }
            }
            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (!this.Controller.selection.selectedTriangles.Contains(index))
                {
                    this.Controller.selection.selectedTriangles.Add(index);
                }
                else
                {
                    this.Controller.selection.selectedTriangles.Remove(index);
                }
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (this.Controller.selection.selectedTriangles.Contains(index))
                {
                    this.Controller.selection.selectedTriangles.Remove(index);
                }
            }
            else
            {
                if (!this.isDragSelection)
                {
                    this.Controller.selection.selectedTriangles.Clear();
                }
                if (!this.Controller.selection.selectedTriangles.Contains(index))
                {
                    this.Controller.selection.selectedTriangles.Add(index);
                }
            }
        }

        public void SelectTriangles()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = base.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                if (this.Controller.referencedCollider.Raycast(ray, out hit, float.PositiveInfinity))
                {
                    this.OverMesh = true;
                    this.SelectTriangle(hit.triangleIndex);
                }
            }
        }

        public void SelectVertex(int vertIndex)
        {
            GDMesh.Vertex vertex = this.Controller.GDMesh.Vertices[vertIndex];
            if (vertex != null)
            {
                int hashCode = vertex.Traits.hashCode;
                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                {
                    this.Controller.selection.selectedVertices.Add(hashCode);
                }
                else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    this.Controller.selection.selectedVertices.Add(hashCode);
                    this.Controller.selection.selectedVertices.Remove(hashCode);
                }
                else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    this.Controller.selection.selectedVertices.Remove(hashCode);
                }
                else
                {
                    if (!this.isDragSelection)
                    {
                        this.Controller.selection.selectedVertices.Clear();
                    }
                    if (!this.Controller.selection.selectedVertices.Contains(hashCode))
                    {
                        this.Controller.selection.selectedVertices.Add(hashCode);
                    }
                }
            }
        }

        public void SelectVertices()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = base.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                if (this.Controller.referencedCollider.Raycast(ray, out hit, float.PositiveInfinity))
                {
                    this.OverMesh = true;
                    int vertIndex = this.getNearestVertex(hit);
                    this.SelectVertex(vertIndex);
                }
            }
        }

        public void SetActiveObject(GDController controller, GDManager manager, MeshFilter filter)
        {
            this.Controller = controller;
            this.Manager = manager;
            this.Filter = filter;
        }

        private void Start()
        {
        }

        public void TransformMesh()
        {
            if (this.transformed && !this.Controller.locked)
            {
                this.transformed = false;
                List<Vector3> verts = new List<Vector3>(this.Controller.mesh.vertices);
                if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Vertex)
                {
                    foreach (int num in this.Controller.selection.selectedVertices)
                    {
                        this.TransformVertex(this.Controller.GDMesh.relatedVertices[num].vert, this.Controller.GDMesh.relatedVertices[num][0]);
                        foreach (int num2 in this.Controller.GDMesh.relatedVertices[num])
                        {
                            this.TransformVertexReferences(this.Controller.GDMesh.relatedVertices[num].vert, num2, verts);
                        }
                    }
                }
                else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Triangle)
                {
                    List<int> list2 = new List<int>();
                    foreach (int num3 in this.Controller.selection.selectedTriangles)
                    {
                        GDMesh.Face face = this.Controller.GDMesh.Faces[num3];
                        foreach (GDMesh.Vertex vertex in face.Vertices)
                        {
                            if (!list2.Contains(vertex.Traits.hashCode))
                            {
                                this.TransformVertex(vertex, this.Controller.GDMesh.relatedVertices[vertex.Traits.hashCode][0]);
                                foreach (int num4 in this.Controller.GDMesh.relatedVertices[vertex.Traits.hashCode])
                                {
                                    this.TransformVertexReferences(vertex, num4, verts);
                                }
                                list2.Add(vertex.Traits.hashCode);
                            }
                        }
                    }
                }
                else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Edge)
                {
                    List<int> list3 = new List<int>();
                    foreach (int num5 in this.Controller.selection.selectedEdges)
                    {
                        GDMesh.Edge edge = this.Controller.GDMesh.Edges[num5];
                        GDMesh.Vertex vert = edge.Vertex0;
                        GDMesh.Vertex vertex3 = edge.Vertex1;
                        if (!list3.Contains(vert.Traits.hashCode))
                        {
                            this.TransformVertex(vert, this.Controller.GDMesh.relatedVertices[vert.Traits.hashCode][0]);
                            foreach (int num6 in this.Controller.GDMesh.relatedVertices[vert.Traits.hashCode])
                            {
                                this.TransformVertexReferences(vert, num6, verts);
                            }
                            list3.Add(vert.Traits.hashCode);
                        }
                        if (!list3.Contains(vertex3.Traits.hashCode))
                        {
                            this.TransformVertex(vertex3, this.Controller.GDMesh.relatedVertices[vertex3.Traits.hashCode][0]);
                            foreach (int num7 in this.Controller.GDMesh.relatedVertices[vertex3.Traits.hashCode])
                            {
                                this.TransformVertexReferences(vertex3, num7, verts);
                            }
                            list3.Add(vertex3.Traits.hashCode);
                        }
                    }
                }
                else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Element)
                {
                    List<int> list4 = new List<int>();
                    foreach (int num8 in this.Controller.selection.selectedElements)
                    {
                        GDMesh.Element element = this.Controller.GDMesh.elements[num8];
                        foreach (GDMesh.Face face2 in element.Faces)
                        {
                            foreach (GDMesh.Vertex vertex4 in face2.Vertices)
                            {
                                if (!list4.Contains(vertex4.Traits.hashCode))
                                {
                                    this.TransformVertex(vertex4, this.Controller.GDMesh.relatedVertices[vertex4.Traits.hashCode][0]);
                                    foreach (int num9 in this.Controller.GDMesh.relatedVertices[vertex4.Traits.hashCode])
                                    {
                                        this.TransformVertexReferences(vertex4, num9, verts);
                                    }
                                    list4.Add(vertex4.Traits.hashCode);
                                }
                            }
                        }
                    }
                }
                this.Controller.mesh.vertices = verts.ToArray();
                if (this.Controller.isSkinned)
                {
                    this.Controller.referencedRenderer.sharedMesh = this.Controller.mesh;
                }
                else
                {
                    this.Controller.referencedFilter.mesh = this.Controller.mesh;
                }
                this.Controller.mesh.RecalculateBounds();
                this.Controller.referencedCollider.sharedMesh = null;
                this.Controller.referencedCollider.sharedMesh = this.Controller.mesh;
            }
        }

        public void TransformVertex(GDMesh.Vertex vert, int pointer)
        {
            if (vert != null)
            {
                if (this.tool == GDTool.Move)
                {
                    vert.Traits.position += this.positionOffset - this.center;
                }
                else if (this.tool == GDTool.Scale)
                {
                    Vector3 vector = this.verticesUntransformed[pointer];
                    vert.Traits.position = new Vector3(this.scaleOffset.x * (vector - this.center).x, this.scaleOffset.y * (vector - this.center).y, this.scaleOffset.z * (vector - this.center).z) + this.center;
                }
                else if (this.tool == GDTool.Rotate)
                {
                    Vector3 vector2 = this.verticesUntransformed[pointer];
                    vert.Traits.position = ((Vector3) (this.rotationOffset * (vector2 - this.center))) + this.center;
                }
            }
        }

        public void TransformVertexReferences(GDMesh.Vertex vert, int pointer, List<Vector3> verts)
        {
            if (vert != null)
            {
                verts[pointer] = vert.Traits.position;
            }
        }

        public void Update()
        {
            this.CleanManager();
            this.UpdateManagerTransform();
            this.TransformMesh();
            if (((this.tool == GDTool.View) || (this.tool == GDTool.None)) && Input.GetMouseButton(0))
            {
                this.HandleSelection();
            }
            else if (Input.GetMouseButton(0))
            {
                this.HandleSelection();
            }
        }

        public void UpdateCenter()
        {
            this.center = this.SelectionCenterPoint;
        }

        public void UpdateManagerTransform()
        {
            if (this.filter != null)
            {
                this.manager.transform.position = this.filter.transform.position;
                this.manager.transform.rotation = this.filter.transform.rotation;
                this.manager.transform.localScale = this.filter.transform.lossyScale;
            }
        }

        private void UpdateReferences()
        {
            this.manager = GDManager.getManager();
            try
            {
                this.filter = this.target;
                this.controller = this.manager.getController(this.filter.gameObject, false);
            }
            catch
            {
                this.wrongTopology = true;
            }
        }

        private void ValidateClearSelection()
        {
            if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Vertex)
            {
                if ((((this.Controller.selection.selectedVertices.Count > 0) && !Input.GetKey(KeyCode.LeftShift)) && (!Input.GetKey(KeyCode.RightShift) && !Input.GetKey(KeyCode.LeftControl))) && (!Input.GetKey(KeyCode.RightControl) && !this.OverMesh))
                {
                    this.Controller.selection.selectedVertices.Clear();
                }
            }
            else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Triangle)
            {
                if ((((this.Controller.selection.selectedTriangles.Count > 0) && !Input.GetKey(KeyCode.LeftShift)) && (!Input.GetKey(KeyCode.RightShift) && !Input.GetKey(KeyCode.LeftControl))) && (!Input.GetKey(KeyCode.RightControl) && !this.OverMesh))
                {
                    this.Controller.selection.selectedTriangles.Clear();
                }
            }
            else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Edge)
            {
                if ((((this.Controller.selection.selectedEdges.Count > 0) && !Input.GetKey(KeyCode.LeftShift)) && (!Input.GetKey(KeyCode.RightShift) && !Input.GetKey(KeyCode.LeftControl))) && (!Input.GetKey(KeyCode.RightControl) && !this.OverMesh))
                {
                    this.Controller.selection.selectedEdges.Clear();
                }
            }
            else if ((((this.Controller.selection.selectionType == MeshSelection.SelectionType.Element) && (this.Controller.selection.selectedElements.Count > 0)) && (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))) && ((!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) && !this.OverMesh))
            {
                this.Controller.selection.selectedElements.Clear();
            }
        }

        public Vector3 SelectionCenterPoint
        {
            get
            {
                int num = 0;
                Vector3 zero = Vector3.zero;
                if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Vertex)
                {
                    foreach (int num2 in this.Controller.selection.selectedVertices)
                    {
                        zero += this.Controller.mesh.vertices[this.Controller.GDMesh.relatedVertices[num2][0]];
                        num++;
                    }
                }
                else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Triangle)
                {
                    foreach (int num3 in this.Controller.selection.selectedTriangles)
                    {
                        GDMesh.Face face = this.Controller.GDMesh.Faces[num3];
                        foreach (GDMesh.Vertex vertex in face.Vertices)
                        {
                            zero += vertex.Traits.position;
                            num++;
                        }
                    }
                }
                else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Element)
                {
                    foreach (int num4 in this.Controller.selection.selectedElements)
                    {
                        GDMesh.Element element = this.Controller.GDMesh.elements[num4];
                        foreach (GDMesh.Face face2 in element.Faces)
                        {
                            foreach (GDMesh.Vertex vertex2 in face2.Vertices)
                            {
                                zero += vertex2.Traits.position;
                                num++;
                            }
                        }
                    }
                }
                else if (this.Controller.selection.selectionType == MeshSelection.SelectionType.Edge)
                {
                    foreach (int num5 in this.Controller.selection.selectedEdges)
                    {
                        GDMesh.Edge edge = this.Controller.GDMesh.Edges[num5];
                        zero += edge.Vertex0.Traits.position;
                        zero += edge.Vertex1.Traits.position;
                        num += 2;
                    }
                }
                return (Vector3) (zero / ((float) num));
            }
        }

        public enum GDTool
        {
            View,
            Move,
            Rotate,
            Scale,
            None
        }
    }
}

