using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Yatzy.Properties;

namespace Yatzy
{
    public class MyForm : Form
    {
        private const int OptimalDieSize = 66;
        private const int ScoreCardStart = 150; // scorecard starts here
        public const string RegFolder = @"Software\PHDGames\";
        private readonly MenuItem ClickDice;
        private readonly MenuItem Danish;
        private readonly Random DiceGen = new Random();
        private readonly MenuItem English;

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

        private readonly MenuItem PlayBalut;
        private readonly string Player;
        private readonly MenuItem PlayMaxiyatzy;
        private readonly MenuItem PlayYahtzee;
        private readonly MenuItem PlayYatzy;

        private readonly TableLayoutPanel scorePanels = new TableLayoutPanel();
        private readonly Button StartAgain = new Button();
        private readonly string[] StartOver = {"Nyt spil", "Starta om", "New game"};

        private readonly MenuItem Swedish;
        private readonly Button TerningeKast = new Button();
        private readonly MenuItem TouchDice;
        private readonly MenuItem Undo;
        private readonly SolidBrush Whiteout = new SolidBrush(Color.White);

        // implement computer player
        // determine who goes first
        private static readonly Random whoIsOnFirst = new Random();
        private ScoreView _score;
        private int ChosenLanguage = CollectLanguage();
        //private RollState[] DiceRoll;
        //private int[] DiceVec;
        private int DieDist;
        private int DieSize;
        private GameOfDice HalGame;
        private GameOfDice HumanGame;
        //private TextBox[] Items;
        private int RollCounter;
        //private TextBox[,] Rubrik;
        private int TargetDie = -1; // when selecting dice by merely moving the mouse
        private string TerningeKastText;
        private bool Undoable;
        //private bool[,] UsedScores;
        private bool _computerPlaysFirst = whoIsOnFirst.Next(2) == 0;
        private ScorePanel _humanPanel;
        private ScorePanel _computerPanel;

        public MyForm()
        {
            //CollectLanguage();

            SetStyle(ControlStyles.ResizeRedraw, true);

            Controls.Add(TerningeKast);

            TerningeKast.Click += OnButtonClicked;
            AcceptButton = TerningeKast;

            Controls.Add(StartAgain);

            StartAgain.Click += OnButtonClicked2;

            // Create a menu
            var menu = new MainMenu();
            var option = menu.MenuItems.Add("&Options");
            option.Popup += OnPopupOptionsMenu;
            option.MenuItems.Add(Danish =
                new MenuItem("&Dansksproget",
                    OnLanguage1)
                );
            option.MenuItems.Add(Swedish =
                new MenuItem("&Svenskspråkig",
                    OnLanguage2)
                );
            option.MenuItems.Add(English =
                new MenuItem("In &English",
                    OnLanguage3)
                );
            option.MenuItems.Add("-");
            option.MenuItems.Add(Undo = new MenuItem("&Undo",
                OnUndo));
            Undo.Shortcut = Shortcut.CtrlZ;
            option.MenuItems.Add(TouchDice = new MenuItem("select by &Touch",
                OnTouch));
            option.MenuItems.Add(ClickDice = new MenuItem("select by &Click",
                OnClick));
            option.MenuItems.Add("-");
            option.MenuItems.Add(new MenuItem("e&Xit",
                OnExit));

            var games = menu.MenuItems.Add("&Games");
            games.Popup += OnPopupOptionsMenu2;
            games.MenuItems.Add(PlayYatzy =
                new MenuItem("&Yatzy",
                    OnGame1)
                );
            games.MenuItems.Add(PlayYahtzee =
                new MenuItem("ya&Htzee",
                    OnGame2)
                );
            games.MenuItems.Add(PlayMaxiyatzy =
                new MenuItem("&Maxiyatzy",
                    OnGame3)
                );
            games.MenuItems.Add(PlayBalut =
                new MenuItem("&Balut",
                    OnGame4)
                );
            games.MenuItems.Add("-");
            games.MenuItems.Add(new MenuItem("show &HiScore",
                OnHiScore));

            var help = menu.MenuItems.Add("&Help");
            help.Popup += OnPopupOptionsMenu3;
            MenuItem rules;
            help.MenuItems.Add(rules =
                new MenuItem("&Rules",
                    OnRules)
                );
            rules.Shortcut = Shortcut.F1;
            option.MenuItems.Add("-");
            help.MenuItems.Add(
                new MenuItem("&About",
                    OnAbout)
                );

            // Attach the menu to the form
            Menu = menu;

            InitForm(CollectGameName());

            var collectPlayerName = new PlayerName();
            collectPlayerName.ShowDialog();
            Player = collectPlayerName.UserName;
        }

        private void ShowSavedRolls(Graphics g)
        {
            var Pins = new Rectangle(DieSize*HumanGame.Dice/2 + 2*DieDist - 7, DieSize + 2*DieDist, 20, 10);
            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (var b = new SolidBrush(BackColor))
            {
                g.FillRectangle(b, Pins);
                if (HumanGame.SavedRolls > 0)
                {
                    using (var br = new SolidBrush(Color.Black))
                    {
                        var PinCount = String.Format("{0}", HumanGame.SavedRolls);
                        g.DrawString(PinCount, Font, br, Pins, format);
                    }
                }
            }
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
                editedScoreTable.Columns.Add("Player", typeof (string));
                editedScoreTable.Columns.Add("Commenced", typeof (string));
                editedScoreTable.Columns.Add("Duration", typeof (TimeSpan));
                editedScoreTable.Columns.Add("Point", typeof (ushort));
                if (Name == "Balut")
                {
                    editedScoreTable.Columns.Add("Bonus", typeof (ushort));
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
                        Target.WriteByte((byte) t);
                    // we are only providing UNIX LF 
                    // Is there a platform independant way of terminating lines, I wonder?
                    // Anyway, this code will when executing on windows create a file with mixed line terminations
                    Target.WriteByte((byte) '\n');
                    foreach (var t in Xsl)
                        Target.WriteByte((byte) t);
                    Target.WriteByte((byte) '\n');
                    while (true)
                    {
                        var b = Source.ReadByte();
                        if (b == -1) break;
                        Target.WriteByte((byte) b);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("" + e);
            }
        }

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
            Name = HumanGame.ToString();
            Icon = Resources.Yatzy;

            //DiceVec = new int[HumanGame.Dice];
            //OldDice = new int[HumanGame.Dice];
            //DiceRoll = new RollState[HumanGame.Dice];
            //UsedScores = new bool[HumanGame.MaxItem + 1, HumanGame.ScoreBoxesPerItem];

            TargetDie = -1;

            RollCounter = 0;
            //Rounds = 0;
            TerningeKastText = String.Format(HumanGame.RollText(RollCounter), HumanGame.Cardinal(0));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = true;
            TerningeKast.Focus();

            StartAgain.Text = StartOver[ChosenLanguage];

            //Rubrik = new TextBox[HumanGame.MaxTotalItem + 1, 6];
            //Items = new TextBox[HumanGame.MaxTotalItem + 1];

            scorePanels.Controls.Clear();
            Controls.Remove(scorePanels);

            //for (int Col = 0; Col < HumanGame.MaxGroup; Col++)
            //{
            //    for (int i = 0; i < Items.Length; i++)
            //        if (Col + 1 == HumanGame.PreferredGroup(i))
            //        {
            //            Items[i] = new myTextBox();
            //            if (HumanGame.FirstScoreBox(i) > 0)
            //            {
            //                Items[i].TextAlign = HorizontalAlignment.Right;
            //                Items[i].Width = ItemWidth + HumanGame.FirstScoreBox(i) * ScoreWidth;
            //            }
            //            else
            //                Items[i].Width = ItemWidth;
            //            Items[i].Text = HumanGame.ItemText(i);
            //            ScoreCards.Controls.Add(Items[i]);
            //            Items[i].Enabled = false;
            //            Items[i].BackColor = Color.Yellow;
            //            Items[i].BorderStyle = BorderStyle.Fixed3D;
            //            Items[i].Left = Col * ItemWidth * 3 / 2;
            //            Items[i].Top = ItemTop + 20 * HumanGame.PreferredRow(i);

            //            for (int j = 0; j < HumanGame.ScoreBoxesPerItem; j++)
            //                if (j >= HumanGame.FirstScoreBox(i))
            //                {
            //                    Rubrik[i, j] = new myTextBox();
            //                    ScoreCards.Controls.Add(Rubrik[i, j]);
            //                    Rubrik[i, j].Name = "Rubrik" + i + "." + j;
            //                    Rubrik[i, j].Enabled = false;
            //                    Rubrik[i, j].Left = Items[i].Left + ItemWidth + j * ScoreWidth;
            //                    Rubrik[i, j].Top = ItemTop + 20 * HumanGame.PreferredRow(i);
            //                    Rubrik[i, j].Width = ScoreWidth;
            //                    Rubrik[i, j].BackColor = j >= HumanGame.UsableScoreBoxesPerItem ? Color.Coral : Color.AliceBlue;
            //                    Rubrik[i, j].BorderStyle = BorderStyle.Fixed3D;
            //                    Rubrik[i, j].MouseMove += myMouseMove;
            //                    Rubrik[i, j].MouseHover += myMouseHover;
            //                    Rubrik[i, j].MouseDown += myMouseDown;
            //                    Rubrik[i, j].MouseLeave += myMouseLeave;
            //                }
            //        }
            //}

            _computerPlaysFirst = false; // ^= true;
            ScorePanel.ChosenLanguage = ChosenLanguage;
            _computerPanel = new ScorePanel(HalGame, "HAL", TerningeKast, CheckHiScore);
            _humanPanel = new ScorePanel(HumanGame, Player, TerningeKast, CheckHiScore);

            // Make the tables nice

            scorePanels.Height = _humanPanel.Height;
            scorePanels.Top = ScoreCardStart;
            scorePanels.Width = _humanPanel.Width * 2;

            // Set the form's size
            ClientSize = new Size(Math.Max(HumanGame.Dice * OptimalDieSize * 11 / 10, scorePanels.Width), ScoreCardStart + _humanPanel.Height);

            //scorePanels.Controls.Add(_computerPlaysFirst ? _computerPanel : _humanPanel, 0, 0);
            //scorePanels.Controls.Add(_computerPlaysFirst ? _humanPanel : _computerPanel, 1, 0);
            var newPanel = new GamePanel(HumanGame, Player, CheckHiScore) { DieColor = Color.Blue };
            var newPanel2 = new GamePanel(HalGame, "HAL", CheckHiScore) { DieColor = Color.Red };
            scorePanels.Controls.Add(newPanel, 0, 0);
            scorePanels.Controls.Add(newPanel2, 1, 0);
            scorePanels.Dock = DockStyle.Fill;
            scorePanels.Height = newPanel.Height;
            //scorePanels.Top = 0;
            scorePanels.Width = newPanel.Width * 2;
            ClientSize = scorePanels.Size;

            Controls.Add(scorePanels);

            using (var g = Graphics.FromHwnd(Handle))
            {
                for (var i = 0; i < HumanGame.Dice; i++)
                {
                    _humanPanel.DiceRoll[i] = RollState.Unborn;
                    _humanPanel.DiceVec[i] = 0;
                    DrawDie(g, i, 0, Color.Red);
                }
            }

            for (var i = 0; i < _humanPanel.UsedScores.GetLength(0); i++)
                for (var j = 0; j < HumanGame.ScoreBoxesPerItem; j++)
                {
                    _humanPanel.UsedScores[i, j] = false;
                }

            //HalfGame(newPanel);
        }

        //private void HalfGame(GamePanel computerPanel)
        //{
        //    var bestScore = -1;
        //    var bestScoreRow = -1;
        //    var bestScoreCol = -1;
        //    computerPanel.DiceVec = new int[HalGame.Dice];
        //    computerPanel.DiceRoll = new GameForm.RollState[HalGame.Dice];
        //    int roll;
        //    for (roll = 1; roll <= 3; roll++)
        //    {
        //        var rolling = false;
        //        for (var i = 0; i < HalGame.Dice; i++)
        //            if (computerPanel.DiceRoll[i] != GameForm.RollState.HoldMe)
        //            {
        //                var die = computerPanel.DiceVec[i] = DiceGen.Next(1, 7);
        //                if (die == 6)
        //                {
        //                    computerPanel.DiceRoll[i] = RollState.HoldMe;
        //                }
        //                else
        //                {
        //                    computerPanel.DiceRoll[i] = RollState.RollMe;
        //                    rolling = true;
        //                }
        //            }
        //        if (!rolling)
        //        {
        //            break; // save a roll or two (only MaxiYatzy takes advantage though)
        //        }
        //    }
        //    for (var row = 0; row < HalGame.UsableItems; row++)
        //    {
        //        for (var col = 0; col < HalGame.UsableScoreBoxesPerItem; col++)
        //        {
        //            if (computerPanel.UsedScores[row, col]) continue;
        //            var diceValue = HalGame.ValueIt(computerPanel.DiceVec, row);
        //            if (diceValue > bestScore)
        //            {
        //                bestScore = diceValue;
        //                bestScoreRow = row;
        //                bestScoreCol = col;
        //            }
        //        }
        //    }
        //    var itemStr = string.Format("{0}{1}.{2}", "Rubrik", bestScoreRow, bestScoreCol);
        //    computerPanel.ScoreIt(itemStr);
        //    computerPanel.UsedScores[bestScoreRow, bestScoreCol] = true;
        //    Thread.Sleep(5);
        //}

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

        // Handler for Options menu popups
        private void OnPopupOptionsMenu(object sender, EventArgs e)
        {
            Danish.Checked = ChosenLanguage == (int) GameOfDice.Language.Danish;
            Swedish.Checked = ChosenLanguage == (int) GameOfDice.Language.Swedish;
            English.Checked = ChosenLanguage == (int) GameOfDice.Language.English;
            TouchDice.Checked = ScorePanel.Touchy;
            ClickDice.Checked = !ScorePanel.Touchy;
            Undo.Enabled = Undoable;
        }

        private void OnPopupOptionsMenu2(object sender, EventArgs e)
        {
            PlayYatzy.Checked = HumanGame.ToString() == "Yatzy";
            PlayYahtzee.Checked = HumanGame.ToString() == "Yahtzee";
            PlayMaxiyatzy.Checked = HumanGame.ToString() == "Maxiyatzy";
            PlayBalut.Checked = HumanGame.ToString() == "Balut";
        }

        private void OnPopupOptionsMenu3(object sender, EventArgs e)
        {
        }

        private void OnLanguage1(object sender, EventArgs e)
        {
            ChosenLanguage = (int) GameOfDice.Language.Danish;
        }

        private void OnLanguage2(object sender, EventArgs e)
        {
            ChosenLanguage = (int) GameOfDice.Language.Swedish;
        }

        private void OnLanguage3(object sender, EventArgs e)
        {
            ChosenLanguage = (int) GameOfDice.Language.English;
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

        private void OnUndo(object sender, EventArgs e)
        {
            //		if(Undoable) 
            {
                _humanPanel.UnDecider();
            }
            Undoable = false;
        }

        private void OnTouch(object sender, EventArgs e)
        {
            TouchDice.Checked = true;
            ClickDice.Checked = false;
            ScorePanel.Touchy = true;
        }

        private void OnClick(object sender, EventArgs e)
        {
            TouchDice.Checked = false;
            ClickDice.Checked = true;
            ScorePanel.Touchy = false;
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

        // Handler for the Exit command
        private void OnExit(object sender, EventArgs e)
        {
            Close();
        }

        // ornament an ordinary six-sided die
        private void OrnamentDie(Graphics g, Rectangle die, int eyes)
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

        private void DrawDie(Graphics g, int die, int val, Color DieCol)
        {
            Contract.Assert(die >= 0);
            Contract.Assert(die < HumanGame.Dice);
            using (var DieBrush = new SolidBrush(DieCol))
            using (var myGraphicsPath = new GraphicsPath())
            {
                DieSize = Math.Min(OptimalDieSize, ClientSize.Width*9/(10*HumanGame.Dice));
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

        /// <summary>
        ///  OnPaint handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //for (var i = 0; i < HumanGame.Dice; i++)
            //{
            //    DrawDie(e.Graphics, i, _humanPanel.DiceVec[i],
            //        _humanPanel.DiceRoll[i] == RollState.RollMe ? Color.Black : Color.Red);
            //}

            //ShowSavedRolls(e.Graphics);

            //TerningeKastText = String.Format(HumanGame.RollText(RollCounter), HumanGame.Cardinal(0));
            //TerningeKast.Text = TerningeKastText;
            //TerningeKast.Location = new Point(0, DieSize + 5*DieDist);
            //TerningeKast.Size = new Size(DieSize*HumanGame.Dice + DieDist*(HumanGame.Dice - 1), 24);

            //StartAgain.Text = StartOver[ChosenLanguage];
            //StartAgain.Location = new Point(0, DieSize + 5*DieDist + 27);
            //StartAgain.Size = new Size(DieSize*HumanGame.Dice + DieDist*(HumanGame.Dice - 1), 24);
        }

        private void OnButtonClicked(Object sender, EventArgs e)
        {
            Undoable = false;
            TerningeKastText = RollCounter == 2 && HumanGame.SavedRolls == 0
                ? Result[ChosenLanguage]
                : klik[ChosenLanguage];
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = false;

            TargetDie = -1;

            RollCounter++;
            if (RollCounter > 3)
            {
                HumanGame.UseARoll();
            }
            using (var g = Graphics.FromHwnd(Handle))
            {
                ShowSavedRolls(g);

                for (var i = 0; i < HumanGame.Dice; i++)
                    if (_humanPanel.DiceRoll[i] != RollState.HoldMe)
                    {
                        _humanPanel.DiceRoll[i] = RollState.HoldMe;
                        _humanPanel.DiceVec[i] = DiceGen.Next(1, 7);
                        DrawDie(g, i, _humanPanel.DiceVec[i], Color.Red);
                    }

                for (var r = 0; r <= HumanGame.MaxItem; r++)
                    for (var j = HumanGame.FirstScoreBox(r); j < HumanGame.UsableScoreBoxesPerItem; j++)
                    {
                        if (!_humanPanel.UsedScores[r, j])
                            _humanPanel.Rubrik[r, j].Enabled = true;
                    }
            }
        }

        private void OnButtonClicked2(Object sender, EventArgs e)
        {
            Undoable = false;

            RollCounter = 0;
            TerningeKastText = String.Format(HumanGame.RollText(RollCounter), HumanGame.Cardinal(RollCounter));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = true;
            TerningeKast.Focus();

            TargetDie = -1;

            //Rounds = 0;
            HumanGame.NewGame();

            using (var g = Graphics.FromHwnd(Handle))
            {
                for (var i = 0; i < HumanGame.Dice; i++)
                {
                    _humanPanel.DiceRoll[i] = RollState.Unborn;
                    _humanPanel.DiceVec[i] = 0;
                    DrawDie(g, i, 0, Color.Red);
                }
                ShowSavedRolls(g);
            }

            int r;
            for (r = 0; r <= HumanGame.MaxItem; r++)
                for (var j = HumanGame.FirstScoreBox(r); j < HumanGame.ScoreBoxesPerItem; j++)
                {
                    _humanPanel.UsedScores[r, j] = false;
                    _humanPanel.Rubrik[r, j].Enabled = false;
                    _humanPanel.Rubrik[r, j].Text = "";
                }

            for (; r <= HumanGame.MaxTotalItem; r++)
            {
                _humanPanel.Rubrik[r, 0].Enabled = false;
                _humanPanel.Rubrik[r, 0].Text = "";
            }
        }

        protected void TouchOrClick(MouseEventArgs e)
        {
            var SaveTargetDie = TargetDie;
            TargetDie = -1;

            if (e.Y > DieSize)
                return;
            var DieClicked = e.X / (DieSize + DieDist);

            if (DieClicked >= HumanGame.Dice)
                return;

            if (RollCounter == 0)
                return;

            if (RollCounter >= 3 && HumanGame.SavedRolls == 0)
                return;

            var SameDie = SaveTargetDie == DieClicked;
            TargetDie = DieClicked;
            if (ScorePanel.Touchy && SameDie)
                return;
            var ThisDieMustRoll = _humanPanel.DiceRoll[DieClicked] == RollState.RollMe;

            _humanPanel.DiceRoll[DieClicked] = ThisDieMustRoll ? RollState.HoldMe : RollState.RollMe;

            using (var g = Graphics.FromHwnd(Handle))
            {
                DrawDie(g, DieClicked, _humanPanel.DiceVec[DieClicked],
                    ThisDieMustRoll ? Color.Red : Color.Black);

                ShowSavedRolls(g);
            }

            TerningeKastText = String.Format(HumanGame.RollText(RollCounter), HumanGame.Cardinal(RollCounter));
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

        public enum RollState
        {
            Unborn,
            RollMe,
            HoldMe
        };
    }

}