using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Z2Randomizer.WinFormUI
{
    public partial class ExternalLinkConfirmationForm : Form
    {
        string Url { get; set; }

        public ExternalLinkConfirmationForm(string url)
        {
            InitializeComponent();
            Url = url;
            UrlLabel.Text = Url;
        }

        private void TakeMeThere_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(Url) { UseShellExecute = true });
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CopyUrl_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Url);
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


    }
}
