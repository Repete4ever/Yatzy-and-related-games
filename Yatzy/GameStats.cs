using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Yatzy
{
    public partial class GameStats : Form
    {
        private DataTable _matchTable;
        private readonly List<MatchStat> _matchStats;
        private readonly string _matchFile;
        private int _addedMatches;

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
                    _matchStats = JsonConvert.DeserializeObject<List<MatchStat>>(json);
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
        private object Source
        {
            set
            {
                ScoreGrid.DataSource = new BindingSource { DataSource = value };
            }
        }

        public void AddMatch(MatchStat matchStat)
        {
            _addedMatches++;
            _matchStats.Insert(0, matchStat);
            //ShowMatches(GameForm.CollectGameName());
        }

        private int matches;
        private decimal victories;

        private void AddRow(MatchStat matchStat)
        {
            TimeSpan duration = matchStat.Ended - matchStat.Started;
            duration = new TimeSpan(duration.Hours, duration.Minutes, duration.Seconds);
            string matchUp = string.Format("{0} v {1}", matchStat.PlayerA, matchStat.PlayerB);
            var score =
                matchStat.GameName == "Balut"
                ?
                string.Format("{2}({0}) - {3}({1})", matchStat.ScoreA, matchStat.ScoreB, matchStat.PointsA, matchStat.PointsB)
                :
                string.Format("{0} - {1}", matchStat.ScoreA, matchStat.ScoreB);
            bool gameNotFinished = matchStat.MaxRound > matchStat.RoundA || matchStat.MaxRound > matchStat.RoundB;
            string winText;
            if (gameNotFinished)
            {
                winText = string.Format("Only {0} % finished",
                    (matchStat.RoundA + matchStat.RoundB) * 100 / (2*matchStat.MaxRound));
            }
            else
            {
                matches++;
                bool draw = matchStat.PointsA == matchStat.PointsB && matchStat.ScoreA == matchStat.ScoreB;
                if (draw)
                {
                    winText = "==";
                    victories += 0.5m;
                }
                else
                {
                    bool aWinner = matchStat.PointsA > matchStat.PointsB || matchStat.PointsA == matchStat.PointsB && matchStat.ScoreA > matchStat.ScoreB;
                    winText = aWinner ? matchStat.PlayerA : matchStat.PlayerB;
                    if (winText.StartsWith("HAL"))
                    {
                        victories++;
                    }
                }
            }
            _matchTable.Rows.Add(matchUp, matchStat.Started, duration, score, winText);
        }

        public void ShowMatches(string gameText = "")
        {
            _matchTable = new DataTable();
            _matchTable.Columns.Add("Match Up", typeof(string));
            _matchTable.Columns.Add("Started", typeof(DateTime));
            _matchTable.Columns.Add("Duration", typeof(TimeSpan));
            _matchTable.Columns.Add("Score", typeof(string));
            _matchTable.Columns.Add("Winner", typeof(string));
            foreach (var gameStat in _matchStats)
            {
                if (gameText == "" || gameStat.GameName == gameText)
                {
                    AddRow(gameStat);
                }
            }
            Source = _matchTable;
            Text = gameText + " Matches";

            if (matches > 1)
            {
                toolStripStatusLabel1.Text = string.Format("HAL is {0} - {1}", victories, matches - victories);
                statusStrip1.Refresh();
                
            }
            ShowDialog();

            matches = 0;
            victories = 0;
        }

        public void ShowHiScores(string gameText, int cutOff)
        {
            var hiScore = new DataTable();
            hiScore.Columns.Add("Player", typeof(string));
            hiScore.Columns.Add("Date", typeof(DateTime));
            if (gameText == "Balut")
            {
                hiScore.Columns.Add("Points", typeof(int));
            }
            hiScore.Columns.Add("Score", typeof(int));
            List<GameStat> scores = new List<GameStat>();
            foreach (MatchStat matchStat in _matchStats)
            {
                if (matchStat.GameName != gameText) continue;
                if (matchStat.MaxRound > matchStat.RoundA) continue;
                if (matchStat.MaxRound > matchStat.RoundB) continue;
                // only completed matches of the specified type are reckoned
                DateTime ended = matchStat.Ended.Date;
                if (ended < DateTime.Today.AddDays(-cutOff)) continue;
                // only recent results are considered
                GameStat gameA = new GameStat();
                GameStat gameB = new GameStat();
                gameA.Player = matchStat.PlayerA;
                gameB.Player = matchStat.PlayerB;
                gameA.Date = ended;
                gameB.Date = ended;
                gameA.Score = matchStat.ScoreA;
                gameB.Score = matchStat.ScoreB;
                gameA.Points = matchStat.PointsA;
                gameB.Points = matchStat.PointsB;
                scores.Add(gameA);
                scores.Add(gameB);
            }

            var gameStats = scores.OrderByDescending(i => i.Points).ThenByDescending(j => j.Score).Take(Properties.Settings.Default.TopResults).ToList();
            int sum = 0;
            foreach (var top in gameStats)
            {
                if (gameText == "Balut")
                {
                    hiScore.Rows.Add(top.Player, top.Date, top.Points, top.Score);
                    sum += top.Points;
                }
                else
                {
                    hiScore.Rows.Add(top.Player, top.Date, top.Score);
                    sum += top.Score;
                }
            }
            Source = hiScore;
            int count = gameStats.Count();
            string days = cutOff < 1000 ? "last " + cutOff + " days" : "of all time";
            Text = gameText + " Top " + Math.Min(Properties.Settings.Default.TopResults, count) + " (" + days + ")";
            if (count > 1)
            {
                toolStripStatusLabel1.Text = string.Format("Game average is {0}", sum / gameStats.Count());
                statusStrip1.Refresh();
            }

            ShowDialog();
        }

        public void SaveMatches()
        {
            if (_addedMatches == 0)
                return;
            string json = JsonConvert.SerializeObject(_matchStats, Formatting.Indented);
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

    public class MatchStat
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

    public class GameStat
    {
        public DateTime Date { get; set; }
        public string Player { get; set; }
        public int Points { get; set; }
        public int Score { get; set; }
    }
}
