namespace GameDraw
{
    using System;
    using UnityEngine;

    public class LineDrawing : MonoBehaviour
    {
        public float handleLength = 5f;
        private static Material lineMaterial;
        public TransformSpace space;
        public Transform target;

        private static void CreateLineMaterial()
        {
            if (lineMaterial == null)
            {
                lineMaterial = new Material("Shader \"Lines/Colored Blended\" {SubShader { Pass {     Blend SrcAlpha OneMinusSrcAlpha     ZWrite Off Cull Off Fog { Mode Off }     BindChannels {      Bind \"vertex\", vertex Bind \"color\", color }} } }");
            }
        }

        private void OnPostRender()
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3 vector3;
            if (this.space == TransformSpace.Global)
            {
                vector = this.target.transform.position + ((Vector3) (Vector3.right * this.handleLength));
                vector2 = this.target.transform.position + ((Vector3) (Vector3.up * this.handleLength));
                vector3 = this.target.transform.position + ((Vector3) (Vector3.forward * this.handleLength));
            }
            else
            {
                vector = this.target.transform.position + this.target.transform.TransformDirection((Vector3) (Vector3.right * this.handleLength));
                vector2 = this.target.transform.position + this.target.transform.TransformDirection((Vector3) (Vector3.up * this.handleLength));
                vector3 = this.target.transform.position + this.target.transform.TransformDirection((Vector3) (Vector3.forward * this.handleLength));
            }
            CreateLineMaterial();
            lineMaterial.SetPass(0);
            GL.Begin(1);
            GL.Color(new Color(1f, 0f, 0f, 0.5f));
            GL.Vertex3(this.target.transform.position.x, this.target.transform.position.y, this.target.transform.position.z);
            GL.Vertex3(vector.x, vector.y, vector.z);
            GL.Color(new Color(0f, 1f, 0f, 0.5f));
            GL.Vertex3(this.target.transform.position.x, this.target.transform.position.y, this.target.transform.position.z);
            GL.Vertex3(vector2.x, vector2.y, vector2.z);
            GL.Color(new Color(0f, 0f, 1f, 0.5f));
            GL.Vertex3(this.target.transform.position.x, this.target.transform.position.y, this.target.transform.position.z);
            GL.Vertex3(vector3.x, vector3.y, vector3.z);
            GL.End();
        }

        public enum TransformSpace
        {
            Global,
            Local
        }
    }
}

