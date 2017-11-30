namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class Voxels
    {
        private float[,,] density;
        private int[] edgeTable = new int[] { 
            0, 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c, 0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00, 
            400, 0x99, 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c, 0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90, 
            560, 0x339, 0x33, 0x13a, 0x636, 0x73f, 0x435, 0x53c, 0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30, 
            0x3a0, 0x2a9, 0x1a3, 170, 0x7a6, 0x6af, 0x5a5, 0x4ac, 0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0, 
            0x460, 0x569, 0x663, 0x76a, 0x66, 0x16f, 0x265, 0x36c, 0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60, 
            0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff, 0x3f5, 0x2fc, 0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0, 
            0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55, 0x15c, 0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950, 
            0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc, 0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0, 
            0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc, 0xcc, 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0, 
            0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c, 0x15c, 0x55, 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650, 
            0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc, 0x2fc, 0x3f5, 0xff, 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0, 
            0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c, 0x36c, 0x265, 0x16f, 0x66, 0x76a, 0x663, 0x569, 0x460, 
            0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac, 0x4ac, 0x5a5, 0x6af, 0x7a6, 170, 0x1a3, 0x2a9, 0x3a0, 
            0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c, 0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33, 0x339, 560, 
            0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c, 0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99, 400, 
            0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c, 0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0
         };
        private Gridcell[,,] gridArray;
        private List<Gridcell> GridList = new List<Gridcell>();
        public float gridSize = 1f;
        public float Isolevel = 0.9f;
        public int length = 0x20;
        private int ratioX;
        private int ratioY;
        private int ratioZ;
        public List<Triangle> triangles = new List<Triangle>();
        private int[][] triTable = new int[][] { 
            new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1 }, new int[] { 8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1 }, new int[] { 3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1 }, new int[] { 4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1 }, new int[] { 4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1 }, new int[] { 9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1 }, new int[] { 10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1 }, new int[] { 5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1 }, new int[] { 5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1 }, new int[] { 8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1 }, new int[] { 2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1 }, new int[] { 2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1 }, new int[] { 11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1 }, new int[] { 5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1 }, new int[] { 11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1 }, new int[] { 11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1 }, new int[] { 2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1 }, new int[] { 6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1 }, new int[] { 3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1 }, new int[] { 6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1 }, new int[] { 6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1 }, new int[] { 8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1 }, new int[] { 7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1 }, new int[] { 3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1 }, new int[] { 0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1 }, new int[] { 9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1 }, new int[] { 8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1 }, new int[] { 5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1 }, new int[] { 0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1 }, new int[] { 6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1 }, 
            new int[] { 10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1 }, new int[] { 1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1 }, new int[] { 0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1 }, new int[] { 3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1 }, new int[] { 6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1 }, new int[] { 9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1 }, new int[] { 8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1 }, new int[] { 3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1 }, new int[] { 10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1 }, new int[] { 10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1 }, new int[] { 2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1 }, new int[] { 7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1 }, new int[] { 2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1 }, new int[] { 1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1 }, new int[] { 11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1 }, new int[] { 8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1 }, new int[] { 0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1 }, new int[] { 7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1 }, new int[] { 7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1 }, new int[] { 10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1 }, new int[] { 0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1 }, new int[] { 7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1 }, new int[] { 6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1 }, new int[] { 4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1 }, new int[] { 10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1 }, new int[] { 8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1 }, new int[] { 1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1 }, new int[] { 10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1 }, new int[] { 10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1 }, new int[] { 9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1 }, new int[] { 7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1 }, new int[] { 3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1 }, new int[] { 7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1 }, new int[] { 3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1 }, new int[] { 6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1 }, new int[] { 9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1 }, new int[] { 1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1 }, new int[] { 4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1 }, new int[] { 7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1 }, 
            new int[] { 6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1 }, new int[] { 0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1 }, new int[] { 6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1 }, new int[] { 0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1 }, new int[] { 11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1 }, new int[] { 6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1 }, new int[] { 5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1 }, new int[] { 9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1 }, new int[] { 1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1 }, new int[] { 10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1 }, new int[] { 0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1 }, new int[] { 11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1 }, new int[] { 9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1 }, new int[] { 7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1 }, new int[] { 2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1 }, new int[] { 9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1 }, new int[] { 9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1 }, new int[] { 1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1 }, new int[] { 0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1 }, new int[] { 10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1 }, new int[] { 2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1 }, new int[] { 0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1 }, new int[] { 0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1 }, new int[] { 9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1 }, new int[] { 5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1 }, new int[] { 5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1 }, new int[] { 8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1 }, new int[] { 9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1 }, new int[] { 1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1 }, new int[] { 3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1 }, new int[] { 4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1 }, new int[] { 9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1 }, new int[] { 11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1 }, new int[] { 2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1 }, new int[] { 9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1 }, new int[] { 3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1 }, new int[] { 1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1 }, new int[] { 4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, 
            new int[] { 9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1 }, new int[] { 0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1 }, new int[] { 1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { 0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
         };

        public Gridcell GenerateGridCell(int x, int y, int z)
        {
            float[] values = new float[] { this.density[x, y, z + this.ratioZ], this.density[x + this.ratioX, y, z + this.ratioZ], this.density[x + this.ratioX, y, z], this.density[x, y, z], this.density[x, y + this.ratioY, z + this.ratioZ], this.density[x + this.ratioX, y + this.ratioY, z + this.ratioZ], this.density[x + this.ratioX, y + this.ratioY, z], this.density[x, y + this.ratioY, z] };
            Vector3[] positions = new Vector3[8];
            float gridSize = this.gridSize;
            positions[0] = new Vector3(x * gridSize, y * gridSize, (z + this.ratioZ) * gridSize);
            positions[1] = new Vector3((x + this.ratioX) * gridSize, y * gridSize, (z + this.ratioZ) * gridSize);
            positions[2] = new Vector3((x + this.ratioX) * gridSize, y * gridSize, z * gridSize);
            positions[3] = new Vector3(x * gridSize, y * gridSize, z * gridSize);
            positions[4] = new Vector3(x * gridSize, (y + this.ratioY) * gridSize, (z + this.ratioZ) * gridSize);
            positions[5] = new Vector3((x + this.ratioX) * gridSize, (y + this.ratioY) * gridSize, (z + this.ratioZ) * gridSize);
            positions[6] = new Vector3((x + this.ratioX) * gridSize, (y + this.ratioY) * gridSize, z * gridSize);
            positions[7] = new Vector3(x * gridSize, (y + this.ratioY) * gridSize, z * gridSize);
            return new Gridcell(positions, values);
        }

        public List<Triangle> GenerateTriangles(float isolevel, Gridcell grid)
        {
            List<Triangle> list = new List<Triangle>();
            new List<Vector3>();
            int index = 0;
            if (grid.val[0] < isolevel)
            {
                index++;
            }
            if (grid.val[1] < isolevel)
            {
                index += 2;
            }
            if (grid.val[2] < isolevel)
            {
                index += 4;
            }
            if (grid.val[3] < isolevel)
            {
                index += 8;
            }
            if (grid.val[4] < isolevel)
            {
                index += 0x10;
            }
            if (grid.val[5] < isolevel)
            {
                index += 0x20;
            }
            if (grid.val[6] < isolevel)
            {
                index += 0x40;
            }
            if (grid.val[7] < isolevel)
            {
                index += 0x80;
            }
            if ((index != 0) && (index != 0xff))
            {
                Vector3[] vectorArray = new Vector3[12];
                if (this.IsBitSet(this.edgeTable[index], 1))
                {
                    vectorArray[0] = this.VertexInterp(isolevel, grid.p[0], grid.p[1], grid.val[0], grid.val[1]);
                }
                if (this.IsBitSet(this.edgeTable[index], 2))
                {
                    vectorArray[1] = this.VertexInterp(isolevel, grid.p[1], grid.p[2], grid.val[1], grid.val[2]);
                }
                if (this.IsBitSet(this.edgeTable[index], 4))
                {
                    vectorArray[2] = this.VertexInterp(isolevel, grid.p[2], grid.p[3], grid.val[2], grid.val[3]);
                }
                if (this.IsBitSet(this.edgeTable[index], 8))
                {
                    vectorArray[3] = this.VertexInterp(isolevel, grid.p[3], grid.p[0], grid.val[3], grid.val[0]);
                }
                if (this.IsBitSet(this.edgeTable[index], 0x10))
                {
                    vectorArray[4] = this.VertexInterp(isolevel, grid.p[4], grid.p[5], grid.val[4], grid.val[5]);
                }
                if (this.IsBitSet(this.edgeTable[index], 0x20))
                {
                    vectorArray[5] = this.VertexInterp(isolevel, grid.p[5], grid.p[6], grid.val[5], grid.val[6]);
                }
                if (this.IsBitSet(this.edgeTable[index], 0x40))
                {
                    vectorArray[6] = this.VertexInterp(isolevel, grid.p[6], grid.p[7], grid.val[6], grid.val[7]);
                }
                if (this.IsBitSet(this.edgeTable[index], 0x80))
                {
                    vectorArray[7] = this.VertexInterp(isolevel, grid.p[7], grid.p[4], grid.val[7], grid.val[4]);
                }
                if (this.IsBitSet(this.edgeTable[index], 0x100))
                {
                    vectorArray[8] = this.VertexInterp(isolevel, grid.p[0], grid.p[4], grid.val[0], grid.val[4]);
                }
                if (this.IsBitSet(this.edgeTable[index], 0x200))
                {
                    vectorArray[9] = this.VertexInterp(isolevel, grid.p[1], grid.p[5], grid.val[1], grid.val[5]);
                }
                if (this.IsBitSet(this.edgeTable[index], 0x400))
                {
                    vectorArray[10] = this.VertexInterp(isolevel, grid.p[2], grid.p[6], grid.val[2], grid.val[6]);
                }
                if (this.IsBitSet(this.edgeTable[index], 0x800))
                {
                    vectorArray[11] = this.VertexInterp(isolevel, grid.p[3], grid.p[7], grid.val[3], grid.val[7]);
                }
                for (int i = 0; this.triTable[index][i] != -1; i += 3)
                {
                    list.Add(new Triangle(vectorArray[this.triTable[index][i]], vectorArray[this.triTable[index][i + 1]], vectorArray[this.triTable[index][i + 2]]));
                }
            }
            return list;
        }

        private bool IsBitSet(int b, int pos)
        {
            return ((b & pos) == pos);
        }

        public void Load( float[,,] vals=null)
        {
            if (vals != null)
            {
                this.density = vals;
            }
            else
            {
                new System.Random();
                this.density = new float[this.length, this.length, this.length];
                for (int k = 0; k < this.length; k++)
                {
                    for (int n = 0; n < this.length; n++)
                    {
                        for (int num3 = 0; num3 < this.length; num3++)
                        {
                            this.density[k, num3, n] = 0f;
                        }
                    }
                }
                for (int m = 5; m < 8; m++)
                {
                    for (int num5 = 5; num5 < 8; num5++)
                    {
                        for (int num6 = 0; num6 < this.length; num6++)
                        {
                            this.density[m, num6, num5] = 1f;
                        }
                    }
                }
                this.density[7, 5, 7] = 0f;
            }
            this.ratioX = vals.GetLength(0) / this.length;
            this.ratioY = vals.GetLength(1) / this.length;
            this.ratioZ = vals.GetLength(2) / this.length;
            this.gridArray = new Gridcell[this.length - 1, this.length - 1, this.length - 1];
            for (int i = 0; i < (this.length - 1); i++)
            {
                for (int num8 = 0; num8 < (this.length - 1); num8++)
                {
                    for (int num9 = 0; num9 < (this.length - 1); num9++)
                    {
                        this.gridArray[i, num9, num8] = this.GenerateGridCell(i * this.ratioX, num9 * this.ratioY, num8 * this.ratioZ);
                    }
                }
            }
            Gridcell[,,] gridArray = this.gridArray;
            int upperBound = gridArray.GetUpperBound(0);
            int num11 = gridArray.GetUpperBound(1);
            int num12 = gridArray.GetUpperBound(2);
            for (int j = gridArray.GetLowerBound(0); j <= upperBound; j++)
            {
                for (int num14 = gridArray.GetLowerBound(1); num14 <= num11; num14++)
                {
                    for (int num15 = gridArray.GetLowerBound(2); num15 <= num12; num15++)
                    {
                        Gridcell grid = gridArray[j, num14, num15];
                        this.triangles.AddRange(this.Polygonise(grid, this.Isolevel));
                    }
                }
            }
        }

        public List<Triangle> Polygonise(Gridcell grid, float isolevel)
        {
            List<Triangle> list = new List<Triangle>();
            new List<Vector3>();
            Vector3[] vectorArray = new Vector3[12];
            int index = 0;
            if (grid.val[0] < isolevel)
            {
                index |= 1;
            }
            if (grid.val[1] < isolevel)
            {
                index |= 2;
            }
            if (grid.val[2] < isolevel)
            {
                index |= 4;
            }
            if (grid.val[3] < isolevel)
            {
                index |= 8;
            }
            if (grid.val[4] < isolevel)
            {
                index |= 0x10;
            }
            if (grid.val[5] < isolevel)
            {
                index |= 0x20;
            }
            if (grid.val[6] < isolevel)
            {
                index |= 0x40;
            }
            if (grid.val[7] < isolevel)
            {
                index |= 0x80;
            }
            if (this.edgeTable[index] == 0)
            {
                return new List<Triangle>();
            }
            if ((this.edgeTable[index] & 1) != 0)
            {
                vectorArray[0] = this.VertexInterp(isolevel, grid.p[0], grid.p[1], grid.val[0], grid.val[1]);
            }
            if ((this.edgeTable[index] & 2) != 0)
            {
                vectorArray[1] = this.VertexInterp(isolevel, grid.p[1], grid.p[2], grid.val[1], grid.val[2]);
            }
            if ((this.edgeTable[index] & 4) != 0)
            {
                vectorArray[2] = this.VertexInterp(isolevel, grid.p[2], grid.p[3], grid.val[2], grid.val[3]);
            }
            if ((this.edgeTable[index] & 8) != 0)
            {
                vectorArray[3] = this.VertexInterp(isolevel, grid.p[3], grid.p[0], grid.val[3], grid.val[0]);
            }
            if ((this.edgeTable[index] & 0x10) != 0)
            {
                vectorArray[4] = this.VertexInterp(isolevel, grid.p[4], grid.p[5], grid.val[4], grid.val[5]);
            }
            if ((this.edgeTable[index] & 0x20) != 0)
            {
                vectorArray[5] = this.VertexInterp(isolevel, grid.p[5], grid.p[6], grid.val[5], grid.val[6]);
            }
            if ((this.edgeTable[index] & 0x40) != 0)
            {
                vectorArray[6] = this.VertexInterp(isolevel, grid.p[6], grid.p[7], grid.val[6], grid.val[7]);
            }
            if ((this.edgeTable[index] & 0x80) != 0)
            {
                vectorArray[7] = this.VertexInterp(isolevel, grid.p[7], grid.p[4], grid.val[7], grid.val[4]);
            }
            if ((this.edgeTable[index] & 0x100) != 0)
            {
                vectorArray[8] = this.VertexInterp(isolevel, grid.p[0], grid.p[4], grid.val[0], grid.val[4]);
            }
            if ((this.edgeTable[index] & 0x200) != 0)
            {
                vectorArray[9] = this.VertexInterp(isolevel, grid.p[1], grid.p[5], grid.val[1], grid.val[5]);
            }
            if ((this.edgeTable[index] & 0x400) != 0)
            {
                vectorArray[10] = this.VertexInterp(isolevel, grid.p[2], grid.p[6], grid.val[2], grid.val[6]);
            }
            if ((this.edgeTable[index] & 0x800) != 0)
            {
                vectorArray[11] = this.VertexInterp(isolevel, grid.p[3], grid.p[7], grid.val[3], grid.val[7]);
            }
            int num2 = 0;
            for (int i = 0; this.triTable[index][i] != -1; i += 3)
            {
                list.Add(new Triangle(vectorArray[this.triTable[index][i]], vectorArray[this.triTable[index][i + 1]], vectorArray[this.triTable[index][i + 2]]));
                num2++;
            }
            return list;
        }

        public void Update()
        {
            for (int i = 0; i < (this.length - 1); i++)
            {
                for (int j = 0; j < (this.length - 1); j++)
                {
                    for (int k = 0; k < (this.length - 1); k++)
                    {
                        this.gridArray[i, k, j].val[0] = this.density[i, k, j + 1];
                        this.gridArray[i, k, j].val[1] = this.density[i + 1, k, j + 1];
                        this.gridArray[i, k, j].val[2] = this.density[i + 1, k, j];
                        this.gridArray[i, k, j].val[3] = this.density[i, k, j];
                        this.gridArray[i, k, j].val[4] = this.density[i, k + 1, j + 1];
                        this.gridArray[i, k, j].val[5] = this.density[i + 1, k + 1, j + 1];
                        this.gridArray[i, k, j].val[6] = this.density[i + 1, k + 1, j];
                        this.gridArray[i, k, j].val[7] = this.density[i, k + 1, j];
                    }
                }
            }
        }

        private Vector3 VertexInterp(float isolevel, Vector3 p1, Vector3 p2, float valp1, float valp2)
        {
            Vector3 vector;
            if (Mathf.Abs((float) (isolevel - valp1)) < 1E-05f)
            {
                return p1;
            }
            if (Mathf.Abs((float) (isolevel - valp2)) < 1E-05f)
            {
                return p2;
            }
            if (Mathf.Abs((float) (valp1 - valp2)) < 1E-05f)
            {
                return p1;
            }
            float num = (isolevel - valp1) / (valp2 - valp1);
            vector.x = p1.x + (num * (p2.x - p1.x));
            vector.y = p1.y + (num * (p2.y - p1.y));
            vector.z = p1.z + (num * (p2.z - p1.z));
            return vector;
        }
    }
}

