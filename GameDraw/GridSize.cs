namespace GameDraw
{
    using System;

    public class GridSize
    {
        public float side;
        public int x;
        public int y;
        public int z;

        public GridSize(GridSize gridSize)
        {
            this.x = gridSize.x;
            this.y = gridSize.y;
            this.z = gridSize.z;
            this.side = gridSize.side;
        }

        public GridSize(int x, int y, int z, float side)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.side = side;
        }
    }
}

