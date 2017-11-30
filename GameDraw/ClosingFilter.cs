namespace GameDraw
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class ClosingFilter
    {
        public static float[,,] Apply(float[,,] volume, Vector3 dims, out Vector3 ndims)
        {
            float[,,] numArray = new float[(int) dims[0], (int) dims[1], (int) dims[2]];
            for (int i = 0; i < dims[2]; i++)
            {
                for (int m = 0; m < dims[1]; m++)
                {
                    for (int n = 0; n < dims[0]; n++)
                    {
                        float a = 1E+16f;
                        for (int num5 = -1; num5 <= 1; num5++)
                        {
                            int num6 = i + num5;
                            if ((num6 >= 0) && (num6 < dims[2]))
                            {
                                for (int num7 = -1; num7 <= 1; num7++)
                                {
                                    int num8 = m + num7;
                                    if ((num8 >= 0) && (num8 < dims[1]))
                                    {
                                        for (int num9 = -1; num9 <= 1; num9++)
                                        {
                                            int num10 = n + num9;
                                            if ((num10 >= 0) && (num10 < dims[0]))
                                            {
                                                if ((((n + num10) < dims[0]) && ((m + num8) < dims[1])) && ((i + num6) < dims[2]))
                                                {
                                                    a = Mathf.Min(a, volume[n + num10, m + num8, i + num6]);
                                                }
                                                else
                                                {
                                                    a = 0f;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        numArray[n, m, i] = a;
                    }
                }
            }
            ndims = new Vector3();
            for (int j = 0; j < 3; j++)
            {
                ndims[j] = Mathf.Floor(dims[j] / 2f);
            }
            float[,,] numArray2 = new float[(int) ndims[0], (int) ndims[1], (int) ndims[2]];
            for (int k = 0; k < ndims[2]; k++)
            {
                for (int num13 = 0; num13 < ndims[1]; num13++)
                {
                    for (int num14 = 0; num14 < ndims[0]; num14++)
                    {
                        float num15 = -1E+16f;
                        for (int num16 = -1; num16 <= 1; num16++)
                        {
                            int num17 = (2 * k) + num16;
                            if ((num17 >= 0) && (num17 < dims[2]))
                            {
                                for (int num18 = -1; num18 <= 1; num18++)
                                {
                                    int num19 = (2 * num13) + num18;
                                    if ((num19 >= 0) && (num19 < dims[1]))
                                    {
                                        for (int num20 = -1; num20 <= 1; num20++)
                                        {
                                            int num21 = (2 * num14) + num20;
                                            if ((num21 >= 0) && (num21 < dims[0]))
                                            {
                                                if ((((num14 + num21) < ndims[0]) && ((num13 + num19) < ndims[1])) && ((k + num17) < ndims[2]))
                                                {
                                                    num15 = Mathf.Max(num15, numArray[num14 + num21, num13 + num19, k + num17]);
                                                }
                                                else
                                                {
                                                    num15 = 0f;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        numArray2[num14, num13, k] = num15;
                    }
                }
            }
            return numArray2;
        }
    }
}

