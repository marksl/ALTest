using System;
using System.Text;
using System.Windows.Forms;

namespace AltMstestGui
{
    public partial class FailureDetails : Form
    {
        public FailureDetails()
        {
            this.AutoSize = true;
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            if (ErrorDetails != null)
            {
                failureTextBox.Text = ErrorDetails.ToString();
            }

            base.OnShown(e);
        }

        public StringBuilder ErrorDetails { get; set; }
    }
}