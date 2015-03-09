namespace Yatzy
{
    public sealed class Yatzy : FiveDice
    {
        public Yatzy()
        {
            InitGameOfDice(5, 15, 50);
        }

        public override string ToString()
        {
            return "Yatzy";
        }

        readonly string[,] Items = {
            {"ENERE","ETTOR","1's"},
            {"TOERE","TVÅOR","2's"},
            {"TREERE","TREOR","3's"},
            {"FIRERE","FYROR","4's"},
            {"FEMMERE","FEMMOR","5's"},
            {"SEKSERE","SEXOR","6's"},
            {"1 PAR","ETT PAR","1 pair"},
            {"2 PAR","TVÅ PAR","2 pairs"},
            {"3 ENS","TRETAL","3 of a kind"},
            {"4 ENS","FYRTAL","4 of a kind"},
            {"LILLE","LITEN STRAIGHT","Sm straight"},
            {"STOR","STOR STRAIGHT","Lg straight"},
            {"HUS","KÅK","House"},
            {"CHANCE","CHANS","Chance"},
            {"YATZY","YATZY","Yatzy"},
            {"SUM","SUMMA","Sum"},
            {"BONUS","BONUS","Bonus"},
            {"TOTAL","SUMMA","Total"}
        };

        public override string ItemText(int item)
        {
            return Items[item, ChosenLanguage];
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
                return item - 9; // sum of items 1-6
            if (IsABonusItem(item, 0))
                return item - 9; // bonus
            if (item > MaxItem + SumItems + BonusItems)
                return 9; // sum total
            if (item < 6)
                return item;
            return item - 6;
        }

        public override int SavedRolls
        {
            get { return 0; } // the notion of saved rolls doesn't exist
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
            int par = 5 + 1;
            int topar = par + 1;
            int treens = topar + 1;
            int fireens = treens + 1;
            int lille = fireens + 1;
            int stor = lille + 1;
            int hus = stor + 1;
            int chance = hus + 1;
            int yatzy_ = chance + 1;

            int maks = 0, isum = 0;
            int ih = 0;

            for (int i = 6; i <= MaxItem; i++) pts[i] = 0;
            for (int j = 6; j >= 1; j--)
            {
                var iv = nx[j - 1];
                pts[j - 1] = iv * j;
                pct[j - 1] = iv * 20;
                isum += iv * j;
                if (iv > maks)
                {
                    maks = iv;
                    ih = j;
                }
            }
            pts[chance] = isum;
            pct[chance] = isum*100/30;
            switch (maks)
            {
                case 1:
                    if (isum == 15)
                    {
                        pts[lille] = 15;
                        pct[lille] = 100;
                    }
                    if (isum == 20)
                    {
                        pts[stor] = 20;
                        pct[stor] = 100;
                    }
                    break;
                case 2:
                    pts[par] = ih * 2;
                    pct[par] = ih * 100 / 6;
                    if (ih == 1) break;
                    for (int i = 1; i < ih; i++)
                        if (nx[i - 1] == 2)
                        {
                            pts[topar] = (i + ih) * 2;
                            pct[topar] = (i + ih) * 100 / 11;
                        }
                    break;
                case 3:
                    pts[par] = ih * 2;
                    pct[par] = ih * 100 / 6;
                    pts[treens] = ih * 3;
                    pct[treens] = ih * 100 / 6;
                    for (int i = 1; i <= 6; i++)
                    {
                        if (nx[i - 1] == 2)
                        {
                            pts[hus] = ih * 3 + i * 2;
                            pct[hus] = pts[hus] * 100 / 28;
                            pts[topar] = (ih + i) * 2;
                            pct[topar] = (ih + i) * 100 / 11;
                            if (i > ih)
                            {
                                pts[par] = i * 2;
                                pct[par] = ih * 100 / 6;
                            }
                            break;
                        }
                    }
                    break;
                case 5:
                    pts[yatzy_] = 50;
                    pct[yatzy_] = 100;
                    pts[par] = ih * 2;
                    pct[par] = ih * 100 / 6;
                    pts[treens] = ih * 3;
                    pct[treens] = ih * 100 / 6;
                    pts[fireens] = ih * 4;
                    pct[fireens] = ih * 100 / 6;
                    break;
                case 4:
                    pts[par] = ih * 2;
                    pct[par] = ih * 100 / 6;
                    pts[treens] = ih * 3;
                    pct[treens] = ih * 100 / 6;
                    pts[fireens] = ih * 4;
                    pct[fireens] = ih * 100 / 6;
                    break;
            }
        }
    }
}