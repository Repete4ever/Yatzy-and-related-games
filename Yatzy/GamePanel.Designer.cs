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
            this.gameComboBox = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
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
            this.StartAgain.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.nameLabel.TabIndex = 1;
            this.nameLabel.Text = "label1";
            this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.nameLabel.Click += new System.EventHandler(this.nameLabel_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.Rolling);
            // 
            // gameComboBox
            // 
            this.gameComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gameComboBox.Items.AddRange(new object[] {
            "Yatzy",
            "Yahtzee",
            "Maxiyatzy",
            "Balut"});
            this.gameComboBox.Location = new System.Drawing.Point(0, 0);
            this.gameComboBox.Name = "gameComboBox";
            this.gameComboBox.Size = new System.Drawing.Size(121, 21);
            this.gameComboBox.TabIndex = 1;
            this.gameComboBox.SelectedIndexChanged += new System.EventHandler(this.gameComboBox_SelectedIndexChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(200, 100);
            this.tableLayoutPanel1.TabIndex = 0;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        protected System.Windows.Forms.Label nameLabel;
        public System.Windows.Forms.Button StartAgain;
        public System.Windows.Forms.Button TerningeKast;
        private System.Windows.Forms.ComboBox gameComboBox;
        public System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
