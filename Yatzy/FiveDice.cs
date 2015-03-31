using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.IO;

namespace Yatzy
{
    public abstract class FiveDice : GameOfDice
    {
        /// abstract class to add a game plan to games played with five dice, e.g. Yatzy
        /// 
        public static readonly bool varians = true; // set to false to ease overhead added by computing (quasi-optimal) game plan

        public double[] expect;
        public double[] vari;
        private readonly double[,] varia = new double[462, 3];

        private readonly double[,] value = new double[462, 3];

        public int[,] Keep
        {
            get { return keep; }
        }

        public int[] Name
        {
            get { return name; }
        }

        static public int power(int f, int n)
        { /* computes f to the n'th power */
            if (n > 0) return f * power(f, n - 1);
            return 1;
        }

        static char single_itoa(int value)
        {
            const string hash = "0123456789abcdef";
            if (value >= 0 && value <= 15)
                return hash[value];
            throw new ArgumentOutOfRangeException("Not a legal itoa value " + value);
        }

        /// <summary>
        /// System.Convert only handles base(radix) 2,4,8 and 16
        ///  Here we handle all bases from 2 until 16 (we only need base 5!)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="radix"></param>
        /// <returns></returns>
        static public string MyItoa(int val, int radix)
        {
            if (val == 0)
                return "0"; // boundary case!

            Stack stk = new Stack();
            while (val != 0)
            {
                stk.Push(val % radix);
                val /= radix;
            }

            string cstr = "";
            while (stk.Count > 0)
            {
                cstr += single_itoa((int)stk.Pop());
            }

            return cstr;
        }

        private readonly double[] prob = new double[462];
        private void probs()
        {
            int n0;

            double[] g = { 1, 1, 2, 6, 24, 120 };
            double[] sixto = new double[6];
            for (int i = 0; i < 6; i++) sixto[i] = power(6, i + 1);
            //{6,36,216,1296,7776,46656};

            int index = 0;
            for (n0 = 0; n0 < 6; n0++)
            {
                var n1max = 5 - n0;
                double k = 6 * g[n1max] / sixto[n1max];
                int n1;
                for (n1 = 0; n1 <= n1max; n1++)
                {
                    var n2max = n1max - n1;
                    int n2;
                    for (n2 = 0; n2 <= n2max; n2++)
                    {
                        var n3max = n2max - n2;
                        int n3;
                        for (n3 = 0; n3 <= n3max; n3++)
                        {
                            var n4max = n3max - n3;
                            int n4;
                            for (n4 = 0; n4 <= n4max; n4++)
                            {
                                var n5max = n4max - n4;
                                int n5;
                                for (n5 = 0; n5 <= n5max; n5++)
                                {
                                    prob[index] = k / (g[n1] * g[n2] * g[n3] * g[n4] * g[n5] * g[n5max - n5]);
                                    index++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private readonly int[] name = new int[252];
        private readonly int[,] keep = new int[462, 2];

        private readonly int[] inn = new int[6];
        private readonly int[,] komb = new int[6, 5];

        private int fseqno(int ny0, int ny1, int ny2, int ny3, int ny4, int ny5)
        {
            int nn = ny1 + ny2 + ny3 + ny5 + ny4 + ny0;

            int index = inn[nn] + ny4;
            nn -= ny0;
            int j;
            for (j = 0; j < ny0; j++) index += komb[nn + j, 4];
            nn -= ny1;
            for (j = 0; j < ny1; j++) index += komb[nn + j, 3];
            nn -= ny2;
            for (j = 0; j < ny2; j++) index += komb[nn + j, 2];
            nn -= ny3;
            for (j = 0; j < ny3; j++) index += komb[nn + j, 1];
            return index;
        }

        public int Seqno(int[] ny)
        {
            int nyi = ny[4];
            int nn = ny[1] + ny[2] + ny[3] + ny[5] + nyi + ny[0];
            int index = inn[nn] + nyi/*+1*/;
            for (int i = 0; i < 4; i++)
            {
                nyi = ny[i];
                nn -= nyi;
                int ii = 4 - i;
                for (int j = 0; j < nyi; j++)
                    index += komb[nn + j, ii];
            }
            return index;
        }

        private void table2and4(int[] n, int index, int ja)
        {
            int[] m = new int[6];
            double v = 0;
            double vv = 0;
            int kp = index;
            for (m[0] = 0; m[0] <= n[0]; m[0]++)
                for (m[1] = 0; m[1] <= n[1]; m[1]++)
                    for (m[2] = 0; m[2] <= n[2]; m[2]++)
                        for (m[3] = 0; m[3] <= n[3]; m[3]++)
                            for (m[4] = 0; m[4] <= n[4]; m[4]++)
                                for (m[5] = 0; m[5] <= n[5]; m[5]++)
                                {
                                    int ixsub = Seqno(m);
                                    if (v < value[ixsub, ja])
                                    {
                                        v = value[ixsub, ja];
                                        if (varians)
                                            vv = varia[ixsub, ja];
                                        kp = ixsub;
                                    }
                                }
            value[index, ja - 1] = v;
            if (varians)
                varia[index, ja - 1] = vv;
            keep[index, ja - 1] = kp;
        }

        protected abstract int SubNode(int Node, int Item, int SubItem);
        public abstract int ActiveItem(int Node, int item);

        /// <summary>
        /// Find R1 strategy by dynamic programming
        /// R1 is maximizing points score (sans bonus). R2 is the more advanced maximing chance to win
        /// </summary>
        /// <param name="LevelOfAnalysis">Analyze the whole game or a subgame</param>
        /// <param name="NewDice">the roll</param>
        /// <param name="UnusedI">unused items (subgame only)</param>
        /// <param name="NodeNo">subgame node</param>
        /// <param name="ActiveI">items still in play</param>
        /// <param name="PointsToWin">end game scenario</param>
        public void GamePlan(int LevelOfAnalysis, int[] NewDice,
            int[,] UnusedI, int NodeNo, int ActiveI, int PointsToWin)
        {
            Contract.Assert(LevelOfAnalysis == 6 || LevelOfAnalysis == 3);
            int[] n = new int[6];
            int loop;
            int index;
            int n1, n2, n3, n4, n5, n6;
            int m1max, m2max, m3max, m4max, m5max;
            int m1, m2, m3, m4, m5;
            int ixsub;
            int it = 0;
            double sum, v;
            double e = 0;
            /* ---finder r1-strategien vha. dynamisk programmering
            ---parametre     :
            ---  LevelOfAnalysis : Analyze the whole game or a subgame
            ---                    6 = Expected score & Variance of Major Nodes in the game tree
            ---                    3 = path from a minor node to a major game node
            ---  NewDice     :  terningekastet
            ---  UnusedI     :  ubrugte rubrikker
            ---  NodeNo      :  subspillets knudenr
            ---  ActiveI     :  antal aktive rubrikker
            ---  PointsToWin :  How to score to win in the last round
            ---  expect      :  vaerdier af successive subspil */

            int[] nmin = new int[6];
            nmin[0] = 1;
            nmin[1] = 2;
            nmin[2] = 1;
            nmin[3] = 2;
            nmin[4] = 1;
            nmin[5] = 1;
            int[] nmax = new int[6];
            nmax[0] = 1;
            nmax[1] = 6;
            nmax[2] = 1;
            nmax[3] = 6;
            nmax[4] = 1;
            nmax[5] = 1;
            int[] jay = new int[6];
            jay[0] = 3;
            jay[1] = 3;
            jay[2] = 2;
            jay[3] = 2;
            jay[4] = 1;
            jay[5] = 0;

            for (loop = 0; loop < LevelOfAnalysis; loop++)
            {
                var n0min = nmin[loop];
                var n0max = nmax[loop];
                var ja = jay[loop];
                index = 0;
                if (n0min == 2) index = 252;
                int n0;
                for (n0 = n0min; n0 <= n0max; n0++)
                {
                    var n1max = 7 - n0;
                    for (n1 = 0; n1 < n1max; n1++)
                    {
                        var n2max = n1max - n1; n[0] = n1;
                        for (n2 = 0; n2 < n2max; n2++)
                        {
                            var n3max = n2max - n2; n[1] = n2;
                            for (n3 = 0; n3 < n3max; n3++)
                            {
                                var n4max = n3max - n3; n[2] = n3;
                                for (n4 = 0; n4 < n4max; n4++)
                                {
                                    var n5max = n4max - n4; n[3] = n4;
                                    for (n5 = 0; n5 < n5max; n5++)
                                    {
                                        n6 = n5max - n5 - 1; n[4] = n5; n[5] = n6;
                                        double a;
                                        switch (loop)
                                        {
                                            case 0:
                                                MyPoints(n);
                                                int SubItem = UnusedI[0, 1];
                                                if (ActiveI == 1 && SubItem == 1)
                                                {
                                                    it = UnusedI[0, 0];
                                                    a = pts[--it];
                                                    if (a < PointsToWin) a = 0;
                                                    else
                                                        if (a == PointsToWin) a /= 2;
                                                    if (varians)
                                                        varia[index, 2] = 0;
                                                }
                                                else
                                                {
                                                    a = 0;
                                                    int i;
                                                    for (i = 0; i < ActiveI; i++)
                                                    {
                                                        var item = UnusedI[i, 0] - 1;
                                                        int SI = UnusedI[i, 1];
                                                        double b = pts[item] + expect[SubNode(NodeNo, item, SI)];
                                                        if (b >= a)
                                                        {
                                                            a = b;
                                                            it = item;
                                                            SubItem = SI;
                                                        }
                                                    }
                                                    if (varians)
                                                        varia[index, 2] = vari[SubNode(NodeNo, it, SubItem)];
                                                }
                                                value[index, 2] = a;
                                                name[index] = it;
                                                break;
                                            case 1:
                                            case 3:
                                                var ix1 = inn[n0 - 1];
                                                sum = 0;
                                                double vv = 0;
                                                for (m1 = 0, m1max = n0; m1 < m1max; m1++)
                                                    for (m2 = 0, m2max = m1max - m1; m2 < m2max; m2++)
                                                        for (m3 = 0, m3max = m2max - m2; m3 < m3max; m3++)
                                                            for (m4 = 0, m4max = m3max - m3; m4 < m4max; m4++)
                                                                for (m5 = 0, m5max = m4max - m4; m5 < m5max; m5++)
                                                                {
                                                                    if (!varians)
                                                                    {
                                                                        ixsub = fseqno(n1 + m1, n2 + m2, n3 + m3, n4 + m4, n5 + m5,
                                                                            n6 + m5max - m5 - 1);
                                                                        sum += prob[ix1] *
                                                                               value[ixsub, ja - 1];
                                                                    }
                                                                    else
                                                                    {
                                                                        a = prob[ix1];
                                                                        int b = fseqno(n1 + m1, n2 + m2, n3 + m3, n4 + m4, n5 + m5,
                                                                            n6 + m5max - m5 - 1);
                                                                        double c = value[b, ja - 1];
                                                                        sum += a * c;
                                                                        vv += a * (c * c + varia[b, ja - 1]);
                                                                    }
                                                                    ix1++;
                                                                }
                                                value[index, ja - 1] = sum;
                                                if (varians)
                                                    varia[index, ja - 1] = vv - sum * sum;
                                                break;
                                            case 2:
                                            case 4:
                                                table2and4(n, index, ja);
                                                break;
                                            case 5:
                                                if (!varians)
                                                    expect[NodeNo] += prob[index] * value[index, 0];
                                                else
                                                {
                                                    e += prob[index] * (v = value[index, 0]);
                                                    vari[NodeNo] += prob[index] * (v * v + varia[index, 0]);
                                                }
                                                break;
                                        } /*switch loop*/
                                        index++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (LevelOfAnalysis >= 6 && varians)
            {
                expect[NodeNo] = e;
                vari[NodeNo] -= e * e;
            }
            /* ---delvis udfyldning af resterende tabeller ved */
            /* ---simulation eller analyse */

            index = Seqno(NewDice);
            v = 0;
            var kp = index;

            for (n1 = 0; n1 <= NewDice[0]; n1++)
                for (n2 = 0; n2 <= NewDice[1]; n2++)
                    for (n3 = 0; n3 <= NewDice[2]; n3++)
                        for (n4 = 0; n4 <= NewDice[3]; n4++)
                            for (n5 = 0; n5 <= NewDice[4]; n5++)
                                for (n6 = 0; n6 <= NewDice[5]; n6++)
                                {
                                    ixsub = fseqno(n1, n2, n3, n4, n5, n6);
                                    m1max = 6 - (n1 + n2 + n3 + n4 + n5 + n6);
                                    sum = 0;
                                    for (m1 = 0; m1 < m1max; m1++)
                                        for (m2 = 0, m2max = m1max - m1; m2 < m2max; m2++)
                                            for (m3 = 0, m3max = m2max - m2; m3 < m3max; m3++)
                                                for (m4 = 0, m4max = m3max - m3; m4 < m4max; m4++)
                                                    for (m5 = 0, m5max = m4max - m4; m5 < m5max; m5++)
                                                    {
                                                        var m6 = m5max - m5 - 1;
                                                        int i1 = fseqno(m1, m2, m3, m4, m5, m6);
                                                        int i2 = fseqno(n1 + m1, n2 + m2, n3 + m3, n4 + m4, n5 + m5, n6 + m6);
                                                        sum += prob[i1] * value[i2, 1];
                                                    }
                                    value[ixsub, 1] = sum;
                                    if (v < sum)
                                    {
                                        v = sum;
                                        kp = ixsub;
                                    }
                                }
            value[index, 0] = v;
            keep[index, 0] = kp;
        }

        private void FillK()
        {
            /* the array must look like this
              {1,2, 3, 4,  5},
              {1,3, 6,10, 15},
              {1,4,10,20, 35},
              {1,5,15,35, 70},
              {1,6,21,56,126},
              {1,7,28,84,210}
            */
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 5; j++)
                {
                    if (i > 0 && j > 0) komb[i, j] = komb[i - 1, j] + komb[i, j - 1];
                    else
                        if (i == 0) komb[0, j] = j + 1;
                        else
                            komb[i, 0] = 1;
                }
            inn[0] = 461;
            inn[1] = 455;
            inn[2] = 434;
            inn[3] = 378;
            inn[4] = 252;
            inn[5] = 0;
        }

        public void Status(int index, int[] nz)
        {
            int i;
            int nn;

            for (i = 0; i < 6; i++) nz[i] = 0;

            for (nn = 4; nn >= 0; nn--)
                if (index < inn[nn])
                    goto LoopEnd;
            return;

            LoopEnd:

            int ind = index - inn[nn + 1];
            for (i = 0; i < 5; i++)
            {
                var ii = 4 - i;
                int j;
                for (j = nn; j >= 0 && ind >= komb[j, ii]; j--)
                {
                    ind -= komb[j, ii];
                    nz[i]++;
                }
                nn -= nz[i];
                if (nn == -1) return;
            }
            nz[5] = nn + 1;
        }

        /// <summary>
        /// fjerner en tabelindgang i "number" og skubber paa plads
        /// returnerer opdateret "nc" (2-tal-system)
        /// </summary>
        /// <param name="nu">remaining rounds</param>
        /// <param name="nam">item being decommissioned</param>
        /// <param name="nc">node number</param>
        /// <param name="number">table of unused items</param>
        /// <returns>new node number</returns>
        protected int Modify(int nu,int nam,int nc,int[] number)
        {
            for (int i = 0; i < nu; i++)
                if (number[i] == nam)
                    for (int j = i; j < nu - 1; j++) number[j] = number[j + 1];
            nc -= 1 << (nam - 1);
            return nc;
        }

        public override void InhabitTables()
        {
            probs();
            FillK();

            int Nodes = GameNodes;
            float e = 0;
            expect = new double[Nodes + 1];
            if (varians)
            {
                vari = new double[Nodes + 1];
            }

            string expect_fn = ToString() + ".tbl";
            bool GenerateTable = !File.Exists(expect_fn);
            string var_fn = ToString() + "v.tbl";

            if (GenerateTable)
            {
                CalculateNodes calc = new CalculateNodes(this, Nodes, expect_fn, var_fn);
                calc.Start();
                calc.ShowDialog();
            }
            else
            {
                using (var es = new FileStream(expect_fn, FileMode.Open))
                using (var vs = new FileStream(var_fn, FileMode.Open))
                using (var tbl = new BinaryReader(es))
                using (var vtbl = new BinaryReader(vs))
                {
                    for (int i = 1; i <= Nodes; i++)
                    {
                        try
                        {
                            e = tbl.ReadSingle();
                            if (varians)
                            {
                                float v = vtbl.ReadSingle();
                                vari[i] = v;
                            }
                            expect[i] = e;
                        }
                        catch (IOException)
                        {
                            throw new Exception("Table not read; stopping with e = " + e + " at node " + i);
                        }
                    }
                }
            }

            //{ // self-test
            //    int[] n = new int[6];
            //    for (int ix = 0; ix < 462; ix++)
            //    {
            //        Status(ix, n);
            //        int i = Seqno(n);
            //        if (ix != i)
            //        {
            //            throw new ApplicationException("bad seqno " + ix + " -> " + i);
            //        }

            //    }
            //}

        }
    }
}