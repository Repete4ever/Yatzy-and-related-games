using System.Windows.Forms;

namespace Yatzy
{
    public partial class ScoreView : Form
    {
        public ScoreView()
        {
            InitializeComponent();
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
                ScoreGrid.Rows[value].Selected = true; ;
            }
        }
    }
}
