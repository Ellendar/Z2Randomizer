namespace Z2Randomizer.WinFormUI
{
    partial class ExternalLinkConfirmationForm
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
            TakeMeThere = new Button();
            CopyUrl = new Button();
            UrlLabel = new LinkLabel();
            MessageLabel = new Label();
            SuspendLayout();
            // 
            // TakeMeThere
            // 
            TakeMeThere.Location = new Point(12, 125);
            TakeMeThere.Name = "TakeMeThere";
            TakeMeThere.Size = new Size(106, 27);
            TakeMeThere.TabIndex = 2;
            TakeMeThere.Text = "Take Me There!";
            TakeMeThere.UseVisualStyleBackColor = true;
            TakeMeThere.Click += TakeMeThere_Click;
            // 
            // CopyUrl
            // 
            CopyUrl.Location = new Point(147, 125);
            CopyUrl.Name = "CopyUrl";
            CopyUrl.Size = new Size(106, 27);
            CopyUrl.TabIndex = 3;
            CopyUrl.Text = "Copy URL";
            CopyUrl.UseVisualStyleBackColor = true;
            CopyUrl.Click += CopyUrl_Click;
            // 
            // UrlLabel
            // 
            UrlLabel.Location = new Point(12, 65);
            UrlLabel.Name = "UrlLabel";
            UrlLabel.Size = new Size(241, 40);
            UrlLabel.TabIndex = 1;
            UrlLabel.TabStop = true;
            UrlLabel.Text = "Url";
            UrlLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MessageLabel
            // 
            MessageLabel.Location = new Point(12, 9);
            MessageLabel.Name = "MessageLabel";
            MessageLabel.Size = new Size(241, 56);
            MessageLabel.TabIndex = 0;
            MessageLabel.Text = "We're about to open a browser tab and redirect you to:";
            MessageLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ExternalLinkConfirmationForm
            // 
            AcceptButton = TakeMeThere;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = CopyUrl;
            ClientSize = new Size(265, 164);
            Controls.Add(MessageLabel);
            Controls.Add(UrlLabel);
            Controls.Add(CopyUrl);
            Controls.Add(TakeMeThere);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ExternalLinkConfirmationForm";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Confirm";
            ResumeLayout(false);
        }

        #endregion

        private Button TakeMeThere;
        private Button CopyUrl;
        private LinkLabel UrlLabel;
        private Label MessageLabel;
    }
}