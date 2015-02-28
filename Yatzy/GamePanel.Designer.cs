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
            this.components = new System.ComponentModel.Container();
            this.TerningeKast = new System.Windows.Forms.Button();
            this.StartAgain = new System.Windows.Forms.Button();
            this.nameLabel = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
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
            // 
            // nameLabel
            // 
            this.nameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameLabel.Location = new System.Drawing.Point(0, 0);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(100, 23);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "label1";
            this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.nameLabel.Click += new System.EventHandler(this.nameLabel_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Button TerningeKast;
        protected System.Windows.Forms.Button StartAgain;
        private System.Windows.Forms.Timer timer1;
        protected System.Windows.Forms.Label nameLabel;
    }
}
