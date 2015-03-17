using System.Drawing;
using System.Windows.Forms;

namespace Yatzy
{
    public partial class ScoreView : Form
    {
        public delegate void ViewClosing();

        private readonly ViewClosing _viewClosing;

        public ScoreView(ViewClosing closing)
        {
            InitializeComponent();
            _viewClosing = closing;
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

        /// <summary>
        /// point to the new entry
        /// </summary>
        public int SelectRow
        {
            get
            {
                for (int i = 0; i < ScoreGrid.Rows.Count; i++)
                {
                    if (ScoreGrid.Rows[i].Selected)
                        return i;
                }
                return -1;
            }
            set
            {
                ScoreGrid.Rows[value].Selected = true; 
            }
        }

        private void ScoreView_Load(object sender, System.EventArgs e)
        {
            if ((ModifierKeys & Keys.Shift) == 0)
            {
                string initLocation = Properties.Settings.Default.ScoreLocation;
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

        private void ScoreView_FormClosing(object sender, FormClosingEventArgs e)
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
                Properties.Settings.Default.ScoreLocation = initLocation;
                Properties.Settings.Default.Save();
            }
            if (_viewClosing != null)
            {
                _viewClosing();
            }
        }
    }
}
