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

public partial class GenerateBatchForm : Form
{
    public int numSeeds;
    public GenerateBatchForm()
    {
        numSeeds = -1;
        InitializeComponent();
        this.AcceptButton = button1;
    }

    private void button1_Click(object sender, EventArgs e)
    {
        try
        {
            int x = Int32.Parse(textBox1.Text);
            if(x <= 0)
            {
                MessageBox.Show("Please enter a number greater than 0!");
                return;
            }
            numSeeds = x;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Please enter a number greater than 0!");
        }
    }

    private int getNumSeeds()
    {
        return numSeeds;
    }

    private void GenerateBatchForm_Load(object sender, EventArgs e)
    {

    }
}
