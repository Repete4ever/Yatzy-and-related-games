namespace Yatzy
{
    partial class GameStats
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameStats));
            this.ScoreGrid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.ScoreGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // ScoreGrid
            // 
            this.ScoreGrid.AllowUserToAddRows = false;
            this.ScoreGrid.AllowUserToDeleteRows = false;
            this.ScoreGrid.AllowUserToOrderColumns = true;
            this.ScoreGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.ScoreGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ScoreGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScoreGrid.Location = new System.Drawing.Point(0, 0);
            this.ScoreGrid.MultiSelect = false;
            this.ScoreGrid.Name = "ScoreGrid";
            this.ScoreGrid.ReadOnly = true;
            this.ScoreGrid.Size = new System.Drawing.Size(591, 261);
            this.ScoreGrid.TabIndex = 0;
            // 
            // GameStats
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 261);
            this.Controls.Add(this.ScoreGrid);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GameStats";
            this.ShowInTaskbar = false;
            this.Text = "Game Stats";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameStats_FormClosing);
            this.Load += new System.EventHandler(this.GameStats_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ScoreGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView ScoreGrid;
    }
}