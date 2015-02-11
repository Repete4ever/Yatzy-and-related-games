namespace Yatzy
{
    partial class Score
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Score));
            this.ScoreGrid = new System.Windows.Forms.DataGrid();
            ((System.ComponentModel.ISupportInitialize)(this.ScoreGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // ScoreGrid
            // 
            this.ScoreGrid.CaptionText = "Score";
            this.ScoreGrid.CaptionVisible = false;
            this.ScoreGrid.DataMember = "";
            this.ScoreGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScoreGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ScoreGrid.Location = new System.Drawing.Point(0, 0);
            this.ScoreGrid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ScoreGrid.Name = "ScoreGrid";
            this.ScoreGrid.ReadOnly = true;
            this.ScoreGrid.Size = new System.Drawing.Size(389, 252);
            this.ScoreGrid.TabIndex = 0;
            // 
            // Score
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 252);
            this.Controls.Add(this.ScoreGrid);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Score";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Score";
            ((System.ComponentModel.ISupportInitialize)(this.ScoreGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid ScoreGrid;
    }
}