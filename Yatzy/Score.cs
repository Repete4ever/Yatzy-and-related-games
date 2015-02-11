using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Yatzy
{
    /// <summary>
    /// PhD 2014-11-30
    /// Replaced by ScoreView which is atop the more moderne DataGridView
    /// </summary>
    public partial class Score : Form
    {
        public Score()
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
                ScoreGrid.SetDataBinding(value, "");
            }
        }

        /// <summary>
        /// point to the new entry
        /// </summary>
        public int SelectRow
        {
            get
            {
                return ScoreGrid.CurrentRowIndex;
            }
            set
            {
                ScoreGrid.CurrentRowIndex = value;
            }
        }
    }
}
