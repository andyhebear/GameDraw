namespace GameDraw
{
    using System;
    using UnityEngine;

    public class Gizmo : MonoBehaviour
    {
        public GizmoHandle axisX;
        public GizmoHandle axisY;
        public GizmoHandle axisZ;
        public Vector3 change;
        public GizmoHandle.GizmoType gizmoType;
        public Transform handle;
        public float handleLength = 5f;
        public Material highligh;
        private static Material lineMaterial;
        public bool lockDrag;
        public RuntimeManager manager;
        public bool needUpdate;
        public TransformSpace space;
        public Transform target;

        private void Awake()
        {
            this.axisX.manager = this.manager;
            this.axisY.manager = this.manager;
            this.axisZ.manager = this.manager;
            this.manager.gizmo = this;
            this.axisX.axis = GizmoHandle.GizmoAxis.X;
            this.axisY.axis = GizmoHandle.GizmoAxis.Y;
            this.axisZ.axis = GizmoHandle.GizmoAxis.Z;
            this.axisX.highligh = this.axisY.highligh = this.axisZ.highligh = this.highligh;
            this.setType(this.gizmoType);
        }

        private static void CreateLineMaterial()
        {
            if (lineMaterial == null)
            {
                lineMaterial = new Material("Shader \"Lines/Colored Blended\" {SubShader { Pass {     Blend SrcAlpha OneMinusSrcAlpha     ZWrite Off Cull Off Fog { Mode Off }     BindChannels {      Bind \"vertex\", vertex Bind \"color\", color }} } }");
            }
        }

        private void OnRenderObject()
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3 vector3;
            Vector3 center = this.manager.center;
            GL.MultMatrix(base.transform.localToWorldMatrix);
            GL.PushMatrix();
            if (this.space == TransformSpace.Global)
            {
                vector = (Vector3) (Vector3.right * this.handleLength);
                vector2 = (Vector3) (Vector3.up * this.handleLength);
                vector3 = (Vector3) (Vector3.forward * this.handleLength);
            }
            else
            {
                vector = this.target.transform.TransformDirection((Vector3) (Vector3.right * this.handleLength));
                vector2 = this.target.transform.TransformDirection((Vector3) (Vector3.up * this.handleLength));
                vector3 = this.target.transform.TransformDirection((Vector3) (Vector3.forward * this.handleLength));
            }
            CreateLineMaterial();
            lineMaterial.SetPass(0);
            GL.Begin(1);
            if (this.axisX.positionEnd.GetComponent<Renderer>().sharedMaterial == this.highligh)
            {
                GL.Color(new Color(1f, 1f, 0f, 0.5f));
                this.lockDrag = true;
            }
            else
            {
                GL.Color(new Color(1f, 0f, 0f, 0.5f));
                this.lockDrag = false;
            }
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(vector.x, vector.y, vector.z);
            if (this.axisY.positionEnd.GetComponent<Renderer>().sharedMaterial == this.highligh)
            {
                GL.Color(new Color(1f, 1f, 0f, 0.5f));
                this.lockDrag = true;
            }
            else
            {
                GL.Color(new Color(0f, 1f, 0f, 0.5f));
                this.lockDrag = false;
            }
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(vector2.x, vector2.y, vector2.z);
            if (this.axisZ.positionEnd.GetComponent<Renderer>().sharedMaterial == this.highligh)
            {
                GL.Color(new Color(1f, 1f, 0f, 0.5f));
                this.lockDrag = true;
            }
            else
            {
                GL.Color(new Color(0f, 0f, 1f, 0.5f));
                this.lockDrag = false;
            }
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(vector3.x, vector3.y, vector3.z);
            GL.End();
            GL.PopMatrix();
        }

        public void setParent(Transform other)
        {
            base.transform.parent = other;
            this.axisX.setParent(other);
            this.axisY.setParent(other);
            this.axisZ.setParent(other);
        }

        public void setType(GizmoHandle.GizmoType type)
        {
            this.axisX.setType(type);
            this.axisY.setType(type);
            this.axisZ.setType(type);
        }

        private void Update()
        {
            this.needUpdate = (this.axisX.needUpdate || this.axisY.needUpdate) || this.axisZ.needUpdate;
            this.change = new Vector3(this.axisX.val, this.axisY.val, this.axisZ.val);
            this.handle.Translate(this.change);
            try
            {
                base.transform.position = this.manager.center;
            }
            catch
            {
            }
        }

        public enum TransformSpace
        {
            Global,
            Local
        }
    }
}

