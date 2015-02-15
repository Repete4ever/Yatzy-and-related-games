namespace Yatzy
{
    partial class ScorePanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScorePanel));
            this.ScoreCard = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // ScoreCard
            // 
            this.ScoreCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScoreCard.Location = new System.Drawing.Point(0, 0);
            this.ScoreCard.Name = "ScoreCard";
            this.ScoreCard.Size = new System.Drawing.Size(232, 141);
            this.ScoreCard.TabIndex = 0;
            // 
            // ScorePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(232, 141);
            this.Controls.Add(this.ScoreCard);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScorePanel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Score Card";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ScoreCard;
    }
}