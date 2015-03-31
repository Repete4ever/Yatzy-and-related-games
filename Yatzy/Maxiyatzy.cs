namespace Yatzy
{
    public class Maxiyatzy : GameOfDice
    {
        public Maxiyatzy()
        {
            InitGameOfDice(6, 20, 100);
        }

        public override string ToString()
        {
            return "Maxiyatzy";
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
            {"3 PAR","TRE PAR","3 pairs"},
            {"3 ENS","TRETAL","3 of a kind"},
            {"4 ENS","FYRTAL","4 of a kind"},
            {"5 ENS","FEMTAL","5 of a kind"},
            {"YATZY!","YATZY!","Yatzy!"},
            {"Lille","Liten straight","Small straight"},
            {"Stor","Stor straight","Large straight"},
            {"Full straight","Full Straight","Full straight"},
            {"Hus","Kåk","House"},
            {"Stort Hus","Hus","Full House"},
            {"Tårn","Torn","Tower"},
            {"Chance","Chans","Chance"},
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
            get { return 3; } // makes layout look nice
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
                return item - 14; // sum of items 1-6
            if (IsABonusItem(item, 0))
                return item - 14; // bonus
            if (item > MaxItem + SumItems + BonusItems)
                return 7; // sum total
            if (item < 6)
                return item;
            if (item < 13)
                return item - 6;
            return item - 13;
        }

        public override int PreferredGroup(int item)
        {
            if (item < 6 || IsASumItem(item, 0) || IsABonusItem(item, 0))
                return 1;
            if (item < 13)
                return 2;
            return MaxGroup;
        }

        protected override void MyPoints(int[] nx)
        {
            int par = 5 + 1;
            int topar = par + 1;
            int trepar = topar + 1;
            int treens = trepar + 1;
            int fireens = treens + 1;
            int femens = fireens + 1;
            int yatzy_ = femens + 1;
            int lille = yatzy_ + 1;
            int stor = lille + 1;
            int fuldstraight = stor + 1;
            int hus = fuldstraight + 1;
            int fuldthus = hus + 1;
            int tower = fuldthus + 1;
            int chance = tower + 1;

            int maks = 0, isum = 0, j;
            int ih = 0;
            int i;

            for (i = 6; i <= MaxItem; i++)
            {
                pts[i] = 0;
                pct[i] = 0;
            }
            for (j = 6; j >= 1; j--)
            {
                int iv = nx[j - 1];
                pts[j - 1] = iv * j;
                pct[j - 1] = iv * 100 / 6;
                isum += iv * j;
                if (iv > maks)
                {
                    maks = iv;
                    ih = j;
                }
            }
            pts[chance] = isum;
            pct[chance] = isum * 100 / 36;
            switch (maks)
            {
                case 1:
                    pts[fuldstraight] = isum;
                    pct[fuldstraight] = 100;
                    pts[lille] = 15;
                    pct[lille] = 100;
                    pts[stor] = 20;
                    pct[stor] = 100;
                    return;
                case 2:
                    pts[lille] = 15;
                    pct[lille] = 100;
                    for (i = 1; i < 6; i++)
                        if (nx[i - 1] == 0)
                        {
                            pts[lille] = 0;
                            pct[lille] = 0;
                        }
                    pts[stor] = 20;
                    pct[stor] = 100;
                    for (i = 2; i <= 6; i++)
                        if (nx[i - 1] == 0)
                        {
                            pts[stor] = 0;
                            pct[stor] = 0;
                        }
                    pts[par] = ih * 2;
                    pct[par] = ih * 100 / 6;
                    // is there another pair?
                    for (i = 1; i < ih; i++)
                        if (nx[i - 1] == 2)
                        {
                            pts[topar] = (i + ih) * 2;
                            pct[topar] = (i + ih) * 100 / 11;
                            // and a third pair?
                            for (j = i - 1; j > 0; j--)
                                if (nx[j - 1] == 2)
                                {
                                    pts[trepar] = (i + j + ih) * 2;
                                    pct[trepar] = (i + j + ih) * 100 / 15;
                                }
                        }
                    return;
                case 3:
                    pts[par] = ih * 2;
                    pct[par] = ih * 100 / 6;
                    pts[treens] = ih * 3;
                    pct[treens] = ih * 100 / 6;
                    for (i = 1; i <= 6; i++)
                    {
                        if (nx[i - 1] == 2)
                        {
                            pts[hus] = ih * 3 + i * 2;
                            pct[hus] = pts[hus] * 100 / 28;
                            pts[topar] = (ih + i) * 2;
                            pct[topar] = pts[topar] * 100 / 22;
                            if (i > ih)
                            {
                                pts[par] = i * 2;
                                pct[par] = i * 100 / 6;
                            }
                            return;
                        }
                    }
                    for (i = 1; i < ih; i++)
                        if (nx[i - 1] == 3) // two threes
                        {
                            pts[hus] = ih * 3 + i * 2;
                            pct[hus] = pts[hus] * 100 / 28;
                            pts[fuldthus] = isum;
                            pct[fuldthus] = isum * 100 / 33;
                            pts[topar] = (ih + i) * 2;
                            pct[topar] = pts[topar] * 100 / 22;
                            return;
                        }
                    return;
                case 4:
                    pts[par] = ih * 2;
                    pct[par] = ih * 100 / 6;
                    pts[treens] = ih * 3;
                    pct[treens] = ih * 100 / 6;
                    pts[fireens] = ih * 4;
                    pct[fireens] = ih * 100 / 6;
                    for (i = 1; i <= 6; i++)
                        if (nx[i - 1] == 2)
                        {
                            pts[tower] = isum;
                            pct[tower] = isum * 100 / 34;
                            pts[hus] = ih * 3 + i * 2;
                            pct[hus] = pts[hus] * 100 / 28;
                            pts[topar] = ih * 2 + i * 2;
                            pct[topar] = pts[topar] * 100 / 22;
                            if (i > ih)
                            {
                                pts[par] = i * 2;
                                pct[par] = i * 100 / 6;
                            }
                        }
                    return;
                case 5:
                    pts[par] = ih * 2;
                    pct[par] = ih * 100 / 6;
                    pts[treens] = ih * 3;
                    pct[treens] = ih * 100 / 6;
                    pts[fireens] = ih * 4;
                    pct[fireens] = ih * 100 / 6;
                    pts[femens] = ih * 5;
                    pct[femens] = ih * 100 / 6;
                    return;
                case 6:
                    pts[par] = ih * 2;
                    pct[par] = ih * 100 / 6;
                    pts[treens] = ih * 3;
                    pct[treens] = ih * 100 / 6;
                    pts[fireens] = ih * 4;
                    pct[fireens] = ih * 100 / 6;
                    pts[femens] = ih * 5;
                    pct[femens] = ih * 100 / 6;
                    pts[yatzy_] = 100;
                    pct[yatzy_] = 100;
                    return;
            }
        }

    }
}