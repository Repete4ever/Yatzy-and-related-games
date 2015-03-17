using System;
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

        /// <summary>
        /// order a dice roll 
        /// </summary>
        /// <param name="nw">roll e.g. 61236</param>
        /// <returns>e.g. 111002</returns>
        public int[] OrderRoll(int[] nw)
        {
            int[] nx = new int[6];
            for (int idie = 0; idie < Dice; idie++)
            {
                nx[nw[idie] - 1]++;
            }

            return nx;
        }

        /// <summary>
        /// Order a roll but only consider unused scores
        /// </summary>
        /// <param name="nw">roll e.g. 61236</param>
        /// <param name="active">tttttf</param>
        /// <returns>111000</returns>
        public int[] OrderRoll(int[] nw, bool[] active)
        {
            int[] nx = new int[6];
            for (int idie = 0; idie < Dice; idie++)
            {
                int activeIndex = nw[idie] - 1;
                if (active[activeIndex])
                {
                    nx[activeIndex]++;
                }
            }

            return nx;
        }

        protected void points(int[] nw)
        {
            MyPoints(OrderRoll(nw));
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

        /// <summary>
        /// See if there is chance to score big in the simple categories.
        /// For Yatzy-type games this is the first 6 items
        /// For Balut it is the first 3.
        /// </summary>
        /// <param name="diceVec"></param>
        /// <param name="usedScores"></param>
        /// <returns></returns>
        public virtual int MostPopular(int[] diceVec, bool[,] usedScores)
        {
            bool[] activeScores = new bool[6];
            for (var row = 0; row < 6; row++)
            {
                for (var col = 0; col < UsableScoreBoxesPerItem; col++)
                {
                    if (usedScores[row, col])
                        continue;
                    activeScores[row] = true;
                }
            }
            int mostPopular = 6;
            int[] roll = OrderRoll(diceVec, activeScores);
            var mx = roll.Max();
            for (var row = 0; row < 6; row++)
            {
                if (roll[row] == mx)
                {
                    mostPopular = row + 1;
                }
            }

            return mostPopular;
        }
    }
}
