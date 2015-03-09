using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Yatzy
{
    /// <summary>
    /// GameOfDice is a generic dice game that could evolve into Yahtzee or Yatzy
    /// Yes, even Maxiyatzy with its notion of saved rolls
    /// And sideways into Balut - a destilled version of Yatzy in which all items
    ///   carry bonuspoints. Each item must be scored four times
    /// </summary>
    public abstract class GameOfDice
    {
        private int myDice;
        protected int[] pts;
        /// <summary>
        /// 100 means that this is the best score available 
        /// e.g. 20 for 'stor' in Yatzy yields 100 while 20 for '4 ens' yields 20*100/24
        /// </summary>
        protected int[] pct;

        private int myMaxItem;
        protected int myPoints;
        protected int myHighPoints;
        private int PotentialHighPoints; // 105 for Yatzy/Yahtzee, 126 for Maxiyatzy
        private int BonusThreshold;      //  63 for Yatzy/Yahtzee,  84 for Maxiyatzy
        private int myPotentialHighPoints;
        private int myBonus;
        private int mySavedRolls;

        protected const string Hyphen = "\u2014"; // better than "--";

        protected void InitGameOfDice(int Die, int items, int Bon)
        {
            myDice = Die;
            pts = new int[items];
            pct = new int[items];
            myMaxItem = items;
            myBonus = Bon;
            mySavedRolls = 0;
            PotentialHighPoints = (1 + 2 + 3 + 4 + 5 + 6) * Die;
            BonusThreshold = (Die - 2) * (1 + 2 + 3 + 4 + 5 + 6); // 63 for Yatzy, 84 for Maxiyatzy
            myPotentialHighPoints = PotentialHighPoints;
        }

        public int Dice
        {
            get { return myDice; }
        }

        public int GamePoints
        {
            get { return myPoints; }
        }

        public virtual int BonusPoints
        {
            get { return 0; }
        }

        public int MaxItem
        {
            get { return myMaxItem - 1; } // count from 0!
        }

        public virtual int MaxRound
        {
            get { return myMaxItem; }
        }

        public virtual int UsableItems
        {   // made virtual so Balut may override it 
            get { return MaxRound; }
        }

        protected virtual int GameNodes
        {   // simple when an item may only be used once (and bonus is ignored)
            // Balut must override (even when bonuses are ignored!)
            get { return (1 << UsableItems) - 1; }
        }

        public virtual int MaxGroup
        {
            get { return 1; } // default points scoring uses one group of scoreboxes
        }

        public virtual int ScoreBoxesPerItem
        {
            get { return 1; } // fits xxYaxx games
        }

        public virtual int UsableScoreBoxesPerItem
        {
            get { return 1; } // fits xxYaxx games
        }

        protected virtual int SumItems
        {
            get { return 1; } // covers xxYaxx but not Balut
        }

        protected virtual bool IsASumItem(int item, int j)
        {
            return false;
        }

        protected virtual int BonusItems
        {
            get { return 1; } // covers xxYaxx but not Balut
        }

        protected virtual bool IsABonusItem(int item, int j)
        {
            return false;
        }

        public virtual int MaxTotalItem
        {
            get { return MaxItem + SumItems + BonusItems + 1; }
        }

        public virtual int FirstScoreBox(int row)
        {
            return 0; // column
        }

        public virtual int PreferredRow(int item)
        {
            if (IsASumItem(item, 0))
                return item - 9; // sum of items 1-6
            if (IsABonusItem(item, 0))
                return item - 9;
            if (item > MaxItem + SumItems + BonusItems)
                return item; // sum total
            if (item < 6)
                return item;
            return item + SumItems + BonusItems;
        }

        public virtual int PreferredGroup(int item)
        {
            if (item < 6 || IsASumItem(item, 0) || IsABonusItem(item, 0))
                return 1;
            return MaxGroup;
        }

        public virtual int SavedRolls
        {
            get { return mySavedRolls; }
        }

        public int UseARoll()
        {
            if (mySavedRolls < 1)
                return 0;
            return --mySavedRolls;
        }

        protected abstract void MyPoints(int[] nw);

        protected void points(int[] nw)
        {
            // repackage a roll so the number of ones, twos, etc. are computed
            // we map nw[Dice] onto nx[6]
            int[] nx = new int[6];
            for (int idie = 0; idie < Dice; idie++)
            {
                nx[nw[idie] - 1]++;
            }

            MyPoints(nx);
        }

        protected virtual string Bonus(int i) // takes care of xxYaxx
        {
            if (myHighPoints >= BonusThreshold)
                return "" + myBonus;
            if (myPotentialHighPoints < BonusThreshold)
                return Hyphen;
            return "";
        }

        public virtual void InhabitTables()
        {
        }

        protected virtual int TotalPoints()
        {
            return myPoints + (myHighPoints >= BonusThreshold ? myBonus : 0);
        }

        public virtual string ScoreIt(int[] nw, int i, int j, int roll)
        {
            if (IsASumItem(i, j)) 
                return "" + myHighPoints;
            if (IsABonusItem(i, j)) 
                return Bonus(i);
            if (i > MaxItem + SumItems + BonusItems)
                return "" + TotalPoints();

            points(nw);

            if (roll < 3)
                mySavedRolls += 3 - roll;

            int p = pts[i];
            myPoints += p;
            if (i < 6)
            {
                myPotentialHighPoints -= (i + 1) * Dice - p;
                myHighPoints += p;
            }
            return "" + (p > 0 ? "" + p : Hyphen);
        }

        public virtual string UnScoreIt(int[] nw, int i, int j, int roll)
        {
            if (IsASumItem(i, j)) 
                return "" + myHighPoints;
            if (IsABonusItem(i, j)) 
                return Bonus(i);
            if (i > MaxItem + SumItems + BonusItems)
                return "" + TotalPoints();

            points(nw);

            if (roll < 3)
                mySavedRolls -= 3 - roll;

            int p = pts[i];
            myPoints -= p;
            if (i < 6)
            {
                myPotentialHighPoints += (i + 1) * Dice - p;
                myHighPoints -= p;
            }
            return "";
        }

        public virtual int ValueIt(int[] nw, int i)
        {
            int d = nw.Sum();
            if (d == 0)
                return 0; // no "real" values in nw, just exit
            if (d < Dice)
                throw new ArgumentException("Not a proper roll");

            points(nw);

            return i < pts.Length ? pts[i] : 0;
        }

        public int GradeIt(int[] nw, int i)
        {
            int d = nw.Sum();
            if (d == 0)
                return 0; // no "real" values in nw, just exit
            if (d < Dice)
                throw new ArgumentException("Not a proper roll");

            points(nw);

            return i < pct.Length ? pct[i] : 0;
        }

        public enum Language { Danish, Swedish, English };

        public static int ChosenLanguage { get; set; }

        public virtual void NewGame()
        {
            myPoints = 0;
            myHighPoints = 0;
            myPotentialHighPoints = PotentialHighPoints;
            mySavedRolls = 0;
        }

        readonly string[,] Cardinals = {
		{"første", "första", "first"},
		{"andet", "andra", "second"},
		{"tredje", "tredje", "third"}};

        public string Cardinal(int roll)
        {
            if (roll < 3)
                return Cardinals[roll, ChosenLanguage];
            return ""; // In Maxiyatzy you may have extra rolls but we don't supply the cardinal
        }

        public abstract string ItemText(int item);

        readonly string[] RollTexts = {
								"Kast terningerne - {0} kast",
								"Kasta tärningerne - {0} kastet",
								"Roll the dice - {0} roll"
							 };

        protected GameOfDice()
        {
            ChosenLanguage = (int)Language.Danish;
        }

        public string RollText(int roll)
        {
            if (roll < 3)
                return RollTexts[ChosenLanguage];
            char[] splitter = { '-' };
            return RollTexts[ChosenLanguage].Split(splitter)[0];
        }
    }

    public abstract class FiveDice : GameOfDice
    {
        /// abstract class to add a game plan to games played with five dice, e.g. Yatzy
        /// 
        public static readonly bool varians = true; // set to false to ease overhead added by computing (quasi-optimal) game plan

        public double[] expect;
        public double[] vari;
        private readonly double[,] varia = new double[462, 3];

        private readonly double[,] value = new double[462, 3];

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

        static public string MyItoa(int val, int radix)
        // System.Convert only handles base(radix) 2,4,8 and 16
        // Here we handle all bases from 2 until 16 (we only need base 5!)
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

        private int Seqno(int[] ny)
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
        /// R1 is maximizing points score. R2 is the more advanced maximing chance to win
        /// </summary>
        /// <param name="LevelOfAnalysis">Analyze the whole game or a subgame</param>
        /// <param name="NewDice">the roll</param>
        /// <param name="UnusedI">unused scores (subgame only)</param>
        /// <param name="NodeNo">subgame node</param>
        /// <param name="ActiveI">scores in play</param>
        /// <param name="PointsToWin">end game scenario</param>
        public void GamePlan(int LevelOfAnalysis, int[] NewDice,
            int[,] UnusedI, int NodeNo, int ActiveI, int PointsToWin)
        {
            Contract.Assert(LevelOfAnalysis == 6 || LevelOfAnalysis == 3);
            int[] n = new int[6];
            int loop;
            int ja, index;
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
                ja = jay[loop];
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

        private void Status(int index, int[] nz)
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

            { // self-test
                int[] n = new int[6];
                for (int ix = 0; ix < 462; ix++)
                {
                    Status(ix, n);
                    int i = Seqno(n);
                    if (ix != i)
                    {
                        throw new ApplicationException("bad seqno " + ix + " -> " + i);
                    }

                }
            }

        }
    }
}
