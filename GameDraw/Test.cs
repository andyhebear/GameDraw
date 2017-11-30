namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class Test : MonoBehaviour
    {
        private List<Voxelization.AABCGrid> aABCGrids;
        public bool createMultipleGrids = true;
        public float cubeSide = 0.1f;
        public bool drawEmptyCube;
        public bool drawMeshInside = true;
        public bool drawMeshShell = true;
        public bool includeChildren = true;
        public Vector3 meshShellPositionFromObject = Vector3.zero;

        private void Awake()
        {
        }

        private void DrawMeshShell()
        {
            if (this.aABCGrids != null)
            {
                foreach (Voxelization.AABCGrid grid in this.aABCGrids)
                {
                    if (this.drawMeshShell && (grid != null))
                    {
                        Vector3 vector = new Vector3(this.cubeSide, this.cubeSide, this.cubeSide);
                        GridSize size = grid.GetSize();
                        for (int i = 0; i < size.x; i++)
                        {
                            for (int j = 0; j < size.y; j++)
                            {
                                for (int k = 0; k < size.z; k++)
                                {
                                    Vector3 center = (grid.GetAABCCenterFromGridCenter(i, j, k) + grid.GetCenter()) + this.meshShellPositionFromObject;
                                    if (grid.IsAABCSet(i, j, k))
                                    {
                                        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                                        Gizmos.DrawCube(center, vector);
                                    }
                                    else if (this.drawEmptyCube)
                                    {
                                        Gizmos.color = new Color(0f, 1f, 0f, 1f);
                                        Gizmos.DrawCube(center, vector);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LateUpdate()
        {
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            this.DrawMeshShell();
        }

        private void Start()
        {
            if (this.createMultipleGrids && this.includeChildren)
            {
                this.aABCGrids = Voxelization.CreateMultipleGridsWithGameObjectMesh(base.gameObject, this.cubeSide, this.drawMeshInside);
            }
            else
            {
                this.aABCGrids = new List<Voxelization.AABCGrid>();
                Voxelization.AABCGrid item = Voxelization.CreateGridWithGameObjectMesh(base.gameObject, this.cubeSide, this.includeChildren, this.drawMeshInside);
                this.aABCGrids.Add(item);
            }
        }

        private void TestTriangleIntersectionAABC()
        {
            Voxelization.AABCGrid grid = new Voxelization.AABCGrid(1, 1, 1, 1f, new Vector3(1f, 1f, 1f));
            Vector3[] triangle = new Vector3[] { new Vector3(1f, 1f, 1f), new Vector3(1f, 2f, 1f), new Vector3(2f, 1f, 2f) };
            Vector3[] vectorArray2 = new Vector3[] { new Vector3(2f, 1f, 1f), new Vector3(1f, 2f, 1f), new Vector3(1f, 1f, 2f) };
            Vector3[] vectorArray3 = new Vector3[] { new Vector3(-1f, -1f, -2f), new Vector3(-1f, -1f, -2f), new Vector3(-1f, -1f, -2f) };
            MonoBehaviour.print("aabc vertices:");
            Vector3[] vectorArray4 = grid.GetAABCCorners(0, 0, 0);
            for (int i = 0; i < 8; i++)
            {
                MonoBehaviour.print(string.Concat(new object[] { "Vertex ", i, ": ", vectorArray4[i] }));
            }
            if (grid.TriangleIntersectAABC(triangle, 0, 0, 0))
            {
                MonoBehaviour.print("Triangle A intersect the AABC, Test is OK");
            }
            else
            {
                MonoBehaviour.print("Triangle A doesn't intersect the AABC, Test is NOT OK");
            }
            if (grid.TriangleIntersectAABC(vectorArray2, 0, 0, 0))
            {
                MonoBehaviour.print("Triangle B intersect the AABC, Test is OK");
            }
            else
            {
                MonoBehaviour.print("Triangle B doesn't intersect the AABC, Test is NOT OK");
            }
            if (grid.TriangleIntersectAABC(vectorArray3, 0, 0, 0))
            {
                MonoBehaviour.print("Triangle C intersect the AABC, Test is NOT OK");
            }
            else
            {
                MonoBehaviour.print("Triangle C doesn't intersect the AABC, Test is OK");
            }
        }
    }
}

