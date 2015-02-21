using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Yatzy
{
    public partial class GameForm : Form
    {
        public const string RegFolder = @"Software\PHDGames\";

        private ScoreView _score;
        private int ChosenLanguage = CollectLanguage();
        
        public GameForm()
        {
            InitializeComponent();

            var collectPlayerName = new PlayerName();
            collectPlayerName.ShowDialog();
            Player = collectPlayerName.UserName;
            InitForm(CollectGameName());
        }

        private void ShowHiScore()
        {
            const string ScoreFile = @"Scores.xml";
            try
            {
                var Scores = new DataSet();
                Scores.ReadXml(ScoreFile);
                var Score = new DataView(Scores.Tables["Score"],
                    /* string Filter = */ "Game = " + "'" + Name + "'",
                    /* string Sort = */ "Bonus DESC, Point DESC",
                    DataViewRowState.CurrentRows);
                var scoreTable = Score.ToTable();

                // get rid of HumanGame Name since it is now redundant
                // prettyprint date and time
                var editedScoreTable = new DataTable();
                editedScoreTable.Columns.Add("Player", typeof(string));
                editedScoreTable.Columns.Add("Commenced", typeof(string));
                editedScoreTable.Columns.Add("Duration", typeof(TimeSpan));
                editedScoreTable.Columns.Add("Point", typeof(ushort));
                if (Name == "Balut")
                {
                    editedScoreTable.Columns.Add("Bonus", typeof(ushort));
                }
                foreach (DataRow dr in scoreTable.Rows)
                {
                    var commenced = dr.Field<string>("Commenced");
                    var ended = dr.Field<string>("Ended");
                    var point = dr.Field<long>("Point");
                    var started = DateTime.Parse(commenced);
                    var ts = DateTime.Parse(ended) - started;
                    if (Name == "Balut")
                    {
                        var bonus = dr.Field<long>("Bonus");
                        editedScoreTable.Rows.Add(dr.Field<string>("Player"), started, ts, ToUShort(point),
                            ToUShort(bonus));
                    }
                    else
                    {
                        editedScoreTable.Rows.Add(dr.Field<string>("Player"), started, ts, ToUShort(point));
                    }
                }
                var hiScore = new ScoreView
                {
                    Text = string.Format("{0} {1}", Name, "Score"),
                    Left = Size.Width + Left,
                    Top = Top,
                    Source = editedScoreTable
                };
                hiScore.ShowDialog();
            }
            catch (Exception e)
            {
                MessageBox.Show("" + e);
            }
        }

        private void CheckHiScore(GameOfDice game, string gamerName, DateTime commenced)
        {
            const string ScoreFile = @"Scores.xml";
            const string OldScoreFile = ScoreFile;
            //		string ScoreFileSchema = @"Scores.xsd";

            try
            {
                var Scores = new DataSet();
                // explicitly reading the XML Schema seemed a good idea bit did not work
                //			Scores.ReadXmlSchema(ScoreFileSchema);
                Scores.ReadXml(ScoreFile);

                var myRow = Scores.Tables["Score"].NewRow();
                myRow["Game"] = game;
                myRow["Player"] = gamerName;
                myRow["Point"] = game.GamePoints;
                var PLayingBalut = game.ToString() == "Balut";
                if (PLayingBalut)
                    myRow["Bonus"] = game.BonusPoints;
                var now = DateTime.Now;
                myRow["Ended"] = now.ToString("s", null); // ISO date format
                myRow["Commenced"] = commenced.ToString("s", null); // ISO date format
                Scores.Tables["Score"].Rows.Add(myRow);

                var Score = new DataView(Scores.Tables["Score"],
                    /* string Filter = */ "Game = " + "'" + game + "'",
                    /* string Sort = */ "Bonus DESC, Point DESC",
                    DataViewRowState.CurrentRows);
                const int HiLen = 10;
                const string HiScore = "You made the Top 10!";
                var ScoreHigh = false;
                if (Score.Count <= HiLen)
                {
                    MessageBox.Show(HiScore);
                    ScoreHigh = true;
                }
                else
                {
                    var ColumnToUse = PLayingBalut ? "Bonus" : "Point";
                    var ps = Score[HiLen - 1][ColumnToUse].ToString();
                    var pi = Int32.Parse(ps);
                    var g = game.GamePoints;
                    var Last = myRow["Ended"] == Score[HiLen - 1]["Ended"];
                    if (g > pi || Last)
                    {
                        MessageBox.Show(HiScore);
                        ScoreHigh = true;
                    }
                }

                var NewRow = 0;
                var Found = false;

                for (; NewRow < Score.Count; NewRow++)
                {
                    Found = Score[NewRow]["Ended"] == myRow["Ended"];
                    if (Found)
                        break;
                }
                if (!Found)
                    MessageBox.Show("Can't find new score in view!");

                if (_score == null)
                {
                    _score = new ScoreView();
                }
                _score.Hide();
                _score.Left = Size.Width + Left;
                _score.Top = Top;
                _score.Source = Score;
                if (ScoreHigh)
                    _score.SelectRow = NewRow;
                _score.Text = game + " Scores";
                _score.Show();
                _score.Focus();

                // save scores in a Memorystream
                using (var Source = new MemoryStream())
                using (var Target = new FileStream(OldScoreFile, FileMode.Open, FileAccess.Write, FileShare.None))
                {
                    Scores.WriteXml(Source, XmlWriteMode.WriteSchema);

                    // Scores.WriteXml(@"NewScores.xml", XmlWriteMode.WriteSchema);
                    // Lines below are lost after reading and writing but should be restored
                    // by inserting them at the top of the scores file afterward
                    // <?xml version="1.0" standalone="yes"?>
                    // <?xml-stylesheet type="text/xsl" href="Scores.xslt"?>
                    Source.Seek(0, SeekOrigin.Begin);
                    var HelloXML = "<?xml version=" + '"' + "1.0" + '"' + " standalone=" + '"' + "yes" + '"' + "?>";
                    var Xsl = "<?xml-stylesheet type=" + '"' + "text/xsl" + '"' + " href=" + '"' + "Scores.xslt" + '"' +
                              "?>";
                    // Read first line
                    //			int b = Source.ReadByte();
                    //			while(b != '\n') 
                    //			{
                    //				Target.WriteByte((byte)b);
                    //				b = Source.ReadByte();
                    //				if(b == -1) 
                    //					break;
                    //			}
                    //			Target.WriteByte((byte)b);
                    foreach (var t in HelloXML)
                        Target.WriteByte((byte)t);
                    // we are only providing UNIX LF 
                    // Is there a platform independant way of terminating lines, I wonder?
                    // Anyway, this code will when executing on windows create a file with mixed line terminations
                    Target.WriteByte((byte)'\n');
                    foreach (var t in Xsl)
                        Target.WriteByte((byte)t);
                    Target.WriteByte((byte)'\n');
                    while (true)
                    {
                        var b = Source.ReadByte();
                        if (b == -1) break;
                        Target.WriteByte((byte)b);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("" + e);
            }
        }

        private GameOfDice HumanGame;
        private GameOfDice HalGame;
        private readonly string Player;

        private void InitForm(string GameText)
        {
            HumanGame = null;
            if (GameText == "Yatzy")
                HumanGame = new Yatzy();
            if (GameText == "Yahtzee")
                HumanGame = new Yahtzee();
            if (GameText == "Maxiyatzy")
                HumanGame = new Maxiyatzy();
            if (GameText == "Balut")
                HumanGame = new Balut();
            if (HumanGame == null)
                HumanGame = new Yatzy();

            HalGame = null;
            if (GameText == "Yatzy")
                HalGame = new Yatzy();
            if (GameText == "Yahtzee")
                HalGame = new Yahtzee();
            if (GameText == "Maxiyatzy")
                HalGame = new Maxiyatzy();
            if (GameText == "Balut")
                HalGame = new Balut();
            if (HalGame == null)
                HalGame = new Yatzy();

            SaveGameName(GameText);

            HumanGame.InhabitTables();
            HalGame.InhabitTables();

            // Set the form's title
            Text = HumanGame + " on a date with C#";
            //Name = HumanGame.ToString();
            //Icon = Resources.Yatzy;

            //DiceVec = new int[HumanGame.Dice];
            //OldDice = new int[HumanGame.Dice];
            //DiceRoll = new RollState[HumanGame.Dice];
            //UsedScores = new bool[HumanGame.MaxItem + 1, HumanGame.ScoreBoxesPerItem];

            //Rounds = 0;
            
            //Rubrik = new TextBox[HumanGame.MaxTotalItem + 1, 6];
            //Items = new TextBox[HumanGame.MaxTotalItem + 1];

            scorePanels.Controls.Clear();
            Controls.Remove(scorePanels);

            
            bool _computerPlaysFirst = false; // ^= true;
            //_computerPanel = new ScorePanel(HalGame, "HAL", TerningeKast, CheckHiScore);
            //_humanPanel = new ScorePanel(HumanGame, Player, TerningeKast, CheckHiScore);

            // Make the tables nice

            //scorePanels.Height = _humanPanel.Height;
            //scorePanels.Top = ScoreCardStart;
            //scorePanels.Width = _humanPanel.Width * 2;

            // Set the form's size
            //ClientSize = new Size(Math.Max(HumanGame.Dice * OptimalDieSize * 11 / 10, scorePanels.Width), ScoreCardStart + _humanPanel.Height);

            //scorePanels.Controls.Add(_computerPlaysFirst ? _computerPanel : _humanPanel, 0, 0);
            //scorePanels.Controls.Add(_computerPlaysFirst ? _humanPanel : _computerPanel, 1, 0);
            GamePanel.ChosenLanguage = ChosenLanguage;
            var newPanel = new GamePanel(HumanGame, Player, CheckHiScore) { DieColor = Color.Blue, };
            var newPanel2 = new GamePanel(HalGame, "HAL", CheckHiScore) { DieColor = Color.Red };
            scorePanels.Controls.Add(newPanel, 0, 0);
            scorePanels.Controls.Add(newPanel2, 1, 0);
            scorePanels.Height = newPanel.Height;
            //scorePanels.Top = 0;
            scorePanels.Width = newPanel.Width * 2;
            ClientSize = scorePanels.Size;

            Controls.Add(scorePanels);

            HalfGame(newPanel);
        }

        public enum RollState
        {
            Unborn,
            RollMe,
            HoldMe
        };

        readonly Random DiceGen = new Random();

        private void HalfGame(GamePanel computerPanel)
        {
            var bestScore = -1;
            var bestScoreRow = -1;
            var bestScoreCol = -1;
            computerPanel.DiceVec = new int[HalGame.Dice];
            computerPanel.DiceRoll = new RollState[HalGame.Dice];
            int roll;
            for (roll = 1; roll <= 3; roll++)
            {
                var rolling = false;
                for (var i = 0; i < HalGame.Dice; i++)
                    if (computerPanel.DiceRoll[i] != RollState.HoldMe)
                    {
                        var die = computerPanel.DiceVec[i] = DiceGen.Next(1, 7);
                        if (die == 6)
                        {
                            computerPanel.DiceRoll[i] = RollState.HoldMe;
                        }
                        else
                        {
                            computerPanel.DiceRoll[i] = RollState.RollMe;
                            rolling = true;
                        }
                    }
                if (!rolling)
                {
                    break; // save a roll or two (only MaxiYatzy takes advantage though)
                }
            }
            for (var row = 0; row < HalGame.UsableItems; row++)
            {
                for (var col = 0; col < HalGame.UsableScoreBoxesPerItem; col++)
                {
                    if (computerPanel.UsedScores[row, col]) continue;
                    var diceValue = HalGame.ValueIt(computerPanel.DiceVec, row);
                    if (diceValue > bestScore)
                    {
                        bestScore = diceValue;
                        bestScoreRow = row;
                        bestScoreCol = col;
                    }
                }
            }
            var itemStr = string.Format("{0}{1}.{2}", "Rubrik", bestScoreRow, bestScoreCol);
            computerPanel.ScoreIt(itemStr);
            computerPanel.UsedScores[bestScoreRow, bestScoreCol] = true;
            Thread.Sleep(5);
        }


        private static int CollectLanguage()
        {
            try
            {
                var currentCulture = Thread.CurrentThread.CurrentCulture;
                switch (currentCulture.Name.Substring(0, 2))
                {
                    default:
                        return (int) GameOfDice.Language.English;
                    case "da":
                        return (int) GameOfDice.Language.Danish;
                    case "sv":
                        return (int) GameOfDice.Language.Swedish;
                }
            }
            catch
            {
                return (int) GameOfDice.Language.English;
            }
        }

        private string CollectGameName()
        {
            var GameKey = Registry.CurrentUser.CreateSubKey(RegFolder);
            return GameKey != null ? GameKey.GetValue("Game Name", "Yatzy").ToString() : "Yatzy";
        }

        private void SaveGameName(string name)
        {
            var GameKey = Registry.CurrentUser.CreateSubKey(RegFolder);
            if (GameKey != null) GameKey.SetValue("Game Name", name);
        }

        //private static string myDateTime(DateTime myDateTime)
        //{
        //    return string.Format("{0} {1}", myDateTime.ToShortDateString(), myDateTime.ToShortTimeString());
        //}

        private static ushort ToUShort(long val)
        {
            return val <= 65535 && val >= 0 ? (ushort) val : (ushort) 0;
        }

        // Handler for Options menu popups
        private void OnPopupOptionsMenu(object sender, EventArgs e)
        {
            Danish.Checked = ChosenLanguage == (int)GameOfDice.Language.Danish;
            Swedish.Checked = ChosenLanguage == (int)GameOfDice.Language.Swedish;
            English.Checked = ChosenLanguage == (int)GameOfDice.Language.English;
            TouchDice.Checked = GamePanel.Touchy;
            ClickDice.Checked = !GamePanel.Touchy;
            Undo.Enabled = GamePanel.Undoable;
        }

        private void OnPopupOptionsMenu2(object sender, EventArgs e)
        {
            PlayYatzy.Checked = HumanGame.ToString() == "Yatzy";
            PlayYahtzee.Checked = HumanGame.ToString() == "Yahtzee";
            PlayMaxiyatzy.Checked = HumanGame.ToString() == "Maxiyatzy";
            PlayBalut.Checked = HumanGame.ToString() == "Balut";
        }

        private void OnLanguage1(object sender, EventArgs e)
        {
            ChosenLanguage = (int)GameOfDice.Language.Danish;
            GamePanel.ChosenLanguage = ChosenLanguage;
            Invalidate();
        }

        private void OnLanguage2(object sender, EventArgs e)
        {
            ChosenLanguage = (int)GameOfDice.Language.Swedish;
            GamePanel.ChosenLanguage = ChosenLanguage;
            Invalidate();
        }

        private void OnLanguage3(object sender, EventArgs e)
        {
            ChosenLanguage = (int)GameOfDice.Language.English;
            GamePanel.ChosenLanguage = ChosenLanguage;
            Invalidate();
        }

        private void OnUndo(object sender, EventArgs e)
        {

        }

        private void OnTouch(object sender, EventArgs e)
        {
            TouchDice.Checked = true;
            ClickDice.Checked = false;
            GamePanel.Touchy = true;
        }

        private void OnClick(object sender, EventArgs e)
        {
            TouchDice.Checked = false;
            ClickDice.Checked = true;
            GamePanel.Touchy = false;
        }

        private void OnExit(object sender, EventArgs e)
        {
            Close();
        }

        private void OnGame1(object sender, EventArgs e)
        {
            var b = PlayYatzy.Checked;
            PlayYatzy.Checked = true;
            PlayYahtzee.Checked = false;
            PlayMaxiyatzy.Checked = false;
            PlayBalut.Checked = false;
            if (!b)
                InitForm("Yatzy");
        }

        private void OnGame2(object sender, EventArgs e)
        {
            var b = PlayYahtzee.Checked;
            PlayYatzy.Checked = false;
            PlayYahtzee.Checked = true;
            PlayMaxiyatzy.Checked = false;
            PlayBalut.Checked = false;
            if (!b)
                InitForm("Yahtzee");
        }

        private void OnGame3(object sender, EventArgs e)
        {
            var b = PlayMaxiyatzy.Checked;
            PlayYatzy.Checked = false;
            PlayYahtzee.Checked = false;
            PlayMaxiyatzy.Checked = true;
            PlayBalut.Checked = false;
            if (!b)
                InitForm("Maxiyatzy");
        }

        private void OnGame4(object sender, EventArgs e)
        {
            var b = PlayBalut.Checked;
            PlayYatzy.Checked = false;
            PlayYahtzee.Checked = false;
            PlayMaxiyatzy.Checked = false;
            PlayBalut.Checked = true;
            if (!b)
                InitForm("Balut");
        }

        private void OnHiScore(object sender, EventArgs e)
        {
            ShowHiScore();
        }

        private void OnRules(object sender, EventArgs e)
        {
            MyHelp(scorePanels);
        }

        private void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show("PhDGames©\n" +
                            "Created by Peter Hegelund\n" +
                            "Version 0.7.0.0, Feb 2015\n" +
                            "Programmed in C#", "About " + HumanGame,
                MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public void MyHelp(Control parent /*, myHelpEnum topic*/)
        {
            // The file to display is chosen by the value of the topic.
            switch (0)
            {
                //			case myHelpEnum.enumWidgets:
                default:
                    var a = new FileInfo(Application.ExecutablePath);
                    var b = "file:///" + a.DirectoryName + '/';
                    switch (ChosenLanguage)
                    {
                        case 0:
                            b += 'D';
                            break;
                        case 1:
                            b += 'S';
                            break;
                        case 2:
                            b += 'E';
                            break;
                    }
                    b += "Help.htm";
                    //string bb = b.Replace(' ', '_');
                    try
                    {
                        Help.ShowHelp(parent, b);
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show(
                            "Sorry, Can't find the requested help file at\n" + b,
                            "Help on " + HumanGame,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    break;
                //			case myHelpEnum.enumMechanism:
                //				// Insert code to implement additional functionality.
                //				break;
            }
        }

    }
}
