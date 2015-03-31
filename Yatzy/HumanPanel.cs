using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Yatzy
{
    class HumanPanel : GamePanel
    {
        public HumanPanel(GameOfDice game, string gamerName, CheckHiScore checkHiScore, InitForm changeGame) :
            base(game, gamerName, checkHiScore, changeGame)
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




    }
}
