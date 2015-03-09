using System;

namespace Yatzy
{
    public sealed class Yahtzee : FiveDice
    {
        public Yahtzee()
        {
            InitGameOfDice(5, 13, 35);
        }

        public override string ToString()
        {
            return "Yahtzee";
        }

        readonly string[,] Items = {
            {"ENERE","ETTOR","1's"},
            {"TOERE","TVÅOR","2's"},
            {"TREERE","TREOR","3's"},
            {"FIRERE","FYROR","4's"},
            {"FEMMERE","FEMMOR","5's"},
            {"SEKSERE","SEXOR","6's"},
            {"3 ens","Tretal","3 of a kind"},
            {"4 ens","Fyrtal","4 of a kind"},
            {"Hus","Kåk","House"},
            {"Lille","Liten straight","Sm straight"},
            {"Stor","Stor straight","Lg straight"},
            {"YAHTZEE","YAHTZEE","Yahtzee"},
            {"Chance","Chans","Chance"},
            {"SUM","SUMMA","Sum"},
            {"BONUS","BONUS","Bonus"},
            {"TOTAL","SUMMA","Total"}
        };

        public override string ItemText(int item)
        {
            return Items[item, ChosenLanguage];
        }

        public override int SavedRolls
        {
            get { return 0; } // the notion of saved rolls doesn't exist
        }
        public override int MaxGroup
        {
            get { return 2; } // makes layout look nice
        }

        protected override bool IsASumItem(int item, int j)
        {
            return item == MaxItem + 1;
        }

        protected override bool IsABonusItem(int item, int j)
        {
            return item == MaxItem + 2;
        }

        public override int PreferredRow(int item)
        {
            if (IsASumItem(item, 0))
                return item - 7; // sum of items 1-6
            if (IsABonusItem(item, 0))
                return item - 7; // bonus
            if (item > MaxItem + SumItems + BonusItems)
                return 7; // sum total
            if (item < 6)
                return item;
            return item - 6;
        }

        protected override int SubNode(int Node, int item, int SubItem)
        {
            return Node - (1 << item);
        }

        public override int ActiveItem(int Node, int item)
        {
            return (Node >> item) % 2; // return bit number 'item' of Node
        }

        protected override void MyPoints(int[] nx)
        {
            int maks = 0, isum = 0, j;

            int three_of_a_kind = 5 + 1;
            int four_of_a_kind = three_of_a_kind + 1;
            int house = four_of_a_kind + 1;
            int small = house + 1;
            int large = small + 1;
            int yahtzee = large + 1;
            int chance = yahtzee + 1;

            for (int i = 6; i <= MaxItem; i++)
            {
                pts[i] = 0;
                pct[i] = 0;
            }
            for (j = 6; j >= 1; j--)
            {
                var iv = nx[j - 1];
                pts[j - 1] = iv * j;
                pct[j - 1] = iv * 20;
                isum += iv * j;
                if (iv > maks)
                {
                    maks = iv;
                }
            }
            pts[chance] = isum;
            pct[chance] = isum * 100 / 30;
            switch (maks)
            {
                case 1:
                case 2:
                    int ih = Math.Min(1, nx[0] * nx[1] * nx[2]) + Math.Min(1, nx[1] * nx[2]) + Math.Min(1, nx[2]) +
                             Math.Min(1, nx[3] * nx[4] * nx[5]) + Math.Min(1, nx[3] * nx[4]) + Math.Min(1, nx[3]);
                    if (ih >= 4)
                    {
                        pts[small] = 30;
                        pct[small] = 100;
                    }
                    if (ih == 5)
                    {
                        pts[large] = 40;
                        pct[large] = 100;
                    }
                    return;
                case 3:
                    pts[three_of_a_kind] = isum;
                    pct[three_of_a_kind] = isum * 100 / 30;
                    for (int i = 0; i < 6; i++)
                        if (nx[i] == 2)
                        {
                            pts[house] = 25;
                            pct[house] = 100;
                        }
                    return;
                case 5:
                    pts[yahtzee] = 50;
                    pct[yahtzee] = 100;
                    pts[house] = 25;
                    pct[house] = 100;
                    pts[three_of_a_kind] = isum;
                    pct[three_of_a_kind] = isum * 100 / 30;
                    pts[four_of_a_kind] = isum;
                    pct[four_of_a_kind] = isum * 100 / 30;
                    return;
                case 4:
                    pts[three_of_a_kind] = isum;
                    pct[three_of_a_kind] = isum * 100 / 30;
                    pts[four_of_a_kind] = isum;
                    pct[four_of_a_kind] = isum * 100 / 30;
                    return;
            }
        }

    }
}