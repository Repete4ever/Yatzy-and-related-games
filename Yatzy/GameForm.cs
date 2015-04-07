using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Yatzy.Properties;

namespace Yatzy
{
    public partial class GameForm : Form
    {
        public static string RegFolder = Path.Combine("Software", Settings.Default.Repository);
        private const string ScoresFileName = "Scores.xml";
        public const string MatchesFileName = "Scores.json";

        private ScoreView _score;
        private int ChosenLanguage = CollectLanguage();
        
        public GameForm()
        {
            InitializeComponent();

            gameSizes.Add("Maxiyatzy", new Size(885, 436));
            gameSizes.Add("Yatzy", new Size(602, 470));
            gameSizes.Add("Yahtzee", new Size(602, 434));
            gameSizes.Add("Balut", new Size(602, 449));

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _gamesFolder = Path.Combine(folder, Settings.Default.Repository);
            if (!Directory.Exists(_gamesFolder))
            {
                Directory.CreateDirectory(_gamesFolder);
            }
            string scoreFile = Path.Combine(_gamesFolder, ScoresFileName);
            if (!File.Exists(scoreFile))
            {
                File.Copy(ScoresFileName, scoreFile);
                const string styleSheet = "Scores.xslt";
                File.Copy(styleSheet, Path.Combine(_gamesFolder, styleSheet));
            }
            string matchFile = Path.Combine(_gamesFolder, MatchesFileName);
            if (!File.Exists(matchFile))
            {
                File.Copy(MatchesFileName, matchFile);
            }
            GameStats = new GameStats();
            InitForm(CollectGameName());
        }

        /// <summary>
        /// XML based, now obsolete, use JSON based GameStats instead
        /// </summary>
        /// <param name="gameText"></param>
        private void ShowHiScore(string gameText)
        {
            string ScoreFile = Path.Combine(_gamesFolder, ScoresFileName);
            try
            {
                var Scores = new DataSet();
                Scores.ReadXml(ScoreFile);
                var Score = new DataView(Scores.Tables["Score"],
                    /* string Filter = */ "Game = " + "'" + gameText + "'",
                    /* string Sort = */ "Bonus DESC, Point DESC",
                    DataViewRowState.CurrentRows);
                var scoreTable = Score.ToTable();

                // get rid of Game Name since it is now redundant
                // prettyprint date and time
                var editedScoreTable = new DataTable();
                editedScoreTable.Columns.Add("Player", typeof(string));
                editedScoreTable.Columns.Add("Commenced", typeof(string));
                editedScoreTable.Columns.Add("Duration", typeof(TimeSpan));
                if (gameText == "Balut")
                {
                    editedScoreTable.Columns.Add("Bonus", typeof(ushort));
                }
                editedScoreTable.Columns.Add("Point", typeof(ushort));
                foreach (DataRow dr in scoreTable.Rows)
                {
                    var commenced = dr.Field<string>("Commenced");
                    var ended = dr.Field<string>("Ended");
                    var point = dr.Field<long>("Point");
                    var started = DateTime.Parse(commenced);
                    var ts = DateTime.Parse(ended) - started;
                    if (gameText == "Balut")
                    {
                        var bonus = dr.Field<long>("Bonus");
                        editedScoreTable.Rows.Add(dr.Field<string>("Player"), started, ts, 
                            ToUShort(bonus),
                            ToUShort(point)
                            );
                    }
                    else
                    {
                        editedScoreTable.Rows.Add(dr.Field<string>("Player"), started, ts, ToUShort(point));
                    }
                }
                var hiScore = new ScoreView(ScoreViewClosing)
                {
                    Text = string.Format("{0} {1}", gameText, "Score"),
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

        /// <summary>
        /// Clean up if case score view is closed
        /// </summary>
        private void ScoreViewClosing()
        {
            _score = null;
        }

        /// <summary>
        /// Use GameStats instead. The new idea is to pull high scores instead of push.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="gamerName"></param>
        /// <param name="commenced"></param>
        private void CheckHiScore(GameOfDice game, string gamerName, DateTime commenced)
        {
            string ScoreFile = Path.Combine(_gamesFolder, ScoresFileName);
            string OldScoreFile = ScoreFile;
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
                myRow["Point"] = game.GameScore;
                var PLayingBalut = game is Balut;
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
                string HiScore = gamerName + " made the Top 10!";
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
                    var g = game.GameScore;
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
                    _score = new ScoreView(ScoreViewClosing);
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

        private void InitForm(string gameText)
        {
            Size = gameSizes[gameText];

            HumanGame = null;
            if (gameText == "Yatzy")
                HumanGame = new Yatzy();
            if (gameText == "Yahtzee")
                HumanGame = new Yahtzee();
            if (gameText == "Maxiyatzy")
                HumanGame = new Maxiyatzy();
            if (gameText == "Balut")
                HumanGame = new Balut();
            if (HumanGame == null)
                HumanGame = new Yatzy();

            HalGame = null;
            if (gameText == "Yatzy")
                HalGame = new Yatzy();
            if (gameText == "Yahtzee")
                HalGame = new Yahtzee();
            if (gameText == "Maxiyatzy")
                HalGame = new Maxiyatzy();
            if (gameText == "Balut")
                HalGame = new Balut();
            if (HalGame == null)
                HalGame = new Yatzy();

            SaveGameName(gameText);

            HumanGame.InhabitTables();
            HalGame.InhabitTables();

            Text = HumanGame + " on a date with C#";

            scorePanels.Controls.Clear();
            Controls.Remove(scorePanels);
            
            _computerPlaysFirst ^= true;

            var collectPlayerName = new PlayerName();

            GameOfDice.ChosenLanguage = ChosenLanguage;
            GamePanel computerPanel = new ComputerPanel(HalGame, InitForm, ShowStatus) { DieColor = Color.Blue, };
            GamePanel humanPanel = new HumanPanel(HumanGame, collectPlayerName.UserName, InitForm, ShowStatus) { DieColor = Color.Red };
            computerPanel.OtherPanel = humanPanel;
            humanPanel.OtherPanel = computerPanel;
            scorePanels.Controls.Add(_computerPlaysFirst ? computerPanel : humanPanel, 0, 0);
            scorePanels.Controls.Add(_computerPlaysFirst ? humanPanel : computerPanel, 1, 0);

            if (_computerPlaysFirst)
            {
                toolStripStatusLabel2.ForeColor = Color.Blue;
                toolStripStatusLabel3.ForeColor = Color.Red;
            }
            else
            {
                toolStripStatusLabel3.ForeColor = Color.Blue;
                toolStripStatusLabel2.ForeColor = Color.Red;
            }

            Controls.Add(scorePanels);

            if (_computerPlaysFirst)
            {
                //AutoGame(computerPanel);
                computerPanel.TerningeKast.Visible = true;
                computerPanel.tableLayoutPanel1.Visible = true;
                humanPanel.TerningeKast.Visible = false;
                humanPanel.tableLayoutPanel1.Visible = false;
            }

            //for (var row = 0; row < HalGame.UsableItems; row++)
            //{
            //    for (var col = 0; col < HalGame.UsableScoreBoxesPerItem; col++)
            //    {
            //        foreach (GamePanel panel in scorePanels.Controls)
            //        {
            //            AutoGame(panel);
            //        }
            //    }
            //}
        }

        public enum RollState
        {
            Unborn,
            RollMe,
            HoldMe
        };

        private static readonly Random DiceGen = new Random();
        private bool _computerPlaysFirst = DiceGen.Next(2) == 0;


        /// <summary>
        /// Pick up language from CurrentCulture. 
        /// If not Swedish or Danish, US English is chosen.
        /// You can dynamically switch between the three languages.
        /// </summary>
        /// <returns></returns>
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

        public static string CollectGameName()
        {
            return Settings.Default.GameSetting;
        }

        private static void SaveGameName(string name)
        {
            Settings.Default.GameSetting = name;
            Settings.Default.Save();
        }

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
        }

        private void OnLanguage1(object sender, EventArgs e)
        {
            ChosenLanguage = (int)GameOfDice.Language.Danish;
            GameOfDice.ChosenLanguage = ChosenLanguage;
            foreach (GamePanel panel in scorePanels.Controls)
            {
                panel.RefreshItemTexts();
            }
        }

        private void OnLanguage2(object sender, EventArgs e)
        {
            ChosenLanguage = (int)GameOfDice.Language.Swedish;
            GameOfDice.ChosenLanguage = ChosenLanguage;
            foreach (GamePanel panel in scorePanels.Controls)
            {
                panel.RefreshItemTexts();
            }
        }

        private void OnLanguage3(object sender, EventArgs e)
        {
            ChosenLanguage = (int)GameOfDice.Language.English;
            GameOfDice.ChosenLanguage = ChosenLanguage;
            foreach (GamePanel panel in scorePanels.Controls)
            {
                panel.RefreshItemTexts();
            }
        }

        private void OnUndo(object sender, EventArgs e)
        {
            foreach (GamePanel panel in scorePanels.Controls)
            {
                if (panel is HumanPanel)
                {
                    panel.UnDecider();
                }
            }
            GamePanel.Undoable = false;
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

        private void OnCurrentHiScores(object sender, EventArgs e)
        {
            GameStats.ShowHiScores(CollectGameName(), Settings.Default.CurentScoreCutOff);
        }

        private void OnAllTimeHiScores(object sender, EventArgs e)
        {
            GameStats.ShowHiScores(CollectGameName(), 100*365);
        }

        private void OnMatchScore(object sender, EventArgs e)
        {
            GameStats.ShowMatches(CollectGameName());
        }

        private void OnRules(object sender, EventArgs e)
        {
            MyHelp(scorePanels);
        }

        private void OnAbout(object sender, EventArgs e)
        {
            var x = Application.ProductVersion;
            MessageBox.Show("PhDGames©\n" +
                            "Created by Peter Hegelund\n" +
                            "Version " + x + ", March 2015\n" +
                            "Programmed in C#", "About " + HumanGame,
                MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        private void MyHelp(Control parent /*, myHelpEnum topic*/)
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

        readonly Dictionary<string, Size> gameSizes = new Dictionary<string, Size>();
        private readonly string _gamesFolder;
        public static GameStats GameStats;

        private void GameForm_Load(object sender, EventArgs e)
        {
            if ((ModifierKeys & Keys.Shift) == 0)
            {
                string initLocation = Settings.Default.InitialLocation;
                Point il = new Point(0, 0);
                Size sz = Size;
                if (!string.IsNullOrWhiteSpace(initLocation))
                {
                    string[] parts = initLocation.Split(',');
                    if (parts.Length >= 2)
                    {
                        il = new Point(int.Parse(parts[0]), int.Parse(parts[1]));
                    }
                    if (parts.Length >= 4)
                    {
                        sz = new Size(int.Parse(parts[2]), int.Parse(parts[3]));
                    }
                }
                Size = sz;
                Location = il;
            }
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            GameStats.SaveMatches();

            if ((ModifierKeys & Keys.Shift) == 0)
            {
                Point location = Location;
                Size size = Size;
                if (WindowState != FormWindowState.Normal)
                {
                    location = RestoreBounds.Location;
                    size = RestoreBounds.Size;
                }
                string initLocation = string.Join(",", location.X, location.Y, size.Width, size.Height);
                Settings.Default.InitialLocation = initLocation;
                Settings.Default.Save();
            }
        }

        public void ShowStatus(Type panel, string status)
        {
            if (_computerPlaysFirst)
            {
                if (panel == typeof (ComputerPanel))
                {
                    toolStripStatusLabel2.Text = string.Format(status);
                }
                else
                {
                    toolStripStatusLabel3.Text = string.Format(status);
                }
            }
            else
            {
                if (panel == typeof(ComputerPanel))
                {
                    toolStripStatusLabel3.Text = string.Format(status);
                }
                else
                {
                    toolStripStatusLabel2.Text = string.Format(status);
                }
            }
            statusStrip1.Refresh();
        }
    }
}
