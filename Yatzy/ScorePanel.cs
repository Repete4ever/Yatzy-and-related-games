using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Yatzy
{
    public partial class ScorePanel : Panel
    {
        public delegate void  CheckHiScore(GameOfDice game, string gamer, DateTime started);
        private const int OptimalDieSize = 66;
        private readonly GameOfDice Game;
        private readonly TextBox[,] Rubrik;
        private readonly int[] OldDice;
        public bool Undoable { get; set; }

        const int ItemWidth = 97;
        const int ScoreWidth = ItemWidth / 3;
        const int ItemTop = 0;

        private readonly string gamerName;
        private readonly DateTime commenced;

        private CheckHiScore checkHiScore;

        public ScorePanel(GameOfDice game, string gamerName, Button terningeKast, CheckHiScore checkHiScore)
        {
            InitializeComponent();

            Game = game;
            this.gamerName = gamerName;
            this.commenced = DateTime.Now;
            this.checkHiScore = checkHiScore;

            Text = string.Format("{0}'s {1} Score Card", gamerName, game);
            TerningeKast = terningeKast;
            Rubrik = new TextBox[Game.MaxTotalItem + 1, 6];
            var items = new TextBox[Game.MaxTotalItem + 1];
            UsedScores = new bool[Game.MaxItem + 1, Game.ScoreBoxesPerItem];
            DiceRoll = new RollState[Game.Dice];
            OldDice = new int[Game.Dice];

            const int TextBoxHeight = 20;

            for (int Col = 0; Col < Game.MaxGroup; Col++)
            {
                for (int i = 0; i < items.Length; i++)
                    if (Col + 1 == Game.PreferredGroup(i))
                    {
                        items[i] = new myTextBox();
                        if (Game.FirstScoreBox(i) > 0)
                        {
                            items[i].TextAlign = HorizontalAlignment.Right;
                            items[i].Width = ItemWidth + Game.FirstScoreBox(i) * ScoreWidth;
                        }
                        else
                            items[i].Width = ItemWidth;
                        items[i].Text = Game.ItemText(i);
                        this.Controls.Add(items[i]);
                        items[i].Enabled = false;
                        items[i].BackColor = Color.Yellow;
                        items[i].BorderStyle = BorderStyle.Fixed3D;
                        items[i].Left = Col * ItemWidth * 3 / 2;
                        items[i].Top = ItemTop + TextBoxHeight * Game.PreferredRow(i);

                        for (int j = 0; j < Game.ScoreBoxesPerItem; j++)
                            if (j >= Game.FirstScoreBox(i))
                            {
                                Rubrik[i, j] = new myTextBox();
                                this.Controls.Add(Rubrik[i, j]);
                                Rubrik[i, j].Name = "Rubrik" + i + "." + j;
                                Rubrik[i, j].Enabled = false;
                                Rubrik[i, j].Left = items[i].Left + ItemWidth + j * ScoreWidth;
                                Rubrik[i, j].Top = ItemTop + TextBoxHeight * Game.PreferredRow(i);
                                Rubrik[i, j].Width = ScoreWidth;
                                Rubrik[i, j].BackColor = j >= Game.UsableScoreBoxesPerItem ? Color.Coral : Color.AliceBlue;
                                Rubrik[i, j].BorderStyle = BorderStyle.Fixed3D;
                            }
                    }
            }

            int maxTop = items.Select(t => t.Top).Concat(new[] { 0 }).Max() + TextBoxHeight;
            const int borderThickness = 2;
            maxTop += borderThickness;
            Size = new Size(Game.MaxGroup * (ItemWidth * 3 / 2) + (Game.ScoreBoxesPerItem - 1) * ScoreWidth, maxTop);

            //Controls.Add(ScoreCard);
        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        int Rounds;

        enum RollState { Unborn, HoldMe };

        readonly RollState[] DiceRoll;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="S"></param>
        /// <param name="DiceVec"></param>
        /// <param name="RollCounter"></param>
        public void ScoreIt(string S, int[] DiceVec, int RollCounter = 3)
        {
            const string R = "Rubrik";
            if (!S.StartsWith(R))
                return;
            int dot = S.IndexOf('.');
            int DecidingRow = int.Parse(S.Substring(R.Length, dot - R.Length));
            int DecidingCol = int.Parse(S.Substring(dot + 1));

            Rubrik[DecidingRow, DecidingCol].Text = Game.ScoreIt(DiceVec, DecidingRow, DecidingCol, RollCounter);
            if (Game.ScoreBoxesPerItem == 1)
            {
                for (int ro = Game.MaxItem + 1; ro <= Game.MaxTotalItem; ro++)
                    Rubrik[ro, DecidingCol].Text = Game.ScoreIt(DiceVec, ro, DecidingCol, RollCounter);
            }
            else
            {
                Rubrik[DecidingRow, 4].Text = Game.ScoreIt(DiceVec, DecidingRow, 4, RollCounter);
                Rubrik[DecidingRow, 5].Text = Game.ScoreIt(DiceVec, DecidingRow, 5, RollCounter);
                Rubrik[7, 5].Text = Game.ScoreIt(DiceVec, 7, 5, RollCounter);
                Rubrik[7, 4].Text = Game.ScoreIt(DiceVec, 7, 4, RollCounter);
                Rubrik[8, 5].Text = Game.ScoreIt(DiceVec, 8, 5, RollCounter);
            }
            Rubrik[DecidingRow, DecidingCol].Enabled = false;
            UsedScores[DecidingRow, DecidingCol] = true;
            RollCounter = 0;
            Rounds++;
            if (Rounds < Game.MaxRound)
            {
                TerningeKastText =
                    String.Format(Game.RollText(RollCounter),
                    Game.Cardinal(RollCounter));
                TerningeKast.Enabled = true;
                TerningeKast.Focus();
                //Undoable = true;
            }
            else
            {
                TerningeKastText = "";
                TerningeKast.Enabled = false;
                checkHiScore(Game, gamerName, commenced);
            }
            TerningeKast.Text = TerningeKastText;

            Graphics g = Graphics.FromHwnd(Handle);

            for (int r = 0; r < Game.Dice; r++)
            {
                DiceRoll[r] = RollState.Unborn;
                OldDice[r] = DiceVec[r];
                DiceVec[r] = 0;
                DrawDie(g, r, 0, Color.Red);
            }

            for (int r = 0; r <= Game.MaxItem; r++)
                for (int d = Game.FirstScoreBox(r); d < Game.UsableScoreBoxesPerItem; d++)
                {
                    Rubrik[r, d].Enabled = false;
                    if (!UsedScores[r, d])
                        Rubrik[r, d].Text = "";
                }

            ShowSavedRolls(g);
        }

        readonly bool[,] UsedScores;
        private readonly Button TerningeKast;
        private int DieSize;
        private int DieDist;
        public string TerningeKastText { get; set; }

        /// <summary>
        ///  ornament an ordinary six-sided die
        /// </summary>
        /// <param name="g"></param>
        /// <param name="die"></param>
        /// <param name="eyes"></param>
        private static void OrnamentDie(Graphics g, Rectangle die, int eyes)
        {
            using (var Whiteout = new SolidBrush(Color.White))
            {
                if (die.Height != die.Width)
                    return;  // not square
                if (eyes == 0)
                    return;  // the die is blank!
                if (eyes > 6)
                    return;  // the die is fake!
                int S = die.Height;
                int X = die.X;
                int Y = die.Y;
                int EyeSize = S / 7;  // seems fair
                Point point = new Point(X + (S - EyeSize) / 2, Y + (S - EyeSize) / 2); // a little NV of the center
                Size Eye = new Size(EyeSize, EyeSize);
                Rectangle SnakeEye = new Rectangle(point, Eye);
                if ((eyes % 2) != 0)
                    g.FillEllipse(Whiteout, SnakeEye); // die 1,3,5
                if (eyes == 1)
                    return; // done with die 1
                // moving to the NE corner
                point.X += S / 4;
                point.Y -= S / 4;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
                // moving to the SV corner
                point.X -= S / 2;
                point.Y += S / 2;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
                if (eyes <= 3)
                    return;
                // going straight E
                point.X += S / 2;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
                // going to the opposite corner, i.e. NV
                point.X -= S / 2;
                point.Y -= S / 2;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
                if (eyes <= 5)
                    return;
                // moving down a notch
                point.Y += S / 4;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
                // moving east for final dot in die 6
                point.X += S / 2;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
            }
        }

        void DrawDie(Graphics g, int die, int val, Color DieCol)
        {
            Contract.Assert(die >= 0);
            Contract.Assert(die < Game.Dice);
            using (var DieBrush = new SolidBrush(DieCol))
            using (var myGraphicsPath = new GraphicsPath())
            {
                DieSize = Math.Min(OptimalDieSize, ClientSize.Width * 9 / (10 * Game.Dice));
                DieDist = DieSize / 10;
                int CornerSize = DieDist * 7 / 6;
                int Overshoot = DieDist + CornerSize;
                int OvershootHalved = Overshoot / 2;
                int OffsetX = die * (DieSize + DieDist);

                Rectangle MyDie = new Rectangle(OffsetX, 10, DieSize, DieSize);

                //intersect the die with an ellipse to produce rounded corners
                Rectangle CornerDie = new Rectangle(OffsetX - OvershootHalved, 10 - OvershootHalved,
                    DieSize + Overshoot, DieSize + Overshoot);
                myGraphicsPath.AddEllipse(CornerDie);
                using (var MyRegion = new Region(myGraphicsPath))
                {
                    MyRegion.Intersect(MyDie);
                    g.FillRegion(DieBrush, MyRegion);

                    OrnamentDie(g, MyDie, val);
                }
            }

        }

        private void ShowSavedRolls(Graphics g)
        {
            Rectangle Pins = new Rectangle(DieSize * Game.Dice / 2 + 2 * DieDist - 7, DieSize + 2 * DieDist, 20, 10);
            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (var b = new SolidBrush(BackColor))
            {
                g.FillRectangle(b, Pins);
                if (Game.SavedRolls > 0)
                {
                    using (var br = new SolidBrush(Color.Black))
                    {
                        string PinCount = String.Format("{0}", Game.SavedRolls);
                        g.DrawString(PinCount, Font, br, Pins, format);
                    }
                }
            }
        }
    }
}
