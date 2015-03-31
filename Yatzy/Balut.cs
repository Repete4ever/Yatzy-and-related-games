using System;
using System.Linq;

namespace Yatzy
{
    public class Balut : FiveDice
    {
        private readonly int[] Points = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private int myPotentialFours;
        private int myPotentialFives;
        private int myPotentialSixes;
        private int myChoice;
        private int myPotentialChoice;
        private bool myStraightBonus = true;
        private bool myFullHouseBonus = true;
        private int FullHouseFill;
        private int StraightFill;
        private int BalutCounter;
        private readonly bool[] Straights = { true, true, true, true };
        private readonly bool[] Houses = { true, true, true, true };

        public Balut()
        {
            InitGameOfDice(5, 9, 2);
            myPotentialFours = 4 * 4 * Dice;
            myPotentialFives = 5 * 4 * Dice;
            myPotentialSixes = 6 * 4 * Dice;
            myPotentialChoice = myPotentialSixes;
        }

        public override string ToString()
        {
            return "Balut";
        }

        public override int ScoreBoxesPerItem
        {
            get { return 6; }
        }

        public override int UsableScoreBoxesPerItem
        {
            get { return 4; }
        }

        public override int UsableItems
        {
            get { return 7; }
        }

        /// <summary>
        /// enumerate from end game = 0000000 to 
        /// beginning = 4444444 (Note: numbers have a radix of 5)
        /// </summary>
        public override int GameNodes
        {   
            get { return power(UsableScoreBoxesPerItem + 1, UsableItems) - 1; }
        }

        public override int MostPopular(int[] diceVec, bool[,] usedScores)
        {
            bool[] activeScores = new bool[6];
            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < UsableScoreBoxesPerItem; col++)
                {
                    if (usedScores[row, col])
                        continue;
                    activeScores[row + 3] = true;
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

        protected override int SubNode(int Node, int Item, int SubItem)
        {
            // ReSharper disable once UnusedVariable
            string Base5 = MyItoa(Node, 5); // for debug purposes
            Node -= power(5, Item); // reduce the Item by 1
            if (Node < 1)
                throw new Exception("Underflow in SubNode");
            return Node;
        }

        public override int ActiveItem(int Node, int item)
        {
            String Base5 = MyItoa(Node, 5);
            int Index = Base5.Length - item - 1;
            if (Index < 0)
                return 0;
            char x = Base5[Index];
            return x - '0';
        }

        public override int MaxRound
        {
            get { return UsableItems * UsableScoreBoxesPerItem; }
        }

        protected override int SumItems
        {
            get { return 7; }
        }

        protected override int BonusItems
        {
            get { return 8; }
        }

        protected override bool IsASumItem(int item, int j)
        {
            return j == 4;
        }

        protected override bool IsABonusItem(int item, int j)
        {
            return j == 5;
        }

        public override int MaxTotalItem
        {
            get { return MaxItem; }
        }

        public override int FirstScoreBox(int row)
        {
            switch (row)
            {
                default:
                    return 0;
                case 7:
                    return 4; // only sums
                case 8:
                    return 5; // total points
            }
        }

        public override int PreferredRow(int item)
        {
            return item;
        }

        public override int PreferredGroup(int item)
        {
            return 1;
        }

        public override int SavedRolls
        {
            get { return 0; }
        }

        readonly string[,] Items = {
            {"Firere","Fyror","Fours"},
            {"Femmere","Femmor","Fives"},
            {"Seksere","Sexor","Sixes"},
            {"Straight","Straight","Straight"},
            {"Hus","Kåk","Full House"},
            {"Chance","Chans","Choice"},
            {"Balut","Balut","Balut"},
            {"Total","Total","Total"},
            {"Total Point","Total Point","Total Point"}
        };

        public override string ItemText(int item)
        {
            return Items[item, ChosenLanguage];
        }

        enum balut { firere, femmere, seksere, straight, hus, chance, balut_, total };

        public override int GameScore
        {
            get { return myPoints; }
        }

        protected override void MyPoints(int[] nx)
        {
            int maks = 0, isum = 0;
            int ih = 0;
            int i;

            for (i = 0; i <= MaxItem; i++)
            {
                pts[i] = 0;
                pct[i] = 0;
            }
            for (i = 6; i > 0; i--)
            {
                var iv = nx[i - 1];
                if (i >= 4)
                {
                    pts[i - 4] = iv * i;
                    pct[i - 4] = iv * 20;
                }
                isum += iv * i;
                if (iv > maks)
                {
                    maks = iv;
                    ih = i;
                }
            }

            pts[(int)balut.chance] = isum;
            pct[(int)balut.chance] = isum * 100 / 30;
            switch (maks)
            {
                case 1:
                    if (isum == 15)
                    {
                        pts[(int)balut.straight] = 15;
                        pct[(int)balut.straight] = 100;
                    }
                    if (isum == 20)
                    {
                        pts[(int)balut.straight] = 20;
                        pct[(int)balut.straight] = 100;
                    }
                    return;
                case 2:
                case 4:
                    return;
                case 3:
                    for (i = 1; i <= 6; i++)
                    {
                        if (nx[i - 1] == 2)
                        {
                            pts[(int)balut.hus] = ih * 3 + i * 2;
                            pct[(int)balut.hus] = pts[(int)balut.hus] * 100 / 28;
                            return;
                        }
                    }
                    return;
                case 5:
                    pts[(int)balut.balut_] = ih * 5 + 20;
                    pct[(int)balut.balut_] = pts[(int)balut.balut_] * 2;
                    return;
            }

        }

        private readonly int[] BonusArr = { 0, 0, 0, 0, 0, 0, 0, 0 };

        // Balut scoring is a two-tounged matter, if two gamers get the same amount the gamepoints decide
        public override int BonusPoints
        {
            get
            {
                return BonusArr.Sum();
            }
        }

        protected override string Bonus(int i)
        {
            int Threshold = (3 * UsableScoreBoxesPerItem + 1) * (i + 4);
            switch (i)
            {
                case 0:
                    if (Points[i] >= Threshold)
                    {
                        BonusArr[i] = 2;
                        return "2";
                    }
                    BonusArr[i] = 0;
                    if (myPotentialFours < Threshold)
                        return Hyphen;
                    break;
                case 1:
                    if (Points[i] >= Threshold)
                    {
                        BonusArr[i] = 2;
                        return "2";
                    }
                    BonusArr[i] = 0;
                    if (myPotentialFives < Threshold)
                        return Hyphen;
                    break;
                case 2:
                    if (Points[i] >= Threshold)
                    {
                        BonusArr[i] = 2;
                        return "2";
                    }
                    BonusArr[i] = 0;
                    if (myPotentialSixes < Threshold)
                        return Hyphen;
                    break;
                case 3:
                    BonusArr[i] = 0;
                    if (!myStraightBonus)
                        return Hyphen;
                    if (StraightFill == UsableScoreBoxesPerItem)
                    {
                        BonusArr[i] = 4;
                        return "4";
                    }
                    break;
                case 4:
                    BonusArr[i] = 0;
                    if (!myFullHouseBonus)
                        return Hyphen;
                    if (FullHouseFill == UsableScoreBoxesPerItem)
                    {
                        BonusArr[i] = 3;
                        return "3";
                    }
                    break;
                case 5:
                    if (myChoice >= UsableScoreBoxesPerItem * 25)
                    {
                        BonusArr[i] = 2;
                        return "2";
                    }
                    BonusArr[i] = 0;
                    if (myPotentialChoice < UsableScoreBoxesPerItem * 25)
                        return Hyphen;
                    break;
                case 6:
                    BonusArr[i] = BalutCounter * 2;
                    return "" + BonusArr[i];
                case 7:
                    Points[i] = myPoints;
                    BonusArr[i] = Math.Min(6, Math.Max(myPoints / 50 - 7, -2));
                    return "" + BonusArr[i];
                case 8:
                    return "" + BonusPoints;
                default:
                    return "?" + i;
            }
            return "";
        }

        public override string ScoreIt(int[] nw, int i, int j, int roll)
        {
            if (IsASumItem(i, j))
                return "" + Points[i];
            if (IsABonusItem(i, j))
                return Bonus(i);

            points(nw);

            int p = pts[i];
            myPoints += p;

            switch (i)
            {
                case 0:
                    myPotentialFours -= (i + 4) * Dice - p;
                    Points[i] += p;
                    break;
                case 1:
                    myPotentialFives -= (i + 4) * Dice - p;
                    Points[i] += p;
                    break;
                case 2:
                    myPotentialSixes -= (i + 4) * Dice - p;
                    Points[i] += p;
                    break;
                case 3:
                    myStraightBonus &= p > 0;
                    Points[i] += p;
                    Straights[j] = p > 0;
                    StraightFill++;
                    break;
                case 4:
                    myFullHouseBonus &= p > 0;
                    Points[i] += p;
                    Houses[j] = p > 0;
                    FullHouseFill++;
                    break;
                case 5:
                    myChoice += p;
                    Points[i] += p;
                    myPotentialChoice -= 6 * Dice - p;
                    break;
                case 6:
                    if (p > 0)
                        BalutCounter++;
                    Points[i] += p;
                    break;
            }
            return "" + (p > 0 ? "" + p : Hyphen);
        }

        public override string UnScoreIt(int[] nw, int i, int j, int roll)
        {
            if (IsASumItem(i, j))
                return "" + Points[i];
            if (IsABonusItem(i, j))
                return Bonus(i);

            points(nw);

            int p = pts[i];
            myPoints -= p;

            switch (i)
            {
                case 0:
                    myPotentialFours += (i + 4) * Dice - p;
                    Points[i] -= p;
                    break;
                case 1:
                    myPotentialFives += (i + 4) * Dice - p;
                    Points[i] -= p;
                    break;
                case 2:
                    myPotentialSixes += (i + 4) * Dice - p;
                    Points[i] -= p;
                    break;
                case 3:
                    myStraightBonus = true;
                    Points[i] -= p;
                    Straights[j] = true;
                    foreach (bool b in Straights)
                        myStraightBonus &= b;
                    StraightFill--;
                    break;
                case 4:
                    myFullHouseBonus = true;
                    Points[i] -= p;
                    Houses[j] = true;
                    foreach (bool b in Houses)
                        myFullHouseBonus &= b;
                    FullHouseFill--;
                    break;
                case 5:
                    myChoice -= p;
                    Points[i] -= p;
                    myPotentialChoice += 6 * Dice - p;
                    break;
                case 6:
                    if (p > 0)
                        BalutCounter--;
                    Points[i] -= p;
                    break;
            }
            return "";
        }

        //public override int ValueIt(int[] nw, int i)
        //{
        //    foreach (int di in nw)
        //    { // check for die with six sides
        //        if (di < 1)
        //            return 0; // can't be a proper roll of a die
        //        if (di > 6)
        //            throw new ArgumentException("OVFL"); // can't be a proper roll of a six-sided die
        //    }

        //    points(nw);

        //    return pts[i];
        //}

        public override void NewGame()
        {
            myPoints = 0;
            myHighPoints = 0;
            //myBonusPoints = 0;
            myPotentialFours = 4 * 4 * Dice;
            myPotentialFives = 5 * 4 * Dice;
            myPotentialSixes = 6 * 4 * Dice;
            myPotentialChoice = myPotentialSixes;
            myStraightBonus = true;
            StraightFill = 0;
            myFullHouseBonus = true;
            FullHouseFill = 0;
            myChoice = 0;
            BalutCounter = 0;
            for (int i = 0; i < BonusArr.Length; i++)
                BonusArr[i] = 0;
            for (int i = 0; i < Points.Length; i++)
                Points[i] = 0;
            for (int i = 0; i < Straights.Length; i++)
                Straights[i] = true;
            for (int i = 0; i < Houses.Length; i++)
                Houses[i] = true;
        }

    }
}