using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Z2Randomizer.WinFormUI;

public partial class GeneratingSeedsForm : Form
{
    public bool isClosed;
    public GeneratingSeedsForm()
    {
        isClosed = false;
        this.CreateHandle();
        InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        label1.Text = "Cancelling after next seed....";
        isClosed = true;
        this.Close();
    }

    public void setText(String t)
    {
        label1.Text = t;
    }

    private void Form3_FormClosed(object sender, FormClosedEventArgs e)
    {
        isClosed = true;
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }
}
