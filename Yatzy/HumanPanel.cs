using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Yatzy
{
    class HumanPanel : GamePanel
    {
        public HumanPanel(GameOfDice game, string gamerName, InitForm changeGame, ShowStatus showStatus) :
            base(game, gamerName, changeGame, showStatus)
        {
        }

        private void myMouseMoveOverRubrik(object sender, MouseEventArgs ex)
        {
            int i, j;
            MyRubrikCoordinates((Control)sender, out i, out j);

            Rubrik[i, j].Text = Game.ValueIt(DiceVec, i).ToString();
            Cursor.Current = Cursors.Arrow;
        }

        private void myMouseMoveOverItem(object sender, MouseEventArgs ex)
        {
            if (DiceVec.Sum() == 0)
                return; // sanity check
            int i, j;
            MyItemCoordinates((Control)sender, out i, out j);

            TextBox textBox = Rubrik[i, j];
            if (textBox != null)
            {
                textBox.Text = Game.ValueIt(DiceVec, i).ToString();
            }
            Cursor.Current = Cursors.Arrow;
        }

        private void myMouseHover(object sender, EventArgs ex)
        {
            if (Touchy)
            {
                myMouseDecider(sender);
            }
        }

        protected override void AssignRubrikHandlers(TextBox rubrik)
        {
            rubrik.MouseDown += myMouseDown;
            rubrik.MouseHover += myMouseHover;
            rubrik.MouseMove += myMouseMoveOverRubrik;
            rubrik.MouseLeave += myMouseLeaveRubrik;
        }

        protected override void AssignItemHandlers(TextBox item)
        {
            item.MouseMove += myMouseMoveOverItem;
            item.MouseLeave += myMouseLeaveItem;
        }

        private void myMouseLeaveRubrik(object sender, EventArgs e)
        {
            int i, j;
            MyRubrikCoordinates((Control)sender, out i, out j);

            if (i < UsedScores.GetUpperBound(0) && !UsedScores[i, j] && Rubrik[i, j] != null)
                Rubrik[i, j].Text = "";
            Cursor.Current = Cursors.Default;
        }

        private void myMouseLeaveItem(object sender, EventArgs e)
        {
            if (DiceVec.Sum() == 0)
                return; // sanity check
            int i, j;
            MyItemCoordinates((Control)sender, out i, out j);

            if (i < UsedScores.GetUpperBound(0) && !UsedScores[i, j] && Rubrik[i,j] != null)
            {
                Rubrik[i, j].Text = "";
            }
            Cursor.Current = Cursors.Default;
        }

        private void myMouseDown(object sender, MouseEventArgs ex)
        {
            if (ex.Button == MouseButtons.Left)
            {
                myMouseDecider(sender);
            }
        }

        protected override void NameLabelClicked()
        {
            var collectPlayerName = new PlayerName();
            var result = collectPlayerName.ShowDialog();
            if (result == DialogResult.OK)
            {
                nameLabel.Text = gamerName = collectPlayerName.UserName;
            }
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

        private void TouchOrClick(MouseEventArgs e)
        {
            var SaveTargetDie = TargetDie;
            TargetDie = -1;

            if (e.Y > DieSize)
                return;
            var dieClicked = e.X / (DieSize + DieDist);

            if (dieClicked >= Game.Dice)
                return;

            if (RollCounter == 0)
                return;

            if (RollCounter >= 3 && Game.SavedRolls == 0)
                return;

            var SameDie = SaveTargetDie == dieClicked;
            TargetDie = dieClicked;
            if (Touchy && SameDie)
                return;
            var ThisDieMustRoll = DiceRoll[dieClicked] == GameForm.RollState.RollMe;

            DiceRoll[dieClicked] = ThisDieMustRoll ? GameForm.RollState.HoldMe : GameForm.RollState.RollMe;

            using (var g = Graphics.FromHwnd(Handle))
            {
                DrawDie(g, dieClicked, DiceVec[dieClicked],
                    ThisDieMustRoll ? DieColor : Color.Black);

                ShowSavedRolls(g);
            }

            UpdateTerningeKast();
        }

        protected override void Judge()
        {
            _status(typeof(HumanPanel), "");
            if (RollCounter > 0)
            {
                FiveDice AiGame = Game as FiveDice;
                if (AiGame != null && AiGame.Bonus(0) != "" && !(AiGame is Balut))
                {
                    int nodeNo = CurrentNodeNo();
                    int[,] UnusedI = new int[Game.UsableItems, 2];
                    var ActiveI = ActiveItems(nodeNo, UnusedI);

                    int[] orderRoll = Game.OrderRoll(DiceVec);
                    AiGame.GamePlan(3, orderRoll, UnusedI, nodeNo, ActiveI, 0);
                    int seqno = AiGame.Seqno(orderRoll);
                    int[] AiKeep = new int[6];
                    int k = AiGame.Keep[seqno, RollCounter - 1];
                    AiGame.Status(k, AiKeep);
                    int[] humanKeep = new int[6];
                    for (var i = 0; i < Game.Dice; i++)
                        if (DiceRoll[i] == GameForm.RollState.HoldMe)
                        {
                            humanKeep[DiceVec[i] - 1]++;
                        }
                    int humanSeq = AiGame.Seqno(humanKeep);
                    int aiSeq = AiGame.Seqno(AiKeep);
                    if (aiSeq != humanSeq)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 6; i > 0; i--)
                        {
                            for (int j = 0; j < AiKeep[i - 1]; j++)
                                sb.Append(i.ToString());
                        }
                        var humanValue = AiGame.Value[humanSeq, RollCounter];
                        var aiValue = AiGame.Value[aiSeq, RollCounter];
                        var lostPoints = aiValue - humanValue;
                        if (lostPoints > 0.01)
                        {
                            _status(typeof (HumanPanel), string.Format("{1} should have kept {0}, a loss of {2} points",sb, GamerName, lostPoints.ToString("F2")));
                        }
                    }
                }
            }
        }

        protected void myMouseDecider(object sender)
        {
            _status(typeof(HumanPanel), "");

            var control = (Control)sender;
            string rowAndCol = control.Tag.ToString();

            FiveDice AiGame = Game as FiveDice;
            if (AiGame != null && AiGame.Bonus(0) != "" && !(AiGame is Balut))
            {
                var dot = rowAndCol.IndexOf('.');
                int decidingRow = int.Parse(rowAndCol.Substring(0, dot));
                int nodeNo = CurrentNodeNo();

                int[,] UnusedI = new int[Game.UsableItems, 2];
                var ActiveI = ActiveItems(nodeNo, UnusedI);
                int[] orderRoll = Game.OrderRoll(DiceVec);
                AiGame.GamePlan(3, orderRoll, UnusedI, nodeNo, ActiveI, 0);
                int seqno = AiGame.Seqno(orderRoll);

                int name = AiGame.Name[seqno];
                if (decidingRow != name)
                {
                    int yourNode = nodeNo - ItemNode(decidingRow);
                    int myNode = nodeNo - ItemNode(name);
                    double lostPoints = AiGame.Expect[myNode] - AiGame.Expect[yourNode];
                    _status(
                        typeof (ComputerPanel),
                        string.Format("You chose {0} but {1} is better by {2} points",
                            Game.ItemText(decidingRow),
                            Game.ItemText(name),
                            lostPoints.ToString("F2")
                            ));
                }
            }

            ScoreIt(rowAndCol, RollCounter);

            if (Rounds == Game.MaxRound)
            {
                TerningeKastText = "";
                TerningeKast.Enabled = false;
            }
            TogglePanels();
        }
    }
}
