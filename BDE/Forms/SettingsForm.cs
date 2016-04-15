using System;
using System.Windows.Forms;
using System.Configuration;

namespace BDEReplacement.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            var config = ConfigurationManager.AppSettings;
            config.Set("bdeConfigFilePath", configFilePathBox.Text);
        }
    }
}
