using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Yatzy
{
    public partial class GameStats : Form
    {
        private DataTable _editedScoreTable;
        private readonly List<GameStat> _gameStats;
        private readonly string _matchFile;

        public GameStats()
        {
            InitializeComponent();

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var gamesFolder = Path.Combine(folder, Properties.Settings.Default.Repository);
            _matchFile = Path.Combine(gamesFolder, GameForm.MatchesFileName);
            try
            {
                using (var stream = File.OpenText(_matchFile))
                {
                    string json = stream.ReadToEnd();
                    _gameStats = JsonConvert.DeserializeObject<List<GameStat>>(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Get the data source
        /// </summary>
        public object Source
        {
            set
            {
                ScoreGrid.DataSource = new BindingSource { DataSource = value };
            }
        }

        public void AddMatch(GameStat gameStat)
        {
            _gameStats.Insert(0, gameStat);
            ShowMatches(GameForm.CollectGameName());
        }

        private void AddRow(GameStat gameStat)
        {
            TimeSpan duration = gameStat.Ended - gameStat.Started;
            string matchUp = string.Format("{0} v {1}", gameStat.PlayerA, gameStat.PlayerB);
            var score =
                gameStat.GameName == "Balut"
                ?
                string.Format("{2}({0}) - {3}({1})", gameStat.ScoreA, gameStat.ScoreB, gameStat.PointsA, gameStat.PointsB)
                :
                string.Format("{0} - {1}", gameStat.ScoreA, gameStat.ScoreB);
            bool gameNotFinished = gameStat.MaxRound > gameStat.RoundA || gameStat.MaxRound > gameStat.RoundB;
            string winText;
            if (gameNotFinished)
            {
                winText = string.Format("Only {0} % finished",
                    (gameStat.RoundA + gameStat.RoundB) * 100 / (2*gameStat.MaxRound));
            }
            else
            {
                bool draw = gameStat.PointsA == gameStat.PointsB && gameStat.ScoreA == gameStat.ScoreB;
                if (draw)
                    winText = "==";
                else
                {
                    bool aWinner = gameStat.PointsA > gameStat.PointsB || gameStat.PointsA == gameStat.PointsB && gameStat.ScoreA > gameStat.ScoreB;
                    winText = aWinner ? gameStat.PlayerA : gameStat.PlayerB;
                }
            }
            _editedScoreTable.Rows.Add(matchUp, gameStat.Started, duration, score, winText);
        }

        public void ShowMatches(string gameText = "")
        {
            _editedScoreTable = new DataTable();
            _editedScoreTable.Columns.Add("Match Up", typeof(string));
            _editedScoreTable.Columns.Add("Started", typeof(DateTime));
            _editedScoreTable.Columns.Add("Duration", typeof(TimeSpan));
            _editedScoreTable.Columns.Add("Score", typeof(string));
            _editedScoreTable.Columns.Add("Winner", typeof(string));
            foreach (var gameStat in _gameStats)
            {
                if (gameText == "" || gameStat.GameName == gameText)
                {
                    AddRow(gameStat);
                }
            }
            Source = _editedScoreTable;
            ShowDialog();
        }

        public void SaveMatches()
        {
            string json = JsonConvert.SerializeObject(_gameStats);
            try
            {
                File.WriteAllText(_matchFile, json);
            }
            catch (Exception ex)
            {
                
                MessageBox.Show(ex.Message);
            }
        }

        private void GameStats_Load(object sender, EventArgs e)
        {
            if ((ModifierKeys & Keys.Shift) == 0)
            {
                string initLocation = Properties.Settings.Default.MatchLocation;
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

        private void GameStats_FormClosing(object sender, FormClosingEventArgs e)
        {
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
                Properties.Settings.Default.MatchLocation = initLocation;
                Properties.Settings.Default.Save();
            }
        }
    }

    public class GameStat
    {
        public string GameName { get; set; }
        public DateTime Started { get; set; }
        public DateTime Ended { get; set; }
        public int MaxRound { get; set; }

        public string PlayerA { get; set; }
        public int PointsA { get; set; }
        public int ScoreA { get; set; }
        public int RoundA { get; set; }

        public string PlayerB { get; set; }
        public int PointsB { get; set; }
        public int ScoreB { get; set; }
        public int RoundB { get; set; }
    }
}
