namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static class Voxelization
    {
        private static float MAX_FLOAT = 1E+16f;
        private static float MIN_FLOAT = -1E+16f;

        public static AABCGrid CreateGridWithGameObjectMesh(GameObject gameObj, float cubeSide, bool includeChildren, bool includeInside)
        {
            Vector3 ori = new Vector3();
            List<GameObject> childrenWithMesh = new List<GameObject>();
            Vector3 a = new Vector3(MAX_FLOAT, MAX_FLOAT, MAX_FLOAT);
            Vector3 vector3 = new Vector3(MIN_FLOAT, MIN_FLOAT, MIN_FLOAT);
            if (includeChildren)
            {
                childrenWithMesh = GetChildrenWithMesh(gameObj);
            }
            else
            {
                childrenWithMesh = new List<GameObject>();
            }
            if (gameObj.GetComponent<Renderer>() != null)
            {
                childrenWithMesh.Add(gameObj);
            }
            foreach (GameObject obj2 in childrenWithMesh)
            {
                a = MinVectorComponents(a, obj2.GetComponent<Renderer>().bounds.min);
                vector3 = MaxVectorComponents(vector3, obj2.GetComponent<Renderer>().bounds.max);
            }
            int x = (int) (Mathf.Ceil((vector3.x - a.x) / cubeSide) + 2f);
            int y = (int) (Mathf.Ceil((vector3.y - a.y) / cubeSide) + 2f);
            int z = (int) (Mathf.Ceil((vector3.z - a.z) / cubeSide) + 2f);
            ori = a - new Vector3(cubeSide, cubeSide, cubeSide);
            AABCGrid grid = new AABCGrid(x, y, z, cubeSide, ori);
            foreach (GameObject obj3 in childrenWithMesh)
            {
                if (includeInside)
                {
                    grid.FillGridWithGameObjectMesh(obj3);
                }
                else
                {
                    grid.FillGridWithGameObjectMeshShell(obj3);
                }
            }
            return grid;
        }

        public static List<AABCGrid> CreateMultipleGridsWithGameObjectMesh(GameObject gameObj, float cubeSide, bool includeMeshInside)
        {
            List<GameObject> childrenWithMesh = new List<GameObject>();
            List<AABCGrid> list2 = new List<AABCGrid>();
            childrenWithMesh = GetChildrenWithMesh(gameObj);
            if (gameObj.GetComponent<Renderer>() != null)
            {
                childrenWithMesh.Add(gameObj);
            }
            foreach (GameObject obj2 in childrenWithMesh)
            {
                list2.Add(CreateGridWithGameObjectMesh(obj2, cubeSide, false, includeMeshInside));
            }
            return list2;
        }

        public static List<GameObject> GetChildrenWithMesh(GameObject gameObj)
        {
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < gameObj.transform.GetChildCount(); i++)
            {
                GameObject gameObject = gameObj.transform.GetChild(i).gameObject;
                if (gameObject.GetComponent<Renderer>() != null)
                {
                    list.Add(gameObject);
                }
                list.AddRange(GetChildrenWithMesh(gameObject));
            }
            return list;
        }

        public static Bounds GetTotalBoundsOfGameObject(GameObject gameObj)
        {
            Bounds bounds = new Bounds();
            Vector3 zero = Vector3.zero;
            Vector3 a = Vector3.zero;
            if (gameObj.GetComponent<Renderer>() != null)
            {
                zero = gameObj.GetComponent<Renderer>().bounds.min;
                a = gameObj.GetComponent<Renderer>().bounds.max;
            }
            for (int i = 0; i < gameObj.transform.GetChildCount(); i++)
            {
                Bounds totalBoundsOfGameObject = GetTotalBoundsOfGameObject(gameObj.transform.GetChild(i).gameObject);
                zero = MinVectorComponents(zero, totalBoundsOfGameObject.min);
                a = MaxVectorComponents(a, totalBoundsOfGameObject.max);
            }
            bounds.SetMinMax(zero, a);
            return bounds;
        }

        public static Vector3 GetTriangleNormal(Vector3[] triangle)
        {
            return Vector3.Cross(triangle[1] - triangle[0], triangle[2] - triangle[0]).normalized;
        }

        public static float MaxOfProjectionOnAxis(Vector3[] solid, Vector3 axis)
        {
            float num = MIN_FLOAT;
            for (int i = 0; i < solid.Length; i++)
            {
                float num2 = Vector3.Dot(solid[i], axis);
                if (num2 > num)
                {
                    num = num2;
                }
            }
            return num;
        }

        public static Vector3 MaxVectorComponents(Vector3 a, Vector3 b)
        {
            Vector3 vector = new Vector3();
            vector.x = Mathf.Max(a.x, b.x);
            vector.y = Mathf.Max(a.y, b.y);
            vector.z = Mathf.Max(a.z, b.z);
            return vector;
        }

        public static float MinOfProjectionOnAxis(Vector3[] solid, Vector3 axis)
        {
            float num = MAX_FLOAT;
            for (int i = 0; i < solid.Length; i++)
            {
                float num2 = Vector3.Dot(solid[i], axis);
                if (num2 < num)
                {
                    num = num2;
                }
            }
            return num;
        }

        public static Vector3 MinVectorComponents(Vector3 a, Vector3 b)
        {
            Vector3 vector = new Vector3();
            vector.x = Mathf.Min(a.x, b.x);
            vector.y = Mathf.Min(a.y, b.y);
            vector.z = Mathf.Min(a.z, b.z);
            return vector;
        }

        public static bool ProjectionsIntersectOnAxis(Vector3[] solidA, Vector3[] solidB, Vector3 axis)
        {
            float num = MinOfProjectionOnAxis(solidA, axis);
            float num2 = MaxOfProjectionOnAxis(solidA, axis);
            float num3 = MinOfProjectionOnAxis(solidB, axis);
            float num4 = MaxOfProjectionOnAxis(solidB, axis);
            if (num > num4)
            {
                return false;
            }
            if (num2 < num3)
            {
                return false;
            }
            return true;
        }

        public class AABCGrid
        {
            private int[,,] cubeNormalSum;
            private bool[,,] cubeSet;
            private bool debug;
            private int depth;
            private int height;
            private Vector3 origin;
            private float side;
            private int width;

            public AABCGrid(int x, int y, int z, float sideLength)
            {
                this.width = x;
                this.height = y;
                this.depth = z;
                this.side = sideLength;
                this.origin = new Vector3();
                this.cubeSet = new bool[this.width, this.height, this.depth];
            }

            public AABCGrid(int x, int y, int z, float sideLength, Vector3 ori)
            {
                this.width = x;
                this.height = y;
                this.depth = z;
                this.side = sideLength;
                this.origin = ori;
                this.cubeSet = new bool[this.width, this.height, this.depth];
            }

            public ParticleSystem.Particle[] AddParticles(ParticleSystem.Particle[] particles, int particlesToAdd)
            {
                int setAABCCount = this.GetSetAABCCount();
                int max = ((int) this.side) / 2;
                int num4 = 0;
                int index = 0;
                if (particlesToAdd <= 0)
                {
                    throw new ArgumentException("The number of particles to add is < 0");
                }
                int num2 = particlesToAdd / setAABCCount;
                if (num2 <= 0)
                {
                    num2 = 1;
                }
                while (particlesToAdd > 0)
                {
                    AABCGridSetAABCIterator iterator = new AABCGridSetAABCIterator(this);
                    int num6 = 0;
                    while (iterator.HasNext())
                    {
                        AABC aabc = iterator.Next();
                        while ((index < (num4 + num2)) && (particlesToAdd > 0))
                        {
                            particles[index].position = aabc.GetCenter() + ((Vector3) (new Vector3((float) UnityEngine.Random.Range(-max, max), (float) UnityEngine.Random.Range(-max, max), (float) UnityEngine.Random.Range(-max, max)) / 100f));
                            particlesToAdd--;
                            index++;
                        }
                        num4 += num2;
                        num6++;
                        if (particlesToAdd <= 0)
                        {
                            break;
                        }
                    }
                    if (particlesToAdd > 0)
                    {
                        num2 = 1;
                    }
                }
                return particles;
            }

            protected void CheckBounds(int x, int y, int z)
            {
                if ((((x < 0) || (y < 0)) || ((z < 0) || (x >= this.width))) || ((y >= this.height) || (z >= this.depth)))
                {
                    throw new ArgumentOutOfRangeException("The requested AABC is out of the grid limits.");
                }
            }

            public void CleanGrid()
            {
                this.cubeSet = new bool[this.width, this.height, this.depth];
            }

            public void FillGridWithGameObjectMesh(GameObject gameObj)
            {
                this.FillGridWithGameObjectMeshShell(gameObj, true);
                for (int i = 0; i < this.width; i++)
                {
                    for (int j = 0; j < this.height; j++)
                    {
                        bool flag = false;
                        int num3 = 0;
                        for (int k = 0; k < this.depth; k++)
                        {
                            if (this.cubeSet[i, j, k])
                            {
                                int num5 = this.cubeNormalSum[i, j, k];
                                if (num5 != 0)
                                {
                                    if (num5 > 0)
                                    {
                                        flag = true;
                                    }
                                    else
                                    {
                                        flag = false;
                                        while (num3 > 1)
                                        {
                                            num3--;
                                            this.cubeSet[i, j, k - num3] = true;
                                        }
                                    }
                                    num3 = 0;
                                }
                            }
                            else if (flag)
                            {
                                num3++;
                            }
                        }
                    }
                }
                this.cubeNormalSum = null;
            }

            public void FillGridWithGameObjectMeshShell(GameObject gameObj)
            {
                this.FillGridWithGameObjectMeshShell(gameObj, false);
            }

            public void FillGridWithGameObjectMeshShell(GameObject gameObj, bool storeNormalSum)
            {
                Mesh mesh = gameObj.GetComponent<MeshFilter>().mesh;
                Transform transform = gameObj.transform;
                Vector3[] triangle = new Vector3[3];
                float realtimeSinceStartup = Time.realtimeSinceStartup;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                int num2 = triangles.Length / 3;
                int num6 = 0;
                if (storeNormalSum)
                {
                    this.cubeNormalSum = new int[this.width, this.height, this.depth];
                }
                if (this.debug)
                {
                    Debug.Log("Start:");
                    Debug.Log("Time: " + realtimeSinceStartup);
                    Debug.Log("\t\tMesh Description: ");
                    Debug.Log("Name: " + mesh.name);
                    Debug.Log("Triangles: " + num2);
                    Debug.Log("Local AABB size: " + mesh.bounds.size);
                    Debug.Log("\t\tAABCGrid Description:");
                    Debug.Log(string.Concat(new object[] { "Size: ", this.width, ',', this.height, ',', this.depth }));
                }
                for (int i = 0; i < num2; i++)
                {
                    int num3;
                    int num4;
                    int num5;
                    triangle[0] = transform.TransformPoint(vertices[triangles[i * 3]]);
                    triangle[1] = transform.TransformPoint(vertices[triangles[(i * 3) + 1]]);
                    triangle[2] = transform.TransformPoint(vertices[triangles[(i * 3) + 2]]);
                    float num8 = Mathf.Floor((Mathf.Min(new float[] { triangle[0].x, triangle[1].x, triangle[2].x }) - this.origin.x) / this.side);
                    float num9 = Mathf.Floor((Mathf.Min(new float[] { triangle[0].y, triangle[1].y, triangle[2].y }) - this.origin.y) / this.side);
                    float num10 = Mathf.Floor((Mathf.Min(new float[] { triangle[0].z, triangle[1].z, triangle[2].z }) - this.origin.z) / this.side);
                    float num11 = Mathf.Ceil((Mathf.Max(new float[] { triangle[0].x, triangle[1].x, triangle[2].x }) - this.origin.x) / this.side);
                    float num12 = Mathf.Ceil((Mathf.Max(new float[] { triangle[0].y, triangle[1].y, triangle[2].y }) - this.origin.y) / this.side);
                    float num13 = Mathf.Ceil((Mathf.Max(new float[] { triangle[0].z, triangle[1].z, triangle[2].z }) - this.origin.z) / this.side);
                    if (storeNormalSum)
                    {
                        num3 = (int) num8;
                        while (num3 <= num11)
                        {
                            num4 = (int) num9;
                            while (num4 <= num12)
                            {
                                num5 = (int) num10;
                                while (num5 <= num13)
                                {
                                    if (this.TriangleIntersectAABC(triangle, num3, num4, num5))
                                    {
                                        Vector3 triangleNormal = Voxelization.GetTriangleNormal(triangle);
                                        this.cubeSet[num3, num4, num5] = true;
                                        if (triangleNormal.z < -num6)
                                        {
                                            this.cubeNormalSum[num3, num4, num5]++;
                                        }
                                        else if (triangleNormal.z > num6)
                                        {
                                            this.cubeNormalSum[num3, num4, num5]--;
                                        }
                                    }
                                    num5++;
                                }
                                num4++;
                            }
                            num3++;
                        }
                    }
                    else
                    {
                        for (num3 = (int) num8; num3 < num11; num3++)
                        {
                            for (num4 = (int) num9; num4 < num12; num4++)
                            {
                                for (num5 = (int) num10; num5 < num13; num5++)
                                {
                                    if (!this.IsAABCSet(num3, num4, num5) && this.TriangleIntersectAABC(triangle, num3, num4, num5))
                                    {
                                        this.cubeSet[num3, num4, num5] = true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (this.debug)
                {
                    Debug.Log("Grid Evaluation Ended!");
                    Debug.Log("Time spent: " + (Time.realtimeSinceStartup - realtimeSinceStartup) + "s");
                    Debug.Log("End: ");
                }
            }

            public Vector3 GetAABCCenter(AABCPosition pos)
            {
                this.CheckBounds(pos.x, pos.y, pos.z);
                return this.GetAABCCenterUnchecked(pos.x, pos.y, pos.z);
            }

            public Vector3 GetAABCCenter(int x, int y, int z)
            {
                this.CheckBounds(x, y, z);
                return this.GetAABCCenterUnchecked(x, y, z);
            }

            public Vector3 GetAABCCenterFromGridCenter(AABCPosition pos)
            {
                this.CheckBounds(pos.x, pos.y, pos.z);
                return this.GetAABCCenterFromGridCenterUnchecked(pos.x, pos.y, pos.z);
            }

            public Vector3 GetAABCCenterFromGridCenter(int x, int y, int z)
            {
                this.CheckBounds(x, y, z);
                return this.GetAABCCenterFromGridCenterUnchecked(x, y, z);
            }

            protected Vector3 GetAABCCenterFromGridCenterUnchecked(int x, int y, int z)
            {
                return new Vector3(this.side * (x - (this.width / 2)), this.side * (y - (this.height / 2)), this.side * (z - (this.depth / 2)));
            }

            public Vector3 GetAABCCenterFromGridOrigin(AABCPosition pos)
            {
                this.CheckBounds(pos.x, pos.y, pos.z);
                return this.GetAABCCenterFromGridOriginUnchecked(pos.x, pos.y, pos.z);
            }

            public Vector3 GetAABCCenterFromGridOrigin(int x, int y, int z)
            {
                this.CheckBounds(x, y, z);
                return this.GetAABCCenterFromGridOriginUnchecked(x, y, z);
            }

            protected Vector3 GetAABCCenterFromGridOriginUnchecked(int x, int y, int z)
            {
                return new Vector3((x * this.side) + (this.side / 2f), (y * this.side) + (this.side / 2f), (z * this.side) + (this.side / 2f));
            }

            protected Vector3 GetAABCCenterUnchecked(int x, int y, int z)
            {
                return (this.GetAABCCenterFromGridCenterUnchecked(x, y, z) + this.GetCenter());
            }

            public Vector3[] GetAABCCorners(AABCPosition pos)
            {
                this.CheckBounds(pos.x, pos.y, pos.z);
                return this.GetAABCCornersUnchecked(pos.x, pos.y, pos.z);
            }

            public Vector3[] GetAABCCorners(int x, int y, int z)
            {
                return this.GetAABCCornersUnchecked(x, y, z);
            }

            protected Vector3[] GetAABCCornersUnchecked(int x, int y, int z)
            {
                Vector3 vector = new Vector3((x * this.side) + (this.side / 2f), (y * this.side) + (this.side / 2f), (z * this.side) + (this.side / 2f));
                return new Vector3[] { (new Vector3(vector.x + this.side, vector.y - this.side, vector.z + this.side) + this.origin), (new Vector3(vector.x + this.side, vector.y - this.side, vector.z - this.side) + this.origin), (new Vector3(vector.x - this.side, vector.y - this.side, vector.z - this.side) + this.origin), (new Vector3(vector.x - this.side, vector.y - this.side, vector.z + this.side) + this.origin), (new Vector3(vector.x + this.side, vector.y + this.side, vector.z + this.side) + this.origin), (new Vector3(vector.x + this.side, vector.y + this.side, vector.z - this.side) + this.origin), (new Vector3(vector.x - this.side, vector.y + this.side, vector.z - this.side) + this.origin), (new Vector3(vector.x - this.side, vector.y + this.side, vector.z + this.side) + this.origin) };
            }

            public Vector3 GetCenter()
            {
                return (this.origin + new Vector3((this.width / 2) * this.side, (this.height / 2) * this.side, (this.depth / 2) * this.side));
            }

            public int GetSetAABCCount()
            {
                int num = 0;
                for (int i = 0; i < this.width; i++)
                {
                    for (int j = 0; j < this.height; j++)
                    {
                        for (int k = 0; k < this.depth; k++)
                        {
                            if (!this.IsAABCSet(i, j, k))
                            {
                                num++;
                            }
                        }
                    }
                }
                return num;
            }

            public GridSize GetSize()
            {
                return new GridSize(this.width, this.height, this.depth, this.side);
            }

            public bool IsAABCSet(AABCPosition pos)
            {
                this.CheckBounds(pos.x, pos.y, pos.z);
                return this.IsAABCSetUnchecked(pos.x, pos.y, pos.z);
            }

            public bool IsAABCSet(int x, int y, int z)
            {
                this.CheckBounds(x, y, z);
                return this.IsAABCSetUnchecked(x, y, z);
            }

            protected bool IsAABCSetUnchecked(int x, int y, int z)
            {
                return this.cubeSet[x, y, z];
            }

            public void SetCenter(Vector3 pos)
            {
                this.origin = pos - new Vector3((this.width / 2) * this.side, (this.height / 2) * this.side, (this.depth / 2) * this.side);
            }

            public void SetDebug(bool debug)
            {
                this.debug = debug;
            }

            public void SetSize(GridSize dimension)
            {
                this.SetSize(dimension.x, dimension.y, dimension.z, dimension.side);
            }

            public void SetSize(int x, int y, int z, float sideLength)
            {
                this.width = x;
                this.height = y;
                this.depth = z;
                this.side = sideLength;
                this.CleanGrid();
            }

            public bool TriangleIntersectAABC(Vector3[] triangle, AABCPosition pos)
            {
                this.CheckBounds(pos.x, pos.y, pos.z);
                return this.TriangleIntersectAABCUnchecked(triangle, pos.x, pos.y, pos.z);
            }

            public bool TriangleIntersectAABC(Vector3[] triangle, int x, int y, int z)
            {
                this.CheckBounds(x, y, z);
                return this.TriangleIntersectAABCUnchecked(triangle, x, y, z);
            }

            protected bool TriangleIntersectAABCUnchecked(Vector3[] triangle, int x, int y, int z)
            {
                Vector3 rhs = new Vector3(1f, 0f, 0f);
                Vector3 vector6 = new Vector3(0f, 1f, 0f);
                Vector3 vector7 = new Vector3(0f, 0f, 1f);
                Vector3[] solidA = this.GetAABCCornersUnchecked(x, y, z);
                Vector3 lhs = triangle[1] - triangle[0];
                Vector3 vector2 = triangle[2] - triangle[1];
                Vector3 vector3 = triangle[0] - triangle[2];
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, Vector3.Cross(lhs, rhs)))
                {
                    return false;
                }
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, Vector3.Cross(lhs, vector6)))
                {
                    return false;
                }
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, Vector3.Cross(lhs, vector7)))
                {
                    return false;
                }
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, Vector3.Cross(vector2, rhs)))
                {
                    return false;
                }
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, Vector3.Cross(vector2, vector6)))
                {
                    return false;
                }
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, Vector3.Cross(vector2, vector7)))
                {
                    return false;
                }
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, Vector3.Cross(vector3, rhs)))
                {
                    return false;
                }
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, Vector3.Cross(vector3, vector6)))
                {
                    return false;
                }
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, Vector3.Cross(vector3, vector7)))
                {
                    return false;
                }
                Vector3 axis = Vector3.Cross(lhs, vector2);
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, axis))
                {
                    return false;
                }
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, rhs))
                {
                    return false;
                }
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, vector6))
                {
                    return false;
                }
                if (!Voxelization.ProjectionsIntersectOnAxis(solidA, triangle, vector7))
                {
                    return false;
                }
                return true;
            }

            public class AABC : Voxelization.AABCGrid.AABCPosition
            {
                private Voxelization.AABCGrid grid;

                public AABC(Voxelization.AABCGrid.AABC aABC) : base(aABC.x, aABC.y, aABC.z)
                {
                    this.grid = aABC.grid;
                }

                public AABC(Voxelization.AABCGrid.AABCPosition position, Voxelization.AABCGrid grid) : base(position.x, position.y, position.z)
                {
                    this.grid = grid;
                }

                public AABC(int x, int y, int z, Voxelization.AABCGrid grid) : base(x, y, z)
                {
                    base.x = x;
                    base.y = y;
                    base.z = z;
                    this.grid = grid;
                }

                public Vector3 GetCenter()
                {
                    return this.grid.GetAABCCenter(base.x, base.y, base.z);
                }

                public Vector3[] GetCorners(int x, int y, int z)
                {
                    return this.grid.GetAABCCorners(x, y, z);
                }

                public Voxelization.AABCGrid GetGrid()
                {
                    return this.grid;
                }

                public bool IsSet()
                {
                    return this.grid.IsAABCSet(base.x, base.y, base.z);
                }
            }

            public class AABCGridIterator : Voxelization.AABCGrid.AABCGridIteratorBase
            {
                public AABCGridIterator(Voxelization.AABCGrid grid) : base(grid)
                {
                }

                public bool HasNext()
                {
                    if (((base.position.x == base.grid.width) && (base.position.y == base.grid.height)) && (base.position.z == base.grid.depth))
                    {
                        return false;
                    }
                    return true;
                }

                public Voxelization.AABCGrid.AABC Next()
                {
                    base.position.z++;
                    if (base.position.z == base.grid.depth)
                    {
                        base.position.z = 0;
                        base.position.y++;
                        if (base.position.y == base.grid.height)
                        {
                            base.position.y = 0;
                            base.position.x++;
                            if (base.position.x > base.grid.width)
                            {
                                throw new IndexOutOfRangeException();
                            }
                        }
                    }
                    return new Voxelization.AABCGrid.AABC(base.position, base.grid);
                }
            }

            public class AABCGridIteratorBase
            {
                protected Voxelization.AABCGrid grid;
                protected Voxelization.AABCGrid.AABCPosition position;

                protected AABCGridIteratorBase(Voxelization.AABCGrid grid)
                {
                    this.grid = grid;
                }

                public bool HasNext()
                {
                    throw new NotImplementedException();
                }

                public Voxelization.AABCGrid.AABC Next()
                {
                    throw new NotImplementedException();
                }
            }

            public class AABCGridSetAABCIterator : Voxelization.AABCGrid.AABCGridIteratorBase
            {
                private bool nextFound;
                private Voxelization.AABCGrid.AABCPosition nextPosition;

                public AABCGridSetAABCIterator(Voxelization.AABCGrid grid) : base(grid)
                {
                    base.position = new Voxelization.AABCGrid.AABCPosition(0, 0, 0);
                    if (grid.IsAABCSet(base.position))
                    {
                        this.nextPosition = base.position;
                    }
                    this.nextFound = true;
                }

                private bool FindNext()
                {
                    this.nextPosition = new Voxelization.AABCGrid.AABCPosition(base.position);
                    this.nextPosition.z++;
                    while (this.nextPosition.x < base.grid.width)
                    {
                        while (this.nextPosition.y < base.grid.height)
                        {
                            while (this.nextPosition.z < base.grid.depth)
                            {
                                if (base.grid.IsAABCSet(this.nextPosition.x, this.nextPosition.y, this.nextPosition.z))
                                {
                                    this.nextFound = true;
                                    return true;
                                }
                                this.nextPosition.z++;
                            }
                            this.nextPosition.z = 0;
                            this.nextPosition.y++;
                        }
                        this.nextPosition.y = 0;
                        this.nextPosition.x++;
                    }
                    this.nextFound = false;
                    return false;
                }

                public bool HasNext()
                {
                    if (!this.nextFound)
                    {
                        return this.FindNext();
                    }
                    return true;
                }

                public Voxelization.AABCGrid.AABC Next()
                {
                    if (!this.nextFound)
                    {
                        this.FindNext();
                    }
                    base.position = this.nextPosition;
                    this.nextFound = false;
                    return new Voxelization.AABCGrid.AABC(base.position, base.grid);
                }
            }

            public class AABCPosition
            {
                public int x;
                public int y;
                public int z;

                public AABCPosition()
                {
                }

                public AABCPosition(Voxelization.AABCGrid.AABCPosition aABCPosition)
                {
                    this.x = aABCPosition.x;
                    this.y = aABCPosition.y;
                    this.z = aABCPosition.z;
                }

                public AABCPosition(int x, int y, int z)
                {
                    this.x = x;
                    this.y = y;
                    this.z = z;
                }
            }
        }
    }
}

