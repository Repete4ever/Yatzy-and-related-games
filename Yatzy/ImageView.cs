using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Yatzy.Properties;

namespace Yatzy
{
    public class MyForm : Form
    {
        readonly MenuItem Danish;
        readonly MenuItem Swedish;
        readonly MenuItem English;

        int ChosenLanguage = CollectLanguage();

        readonly MenuItem PlayYatzy;
        readonly MenuItem PlayYahtzee;
        readonly MenuItem PlayMaxiyatzy;
        readonly MenuItem PlayBalut;

        readonly MenuItem TouchDice;
        readonly MenuItem ClickDice;
        bool Touchy = true;

        readonly MenuItem Undo;
        bool Undoable;

        readonly Button StartAgain = new Button();
        const int OptimalDieSize = 66;
        readonly SolidBrush Whiteout = new SolidBrush(Color.White);
        readonly Random DiceGen = new Random();
        int DieSize;
        int DieDist;
        int[] DiceVec;
        int[] OldDice;
        enum RollState { Unborn, RollMe, HoldMe };
        RollState[] DiceRoll;
        readonly string[] StartOver = { "Nyt spil", "Starta om", "New game" };

        readonly string[] klik = {
						 "Klik på de terninger du vil slå om",
						 "Klicka på de tärninger du vill slå om",
						 "Select dice and reroll"
					 };

        readonly string[] Result = {
						  "Registrér resultatet i tabellen",
						  "Registrere resultatet i tabellen",
						  "Select a score box"
					  };
        int RollCounter;
        int Rounds;

        // Todo implement computer player
        const int players = 2;

        const int ScoreCard = 150; // scorecard starts here

        readonly Button TerningeKast = new Button();
        string TerningeKastText;
        TextBox[,] Rubrik;
        TextBox[] Items;
        GameOfDice Game;
        readonly Panel p;
        bool[,] UsedScores;

        int TargetDie = -1; // when selecting dice by merely moving the mouse

        void ShowSavedRolls(Graphics g)
        {
            Rectangle Pins = new Rectangle(DieSize * Game.Dice / 2 + 2 * DieDist - 7, DieSize + 2 * DieDist, 20, 10);
            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            SolidBrush b = new SolidBrush(BackColor);
            g.FillRectangle(b, Pins);
            if (Game.SavedRolls > 0)
            {
                SolidBrush br = new SolidBrush(Color.Black);
                string PinCount = String.Format("{0}", Game.SavedRolls);
                g.DrawString(PinCount, Font, br, Pins, format);
                br.Dispose();
            }
            b.Dispose();

        }

        private static int CollectLanguage()
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                switch (currentCulture.Name.Substring(0, 2))
                {
                    default:
                        return (int)GameOfDice.Language.English;
                    case "da":
                        return (int)GameOfDice.Language.Danish;
                    case "sv":
                        return (int)GameOfDice.Language.Swedish;
                }
            }
            catch
            {
                return (int)GameOfDice.Language.English;
            }
        }

        private const string RegFolder = @"Software\PHDGames\";

        private string CollectGameName()
        {
            RegistryKey GameKey = Registry.CurrentUser.CreateSubKey(RegFolder);
            return GameKey != null ? GameKey.GetValue("Game Name", Player).ToString() : "Yatzy";
        }

        private void SaveGameName(string name)
        {
            RegistryKey GameKey = Registry.CurrentUser.CreateSubKey(RegFolder);
            if (GameKey != null) GameKey.SetValue("Game Name", name);
        }

        string Player;

        private void CollectPlayerName()
        {
            try
            {
                Player = Environment.UserName;
            }
            catch
            {
                Player = "NN";
            }

            RegistryKey GameKey = Registry.CurrentUser.CreateSubKey(RegFolder);
            Player = GameKey.GetValue("Player Name", Player).ToString();

            PlayerNameDialog dlg = new PlayerNameDialog {UserName = Player};

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                Player = dlg.UserName;
                GameKey.SetValue("Player Name", Player);
                Invalidate();
            }

        }

        DateTime Commenced = DateTime.Now;

        ScoreView _score;

        //private static string myDateTime(DateTime myDateTime)
        //{
        //    return string.Format("{0} {1}", myDateTime.ToShortDateString(), myDateTime.ToShortTimeString());
        //}

        private static ushort ToUShort(long val)
        {
            return val <= 65535 && val >= 0 ? (ushort)val : (ushort)0;
        }

        private void ShowHiScore()
        {
            const string ScoreFile = @"Scores.xml";
            try
            {
                DataSet Scores = new DataSet();
                Scores.ReadXml(ScoreFile);
                DataView Score = new DataView(Scores.Tables["Score"],
                    /* string Filter = */ "Game = " + "'" + Name + "'",
                    /* string Sort = */ "Bonus DESC, Point DESC",
                    DataViewRowState.CurrentRows);
                DataTable scoreTable = Score.ToTable();

                // get rid of Game Name since it is now redundant
                // prettyprint date and time
                DataTable editedScoreTable = new DataTable();
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
                    string commenced = dr.Field<string>("Commenced");
                    string ended = dr.Field<string>("Ended");
                    long point = dr.Field<long>("Point");
                    DateTime started = DateTime.Parse(commenced);
                    TimeSpan ts = DateTime.Parse(ended) - started;
                    if (Name == "Balut")
                    {
                        long bonus = dr.Field<long>("Bonus");
                        editedScoreTable.Rows.Add(dr.Field<string>("Player"), started, ts, ToUShort(point), ToUShort(bonus));
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
                    Top = this.Top,
                    Source = editedScoreTable
                };
                hiScore.ShowDialog();
            }
            catch (Exception e)
            {
                MessageBox.Show("" + e);
            }
        }

        private void CheckHiScore()
        {
            const string ScoreFile = @"Scores.xml";
            string OldScoreFile = ScoreFile;
            //		string ScoreFileSchema = @"Scores.xsd";
            MemoryStream Source = null;
            FileStream Target = null;

            try
            {
                DataSet Scores = new DataSet();
                // explicitly reading the XML Schema seemed a good idea bit did not work
                //			Scores.ReadXmlSchema(ScoreFileSchema);
                Scores.ReadXml(ScoreFile);

                DataRow myRow = Scores.Tables["Score"].NewRow();
                myRow["Game"] = Name;
                myRow["Player"] = Player;
                myRow["Point"] = Game.GamePoints;
                bool PLayingBalut = Name == "Balut";
                if (PLayingBalut)
                    myRow["Bonus"] = Game.BonusPoints;
                DateTime now = DateTime.Now;
                myRow["Ended"] = now.ToString("s", null); // ISO date format
                myRow["Commenced"] = Commenced.ToString("s", null); // ISO date format
                Scores.Tables["Score"].Rows.Add(myRow);

                DataView Score = new DataView(Scores.Tables["Score"],
                    /* string Filter = */ "Game = " + "'" + Name + "'",
                    /* string Sort = */ "Bonus DESC, Point DESC",
                DataViewRowState.CurrentRows);
                const int HiLen = 10;
                const string HiScore = "You made the Top 10!";
                bool ScoreHigh = false;
                if (Score.Count <= HiLen)
                {
                    MessageBox.Show(HiScore);
                    ScoreHigh = true;
                }
                else
                {
                    string ColumnToUse = PLayingBalut ? "Bonus" : "Point";
                    string ps = Score[HiLen - 1][ColumnToUse].ToString();
                    int pi = Int32.Parse(ps);
                    int g = Game.GamePoints;
                    bool Last = myRow["Ended"] == Score[HiLen - 1]["Ended"];
                    if (g > pi || Last)
                    {
                        MessageBox.Show(HiScore);
                        ScoreHigh = true;
                    }

                }

                int NewRow = 0;
                bool Found = false;

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
                _score.Left = this.Size.Width + this.Left;
                _score.Top = this.Top;
                _score.Source = Score;
                if (ScoreHigh)
                    _score.SelectRow = NewRow;
                _score.Text = Name + " Scores";
                _score.Show();
                _score.Focus();

                // save scores in a Memorystream
                Source = new MemoryStream();
                Scores.WriteXml(Source, XmlWriteMode.WriteSchema);

                // Scores.WriteXml(@"NewScores.xml", XmlWriteMode.WriteSchema);
                // Lines below are lost after reading and writing but should be restored
                // by inserting them at the top of the scores file afterward
                // <?xml version="1.0" standalone="yes"?>
                // <?xml-stylesheet type="text/xsl" href="Scores.xslt"?>
                Source.Seek(0, SeekOrigin.Begin);
                Target = new FileStream(OldScoreFile, FileMode.Open, FileAccess.Write, FileShare.None);
                string HelloXML = "<?xml version=" + '"' + "1.0" + '"' + " standalone=" + '"' + "yes" + '"' + "?>";
                string Xsl = "<?xml-stylesheet type=" + '"' + "text/xsl" + '"' + " href=" + '"' + "Scores.xslt" + '"' + "?>";
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
                foreach (char t in HelloXML)
                    Target.WriteByte((byte)t);
                // we are only providing UNIX LF 
                // Is there a platform independant way of terminating lines, I wonder?
                // Anyway, this code will when executing on windows create a file with mixed line terminations
                Target.WriteByte((byte)'\n');
                foreach (char t in Xsl)
                    Target.WriteByte((byte)t);
                Target.WriteByte((byte)'\n');
                while (true)
                {
                    int b = Source.ReadByte();
                    if (b == -1) break;
                    Target.WriteByte((byte)b);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("" + e);
            }
            finally
            {
                if (Source != null)
                    Source.Close();
                if (Target != null)
                    Target.Close();
            }
        }

        Int32 DecidingRow;
        Int32 DecidingCol;
        int OldRollCounter;

        private void myMouseDecider(object sender)
        {
            const string R = "Rubrik";
            if (!((Control)sender).Name.StartsWith(R))
                return;
            string S = ((Control)sender).Name;
            int dot = S.IndexOf('.');
            DecidingRow = Int32.Parse(S.Substring(R.Length, dot - R.Length));
            DecidingCol = Int32.Parse(S.Substring(dot + 1));

            Rubrik[DecidingRow, DecidingCol].Text = Game.ScoreIt(DiceVec, DecidingRow, DecidingCol, RollCounter);
            if (Game.ScoreBoxesPerItem == 1)
            {
                for (int ro = Game.MaxItem + 1; ro <= Game.MaxTotalItem; ro++)
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
            else
            {
                TerningeKastText = "";
                TerningeKast.Enabled = false;
                CheckHiScore();
            }
            TerningeKast.Text = TerningeKastText;

            Graphics g = Graphics.FromHwnd(Handle);

            for (int r = 0; r < Game.Dice; r++)
            {
                DiceRoll[r] = RollState.Unborn;
                OldDice[r] = DiceVec[r];
                DiceVec[r] = 0;
                DrawDie(g, r, 0, Color.Red);
            }

            for (int r = 0; r <= Game.MaxItem; r++)
                for (int d = Game.FirstScoreBox(r); d < Game.UsableScoreBoxesPerItem; d++)
                {
                    Rubrik[r, d].Enabled = false;
                    if (!UsedScores[r, d])
                        Rubrik[r, d].Text = "";
                }
            ShowSavedRolls(g);
        }

        private void UnDecider()
        {
            for (int d = 0; d < Game.Dice; d++)
                DiceVec[d] = OldDice[d]; // get that roll back

            RollCounter = OldRollCounter;
            Rubrik[DecidingRow, DecidingCol].Text = Game.UnScoreIt(DiceVec, DecidingRow, DecidingCol, RollCounter);
            if (Game.ScoreBoxesPerItem == 1)
            {
                for (int ro = Game.MaxItem + 1; ro <= Game.MaxTotalItem; ro++)
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
                TerningeKastText = RollCounter == 3 && Game.SavedRolls == 0 ?
                    Result[ChosenLanguage] :
                    klik[ChosenLanguage];
                TerningeKast.Enabled = false;
            }
            TerningeKast.Text = TerningeKastText;

            Graphics g = Graphics.FromHwnd(Handle);

            for (int r = 0; r < Game.Dice; r++)
            {
                DiceRoll[r] = RollState.HoldMe;
                DrawDie(g, r, DiceVec[r], Color.Red);
            }

            for (int r = 0; r <= Game.MaxItem; r++)
                for (int d = Game.FirstScoreBox(r); d < Game.UsableScoreBoxesPerItem; d++)
                {
                    if (!UsedScores[r, d])
                    {
                        Rubrik[r, d].Enabled = true;
                        Rubrik[r, d].Text = "";
                    }
                }
            ShowSavedRolls(g);
        }
        // Function signatures must match the signature of the
        // MouseEventHandler class.
        private void myMouseDown(object sender, MouseEventArgs ex)
        {
            if (ex.Button == MouseButtons.Left)
            {
                myMouseDecider(sender);
            }
        }

        // A method that shows your scoring if you choose to score it here
        private void myMouseMove(object sender, MouseEventArgs ex)
        {
            string SenderName = ((Control)sender).Name;
            Int32 dot = SenderName.IndexOf('.');
            Int32 i = Int32.Parse(SenderName.Substring(6, dot - 6));
            Int32 j = Int32.Parse(SenderName.Substring(dot + 1));

            Rubrik[i, j].Text = Game.ValueIt(DiceVec, i, j);
            Cursor.Current = Cursors.Arrow;
        }

        private void myMouseHover(object sender, EventArgs e)
        {
            if (Touchy)
            {
                myMouseDecider(sender);
            }
        }

        // A method that deletes the potential score
        private void myMouseLeave(object sender, EventArgs ex)
        {
            Int32 dot = ((Control)sender).Name.IndexOf('.');
            Int32 i = Int32.Parse(((Control)sender).Name.Substring(6, dot - 6));
            Int32 j = Int32.Parse(((Control)sender).Name.Substring(dot + 1));

            if (!UsedScores[i, j])
                Rubrik[i, j].Text = "";
            Cursor.Current = Cursors.Default;
        }

        private void InitForm(string GameText)
        {
            Game = null;
            if (GameText == "Yatzy")
                Game = new Yatzy();
            if (GameText == "Yahtzee")
                Game = new Yahtzee();
            if (GameText == "Maxiyatzy")
                Game = new Maxiyatzy();
            if (GameText == "Balut")
                Game = new Balut();
            if (Game == null)
                Game = new Yatzy();

            SaveGameName(GameText);

            Game.InhabitTables();

            // Set the form's title
            Text = Game.toString() + " on a date with C#";
            Name = Game.toString();
            Icon = Resources.Yatzy;

            DiceVec = new int[Game.Dice];
            OldDice = new int[Game.Dice];
            DiceRoll = new RollState[Game.Dice];
            UsedScores = new bool[Game.MaxItem + 1, Game.ScoreBoxesPerItem];

            TargetDie = -1;

            // Set the form's size
            ClientSize = new Size(Game.Dice * OptimalDieSize * 11 / 10, 360);

            RollCounter = 0;
            Rounds = 0;
            TerningeKastText = String.Format(Game.RollText(RollCounter), Game.Cardinal(0));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = true;
            TerningeKast.Focus();

            StartAgain.Text = StartOver[ChosenLanguage];

            Graphics g = Graphics.FromHwnd(Handle);

            for (int i = 0; i < Game.Dice; i++)
            {
                DiceRoll[i] = RollState.Unborn;
                DiceVec[i] = 0;
                DrawDie(g, i, 0, Color.Red);
            }

            for (int i = 0; i < UsedScores.GetLength(0); i++)
                for (int j = 0; j < Game.ScoreBoxesPerItem; j++)
                    UsedScores[i, j] = false;

            // Make the panel nice
            p.Height = 550;
            p.Top = ScoreCard;
            p.Width = 450;

            Rubrik = new TextBox[Game.MaxTotalItem + 1, 6];
            Items = new TextBox[Game.MaxTotalItem + 1];
            const int ItemWidth = 97;
            const int ScoreWidth = ItemWidth / 3;
            const int ItemTop = 0;

            p.Controls.Clear();
            Controls.Remove(p);

            for (int Col = 0; Col < Game.MaxGroup; Col++)
            {
                for (int i = 0; i < Items.Length; i++)
                    if (Col + 1 == Game.PreferredGroup(i))
                    {
                        Items[i] = new myTextBox();
                        if (Game.FirstScoreBox(i) > 0)
                        {
                            Items[i].TextAlign = HorizontalAlignment.Right;
                            Items[i].Width = ItemWidth + Game.FirstScoreBox(i) * ScoreWidth;
                        }
                        else
                            Items[i].Width = ItemWidth;
                        Items[i].Text = Game.ItemText(i);
                        p.Controls.Add(Items[i]);
                        Items[i].Enabled = false;
                        Items[i].BackColor = Color.Yellow;
                        Items[i].BorderStyle = BorderStyle.Fixed3D;
                        Items[i].Left = Col * ItemWidth * 3 / 2;
                        Items[i].Top = ItemTop + 20 * Game.PreferredRow(i);

                        for (int j = 0; j < Game.ScoreBoxesPerItem; j++)
                            if (j >= Game.FirstScoreBox(i))
                            {
                                Rubrik[i, j] = new myTextBox();
                                p.Controls.Add(Rubrik[i, j]);
                                Rubrik[i, j].Name = "Rubrik" + i + "." + j;
                                Rubrik[i, j].Enabled = false;
                                Rubrik[i, j].Left = Items[i].Left + ItemWidth + j * ScoreWidth;
                                Rubrik[i, j].Top = ItemTop + 20 * Game.PreferredRow(i);
                                Rubrik[i, j].Width = ScoreWidth;
                                Rubrik[i, j].BackColor = j >= Game.UsableScoreBoxesPerItem ? Color.Coral : Color.AliceBlue;
                                Rubrik[i, j].BorderStyle = BorderStyle.Fixed3D;
                                Rubrik[i, j].MouseMove += this.myMouseMove;
                                Rubrik[i, j].MouseHover += this.myMouseHover;
                                Rubrik[i, j].MouseDown += this.myMouseDown;
                                Rubrik[i, j].MouseLeave += this.myMouseLeave;
                            }
                    }
            }
            Controls.Add(p);
        }


        MyForm()
        {
            //CollectLanguage();

            SetStyle(ControlStyles.ResizeRedraw, true);

            Controls.Add(TerningeKast);

            TerningeKast.Click += OnButtonClicked;
            AcceptButton = TerningeKast;

            Controls.Add(StartAgain);

            StartAgain.Click += OnButtonClicked2;

            // Create a menu
            MainMenu menu = new MainMenu();
            MenuItem option = menu.MenuItems.Add("&Options");
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

            MenuItem games = menu.MenuItems.Add("&Games");
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

            MenuItem help = menu.MenuItems.Add("&Help");
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

            // Instantiate a panel for scoring
            p = new Panel();

            InitForm(CollectGameName());

            CollectPlayerName();
        }

        public void MyHelp(Control parent/*, 
		myHelpEnum topic*/
            )
        {
            // The file to display is chosen by the value of the topic.
            switch (0)
            {
                //			case myHelpEnum.enumWidgets:
                default:
                    FileInfo a = new FileInfo(Application.ExecutablePath);
                    string b = "file:///" + a.DirectoryName + '/';
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
                            "Help on " + Game.toString(),
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    break;
                //			case myHelpEnum.enumMechanism:
                //				// Insert code to implement additional functionality.
                //				break;
            }
        }

        // Handler for Options menu popups
        void OnPopupOptionsMenu(object sender, EventArgs e)
        {
            Danish.Checked = ChosenLanguage == (int)GameOfDice.Language.Danish;
            Swedish.Checked = ChosenLanguage == (int)GameOfDice.Language.Swedish;
            English.Checked = ChosenLanguage == (int)GameOfDice.Language.English;
            TouchDice.Checked = Touchy;
            ClickDice.Checked = !Touchy;
            Undo.Enabled = Undoable;
        }

        void OnPopupOptionsMenu2(object sender, EventArgs e)
        {
            PlayYatzy.Checked = Game.toString() == "Yatzy";
            PlayYahtzee.Checked = Game.toString() == "Yahtzee";
            PlayMaxiyatzy.Checked = Game.toString() == "Maxiyatzy";
            PlayBalut.Checked = Game.toString() == "Balut";
        }

        void OnPopupOptionsMenu3(object sender, EventArgs e)
        {
        }

        void OnLanguage1(object sender, EventArgs e)
        {
            ChosenLanguage = (int)GameOfDice.Language.Danish;
            Game.ChosenLanguage = ChosenLanguage;
            for (int i = 0; i <= Game.MaxTotalItem; i++)
                Items[i].Text = Game.ItemText(i);
            Invalidate();
        }

        void OnLanguage2(object sender, EventArgs e)
        {
            ChosenLanguage = (int)GameOfDice.Language.Swedish;
            Game.ChosenLanguage = ChosenLanguage;
            for (int i = 0; i <= Game.MaxTotalItem; i++)
                Items[i].Text = Game.ItemText(i);
            Invalidate();
        }

        void OnLanguage3(object sender, EventArgs e)
        {
            ChosenLanguage = (int)GameOfDice.Language.English;
            Game.ChosenLanguage = ChosenLanguage;
            for (int i = 0; i <= Game.MaxTotalItem; i++)
                Items[i].Text = Game.ItemText(i);
            Invalidate();
        }

        void OnGame1(object sender, EventArgs e)
        {
            bool b = PlayYatzy.Checked;
            PlayYatzy.Checked = true;
            PlayYahtzee.Checked = false;
            PlayMaxiyatzy.Checked = false;
            PlayBalut.Checked = false;
            if (!b)
                InitForm("Yatzy");
        }

        void OnGame2(object sender, EventArgs e)
        {
            bool b = PlayYahtzee.Checked;
            PlayYatzy.Checked = false;
            PlayYahtzee.Checked = true;
            PlayMaxiyatzy.Checked = false;
            PlayBalut.Checked = false;
            if (!b)
                InitForm("Yahtzee");
        }

        void OnGame3(object sender, EventArgs e)
        {
            bool b = PlayMaxiyatzy.Checked;
            PlayYatzy.Checked = false;
            PlayYahtzee.Checked = false;
            PlayMaxiyatzy.Checked = true;
            PlayBalut.Checked = false;
            if (!b)
                InitForm("Maxiyatzy");
        }

        void OnGame4(object sender, EventArgs e)
        {
            bool b = PlayBalut.Checked;
            PlayYatzy.Checked = false;
            PlayYahtzee.Checked = false;
            PlayMaxiyatzy.Checked = false;
            PlayBalut.Checked = true;
            if (!b)
                InitForm("Balut");
        }

        void OnUndo(object sender, EventArgs e)
        {
            //		if(Undoable) 
            {
                UnDecider();
            }
            Undoable = false;
        }

        void OnTouch(object sender, EventArgs e)
        {
            TouchDice.Checked = true;
            ClickDice.Checked = false;
            Touchy = true;
        }

        void OnClick(object sender, EventArgs e)
        {
            TouchDice.Checked = false;
            ClickDice.Checked = true;
            Touchy = false;
        }

        void OnHiScore(object sender, EventArgs e)
        {
            ShowHiScore();
        }

        void OnRules(object sender, EventArgs e)
        {
            MyHelp(p);
        }

        void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show("PhDGames©\n" +
                            "Created by Peter Hegelund\n" +
                            "Version 0.7.0.0, Nov 2014\n" +
                            "Programmed in C#", "About " + Game.toString(),
                MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        // Handler for the Exit command
        void OnExit(object sender, EventArgs e)
        {
            Close();
        }

        // ornament an ordinary six-sided die
        void OrnamentDie(Graphics g, Rectangle die, int eyes)
        {
            if (die.Height != die.Width)
                return;  // not square
            if (eyes == 0)
                return;  // the die is blank!
            if (eyes > 6)
                return;  // the die is fake!
            int S = die.Height;
            int X = die.X;
            int Y = die.Y;
            int EyeSize = S / 7;  // seems fair
            Point point = new Point(X + (S - EyeSize) / 2, Y + (S - EyeSize) / 2); // a little NV of the center
            Size Eye = new Size(EyeSize, EyeSize);
            Rectangle SnakeEye = new Rectangle(point, Eye);
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

        void DrawDie(Graphics g, int die, int val, Color DieCol)
        {
            if (die < 0)
                return;
            if (die >= Game.Dice)
                return;
            Pen pen = new Pen(Color.Black);
            GraphicsPath myGraphicsPath = new GraphicsPath();
            DieSize = Math.Min(OptimalDieSize, ClientSize.Width * 9 / (10 * Game.Dice));
            DieDist = DieSize / 10;
            int CornerSize = DieDist * 7 / 6;
            int Overshoot = DieDist + CornerSize;
            int OvershootHalved = Overshoot / 2;
            int OffsetX = die * (DieSize + DieDist);
            SolidBrush DieBrush = new SolidBrush(DieCol);

            Rectangle MyDie = new Rectangle(OffsetX, 10, DieSize, DieSize);

            //intersect the die with an ellipse to produce rounded corners
            Rectangle CornerDie = new Rectangle(OffsetX - OvershootHalved, 10 - OvershootHalved,
                DieSize + Overshoot, DieSize + Overshoot);
            myGraphicsPath.AddEllipse(CornerDie);
            Region MyRegion = new Region(myGraphicsPath);
            MyRegion.Intersect(MyDie);
            g.FillRegion(DieBrush, MyRegion);

            OrnamentDie(g, MyDie, val);

            pen.Dispose();
            DieBrush.Dispose();

        }


        // OnPaint handler
        protected override void OnPaint(PaintEventArgs e)
        {
            for (int i = 0; i < Game.Dice; i++)
            {
                DrawDie(e.Graphics, i, DiceVec[i],
                    DiceRoll[i] == RollState.RollMe ? Color.Black : Color.Red);
            }

            ShowSavedRolls(e.Graphics);

            TerningeKastText = String.Format(Game.RollText(RollCounter), Game.Cardinal(0));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Location = new Point(0, DieSize + 5 * DieDist);
            TerningeKast.Size = new Size(DieSize * Game.Dice + DieDist * (Game.Dice - 1), 24);

            StartAgain.Text = StartOver[ChosenLanguage];
            StartAgain.Location = new Point(0, DieSize + 5 * DieDist + 27);
            StartAgain.Size = new Size(DieSize * Game.Dice + DieDist * (Game.Dice - 1), 24);

        }

        void OnButtonClicked(Object sender, EventArgs e)
        {
            Undoable = false;
            Graphics g = Graphics.FromHwnd(Handle);
            TerningeKastText = RollCounter == 2 && Game.SavedRolls == 0 ?
                Result[ChosenLanguage] :
                klik[ChosenLanguage];
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = false;

            TargetDie = -1;

            RollCounter++;
            if (RollCounter > 3)
            {
                Game.UseARoll();
            }
            ShowSavedRolls(g);

            for (int i = 0; i < Game.Dice; i++)
                if (DiceRoll[i] != RollState.HoldMe)
                {
                    DiceRoll[i] = RollState.HoldMe;
                    DiceVec[i] = DiceGen.Next(1, 7);
                    DrawDie(g, i, DiceVec[i], Color.Red);
                }

            for (int r = 0; r <= Game.MaxItem; r++)
                for (int j = Game.FirstScoreBox(r); j < Game.UsableScoreBoxesPerItem; j++)
                {
                    if (!UsedScores[r, j])
                        Rubrik[r, j].Enabled = true;
                }
        }

        void OnButtonClicked2(Object sender, EventArgs e)
        {
            Undoable = false;
            Graphics g = Graphics.FromHwnd(Handle);

            RollCounter = 0;
            TerningeKastText = String.Format(Game.RollText(RollCounter), Game.Cardinal(RollCounter));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = true;
            TerningeKast.Focus();

            TargetDie = -1;

            Rounds = 0;
            Game.NewGame();
            for (int i = 0; i < Game.Dice; i++)
            {
                DiceRoll[i] = RollState.Unborn;
                DiceVec[i] = 0;
                DrawDie(g, i, 0, Color.Red);
            }

            ShowSavedRolls(g);

            int r;
            for (r = 0; r <= Game.MaxItem; r++)
                for (int j = Game.FirstScoreBox(r); j < Game.ScoreBoxesPerItem; j++)
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
            int SaveTargetDie = TargetDie;
            TargetDie = -1;

            if (e.Y > DieSize)
                return;
            Graphics g = Graphics.FromHwnd(Handle);
            int DieClicked = e.X / (DieSize + DieDist);

            if (DieClicked >= Game.Dice)
                return;

            if (RollCounter == 0)
                return;

            if (RollCounter >= 3 && Game.SavedRolls == 0)
                return;

            bool SameDie = SaveTargetDie == DieClicked;
            TargetDie = DieClicked;
            if (Touchy && SameDie)
                return;
            bool ThisDieMustRoll = DiceRoll[DieClicked] == RollState.RollMe;

            DiceRoll[DieClicked] = ThisDieMustRoll ? RollState.HoldMe : RollState.RollMe;

            DrawDie(g, DieClicked, DiceVec[DieClicked],
                ThisDieMustRoll ? Color.Red : Color.Black);

            ShowSavedRolls(g);

            TerningeKastText = String.Format(Game.RollText(RollCounter), Game.Cardinal(RollCounter));
            TerningeKast.Text = TerningeKastText;
            TerningeKast.Enabled = true;
            TerningeKast.Focus();

            g.Dispose();
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

        static void Main()
        {
            Application.Run(new MyForm());
        }

        //private void InitializeComponent()
        //{
        //    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MyForm));
        //    this.SuspendLayout();
        //    // 
        //    // MyForm
        //    // 
        //    this.ClientSize = new System.Drawing.Size(284, 261);
        //    this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        //    this.Name = "MyForm";
        //    this.ResumeLayout(false);

        //}
    }

    class myTextBox : TextBox
    {
        public myTextBox()
        {
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            SolidBrush drawBrush = new SolidBrush(ForeColor); //Use the ForeColor property 

            // Draw string to screen. 

            e.Graphics.DrawString(Text, Font, drawBrush, 0f, 0f); //Use the Font property 

        }

    }

    class PlayerNameDialog : Form
    {
        readonly TextBox NameBox;

        public string UserName
        {
            get { return NameBox.Text; }
            set { NameBox.Text = value; }
        }

        public PlayerNameDialog()
        {
            const int width = 300;
            const int border = 10;
            ClientSize = new Size(width, 100);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Text = "Name";
            ShowInTaskbar = false;

            Label nameLabel = new Label
            {
                Location = new Point(border, 12),
                Size = new Size(width/2, 24),
                Text = "Enter your name:"
            };

            NameBox = new TextBox
            {
                Location = new Point(border, 76),
                Size = new Size(width - 2*border, 48),
                TabIndex = 1
            };

            const int button = 64;
            Button okButton = new Button
            {
                Location = new Point(width - button - border, 12),
                Size = new Size(button, border*2),
                TabIndex = 2,
                Text = "OK",
                DialogResult = DialogResult.OK
            };

            Button notOkButton = new Button
            {
                Location = new Point(width - button - border, 44),
                Size = new Size(button, border*2),
                TabIndex = 3,
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };

            AcceptButton = okButton;
            CancelButton = notOkButton;

            Controls.Add(nameLabel);
            Controls.Add(NameBox);
            Controls.Add(okButton);
            Controls.Add(notOkButton);

        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }
    }

}