using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BDEReplacement
{
    public partial class BDEForm : Form
    {
        public BDEForm()
        {
            InitializeComponent();
        }

        private void pathButton_Click(object sender, EventArgs e)
        {
            var databasePathFinder = new FolderBrowserDialog();
            if (databasePathFinder.ShowDialog() == DialogResult.OK)
            {
                pathTextBox.Text = databasePathFinder.SelectedPath;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Forms.SettingsForm settingsForm = new Forms.SettingsForm();
            settingsForm.ShowDialog();
        }
    }
}
