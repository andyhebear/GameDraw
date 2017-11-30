using System;
using System.Collections.Generic;
using UnityEngine;


namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class NaiveSurfaceNets
    {
        private int[] buffer = new int[0x1000];
        private int[] cube_edges = new int[0x18];
        private int[] edge_table = new int[0x100];

        public NaiveSurfaceNets() {
            for (int i = 0; i < this.buffer.Length; i++) {
                this.buffer[i] = 0;
            }
            int num2 = 0;
            for (int j = 0; j < 8; j++) {
                for (int m = 1; m <= 4; m = m << 1) {
                    int num5 = j ^ m;
                    if (j <= num5) {
                        this.cube_edges[num2++] = j;
                        this.cube_edges[num2++] = num5;
                    }
                }
            }
            for (int k = 0; k < 0x100; k++) {
                int num7 = 0;
                for (int n = 0; n < 0x18; n += 2) {
                    bool flag = (k & (((int)1) << this.cube_edges[n])) == 0;
                    bool flag2 = (k & (((int)1) << this.cube_edges[n + 1])) == 0;
                    num7 |= (flag != flag2) ? (((int)1) << (n >> 1)) : 0;
                }
                this.edge_table[k] = num7;
            }
        }

        public unsafe void UpdateMesh(Vector3 dims, float[, ,] voxels, Bounds bounds, Mesh mesh, int smooth = 0)
        {
            int[] numArray = new int[3];
            int[] numArray2 = new int[3];
            for (int i = 0; i < 3; i++)
            {
                numArray[i] = (int) ((bounds.max[i] - bounds.min[i]) / dims[i]);
                numArray2[i] = (int) bounds.min[i];
            }
            List<Vector3> list = new List<Vector3>();
            List<int> list2 = new List<int>();
            float num2 = 0f;
            int[] numArray3 = new int[3];
            int[] numArray4 = new int[] { 1, (int) (dims.x + 1f), (int) ((dims.x + 1f) * (dims.y + 1f)) };
            float[] numArray5 = new float[8];
            int num3 = 1;
            if ((numArray4[2] * 2) > this.buffer.Length)
            {
                this.buffer = new int[numArray4[2] * 2];
            }
            numArray3[2] = 0;
            while (numArray3[2] < (dims[2] - 1f))
            {
                int index = (int) (1f + ((dims[0] + 1f) * (1f + (num3 * (dims[1] + 1f)))));
                numArray3[1] = 0;
                while (numArray3[1] < (dims[1] - 1f))
                {
                    numArray3[0] = 0;
                    while (numArray3[0] < (dims[0] - 1f))
                    {
                        int num5 = 0;
                        int num6 = 0;
                        float num7 = 0f;
                        for (int j = 0; j < 2; j++)
                        {
                            for (int k = 0; k < 2; k++)
                            {
                                int num10 = 0;
                                while (num10 < 2)
                                {
                                    num7 = voxels[((numArray[0] * (numArray3[0] + num10)) + numArray2[0]) + (((int) dims.x) / 2), ((numArray[1] * (numArray3[1] + k)) + numArray2[1]) + (((int) dims.y) / 2), ((numArray[2] * (numArray3[2] + j)) + numArray2[2]) + (((int) dims.z) / 2)];
                                    numArray5[num6] = num7;
                                    num5 |= (num7 == 0f) ? (((int) 1) << num6) : 0;
                                    num10++;
                                    num6++;
                                }
                            }
                        }
                        switch (num5)
                        {
                            case 0:
                            case 0xff:
                                break;

                            default:
                            {
                                int num11 = this.edge_table[num5];
                                Vector3 zero = Vector3.zero;
                                int num12 = 0;
                                for (int m = 0; m < 12; m++)
                                {
                                    if ((num11 & (((int) 1) << m)) != 0)
                                    {
                                        num12++;
                                        int num14 = this.cube_edges[m << 1];
                                        int num15 = this.cube_edges[(m << 1) + 1];
                                        float num16 = numArray5[num14];
                                        float num17 = numArray5[num15];
                                        float fnum18 = num16 - num17;
                                        if (Mathf.Abs(fnum18) > 1E-06)
                                        {
                                            fnum18 = num16 / fnum18;
                                            if (smooth > 0)
                                            {
                                                int num19 = 0;
                                                for (int num20 = 1; num19 < 3; num20 = num20 << 1)
                                                {
                                                    int num21 = num14 & num20;
                                                    int num22 = num15 & num20;
                                                    if (num21 != num22)
                                                    {
                                                         Vector3 vectorRef=zero;
                                                        int num30;
                                                        (vectorRef)[num30 = num19] = vectorRef[num30] + ((num21 != 0) ? (1f - fnum18) : fnum18);
                                                    }
                                                    else
                                                    {
                                                         Vector3 vectorRef2=zero;
                                                        int num31;
                                                        (vectorRef2 )[num31 = num19] = vectorRef2[num31] + ((num21 != 0) ? 1f : 0f);
                                                    }
                                                    num19++;
                                                }
                                            }
                                        }
                                    }
                                }
                                float num23 = 1f;
                                if (smooth > 1)
                                {
                                    num23 = 1f / ((float) num12);
                                }
                                for (int n = 0; n < 3; n++)
                                {
                                    zero[n] = (numArray[n] * (numArray3[n] + (num23 * zero[n]))) + numArray2[n];
                                }
                                this.buffer[index] = list.Count;
                                list.Add(zero);
                                for (int num25 = 0; num25 < 3; num25++)
                                {
                                    if ((num11 & (((int) 1) << num25)) != 0)
                                    {
                                        int num26 = (num25 + 1) % 3;
                                        int num27 = (num25 + 2) % 3;
                                        if ((numArray3[num26] != 0) && (numArray3[num27] != 0))
                                        {
                                            int num28 = numArray4[num26];
                                            int num29 = numArray4[num27];
                                            if ((num5 & 1) <= 0)
                                            {
                                                list2.Add(this.buffer[index]);
                                                list2.Add(this.buffer[index - num28]);
                                                list2.Add(this.buffer[index - num29]);
                                                list2.Add(this.buffer[index - num29]);
                                                list2.Add(this.buffer[index - num28]);
                                                list2.Add(this.buffer[(index - num28) - num29]);
                                            }
                                            else
                                            {
                                                list2.Add(this.buffer[index]);
                                                list2.Add(this.buffer[index - num29]);
                                                list2.Add(this.buffer[index - num28]);
                                                list2.Add(this.buffer[index - num28]);
                                                list2.Add(this.buffer[index - num29]);
                                                list2.Add(this.buffer[(index - num28) - num29]);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        numArray3[0]++;
                        num2++;
                        index++;
                    }
                    numArray3[1]++;
                    num2++;
                    index += 2;
                }
                numArray3[2]++;
                num2 += dims.x;
                num3 ^= 1;
                numArray4[2] = -numArray4[2];
            }
            mesh.vertices = list.ToArray();
            mesh.triangles = list2.ToArray();
        }
    }
}

