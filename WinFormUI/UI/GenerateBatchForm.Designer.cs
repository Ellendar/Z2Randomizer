namespace Z2Randomizer.WinFormUI;

partial class GenerateBatchForm
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
        label1 = new Label();
        textBox1 = new TextBox();
        label2 = new Label();
        button1 = new Button();
        SuspendLayout();
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(14, 10);
        label1.Margin = new Padding(4, 0, 4, 0);
        label1.MaximumSize = new Size(408, 0);
        label1.Name = "label1";
        label1.Size = new Size(397, 45);
        label1.TabIndex = 0;
        label1.Text = "Please enter the number of seeds you would like to create. Note that each seed can take a few minutes to generate, depending on the settings you chose.";
        // 
        // textBox1
        // 
        textBox1.Location = new Point(156, 68);
        textBox1.Margin = new Padding(4, 3, 4, 3);
        textBox1.Name = "textBox1";
        textBox1.Size = new Size(116, 23);
        textBox1.TabIndex = 1;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(42, 72);
        label2.Margin = new Padding(4, 0, 4, 0);
        label2.Name = "label2";
        label2.Size = new Size(101, 15);
        label2.TabIndex = 2;
        label2.Text = "Number of Seeds:";
        // 
        // button1
        // 
        button1.Location = new Point(280, 66);
        button1.Margin = new Padding(4, 3, 4, 3);
        button1.Name = "button1";
        button1.Size = new Size(88, 27);
        button1.TabIndex = 3;
        button1.Text = "Generate!";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // GenerateBatchForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(413, 107);
        Controls.Add(button1);
        Controls.Add(label2);
        Controls.Add(textBox1);
        Controls.Add(label1);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Margin = new Padding(4, 3, 4, 3);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "GenerateBatchForm";
        ShowIcon = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Generate Batch Seeds";
        Load += GenerateBatchForm_Load;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button button1;
}