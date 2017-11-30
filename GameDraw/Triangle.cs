namespace GameDraw
{
    using System;
    using UnityEngine;

    public class Triangle
    {
        public Vector3 pointOne;
        public Vector3 pointThree;
        public Vector3 pointTwo;

        public Triangle(Vector3 PointOne, Vector3 PointTwo, Vector3 PointThree)
        {
            this.pointOne = PointOne;
            this.pointTwo = PointTwo;
            this.pointThree = PointThree;
        }
    }
}

