namespace Yatzy
{
    partial class GameForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameForm));
            this.scorePanels = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Danish = new System.Windows.Forms.ToolStripMenuItem();
            this.Swedish = new System.Windows.Forms.ToolStripMenuItem();
            this.English = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.Undo = new System.Windows.Forms.ToolStripMenuItem();
            this.TouchDice = new System.Windows.Forms.ToolStripMenuItem();
            this.ClickDice = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.eXitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PlayYatzy = new System.Windows.Forms.ToolStripMenuItem();
            this.PlayYahtzee = new System.Windows.Forms.ToolStripMenuItem();
            this.PlayMaxiyatzy = new System.Windows.Forms.ToolStripMenuItem();
            this.PlayBalut = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.showHiScoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // scorePanels
            // 
            this.scorePanels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scorePanels.ColumnCount = 2;
            this.scorePanels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.scorePanels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.scorePanels.Location = new System.Drawing.Point(0, 27);
            this.scorePanels.Name = "scorePanels";
            this.scorePanels.RowCount = 1;
            this.scorePanels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.scorePanels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.scorePanels.Size = new System.Drawing.Size(853, 425);
            this.scorePanels.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.gameToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(853, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Danish,
            this.Swedish,
            this.English,
            this.toolStripSeparator1,
            this.Undo,
            this.TouchDice,
            this.ClickDice,
            this.toolStripSeparator2,
            this.eXitToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.OnPopupOptionsMenu);
            // 
            // Danish
            // 
            this.Danish.Name = "Danish";
            this.Danish.Size = new System.Drawing.Size(156, 22);
            this.Danish.Text = "&Dansksproget";
            this.Danish.Click += new System.EventHandler(this.OnLanguage1);
            // 
            // Swedish
            // 
            this.Swedish.Name = "Swedish";
            this.Swedish.Size = new System.Drawing.Size(156, 22);
            this.Swedish.Text = "&Svenskspråkig";
            this.Swedish.Click += new System.EventHandler(this.OnLanguage2);
            // 
            // English
            // 
            this.English.Name = "English";
            this.English.Size = new System.Drawing.Size(156, 22);
            this.English.Text = "In &English";
            this.English.Click += new System.EventHandler(this.OnLanguage3);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(153, 6);
            // 
            // Undo
            // 
            this.Undo.Enabled = false;
            this.Undo.Name = "Undo";
            this.Undo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.Undo.Size = new System.Drawing.Size(156, 22);
            this.Undo.Text = "&Undo";
            this.Undo.Click += new System.EventHandler(this.OnUndo);
            // 
            // TouchDice
            // 
            this.TouchDice.CheckOnClick = true;
            this.TouchDice.Name = "TouchDice";
            this.TouchDice.Size = new System.Drawing.Size(156, 22);
            this.TouchDice.Text = "select by &Touch";
            this.TouchDice.Click += new System.EventHandler(this.OnTouch);
            // 
            // ClickDice
            // 
            this.ClickDice.Checked = true;
            this.ClickDice.CheckOnClick = true;
            this.ClickDice.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ClickDice.Name = "ClickDice";
            this.ClickDice.Size = new System.Drawing.Size(156, 22);
            this.ClickDice.Text = "select by &Click";
            this.ClickDice.Click += new System.EventHandler(this.OnClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(153, 6);
            // 
            // eXitToolStripMenuItem
            // 
            this.eXitToolStripMenuItem.Name = "eXitToolStripMenuItem";
            this.eXitToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.eXitToolStripMenuItem.Text = "e&Xit";
            this.eXitToolStripMenuItem.Click += new System.EventHandler(this.OnExit);
            // 
            // gameToolStripMenuItem
            // 
            this.gameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PlayYatzy,
            this.PlayYahtzee,
            this.PlayMaxiyatzy,
            this.PlayBalut,
            this.toolStripSeparator3,
            this.showHiScoreToolStripMenuItem});
            this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            this.gameToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.gameToolStripMenuItem.Text = "&Games";
            this.gameToolStripMenuItem.Click += new System.EventHandler(this.OnPopupOptionsMenu2);
            // 
            // PlayYatzy
            // 
            this.PlayYatzy.Name = "PlayYatzy";
            this.PlayYatzy.Size = new System.Drawing.Size(146, 22);
            this.PlayYatzy.Text = "&Yatzy";
            this.PlayYatzy.Click += new System.EventHandler(this.OnGame1);
            // 
            // PlayYahtzee
            // 
            this.PlayYahtzee.Name = "PlayYahtzee";
            this.PlayYahtzee.Size = new System.Drawing.Size(146, 22);
            this.PlayYahtzee.Text = "ya&Htzee";
            this.PlayYahtzee.Click += new System.EventHandler(this.OnGame2);
            // 
            // PlayMaxiyatzy
            // 
            this.PlayMaxiyatzy.Name = "PlayMaxiyatzy";
            this.PlayMaxiyatzy.Size = new System.Drawing.Size(146, 22);
            this.PlayMaxiyatzy.Text = "&Maxiyatzy";
            this.PlayMaxiyatzy.Click += new System.EventHandler(this.OnGame3);
            // 
            // PlayBalut
            // 
            this.PlayBalut.Name = "PlayBalut";
            this.PlayBalut.Size = new System.Drawing.Size(146, 22);
            this.PlayBalut.Text = "&Balut";
            this.PlayBalut.Click += new System.EventHandler(this.OnGame4);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(143, 6);
            // 
            // showHiScoreToolStripMenuItem
            // 
            this.showHiScoreToolStripMenuItem.Name = "showHiScoreToolStripMenuItem";
            this.showHiScoreToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.showHiScoreToolStripMenuItem.Text = "show &HiScore";
            this.showHiScoreToolStripMenuItem.Click += new System.EventHandler(this.OnHiScore);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rulesToolStripMenuItem,
            this.toolStripSeparator4,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // rulesToolStripMenuItem
            // 
            this.rulesToolStripMenuItem.Name = "rulesToolStripMenuItem";
            this.rulesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.rulesToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.rulesToolStripMenuItem.Text = "&Rules";
            this.rulesToolStripMenuItem.Click += new System.EventHandler(this.OnRules);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(118, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.OnAbout);
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(853, 449);
            this.Controls.Add(this.scorePanels);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "GameForm";
            this.Text = "GameForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel scorePanels;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem Danish;
        private System.Windows.Forms.ToolStripMenuItem Swedish;
        private System.Windows.Forms.ToolStripMenuItem English;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem Undo;
        private System.Windows.Forms.ToolStripMenuItem TouchDice;
        private System.Windows.Forms.ToolStripMenuItem ClickDice;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem eXitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PlayYatzy;
        private System.Windows.Forms.ToolStripMenuItem PlayYahtzee;
        private System.Windows.Forms.ToolStripMenuItem PlayMaxiyatzy;
        private System.Windows.Forms.ToolStripMenuItem PlayBalut;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem showHiScoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rulesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}