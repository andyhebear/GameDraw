namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class MeshBuffer
    {
        public List<Matrix4x4> bindposes;
        public List<BoneWeight> boneweights;
        public List<Color> colors;
        public List<Vector3> normals;
        public MeshSelection selection;
        public List<Vector4> tangents;
        public List<SubMesh> triangles;
        public List<Vector2> uv;
        public List<Vector2> uv1;
        public List<Vector2> uv2;
        public List<Vector3> vertices;

        public MeshBuffer()
        {
            this.vertices = new List<Vector3>();
            this.triangles = new List<SubMesh>();
            this.normals = new List<Vector3>();
            this.tangents = new List<Vector4>();
            this.uv = new List<Vector2>();
            this.uv1 = new List<Vector2>();
            this.uv2 = new List<Vector2>();
            this.colors = new List<Color>();
            this.boneweights = new List<BoneWeight>();
            this.bindposes = new List<Matrix4x4>();
        }

        public MeshBuffer(MeshBuffer original)
        {
            this.vertices = new List<Vector3>(original.vertices);
            this.triangles = new List<SubMesh>(original.triangles);
            this.normals = new List<Vector3>(original.normals);
            this.tangents = new List<Vector4>(original.tangents);
            this.uv = new List<Vector2>(original.uv);
            this.uv1 = new List<Vector2>(original.uv1);
            this.uv2 = new List<Vector2>(original.uv2);
            this.colors = new List<Color>(original.colors);
            this.boneweights = new List<BoneWeight>(original.boneweights);
            this.bindposes = new List<Matrix4x4>(original.bindposes);
            this.selection = original.selection;
        }

        public MeshBuffer(Mesh m)
        {
            this.vertices = new List<Vector3>(m.vertices);
            this.triangles = new List<SubMesh>();
            if (m.subMeshCount == 1)
            {
                SubMesh item = new SubMesh();
                item.triangles = new List<int>(m.triangles);
                this.triangles.Add(item);
            }
            else
            {
                for (int i = 0; i < m.subMeshCount; i++)
                {
                    SubMesh mesh2 = new SubMesh();
                    mesh2.triangles = new List<int>(m.GetTriangles(i));
                    this.triangles.Add(mesh2);
                }
            }
            if ((m.normals != null) && (m.normals.Length > 0))
            {
                this.normals = new List<Vector3>(m.normals);
            }
            else
            {
                this.normals = new List<Vector3>();
            }
            if ((m.tangents != null) && (m.tangents.Length > 0))
            {
                this.tangents = new List<Vector4>(m.tangents);
            }
            else
            {
                this.tangents = new List<Vector4>();
            }
            if ((m.uv != null) && (m.uv.Length > 0))
            {
                this.uv = new List<Vector2>(m.uv);
            }
            else
            {
                this.uv = new List<Vector2>();
            }
            if ((m.uv2 != null) && (m.uv2.Length > 0))
            {
                this.uv1 = new List<Vector2>(m.uv2);
            }
            else
            {
                this.uv1 = new List<Vector2>();
            }
            if ((m.uv2 != null) && (m.uv2.Length > 0))
            {
                this.uv2 = new List<Vector2>(m.uv2);
            }
            else
            {
                this.uv2 = new List<Vector2>();
            }
            if ((m.colors != null) && (m.colors.Length > 0))
            {
                this.colors = new List<Color>(m.colors);
            }
            else
            {
                this.colors = new List<Color>();
            }
            if ((m.boneWeights != null) && (m.boneWeights.Length > 0))
            {
                this.boneweights = new List<BoneWeight>(m.boneWeights);
            }
            else
            {
                this.boneweights = new List<BoneWeight>();
            }
            if ((m.bindposes != null) && (m.bindposes.Length > 0))
            {
                this.bindposes = new List<Matrix4x4>(m.bindposes);
            }
            else
            {
                this.bindposes = new List<Matrix4x4>();
            }
        }

        public void FillMesh(Mesh m)
        {
            m.Clear();
            m.vertices = this.vertices.ToArray();
            m.normals = this.normals.ToArray();
            m.tangents = this.tangents.ToArray();
            m.uv = this.uv.ToArray();
            m.uv2 = this.uv1.ToArray();
            m.uv2 = this.uv2.ToArray();
            m.colors = this.colors.ToArray();
            m.boneWeights = this.boneweights.ToArray();
            m.bindposes = this.bindposes.ToArray();
            m.subMeshCount = this.triangles.Count;
            int submesh = 0;
            foreach (SubMesh mesh in this.triangles)
            {
                m.SetTriangles(mesh.triangles.ToArray(), submesh);
                submesh++;
            }
            m.RecalculateBounds();
        }

        public void Update(Mesh m)
        {
            this.vertices = new List<Vector3>(m.vertices);
            this.triangles = new List<SubMesh>();
            if (m.subMeshCount == 1)
            {
                SubMesh item = new SubMesh();
                item.triangles = new List<int>(m.triangles);
                this.triangles.Add(item);
            }
            else
            {
                for (int i = 0; i < m.subMeshCount; i++)
                {
                    SubMesh mesh2 = new SubMesh();
                    mesh2.triangles = new List<int>(m.GetTriangles(i));
                    this.triangles.Add(mesh2);
                }
            }
            if ((m.normals != null) && (m.normals.Length > 0))
            {
                this.normals = new List<Vector3>(m.normals);
            }
            else
            {
                this.normals = new List<Vector3>();
            }
            if ((m.tangents != null) && (m.tangents.Length > 0))
            {
                this.tangents = new List<Vector4>(m.tangents);
            }
            else
            {
                this.tangents = new List<Vector4>();
            }
            if ((m.uv != null) && (m.uv.Length > 0))
            {
                this.uv = new List<Vector2>(m.uv);
            }
            else
            {
                this.uv = new List<Vector2>();
            }
            if ((m.uv2 != null) && (m.uv2.Length > 0))
            {
                this.uv1 = new List<Vector2>(m.uv2);
            }
            else
            {
                this.uv1 = new List<Vector2>();
            }
            if ((m.uv2 != null) && (m.uv2.Length > 0))
            {
                this.uv2 = new List<Vector2>(m.uv2);
            }
            else
            {
                this.uv2 = new List<Vector2>();
            }
            if ((m.colors != null) && (m.colors.Length > 0))
            {
                this.colors = new List<Color>(m.colors);
            }
            else
            {
                this.colors = new List<Color>();
            }
            if ((m.boneWeights != null) && (m.boneWeights.Length > 0))
            {
                this.boneweights = new List<BoneWeight>(m.boneWeights);
            }
            else
            {
                this.boneweights = new List<BoneWeight>();
            }
            if ((m.bindposes != null) && (m.bindposes.Length > 0))
            {
                this.bindposes = new List<Matrix4x4>(m.bindposes);
            }
            else
            {
                this.bindposes = new List<Matrix4x4>();
            }
        }
    }
}

