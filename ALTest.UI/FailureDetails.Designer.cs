namespace ALTest.UI
{
    partial class FailureDetails
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
            this.failureTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // failureTextBox
            // 
            this.failureTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.failureTextBox.Location = new System.Drawing.Point(0, 0);
            this.failureTextBox.MaxLength = 1000000;
            this.failureTextBox.Multiline = true;
            this.failureTextBox.Name = "failureTextBox";
            this.failureTextBox.ReadOnly = true;
            this.failureTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.failureTextBox.Size = new System.Drawing.Size(783, 460);
            this.failureTextBox.TabIndex = 0;
            // 
            // FailureDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(783, 460);
            this.Controls.Add(this.failureTextBox);
            this.Name = "FailureDetails";
            this.Text = "FailureDetails";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox failureTextBox;
    }
}