﻿using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Yatzy
{
    public partial class ScorePanel : Panel
    {
        public delegate void CheckHiScore(GameOfDice game, string gamer, DateTime started);

        private const int OptimalDieSize = 66;
        private readonly GameOfDice game;
        public TextBox[,] Rubrik;
        private readonly int[] OldDice;
        public bool Undoable { get; set; }

        private const int ItemWidth = 97;
        private const int ScoreWidth = ItemWidth/3;
        private const int ItemTop = 0;

        private readonly string gamerName;
        private readonly DateTime commenced;

        private readonly CheckHiScore checkHiScore;

        private int OldRollCounter;
        private int RollCounter;

        private static bool _touchy = true;

        public int[] DiceVec;

        public ScorePanel(GameOfDice game, string gamerName, Button terningeKast, CheckHiScore checkHiScore)
        {
            InitializeComponent();

            DiceVec = new int[game.Dice];
            this.game = game;
            this.gamerName = gamerName;
            commenced = DateTime.Now;
            this.checkHiScore = checkHiScore;

            TerningeKast = terningeKast;
            Rubrik = new TextBox[this.game.MaxTotalItem + 1, 6];
            Items = new TextBox[this.game.MaxTotalItem + 1];
            UsedScores = new bool[this.game.MaxItem + 1, this.game.ScoreBoxesPerItem];
            DiceRoll = new MyForm.RollState[this.game.Dice];
            OldDice = new int[this.game.Dice];

            const int TextBoxHeight = 20;

            for (var Col = 0; Col < this.game.MaxGroup; Col++)
            {
                for (var i = 0; i < Items.Length; i++)
                    if (Col + 1 == this.game.PreferredGroup(i))
                    {
                        Items[i] = new myTextBox();
                        if (this.game.FirstScoreBox(i) > 0)
                        {
                            Items[i].TextAlign = HorizontalAlignment.Right;
                            Items[i].Width = ItemWidth + this.game.FirstScoreBox(i)*ScoreWidth;
                        }
                        else
                            Items[i].Width = ItemWidth;
                        Items[i].Text = this.game.ItemText(i);
                        Controls.Add(Items[i]);
                        Items[i].Enabled = false;
                        Items[i].BackColor = Color.Yellow;
                        Items[i].BorderStyle = BorderStyle.Fixed3D;
                        Items[i].Left = Col*ItemWidth*3/2;
                        Items[i].Top = ItemTop + TextBoxHeight*this.game.PreferredRow(i);

                        for (var j = 0; j < this.game.ScoreBoxesPerItem; j++)
                            if (j >= this.game.FirstScoreBox(i))
                            {
                                Rubrik[i, j] = new myTextBox();
                                Controls.Add(Rubrik[i, j]);
                                Rubrik[i, j].Name = "Rubrik" + i + "." + j;
                                Rubrik[i, j].Enabled = false;
                                Rubrik[i, j].Left = Items[i].Left + ItemWidth + j*ScoreWidth;
                                Rubrik[i, j].Top = ItemTop + TextBoxHeight*this.game.PreferredRow(i);
                                Rubrik[i, j].Width = ScoreWidth;
                                Rubrik[i, j].BackColor = j >= this.game.UsableScoreBoxesPerItem
                                    ? Color.Coral
                                    : Color.AliceBlue;
                                Rubrik[i, j].BorderStyle = BorderStyle.Fixed3D;
                                Rubrik[i, j].MouseDown += myMouseDown;
                                Rubrik[i, j].MouseHover += myMouseHover;
                                Rubrik[i, j].MouseMove += myMouseMove;
                                Rubrik[i, j].MouseLeave += myMouseLeave;
                            }
                    }
            }

            var maxTop = Items.Select(t => t.Top).Concat(new[] {0}).Max() + TextBoxHeight;
            const int borderThickness = 4;
            maxTop += borderThickness;
            Size = new Size(this.game.MaxGroup*(ItemWidth*3/2) + (this.game.ScoreBoxesPerItem - 1)*ScoreWidth, maxTop);
        }

        private int Rounds;

        public MyForm.RollState[] DiceRoll { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="S"></param>
        public void ScoreIt(string S)
        {
            const string R = "Rubrik";
            if (!S.StartsWith(R))
                return;
            var dot = S.IndexOf('.');
            DecidingRow = int.Parse(S.Substring(R.Length, dot - R.Length));
            DecidingCol = int.Parse(S.Substring(dot + 1));

            Rubrik[DecidingRow, DecidingCol].Text = game.ScoreIt(DiceVec, DecidingRow, DecidingCol, RollCounter);
            if (game.ScoreBoxesPerItem == 1)
            {
                for (var ro = game.MaxItem + 1; ro <= game.MaxTotalItem; ro++)
                    Rubrik[ro, DecidingCol].Text = game.ScoreIt(DiceVec, ro, DecidingCol, RollCounter);
            }
            else
            {
                Rubrik[DecidingRow, 4].Text = game.ScoreIt(DiceVec, DecidingRow, 4, RollCounter);
                Rubrik[DecidingRow, 5].Text = game.ScoreIt(DiceVec, DecidingRow, 5, RollCounter);
                Rubrik[7, 5].Text = game.ScoreIt(DiceVec, 7, 5, RollCounter);
                Rubrik[7, 4].Text = game.ScoreIt(DiceVec, 7, 4, RollCounter);
                Rubrik[8, 5].Text = game.ScoreIt(DiceVec, 8, 5, RollCounter);
            }
            Rubrik[DecidingRow, DecidingCol].Enabled = false;
            UsedScores[DecidingRow, DecidingCol] = true;
            RollCounter = 0;
            Rounds++;
            if (Rounds < game.MaxRound)
            {
                TerningeKastText =
                    String.Format(game.RollText(RollCounter),
                        game.Cardinal(RollCounter));
                TerningeKast.Enabled = true;
                TerningeKast.Focus();
                //Undoable = true;
            }
            else
            {
                TerningeKastText = "";
                TerningeKast.Enabled = false;
                checkHiScore(game, gamerName, commenced);
            }
            TerningeKast.Text = TerningeKastText;

            using (var g = Graphics.FromHwnd(Handle))
            {
                for (var r = 0; r < game.Dice; r++)
                {
                    DiceRoll[r] = MyForm.RollState.Unborn;
                    OldDice[r] = DiceVec[r];
                    DiceVec[r] = 0;
                    DrawDie(g, r, 0, Color.Red);
                }

                for (var r = 0; r <= game.MaxItem; r++)
                    for (var d = game.FirstScoreBox(r); d < game.UsableScoreBoxesPerItem; d++)
                    {
                        Rubrik[r, d].Enabled = false;
                        if (!UsedScores[r, d])
                            Rubrik[r, d].Text = "";
                    }

                ShowSavedRolls(g);
            }
        }

        public bool[,] UsedScores;
        private readonly Button TerningeKast;
        private int DieSize;
        private int DieDist;
        public string TerningeKastText { get; set; }

        public static int ChosenLanguage { get; set; }

        public static bool Touchy
        {
            get { return _touchy; }
            set { _touchy = value; }
        }

        /// <summary>
        ///     ornament an ordinary six-sided die
        /// </summary>
        /// <param name="g"></param>
        /// <param name="die"></param>
        /// <param name="eyes"></param>
        private static void OrnamentDie(Graphics g, Rectangle die, int eyes)
        {
            using (var Whiteout = new SolidBrush(Color.White))
            {
                if (die.Height != die.Width)
                    return; // not square
                if (eyes == 0)
                    return; // the die is blank!
                if (eyes > 6)
                    return; // the die is fake!
                var S = die.Height;
                var X = die.X;
                var Y = die.Y;
                var EyeSize = S/7; // seems fair
                var point = new Point(X + (S - EyeSize)/2, Y + (S - EyeSize)/2); // a little NV of the center
                var Eye = new Size(EyeSize, EyeSize);
                var SnakeEye = new Rectangle(point, Eye);
                if ((eyes%2) != 0)
                    g.FillEllipse(Whiteout, SnakeEye); // die 1,3,5
                if (eyes == 1)
                    return; // done with die 1
                // moving to the NE corner
                point.X += S/4;
                point.Y -= S/4;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
                // moving to the SV corner
                point.X -= S/2;
                point.Y += S/2;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
                if (eyes <= 3)
                    return;
                // going straight E
                point.X += S/2;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
                // going to the opposite corner, i.e. NV
                point.X -= S/2;
                point.Y -= S/2;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
                if (eyes <= 5)
                    return;
                // moving down a notch
                point.Y += S/4;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
                // moving east for final dot in die 6
                point.X += S/2;
                SnakeEye.Location = point;
                g.FillEllipse(Whiteout, SnakeEye);
            }
        }

        private void DrawDie(Graphics g, int die, int val, Color DieCol)
        {
            Contract.Assert(die >= 0);
            Contract.Assert(die < game.Dice);
            using (var DieBrush = new SolidBrush(DieCol))
            using (var myGraphicsPath = new GraphicsPath())
            {
                DieSize = Math.Min(OptimalDieSize, ClientSize.Width*9/(10*game.Dice));
                DieDist = DieSize/10;
                var CornerSize = DieDist*7/6;
                var Overshoot = DieDist + CornerSize;
                var OvershootHalved = Overshoot/2;
                var OffsetX = die*(DieSize + DieDist);

                var MyDie = new Rectangle(OffsetX, 10, DieSize, DieSize);

                //intersect the die with an ellipse to produce rounded corners
                var CornerDie = new Rectangle(OffsetX - OvershootHalved, 10 - OvershootHalved,
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
            var Pins = new Rectangle(DieSize*game.Dice/2 + 2*DieDist - 7, DieSize + 2*DieDist, 20, 10);
            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (var b = new SolidBrush(BackColor))
            {
                g.FillRectangle(b, Pins);
                if (game.SavedRolls > 0)
                {
                    using (var br = new SolidBrush(Color.Black))
                    {
                        var PinCount = String.Format("{0}", game.SavedRolls);
                        g.DrawString(PinCount, Font, br, Pins, format);
                    }
                }
            }
        }

        private int DecidingRow;
        private int DecidingCol;
        public TextBox[] Items;

        private void myMouseDecider(object sender)
        {
            const string R = "Rubrik";
            var control = (Control) sender;
            if (!control.Name.StartsWith(R))
                return;
            var S = control.Name;
            var dot = S.IndexOf('.');
            DecidingRow = Int32.Parse(S.Substring(R.Length, dot - R.Length));
            DecidingCol = Int32.Parse(S.Substring(dot + 1));

            Rubrik[DecidingRow, DecidingCol].Text = game.ScoreIt(DiceVec, DecidingRow, DecidingCol, RollCounter);
            if (game.ScoreBoxesPerItem == 1)
            {
                for (var ro = game.MaxItem + 1; ro <= game.MaxTotalItem; ro++)
                    Rubrik[ro, DecidingCol].Text = game.ScoreIt(DiceVec, ro, DecidingCol, RollCounter);
            }
            else
            {
                Rubrik[DecidingRow, 4].Text = game.ScoreIt(DiceVec, DecidingRow, 4, RollCounter);
                Rubrik[DecidingRow, 5].Text = game.ScoreIt(DiceVec, DecidingRow, 5, RollCounter);
                Rubrik[7, 5].Text = game.ScoreIt(DiceVec, 7, 5, RollCounter);
                Rubrik[7, 4].Text = game.ScoreIt(DiceVec, 7, 4, RollCounter);
                Rubrik[8, 5].Text = game.ScoreIt(DiceVec, 8, 5, RollCounter);
            }
            Rubrik[DecidingRow, DecidingCol].Enabled = false;
            UsedScores[DecidingRow, DecidingCol] = true;
            OldRollCounter = RollCounter;
            RollCounter = 0;
            Rounds++;
            if (Rounds < game.MaxRound)
            {
                TerningeKastText =
                    String.Format(game.RollText(RollCounter),
                        game.Cardinal(RollCounter));
                TerningeKast.Enabled = true;
                TerningeKast.Focus();
                Undoable = true;
            }
            else
            {
                TerningeKastText = "";
                TerningeKast.Enabled = false;
                checkHiScore(game, gamerName, commenced);
            }
            TerningeKast.Text = TerningeKastText;

            using (var g = Graphics.FromHwnd(Handle))
            {
                for (var r = 0; r < game.Dice; r++)
                {
                    DiceRoll[r] = MyForm.RollState.Unborn;
                    OldDice[r] = DiceVec[r];
                    DiceVec[r] = 0;
                    DrawDie(g, r, 0, Color.Red);
                }

                for (var r = 0; r <= game.MaxItem; r++)
                    for (var d = game.FirstScoreBox(r); d < game.UsableScoreBoxesPerItem; d++)
                    {
                        Rubrik[r, d].Enabled = false;
                        if (!UsedScores[r, d])
                            Rubrik[r, d].Text = "";
                    }
                ShowSavedRolls(g);
            }
        }

        public void UnDecider()
        {
            for (var d = 0; d < game.Dice; d++)
                DiceVec[d] = OldDice[d]; // get that roll back

            RollCounter = OldRollCounter;
            Rubrik[DecidingRow, DecidingCol].Text = game.UnScoreIt(DiceVec, DecidingRow, DecidingCol, RollCounter);
            if (game.ScoreBoxesPerItem == 1)
            {
                for (var ro = game.MaxItem + 1; ro <= game.MaxTotalItem; ro++)
                    Rubrik[ro, DecidingCol].Text = game.UnScoreIt(DiceVec, ro, DecidingCol, RollCounter);
            }
            else
            {
                Rubrik[DecidingRow, 4].Text = game.UnScoreIt(DiceVec, DecidingRow, 4, RollCounter);
                Rubrik[DecidingRow, 5].Text = game.UnScoreIt(DiceVec, DecidingRow, 5, RollCounter);
                Rubrik[7, 5].Text = game.UnScoreIt(DiceVec, 7, 5, RollCounter);
                Rubrik[7, 4].Text = game.UnScoreIt(DiceVec, 7, 4, RollCounter);
                Rubrik[8, 5].Text = game.UnScoreIt(DiceVec, 8, 5, RollCounter);
            }
            Rubrik[DecidingRow, DecidingCol].Enabled = true;
            UsedScores[DecidingRow, DecidingCol] = false;
            Rounds--;
            //		if(Rounds<game.MaxRound)
            {
                TerningeKastText = RollCounter == 3 && game.SavedRolls == 0
                    ? MyForm.Result[ChosenLanguage]
                    : MyForm.klik[ChosenLanguage];
                TerningeKast.Enabled = false;
            }
            TerningeKast.Text = TerningeKastText;

            using (var g = Graphics.FromHwnd(Handle))
            {
                for (var r = 0; r < game.Dice; r++)
                {
                    DiceRoll[r] = MyForm.RollState.HoldMe;
                    DrawDie(g, r, DiceVec[r], Color.Red);
                }

                for (var r = 0; r <= game.MaxItem; r++)
                    for (var d = game.FirstScoreBox(r); d < game.UsableScoreBoxesPerItem; d++)
                    {
                        if (!UsedScores[r, d])
                        {
                            Rubrik[r, d].Enabled = true;
                            Rubrik[r, d].Text = "";
                        }
                    }

                ShowSavedRolls(g);
            }
        }

        /// <summary>
        ///     Function signature must match the signature of the
        ///     MouseEventHandler class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        private void myMouseDown(object sender, MouseEventArgs ex)
        {
            if (ex.Button == MouseButtons.Left)
            {
                myMouseDecider(sender);
            }
        }

        /// <summary>
        ///     A method that shows your scoring if you choose to score it here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        private void myMouseMove(object sender, MouseEventArgs ex)
        {
            var SenderName = ((Control) sender).Name;
            var dot = SenderName.IndexOf('.');
            var i = Int32.Parse(SenderName.Substring(6, dot - 6));
            var j = Int32.Parse(SenderName.Substring(dot + 1));

            Rubrik[i, j].Text = game.ValueIt(DiceVec, i).ToString();
            Cursor.Current = Cursors.Arrow;
        }

        private void myMouseHover(object sender, EventArgs e)
        {
            if (_touchy)
            {
                myMouseDecider(sender);
            }
        }

        /// <summary>
        ///     A method that deletes the potential score
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        private void myMouseLeave(object sender, EventArgs ex)
        {
            var dot = ((Control) sender).Name.IndexOf('.');
            var i = Int32.Parse(((Control) sender).Name.Substring(6, dot - 6));
            var j = Int32.Parse(((Control) sender).Name.Substring(dot + 1));

            if (!UsedScores[i, j])
                Rubrik[i, j].Text = "";
            Cursor.Current = Cursors.Default;
        }
    }
}
