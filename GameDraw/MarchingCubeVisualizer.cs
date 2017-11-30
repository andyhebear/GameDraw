namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class MarchingCubeVisualizer
    {
        private float cubex;
        private float cubey;
        private float cubez;
        private Material faceMat;
        private List<short> indices;
        private List<Vector3> list;
        public Renderer renderer;
        private List<int> repCount;
        public int sizeX;
        public int sizeY;
        public int sizeZ;
        private List<Vector3> vertColor;
        private Mesh WholeObject;

        private void addNewVertex(List<Vector3> vertices, List<short> indices, Vector3 newVertex, Vector3 color)
        {
            for (short i = 0; i < vertices.Count; i = (short) (i + 1))
            {
                if (this.verticesEquals(vertices[i], newVertex))
                {
                    indices.Add(i);
                    return;
                }
            }
            indices.Add((short) vertices.Count);
            vertices.Add(newVertex);
            this.repCount.Add(1);
            this.vertColor.Add(color);
        }

        private Mesh buildMesh(List<Vector3> verts, List<short> indices)
        {
            Vector3[] vectorArray = new Vector3[verts.Count];
            for (int i = 0; i < verts.Count; i++)
            {
                vectorArray[i] = verts[i];
            }
            Mesh mesh = new Mesh();
            mesh.vertices = vectorArray;
            int[] numArray = new int[indices.Count];
            for (int j = 0; j < indices.Count; j++)
            {
                numArray[j] = indices[j];
            }
            mesh.triangles = numArray;
            mesh.RecalculateNormals();
            return mesh;
        }

        public void CalculateCubeSize(  float x=0f, float y=0f,  float z=0f)
        {
            if (x == 0f)
            {
                this.cubex = (this.renderer.bounds.max.x - this.renderer.bounds.min.x) / ((float) this.sizeX);
            }
            else
            {
                this.cubex = (this.renderer.bounds.max.x - this.renderer.bounds.min.x) / x;
            }
            if (y == 0f)
            {
                this.cubey = (this.renderer.bounds.max.y - this.renderer.bounds.min.y) / ((float) this.sizeY);
            }
            else
            {
                this.cubey = (this.renderer.bounds.max.y - this.renderer.bounds.min.y) / y;
            }
            if (z == 0f)
            {
                this.cubez = (this.renderer.bounds.max.z - this.renderer.bounds.min.z) / ((float) this.sizeZ);
            }
            else
            {
                this.cubez = (this.renderer.bounds.max.z - this.renderer.bounds.min.z) / z;
            }
        }

        public Mesh ConstructMesh()
        {
            this.WholeObject = this.buildMesh(this.list, this.indices);
            return this.WholeObject;
        }

        private bool IsBorderVoxel(int i, int j, int k, Color[,,] voxels)
        {
            float a = voxels[i, j, k].a;
            if (a == 0f)
            {
                return false;
            }
            if ((((voxels[i + 1, j, k].a >= a) && (voxels[i - 1, j, k].a >= a)) && ((voxels[i, j + 1, k].a >= a) && (voxels[i, j - 1, k].a >= a))) && (voxels[i, j, k + 1].a >= a))
            {
                return (voxels[i, j, k - 1].a < a);
            }
            return true;
        }

        public virtual bool UpdateData(Color[,,] voxels)
        {
            MarchingCubeDataBase base2 = new MarchingCubeDataBase(this.cubex, this.cubey, this.cubez);
            this.list = new List<Vector3>(0x9c40);
            this.indices = new List<short>(0x101d0);
            this.repCount = new List<int>(0x101d0);
            this.vertColor = new List<Vector3>(0x101d0);
            bool[] p = new bool[8];
            Vector3[] vectorArray = new Vector3[8];
            for (int i = 1; i < (this.sizeX - 1); i++)
            {
                for (int j = 1; j < (this.sizeY - 1); j++)
                {
                    for (int k = 1; k < (this.sizeZ - 1); k++)
                    {
                        p[0] = voxels[i, j, k].a == 1f;
                        p[1] = voxels[i + 1, j, k].a == 1f;
                        p[2] = voxels[i + 1, j, k + 1].a == 1f;
                        p[3] = voxels[i, j, k + 1].a == 1f;
                        p[4] = voxels[i, j + 1, k].a == 1f;
                        p[5] = voxels[i + 1, j + 1, k].a == 1f;
                        p[6] = voxels[i + 1, j + 1, k + 1].a == 1f;
                        p[7] = voxels[i, j + 1, k + 1].a == 1f;
                        vectorArray[0] = new Vector3(voxels[i, j, k].r, voxels[i, j, k].g, voxels[i, j, k].b);
                        vectorArray[1] = new Vector3(voxels[i + 1, j, k].r, voxels[i + 1, j, k].g, voxels[i + 1, j, k].b);
                        vectorArray[2] = new Vector3(voxels[i + 1, j, k + 1].r, voxels[i + 1, j, k + 1].g, voxels[i + 1, j, k + 1].b);
                        vectorArray[3] = new Vector3(voxels[i, j, k + 1].r, voxels[i, j, k + 1].g, voxels[i, j, k + 1].b);
                        vectorArray[4] = new Vector3(voxels[i, j + 1, k].r, voxels[i, j + 1, k].g, voxels[i, j + 1, k].b);
                        vectorArray[5] = new Vector3(voxels[i + 1, j + 1, k].r, voxels[i + 1, j + 1, k].g, voxels[i + 1, j + 1, k].b);
                        vectorArray[6] = new Vector3(voxels[i + 1, j + 1, k + 1].r, voxels[i + 1, j + 1, k + 1].g, voxels[i + 1, j + 1, k + 1].b);
                        vectorArray[7] = new Vector3(voxels[i, j + 1, k + 1].r, voxels[i, j + 1, k + 1].g, voxels[i, j + 1, k + 1].b);
                        bool[] flagArray2 = new bool[] { this.IsBorderVoxel(i, j, k, voxels), this.IsBorderVoxel(i + 1, j, k, voxels), this.IsBorderVoxel(i + 1, j, k + 1, voxels), this.IsBorderVoxel(i, j, k + 1, voxels), this.IsBorderVoxel(i, j + 1, k, voxels), this.IsBorderVoxel(i + 1, j + 1, k, voxels), this.IsBorderVoxel(i + 1, j + 1, k + 1, voxels), this.IsBorderVoxel(i, j + 1, k + 1, voxels) };
                        float x = i * this.cubex;
                        float y = j * this.cubey;
                        float z = k * this.cubez;
                        List<Vector3> list = base2.getFaces(p, x, y, z);
                        for (int m = 0; m < list.Count; m++)
                        {
                            int num8 = 0;
                            Vector3 color = new Vector3(0f, 0f, 0f);
                            for (int n = 0; n < 8; n++)
                            {
                                if (flagArray2[n])
                                {
                                    color += vectorArray[n];
                                    num8++;
                                }
                            }
                            if (num8 != 0)
                            {
                                color.x /= (float) num8;
                                color.y /= (float) num8;
                                color.z /= (float) num8;
                            }
                            this.addNewVertex(this.list, this.indices, new Vector3(list[m].x, list[m].y, list[m].z), color);
                        }
                    }
                }
            }
            return true;
        }

        public virtual void UpdateData(object voxels)
        {
            this.UpdateData((Color[,,]) voxels);
        }

        private bool verticesEquals(Vector3 v1, Vector3 v2)
        {
            Vector3 vector = v1 - v2;
            return (vector.magnitude < 1E-05);
        }

        public Mesh mesh
        {
            get
            {
                return this.WholeObject;
            }
        }
    }
}

