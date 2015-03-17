using System;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Speech.Synthesis;
using System.Windows.Forms;

namespace Yatzy
{
    public class ComputerPanel : GamePanel
    {
        private readonly Timer bgPainter = new Timer();
        private readonly Timer diePainter = new Timer();
        private readonly SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
        private string _nameLabelText;
        private Color _myDieColor;

        public ComputerPanel(GameOfDice game, CheckHiScore checkHiScore)
            : base(game, "HAL 6000", checkHiScore)
        {
            TerningeKast.Visible = false;
            StartAgain.Visible = false;

            _synthesizer.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Senior);
            _synthesizer.SetOutputToDefaultAudioDevice();

            bgPainter.Interval = 5;
            bgPainter.Tick += PaintItRed;

            diePainter.Interval = 10;
            diePainter.Tick += PaintItBlack;

        }

        /// <summary>
        /// Dial down Green and Blue, dial up Red until red hot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void PaintItRed(object sender, EventArgs eventArgs)
        {
            Color color = BackColor;
            int g = Math.Max(0, color.G - 1);
            int b = Math.Max(0, color.B - 1);
            int r = Math.Min(255, color.R + 1);
            Application.DoEvents();
            BackColor = Color.FromArgb(255, r, g, b);
            if (g == 0)
            {
                bgPainter.Stop();
                BackColor = Color.FromKnownColor(KnownColor.Control);
                gamerName = _nameLabelText;
            }
        }

        
        /// <summary>
        /// Start speaking infamous line from 2001 A Space Odyssee
        /// whereafter HAL decides to eliminate the crew from the equation.
        /// Then start bgPainter.
        /// </summary>
        protected override void NameLabelClicked()
        {
            _nameLabelText = gamerName;
            gamerName = "TAMPER ALARM";
            SystemSounds.Hand.Play();
            const string sorry = "I'm Sorry Dave, I'm Afraid I can't Do That";
            _synthesizer.SpeakAsync(sorry);
            bgPainter.Start();
        }

        private void PaintItBlack(object sender, EventArgs eventArgs)
        {
            Color color = _myDieColor;
            int green = Math.Max(0, color.G - 1);
            int blue = Math.Max(0, color.B - 1);
            int red = Math.Max(0, color.R - 1);
            Application.DoEvents();
            _myDieColor = Color.FromArgb(255, red, green, blue);
            using (var g = Graphics.FromHwnd(Handle))
            {
                for (var i = 0; i < Game.Dice; i++)
                    if (DiceRoll[i] == GameForm.RollState.RollMe)
                    {
                        DrawDie(g, i, DiceVec[i], _myDieColor);
                    }

                ShowSavedRolls(g);
            } 
            if (red == 0)
            {
                diePainter.Stop();

                UpdateTerningeKast();
            }
        }

        public void Reroll()
        {
            if (RollCounter == 0)
                return;

            if (RollCounter >= 3 && Game.SavedRolls == 0)
            {
                // we are out of rolls and must decide
                int bestScoreCol;
                int bestScoreRow = BestChoice(Game, out bestScoreCol);
                if (bestScoreRow >= 0)
                {
                    var itemStr = string.Format("{0}.{1}", bestScoreRow, bestScoreCol);
                    ScoreIt(itemStr, RollCounter);
                    TogglePanels();
                }
            }
            diePainter.Start();
        }

        protected override void AutoDecide()
        {
            ScoreType bestScore;
            int bestScoreCol;
            int bestScoreRow = BestChoice(Game, out bestScoreCol, out bestScore);
            if (bestScore.grade >= 90)
            {
                if (bestScore.score >= 15)
                {
                    // we have a winner, a perfect score e.g. 'small straight'
                    // or a good score, like a house of 66644 
                    // lousy winners such as one pair in Yatzy are not considered.
                    var itemStr = string.Format("{0}.{1}", bestScoreRow, bestScoreCol);
                    ScoreIt(itemStr, RollCounter);
                    TogglePanels();
                    return;
                }
            }
            bool rolling = false;
            for (var i = 0; i < Game.Dice; i++)
            {
                //if (DiceRoll[i] != GameForm.RollState.HoldMe)
                {
                    if (DiceVec[i] == Game.MostPopular(DiceVec, UsedScores))
                    {
                        DiceRoll[i] = GameForm.RollState.HoldMe;
                    }
                    else
                    {
                        DiceRoll[i] = GameForm.RollState.RollMe;
                        rolling = true;
                    }
                }
            }

            if (rolling)
            {
                Reroll();
            }
            else
            {
                var itemStr = string.Format("{0}.{1}", bestScoreRow, bestScoreCol);
                ScoreIt(itemStr, RollCounter);
                TogglePanels();
            }
        }

        public struct ScoreType
        {
            public int score;
            public int grade; // 0..100

            public int CompareTo(object s)
            {
                var rhs = (ScoreType) s;
                if (rhs.grade < grade)
                    return 1;
                if (rhs.grade == grade && rhs.score < score)
                    return 1;
                if (rhs.score == score && rhs.grade == grade)
                    return 0;
                return -1;
            }
        }

        private int BestChoice(GameOfDice game, out int bestScoreCol)
        {
            bestScoreCol = -1;
            int bestScoreRow = -1;
            int hiScore = -1;
            for (var row = 0; row < Game.UsableItems; row++)
            {
                for (var col = 0; col < Game.UsableScoreBoxesPerItem; col++)
                {
                    if (UsedScores[row, col]) continue;
                    int currentScore = game.ValueIt(DiceVec, row);
                    if (currentScore > hiScore)
                    {
                        hiScore = currentScore;
                        bestScoreRow = row;
                        bestScoreCol = col;
                    }
                }
            }
            return bestScoreRow;
        }
        private int BestChoice(GameOfDice game, out int bestScoreCol, out ScoreType st)
        {
            bestScoreCol = -1;
            st.grade = 0;
            st.score = -1;
            int bestScoreRow = -1;
            ScoreType currentScore = new ScoreType();
            for (var row = 0; row < Game.UsableItems; row++)
            {
                for (var col = 0; col < Game.UsableScoreBoxesPerItem; col++)
                {
                    if (UsedScores[row, col]) continue;
                    currentScore.score = game.ValueIt(DiceVec, row);
                    currentScore.grade = game.GradeIt(DiceVec, row);
                    if (currentScore.CompareTo(st) > 0)
                    {
                        st = currentScore;
                        bestScoreRow = row;
                        bestScoreCol = col;
                    }
                }
            }
            return bestScoreRow;
        }

       
    }
}
