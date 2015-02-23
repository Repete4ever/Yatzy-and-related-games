﻿using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Yatzy
{
    public partial class GamePanel : Panel
    {
        public delegate void CheckHiScore(GameOfDice game, string gamer, DateTime started);

        public delegate void AutoGame(GamePanel panel);

        private const int OptimalDieSize = 66;
        public GameOfDice Game { get; private set; }

        // The two buttons are on the Design page

        private readonly string[] StartOver = { "Nyt spil", "Starta om", "New game" };
        public static readonly string[] klik =
        {
            "Klik på de terninger du vil slå om",
            "Klicka på de tärninger du vill slå om",
            "Select dice and reroll"
        };

        public static readonly string[] Result =
        {
            "Registrér resultatet i tabellen",
            "Registrere resultatet i tabellen",
            "Select a score box"
        };

        public TextBox[,] Rubrik;

        private int[] OldDice;
        public static bool Undoable { get; set; }

        private const int ItemWidth = 97;
        private const int ScoreSpacing = ItemWidth / 3;
        private const int ItemTop = 160;

        public readonly string gamerName;
        private readonly DateTime commenced;

        private readonly CheckHiScore checkHiScore;

        private int OldRollCounter;
        private int RollCounter;

        private static bool _touchy;

        public int[] DiceVec;

        private readonly AutoGame _computerGame;
        private readonly GamePanel _computerPanel;

        public GamePanel(GameOfDice game, string gamerName, CheckHiScore checkHiScore, AutoGame computerGame, GamePanel computerPanel)
        {
            ResizeRedraw = true; 
            
            InitializeComponent();

            Controls.Add(TerningeKast);
            Controls.Add(StartAgain);

            DiceVec = new int[game.Dice];
            this.Game = game;
            this.gamerName = gamerName;
            commenced = DateTime.Now;
            this.checkHiScore = checkHiScore;

            _computerGame = computerGame;
            _computerPanel = computerPanel;

            DrawPanel();
        }

        public Color DieColor { get; set; }

        /// <summary>
        /// In case the language changes
        /// </summary>
        public void RefreshItemTexts()
        {
            StartAgain.Text = StartOver[GameOfDice.ChosenLanguage];

            TerningeKastText = String.Format(Game.RollText(RollCounter), Game.Cardinal(0));
            TerningeKast.Text = TerningeKastText; for (var i = 0; i < Items.Length; i++)

            {
                Items[i].Text = Game.ItemText(i);
            }
        }

        private void DrawPanel()
        {
            Rubrik = new TextBox[Game.MaxTotalItem + 1, 6];
            Items = new TextBox[Game.MaxTotalItem + 1];
            UsedScores = new bool[Game.MaxItem + 1, Game.ScoreBoxesPerItem];
            DiceRoll = new GameForm.RollState[Game.Dice];
            OldDice = new int[Game.Dice];

            const int TextBoxHeight = 20;

            for (var Col = 0; Col < Game.MaxGroup; Col++)
            {
                for (var i = 0; i < Items.Length; i++)
                    if (Col + 1 == Game.PreferredGroup(i))
                    {
                        Items[i] = new myTextBox();
                        if (Game.FirstScoreBox(i) > 0)
                        {
                            Items[i].TextAlign = HorizontalAlignment.Right;
                            Items[i].Width = ItemWidth + Game.FirstScoreBox(i) * ScoreSpacing;
                        }
                        else
                            Items[i].Width = ItemWidth;
                        Items[i].Text = Game.ItemText(i);
                        Controls.Add(Items[i]);
                        Items[i].Enabled = false;
                        Items[i].BackColor = Color.Yellow;
                        Items[i].BorderStyle = BorderStyle.Fixed3D;
                        Items[i].Left = Col * ItemWidth * 3 / 2;
                        Items[i].Top = ItemTop + TextBoxHeight * Game.PreferredRow(i);

                        for (var j = 0; j < Game.ScoreBoxesPerItem; j++)
                            if (j >= Game.FirstScoreBox(i))
                            {
                                Rubrik[i, j] = new myTextBox();
                                Controls.Add(Rubrik[i, j]);
                                Rubrik[i, j].Name = "Rubrik" + i + "." + j;
                                Rubrik[i, j].Enabled = false;
                                Rubrik[i, j].Left = Items[i].Left + ItemWidth + j * ScoreSpacing;
                                Rubrik[i, j].Top = ItemTop + TextBoxHeight * Game.PreferredRow(i);
                                Rubrik[i, j].Width = ScoreSpacing;
                                Rubrik[i, j].BackColor = j >= Game.UsableScoreBoxesPerItem
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

            var maxTop = Items.Select(t => t.Top).Concat(new[] { 0 }).Max() + TextBoxHeight;
            const int borderThickness = TextBoxHeight + 10;
            maxTop += borderThickness;
            DieSize = Math.Min(OptimalDieSize, ClientSize.Width * 9 / (10 * Game.Dice));
            DieDist = DieSize / 10; 
            int diceWidth = (DieSize + DieDist) * Game.Dice;
            int scoreWidth = Game.MaxGroup*(ItemWidth*3/2) + (Game.ScoreBoxesPerItem - 1)*ScoreSpacing;
            Size = new Size(Math.Max(scoreWidth, diceWidth), maxTop);
            this.Paint += OnPaint;
            //this.Invalidate();
        }

        private void myMouseLeave(object sender, EventArgs e)
        {
            Int32 dot = ((Control)sender).Name.IndexOf('.');
            Int32 i = Int32.Parse(((Control)sender).Name.Substring(6, dot - 6));
            Int32 j = Int32.Parse(((Control)sender).Name.Substring(dot + 1));

            if (!UsedScores[i, j])
                Rubrik[i, j].Text = "";
            Cursor.Current = Cursors.Default;
        }

        private void myMouseMove(object sender, MouseEventArgs ex)
        {
            string SenderName = ((Control)sender).Name;
            Int32 dot = SenderName.IndexOf('.');
            Int32 i = Int32.Parse(SenderName.Substring(6, dot - 6));
            Int32 j = Int32.Parse(SenderName.Substring(dot + 1));

            Rubrik[i, j].Text = Game.ValueIt(DiceVec, i).ToString();
            Cursor.Current = Cursors.Arrow;
        }

        private void myMouseHover(object sender, EventArgs ex)
        {
            if (Touchy)
            {
                myMouseDecider(sender);
            }
        }

        private void myMouseDown(object sender, MouseEventArgs ex)
        {
            if (ex.Button == MouseButtons.Left)
            {
                myMouseDecider(sender);
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            for (var i = 0; i < Game.Dice; i++)
            {
                DrawDie(e.Graphics, i, DiceVec[i],
                    DiceRoll[i] == GameForm.RollState.RollMe ? Color.Black : DieColor);
            }

            ShowSavedRolls(e.Graphics);

            TerningeKastText = String.Format(Game.RollText(RollCounter), Game.Cardinal(0));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Location = new Point(0, DieSize + 5 * DieDist);
            TerningeKast.Size = new Size(DieSize * Game.Dice + DieDist * (Game.Dice - 1), 24);

            StartAgain.Text = StartOver[GameOfDice.ChosenLanguage];
            StartAgain.Location = new Point(0, DieSize + 5 * DieDist + 27);
            StartAgain.Size = new Size(DieSize * Game.Dice + DieDist * (Game.Dice - 1), 24);
        }

        private int Rounds;

        public GameForm.RollState[] DiceRoll { get; set; }

        private void ShowScore()
        {
            int diffPoints = Game.GamePoints - _computerPanel.Game.GamePoints;
            if (diffPoints > 0)
                MessageBox.Show(string.Format("{0} scored {1} and won by {2}", gamerName, Game.GamePoints, diffPoints));
            if (diffPoints == 0)
                MessageBox.Show(string.Format("{0} scored {1} and made a draw", gamerName, Game.GamePoints));
            if (diffPoints < 0)
                MessageBox.Show(string.Format("{0} scored {1} and lost by {2}", gamerName, Game.GamePoints, -diffPoints));
        }

        /// <summary>
        /// </summary>
        /// <param name="S"></param>
        /// <param name="roll"></param>
        public void ScoreIt(string S, int roll)
        {
            const string R = "Rubrik";
            if (!S.StartsWith(R))
                return;
            var dot = S.IndexOf('.');
            DecidingRow = int.Parse(S.Substring(R.Length, dot - R.Length));
            DecidingCol = int.Parse(S.Substring(dot + 1));
            RollCounter = roll;

            Rubrik[DecidingRow, DecidingCol].Text = Game.ScoreIt(DiceVec, DecidingRow, DecidingCol, RollCounter);
            if (Game.ScoreBoxesPerItem == 1)
            {
                for (var ro = Game.MaxItem + 1; ro <= Game.MaxTotalItem; ro++)
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
                Undoable = true;
            }
            else
            {
                TerningeKastText = "";
                TerningeKast.Enabled = false;
                checkHiScore(Game, gamerName, commenced);
            }
            TerningeKast.Text = TerningeKastText;

            using (var g = Graphics.FromHwnd(Handle))
            {
                for (var r = 0; r < Game.Dice; r++)
                {
                    DiceRoll[r] = GameForm.RollState.Unborn;
                    OldDice[r] = DiceVec[r];
                    DiceVec[r] = 0;
                    DrawDie(g, r, 0, DieColor);
                }

                for (var r = 0; r <= Game.MaxItem; r++)
                    for (var d = Game.FirstScoreBox(r); d < Game.UsableScoreBoxesPerItem; d++)
                    {
                        Rubrik[r, d].Enabled = false;
                        if (!UsedScores[r, d])
                            Rubrik[r, d].Text = "";
                    }

                ShowSavedRolls(g);
            }
        }

        public bool[,] UsedScores;
        private int DieSize;
        private int DieDist;
        public string TerningeKastText { get; set; }

        public static bool Touchy
        {
            get { return _touchy; }
            set { _touchy = value; }
        }

        public string GamerName
        {
            get { return gamerName; }
        }

        /// <summary>
        /// ornament an ordinary six-sided die
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
                var EyeSize = S / 7; // seems fair
                var point = new Point(X + (S - EyeSize) / 2, Y + (S - EyeSize) / 2); // a little NV of the center
                var Eye = new Size(EyeSize, EyeSize);
                var SnakeEye = new Rectangle(point, Eye);
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

        private void DrawDie(Graphics g, int die, int val, Color DieCol)
        {
            Contract.Assert(die >= 0);
            Contract.Assert(die < Game.Dice);

            DieSize = Math.Min(OptimalDieSize, ClientSize.Width * 9 / (10 * Game.Dice));
            DieDist = DieSize / 10;
            var CornerSize = DieDist * 7 / 6;
            var Overshoot = DieDist + CornerSize;
            var OvershootHalved = Overshoot / 2;
            var OffsetX = die * (DieSize + DieDist);

            var MyDie = new Rectangle(OffsetX, 10, DieSize, DieSize);

            //intersect the die with an ellipse to produce rounded corners
            var CornerDie = new Rectangle(OffsetX - OvershootHalved, 10 - OvershootHalved,
                DieSize + Overshoot, DieSize + Overshoot);

            using (var DieBrush = new SolidBrush(DieCol))
            using (var myGraphicsPath = new GraphicsPath())
            {
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
            var Pins = new Rectangle(DieSize * Game.Dice / 2 + 2 * DieDist - 7, DieSize + 2 * DieDist, 20, 10);
            var format = new StringFormat
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
                        var PinCount = String.Format("{0}", Game.SavedRolls);
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
            var control = (Control)sender;
            if (!control.Name.StartsWith(R))
                return;
            var S = control.Name;
            var dot = S.IndexOf('.');
            DecidingRow = Int32.Parse(S.Substring(R.Length, dot - R.Length));
            DecidingCol = Int32.Parse(S.Substring(dot + 1));

            Rubrik[DecidingRow, DecidingCol].Text = Game.ScoreIt(DiceVec, DecidingRow, DecidingCol, RollCounter);
            if (Game.ScoreBoxesPerItem == 1)
            {
                for (var ro = Game.MaxItem + 1; ro <= Game.MaxTotalItem; ro++)
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
            OldRollCounter = RollCounter;
            RollCounter = 0;
            Rounds++;
            if (Rounds < Game.MaxRound)
            {
                TerningeKastText =
                    String.Format(Game.RollText(RollCounter),
                        Game.Cardinal(RollCounter));
                TerningeKast.Enabled = true;
                TerningeKast.Focus();
                Undoable = true;
            }
            
            TerningeKast.Text = TerningeKastText;

            using (var g = Graphics.FromHwnd(Handle))
            {
                for (var r = 0; r < Game.Dice; r++)
                {
                    DiceRoll[r] = GameForm.RollState.Unborn;
                    OldDice[r] = DiceVec[r];
                    DiceVec[r] = 0;
                    DrawDie(g, r, 0, DieColor);
                }

                for (var r = 0; r <= Game.MaxItem; r++)
                    for (var d = Game.FirstScoreBox(r); d < Game.UsableScoreBoxesPerItem; d++)
                    {
                        Rubrik[r, d].Enabled = false;
                        if (!UsedScores[r, d])
                            Rubrik[r, d].Text = "";
                    }
                ShowSavedRolls(g);
            }

            if (_computerGame != null && _computerPanel != null)
            {
                _computerGame(_computerPanel); // play the other side now
            }

            if (Rounds == Game.MaxRound)
            {
                TerningeKastText = "";
                TerningeKast.Enabled = false;
                checkHiScore(Game, gamerName, commenced);
                ShowScore();
            }
        }

        public void UnDecider()
        {
            for (var d = 0; d < Game.Dice; d++)
                DiceVec[d] = OldDice[d]; // get that roll back

            RollCounter = OldRollCounter;
            Rubrik[DecidingRow, DecidingCol].Text = Game.UnScoreIt(DiceVec, DecidingRow, DecidingCol, RollCounter);
            if (Game.ScoreBoxesPerItem == 1)
            {
                for (var ro = Game.MaxItem + 1; ro <= Game.MaxTotalItem; ro++)
                    Rubrik[ro, DecidingCol].Text = Game.UnScoreIt(DiceVec, ro, DecidingCol, RollCounter);
            }
            else
            {
                Rubrik[DecidingRow, 4].Text = Game.UnScoreIt(DiceVec, DecidingRow, 4, RollCounter);
                Rubrik[DecidingRow, 5].Text = Game.UnScoreIt(DiceVec, DecidingRow, 5, RollCounter);
                Rubrik[7, 5].Text = Game.UnScoreIt(DiceVec, 7, 5, RollCounter);
                Rubrik[7, 4].Text = Game.UnScoreIt(DiceVec, 7, 4, RollCounter);
                Rubrik[8, 5].Text = Game.UnScoreIt(DiceVec, 8, 5, RollCounter);
            }
            Rubrik[DecidingRow, DecidingCol].Enabled = true;
            UsedScores[DecidingRow, DecidingCol] = false;
            Rounds--;
            //		if(Rounds<Game.MaxRound)
            {
                TerningeKastText = RollCounter == 3 && Game.SavedRolls == 0
                    ? Result[GameOfDice.ChosenLanguage]
                    : klik[GameOfDice.ChosenLanguage];
                TerningeKast.Enabled = false;
            }
            TerningeKast.Text = TerningeKastText;

            using (var g = Graphics.FromHwnd(Handle))
            {
                for (var r = 0; r < Game.Dice; r++)
                {
                    DiceRoll[r] = GameForm.RollState.HoldMe;
                    DrawDie(g, r, DiceVec[r], Color.Red);
                }

                for (var r = 0; r <= Game.MaxItem; r++)
                    for (var d = Game.FirstScoreBox(r); d < Game.UsableScoreBoxesPerItem; d++)
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

        private int TargetDie = -1; // when selecting dice by merely moving the mouse

        private readonly Random DiceGen = new Random();

        private void OnButtonClicked(Object sender, EventArgs e)
        {
            Undoable = false;
            TerningeKastText = RollCounter == 2 && Game.SavedRolls == 0
                ? Result[GameOfDice.ChosenLanguage]
                : klik[GameOfDice.ChosenLanguage];
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = false;

            TargetDie = -1;

            RollCounter++;
            if (RollCounter > 3)
            {
                Game.UseARoll();
            }
            using (var g = Graphics.FromHwnd(Handle))
            {
                ShowSavedRolls(g);

                for (var i = 0; i < Game.Dice; i++)
                    if (DiceRoll[i] != GameForm.RollState.HoldMe)
                    {
                        DiceRoll[i] = GameForm.RollState.HoldMe;
                        DiceVec[i] = DiceGen.Next(1, 7);
                        DrawDie(g, i, DiceVec[i], DieColor);
                    }

                for (var r = 0; r <= Game.MaxItem; r++)
                    for (var j = Game.FirstScoreBox(r); j < Game.UsableScoreBoxesPerItem; j++)
                    {
                        if (!UsedScores[r, j])
                            Rubrik[r, j].Enabled = true;
                    }
            }
        }

        private void OnButtonClicked2(Object sender, EventArgs e)
        {
            Undoable = false;

            RollCounter = 0;
            TerningeKastText = String.Format(Game.RollText(RollCounter), Game.Cardinal(RollCounter));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = true;
            TerningeKast.Focus();

            TargetDie = -1;

            Rounds = 0;
            Game.NewGame();

            using (var g = Graphics.FromHwnd(Handle))
            {
                for (var i = 0; i < Game.Dice; i++)
                {
                    DiceRoll[i] = GameForm.RollState.Unborn;
                    DiceVec[i] = 0;
                    DrawDie(g, i, 0, DieColor);
                }
                ShowSavedRolls(g);
            }

            int r;
            for (r = 0; r <= Game.MaxItem; r++)
                for (var j = Game.FirstScoreBox(r); j < Game.ScoreBoxesPerItem; j++)
                {
                    UsedScores[r, j] = false;
                    Rubrik[r, j].Enabled = false;
                    Rubrik[r, j].Text = "";
                }

            for (; r <= Game.MaxTotalItem; r++)
            {
                Rubrik[r, 0].Enabled = false;
                Rubrik[r, 0].Text = "";
            }
        }

        protected void TouchOrClick(MouseEventArgs e)
        {
            var SaveTargetDie = TargetDie;
            TargetDie = -1;

            if (e.Y > DieSize)
                return;
            var DieClicked = e.X / (DieSize + DieDist);

            if (DieClicked >= Game.Dice)
                return;

            if (RollCounter == 0)
                return;

            if (RollCounter >= 3 && Game.SavedRolls == 0)
                return;

            var SameDie = SaveTargetDie == DieClicked;
            TargetDie = DieClicked;
            if (ScorePanel.Touchy && SameDie)
                return;
            var ThisDieMustRoll = DiceRoll[DieClicked] == GameForm.RollState.RollMe;

            DiceRoll[DieClicked] = ThisDieMustRoll ? GameForm.RollState.HoldMe : GameForm.RollState.RollMe;

            using (var g = Graphics.FromHwnd(Handle))
            {
                DrawDie(g, DieClicked, DiceVec[DieClicked],
                    ThisDieMustRoll ? DieColor : Color.Black);

                ShowSavedRolls(g);
            }

            TerningeKastText = String.Format(Game.RollText(RollCounter), Game.Cardinal(RollCounter));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = true;
            TerningeKast.Focus();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!Touchy)
                TouchOrClick(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Touchy)
                TouchOrClick(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            TargetDie = -1;
        }

    }

    internal class myTextBox : TextBox
    {
        public myTextBox()
        {
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var drawBrush = new SolidBrush(ForeColor); //Use the ForeColor property 

            // Draw string to screen. 

            e.Graphics.DrawString(Text, Font, drawBrush, 0f, 0f); //Use the Font property 
        }
    }
}
