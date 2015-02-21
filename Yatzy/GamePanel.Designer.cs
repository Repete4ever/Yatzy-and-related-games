namespace Yatzy
{
    partial class GamePanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TerningeKast = new System.Windows.Forms.Button();
            this.StartAgain = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TerningeKast
            // 
            this.TerningeKast.Location = new System.Drawing.Point(0, 0);
            this.TerningeKast.Name = "TerningeKast";
            this.TerningeKast.Size = new System.Drawing.Size(75, 23);
            this.TerningeKast.TabIndex = 0;
            this.TerningeKast.Text = "TerningeKast";
            this.TerningeKast.UseVisualStyleBackColor = true;
            this.TerningeKast.Click += new System.EventHandler(this.OnButtonClicked);
            // 
            // StartAgain
            // 
            this.StartAgain.Location = new System.Drawing.Point(0, 0);
            this.StartAgain.Name = "StartAgain";
            this.StartAgain.Size = new System.Drawing.Size(75, 23);
            this.StartAgain.TabIndex = 0;
            this.StartAgain.Text = "StartAgain";
            this.StartAgain.UseVisualStyleBackColor = true;
            this.StartAgain.Click += new System.EventHandler(this.OnButtonClicked2);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button TerningeKast;
        private System.Windows.Forms.Button StartAgain;
    }
}
