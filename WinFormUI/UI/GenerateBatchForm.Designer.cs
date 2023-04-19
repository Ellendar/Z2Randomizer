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
        this.label1 = new System.Windows.Forms.Label();
        this.textBox1 = new System.Windows.Forms.TextBox();
        this.label2 = new System.Windows.Forms.Label();
        this.button1 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(14, 10);
        this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        this.label1.MaximumSize = new System.Drawing.Size(408, 0);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(397, 45);
        this.label1.TabIndex = 0;
        this.label1.Text = "Please enter the number of seeds you would like to create. Note that each seed ca" +
"n take a few minutes to generate, depending on the settings you chose.";
        // 
        // textBox1
        // 
        this.textBox1.Location = new System.Drawing.Point(156, 68);
        this.textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        this.textBox1.Name = "textBox1";
        this.textBox1.Size = new System.Drawing.Size(116, 23);
        this.textBox1.TabIndex = 1;
        // 
        // label2
        // 
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(42, 72);
        this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(101, 15);
        this.label2.TabIndex = 2;
        this.label2.Text = "Number of Seeds:";
        // 
        // button1
        // 
        this.button1.Location = new System.Drawing.Point(280, 66);
        this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        this.button1.Name = "button1";
        this.button1.Size = new System.Drawing.Size(88, 27);
        this.button1.TabIndex = 3;
        this.button1.Text = "Generate!";
        this.button1.UseVisualStyleBackColor = true;
        this.button1.Click += new System.EventHandler(this.button1_Click);
        // 
        // GenerateBatchForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(413, 107);
        this.Controls.Add(this.button1);
        this.Controls.Add(this.label2);
        this.Controls.Add(this.textBox1);
        this.Controls.Add(this.label1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "GenerateBatchForm";
        this.ShowIcon = false;
        this.Text = "Generate Batch Seeds";
        this.Load += new System.EventHandler(this.GenerateBatchForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button button1;
}