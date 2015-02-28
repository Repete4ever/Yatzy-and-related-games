using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Yatzy
{
    public partial class PlayerName : Form
    {
        private readonly RegistryKey _gameKey;

        public PlayerName()
        {
            InitializeComponent();
            try
            {
                UserName = Environment.UserName;
            }
            catch
            {
                UserName = "NN";
            }

            _gameKey = Registry.CurrentUser.CreateSubKey(GameForm.RegFolder);
            UserName = _gameKey.GetValue("Player Name", UserName).ToString();
        }

        public string UserName
        {
            get { return NameBox.Text; }
            private set { NameBox.Text = value; }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _gameKey.SetValue("Player Name", UserName);
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            string name = NameBox.Text.Trim().ToUpper();
            okButton.Enabled = !string.IsNullOrEmpty(name) && !name.StartsWith("HAL");
        }
    }
}
