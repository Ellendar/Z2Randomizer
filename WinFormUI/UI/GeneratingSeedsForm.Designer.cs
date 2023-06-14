namespace Z2Randomizer.WinFormUI;

partial class GeneratingSeedsForm
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
        button1 = new Button();
        SuspendLayout();
        // 
        // label1
        // 
        label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        label1.Location = new Point(14, 22);
        label1.Margin = new Padding(4, 0, 4, 0);
        label1.Name = "label1";
        label1.Size = new Size(173, 15);
        label1.TabIndex = 0;
        label1.Text = "Generating seed 100 of 100...";
        label1.TextAlign = ContentAlignment.MiddleCenter;
        label1.Click += label1_Click;
        // 
        // button1
        // 
        button1.Location = new Point(75, 55);
        button1.Margin = new Padding(4, 3, 4, 3);
        button1.Name = "button1";
        button1.Size = new Size(88, 27);
        button1.TabIndex = 1;
        button1.Text = "Cancel";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // GeneratingSeedsForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(243, 96);
        Controls.Add(button1);
        Controls.Add(label1);
        Margin = new Padding(4, 3, 4, 3);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "GeneratingSeedsForm";
        ShowIcon = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Generating Seeds";
        FormClosed += Form3_FormClosed;
        ResumeLayout(false);
    }

    #endregion

    public System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button button1;
}