using System;
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

        private const int OptimalDieSize = 66;
        private readonly GameOfDice game;

        // Controls and texts embedded in this Panel

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
        private const int ScoreWidth = ItemWidth / 3;
        private const int ItemTop = 200;

        private readonly string gamerName;
        private readonly DateTime commenced;

        private readonly CheckHiScore checkHiScore;

        private int OldRollCounter;
        private int RollCounter;

        private static bool _touchy;

        public int[] DiceVec;

        public GamePanel(GameOfDice game, string gamerName, CheckHiScore checkHiScore)
        {
            ResizeRedraw = true; 
            
            InitializeComponent();

            Controls.Add(TerningeKast);
            Controls.Add(StartAgain);

            DiceVec = new int[game.Dice];
            this.game = game;
            this.gamerName = gamerName;
            commenced = DateTime.Now;
            this.checkHiScore = checkHiScore;

            DrawPanel();
        }

        public Color DieColor { get; set; }

        private void DrawPanel()
        {
            Rubrik = new TextBox[game.MaxTotalItem + 1, 6];
            Items = new TextBox[game.MaxTotalItem + 1];
            UsedScores = new bool[game.MaxItem + 1, game.ScoreBoxesPerItem];
            DiceRoll = new GameForm.RollState[game.Dice];
            OldDice = new int[game.Dice];

            const int TextBoxHeight = 20;

            for (var Col = 0; Col < game.MaxGroup; Col++)
            {
                for (var i = 0; i < Items.Length; i++)
                    if (Col + 1 == game.PreferredGroup(i))
                    {
                        Items[i] = new myTextBox();
                        if (game.FirstScoreBox(i) > 0)
                        {
                            Items[i].TextAlign = HorizontalAlignment.Right;
                            Items[i].Width = ItemWidth + game.FirstScoreBox(i) * ScoreWidth;
                        }
                        else
                            Items[i].Width = ItemWidth;
                        Items[i].Text = game.ItemText(i);
                        Controls.Add(Items[i]);
                        Items[i].Enabled = false;
                        Items[i].BackColor = Color.Yellow;
                        Items[i].BorderStyle = BorderStyle.Fixed3D;
                        Items[i].Left = Col * ItemWidth * 3 / 2;
                        Items[i].Top = ItemTop + TextBoxHeight * game.PreferredRow(i);

                        for (var j = 0; j < game.ScoreBoxesPerItem; j++)
                            if (j >= game.FirstScoreBox(i))
                            {
                                Rubrik[i, j] = new myTextBox();
                                Controls.Add(Rubrik[i, j]);
                                Rubrik[i, j].Name = "Rubrik" + i + "." + j;
                                Rubrik[i, j].Enabled = false;
                                Rubrik[i, j].Left = Items[i].Left + ItemWidth + j * ScoreWidth;
                                Rubrik[i, j].Top = ItemTop + TextBoxHeight * game.PreferredRow(i);
                                Rubrik[i, j].Width = ScoreWidth;
                                Rubrik[i, j].BackColor = j >= game.UsableScoreBoxesPerItem
                                    ? Color.Coral
                                    : Color.AliceBlue;
                                Rubrik[i, j].BorderStyle = BorderStyle.Fixed3D;
                                //Rubrik[i, j].MouseDown += myMouseDown;
                                //Rubrik[i, j].MouseHover += myMouseHover;
                                //Rubrik[i, j].MouseMove += myMouseMove;
                                //Rubrik[i, j].MouseLeave += myMouseLeave;
                            }
                    }
            }

            var maxTop = Items.Select(t => t.Top).Concat(new[] { 0 }).Max() + TextBoxHeight;
            const int borderThickness = 4;
            maxTop += borderThickness;
            Size = new Size(Math.Max(game.MaxGroup * (ItemWidth * 3 / 2) + (game.ScoreBoxesPerItem - 1) * ScoreWidth,0), maxTop);
            this.Paint += OnPaint;
            //this.Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            for (var i = 0; i < game.Dice; i++)
            {
                DrawDie(e.Graphics, i, DiceVec[i],
                    DiceRoll[i] == GameForm.RollState.RollMe ? Color.Black : DieColor);
            }

            ShowSavedRolls(e.Graphics);

            TerningeKastText = String.Format(game.RollText(RollCounter), game.Cardinal(0));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Location = new Point(0, DieSize + 5 * DieDist);
            TerningeKast.Size = new Size(DieSize * game.Dice + DieDist * (game.Dice - 1), 24);

            StartAgain.Text = StartOver[ChosenLanguage];
            StartAgain.Location = new Point(0, DieSize + 5 * DieDist + 27);
            StartAgain.Size = new Size(DieSize * game.Dice + DieDist * (game.Dice - 1), 24);
        }

        private int Rounds;

        public GameForm.RollState[] DiceRoll { get; set; }

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
                    DiceRoll[r] = GameForm.RollState.Unborn;
                    OldDice[r] = DiceVec[r];
                    DiceVec[r] = 0;
                    DrawDie(g, r, 0, DieColor);
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

        private const int CompensateMenu = 25;
        private void DrawDie(Graphics g, int die, int val, Color DieCol)
        {
            Contract.Assert(die >= 0);
            Contract.Assert(die < game.Dice);

            DieSize = Math.Min(OptimalDieSize, ClientSize.Width * 10 / (10 * game.Dice));
            DieDist = DieSize / 10;
            var CornerSize = DieDist * 7 / 6;
            var Overshoot = DieDist + CornerSize;
            var OvershootHalved = Overshoot / 2;
            var OffsetX = die * (DieSize + DieDist);

            var MyDie = new Rectangle(OffsetX, CompensateMenu, DieSize, DieSize);

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
            var Pins = new Rectangle(DieSize * game.Dice / 2 + 2 * DieDist - 7, DieSize + 2 * DieDist + CompensateMenu, 20, 10);
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
            var control = (Control)sender;
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
                    DiceRoll[r] = GameForm.RollState.Unborn;
                    OldDice[r] = DiceVec[r];
                    DiceVec[r] = 0;
                    DrawDie(g, r, 0, DieColor);
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

        private int TargetDie = -1; // when selecting dice by merely moving the mouse

        private readonly Random DiceGen = new Random();

        private void OnButtonClicked(Object sender, EventArgs e)
        {
            Undoable = false;
            TerningeKastText = RollCounter == 2 && game.SavedRolls == 0
                ? Result[ChosenLanguage]
                : klik[ChosenLanguage];
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = false;

            TargetDie = -1;

            RollCounter++;
            if (RollCounter > 3)
            {
                game.UseARoll();
            }
            using (var g = Graphics.FromHwnd(Handle))
            {
                ShowSavedRolls(g);

                for (var i = 0; i < game.Dice; i++)
                    if (DiceRoll[i] != GameForm.RollState.HoldMe)
                    {
                        DiceRoll[i] = GameForm.RollState.HoldMe;
                        DiceVec[i] = DiceGen.Next(1, 7);
                        DrawDie(g, i, DiceVec[i], DieColor);
                    }

                for (var r = 0; r <= game.MaxItem; r++)
                    for (var j = game.FirstScoreBox(r); j < game.UsableScoreBoxesPerItem; j++)
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
            TerningeKastText = String.Format(game.RollText(RollCounter), game.Cardinal(RollCounter));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = true;
            TerningeKast.Focus();

            TargetDie = -1;

            Rounds = 0;
            game.NewGame();

            using (var g = Graphics.FromHwnd(Handle))
            {
                for (var i = 0; i < game.Dice; i++)
                {
                    DiceRoll[i] = GameForm.RollState.Unborn;
                    DiceVec[i] = 0;
                    DrawDie(g, i, 0, DieColor);
                }
                ShowSavedRolls(g);
            }

            int r;
            for (r = 0; r <= game.MaxItem; r++)
                for (var j = game.FirstScoreBox(r); j < game.ScoreBoxesPerItem; j++)
                {
                    UsedScores[r, j] = false;
                    Rubrik[r, j].Enabled = false;
                    Rubrik[r, j].Text = "";
                }

            for (; r <= game.MaxTotalItem; r++)
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

            if (DieClicked >= game.Dice)
                return;

            if (RollCounter == 0)
                return;

            if (RollCounter >= 3 && game.SavedRolls == 0)
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

            TerningeKastText = String.Format(game.RollText(RollCounter), game.Cardinal(RollCounter));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = true;
            TerningeKast.Focus();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!ScorePanel.Touchy)
                TouchOrClick(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (ScorePanel.Touchy)
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
