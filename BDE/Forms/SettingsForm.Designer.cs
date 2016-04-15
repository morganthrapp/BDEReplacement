namespace BDEReplacement.Forms
{
    partial class SettingsForm
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
            this.configFilePathLabel = new System.Windows.Forms.Label();
            this.configFilePathBox = new System.Windows.Forms.TextBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // configFilePathLabel
            // 
            this.configFilePathLabel.AutoSize = true;
            this.configFilePathLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.configFilePathLabel.Location = new System.Drawing.Point(8, 20);
            this.configFilePathLabel.Name = "configFilePathLabel";
            this.configFilePathLabel.Size = new System.Drawing.Size(97, 13);
            this.configFilePathLabel.TabIndex = 0;
            this.configFilePathLabel.Text = "Config File Path";
            // 
            // configFilePathBox
            // 
            this.configFilePathBox.Location = new System.Drawing.Point(111, 17);
            this.configFilePathBox.Name = "configFilePathBox";
            this.configFilePathBox.Size = new System.Drawing.Size(161, 20);
            this.configFilePathBox.TabIndex = 1;
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(99, 226);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 2;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.configFilePathBox);
            this.Controls.Add(this.configFilePathLabel);
            this.Name = "SettingsForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label configFilePathLabel;
        private System.Windows.Forms.TextBox configFilePathBox;
        private System.Windows.Forms.Button saveButton;
    }
}