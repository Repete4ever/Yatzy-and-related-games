using System;
using System.Linq;
using System.Windows.Forms;

namespace Yatzy
{
    class HumanPanel : GamePanel
    {
        public HumanPanel(GameOfDice game, string gamerName, CheckHiScore checkHiScore, AutoGame computerGame,
            GamePanel computerPanel) :
                base(game, gamerName, checkHiScore, computerGame, computerPanel)
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

            if (i < UsedScores.GetUpperBound(0) && !UsedScores[i, j])
                Rubrik[i, j].Text = "";
            Cursor.Current = Cursors.Default;
        }

        private void myMouseLeaveItem(object sender, EventArgs e)
        {
            if (DiceVec.Sum() == 0)
                return; // sanity check
            int i, j;
            MyItemCoordinates((Control)sender, out i, out j);

            if (i < UsedScores.GetUpperBound(0) && !UsedScores[i, j])
                Rubrik[i, j].Text = "";
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
    }
}
