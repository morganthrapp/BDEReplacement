using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace BDEReplacement
{
    public partial class BDEForm : Form
    {
        private List<BDEAliasesFile.Alias> aliases;
        private string currentPath;
        private string currentName;
        public BDEForm()
        {
            InitializeComponent();
        }

        private void loadAliases()
        {
            var config = ConfigurationManager.AppSettings;
            BDEAliasesFile aliasFile = new BDEAliasesFile(config.Get("bdeAliasesFile"));
            aliases = aliasFile.GetAliases();
            aliasList.Items.Clear();
            foreach (var alias in aliases)
            {
                aliasList.Items.Add(alias.name);
            }

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
            var config = ConfigurationManager.AppSettings;
            BDEAliasesFile aliasFile = new BDEAliasesFile(config.Get("bdeAliasesFile"));
            string name = nameTextBox.Text;
            string path = pathTextBox.Text;
            bool enableBCD = enableBCDCheckBox.Checked;
            if (aliasList.SelectedIndex > -1)
            {
                aliasFile.UpdateAlias(currentName, currentPath, name, path, enableBCD);
            } else
            {
                aliasFile.CreateNewAlias(name, path, enableBCD);
            }
            
            loadAliases();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Forms.SettingsForm settingsForm = new Forms.SettingsForm();
            settingsForm.ShowDialog();
        }

        private void BDEForm_Load(object sender, EventArgs e)
        {
            loadAliases();
        }

        private void aliasList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (aliasList.SelectedIndex > -1)
            {
                currentName = aliasList.SelectedItem.ToString() ?? "";
                if (currentName != "")
                {
                    nameTextBox.Text = currentName;
                    foreach (var alias in aliases)
                    {
                        if (alias.name == currentName)
                        {
                            currentPath = alias.path;
                            pathTextBox.Text = currentPath;
                            enableBCDCheckBox.Checked = alias.enableBCD;
                            break;
                        }
                    }

                }
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aliasList.ClearSelected();
            nameTextBox.Text = "";
            pathTextBox.Text = "";
        }
    }
}
