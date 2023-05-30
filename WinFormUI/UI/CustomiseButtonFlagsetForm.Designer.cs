namespace Z2Randomizer.WinFormUI
{
    partial class CustomiseButtonFlagsetForm
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
            components = new System.ComponentModel.Container();
            txtName = new TextBox();
            labelName = new Label();
            btnSave = new Button();
            labelFlagset = new Label();
            txtFlagset = new TextBox();
            labelTooltip = new Label();
            txtToolTip = new TextBox();
            btnCancel = new Button();
            btnUpdateFlagset = new Button();
            errorProvider1 = new ErrorProvider(components);
            ((System.ComponentModel.ISupportInitialize)errorProvider1).BeginInit();
            SuspendLayout();
            // 
            // txtName
            // 
            txtName.Location = new Point(73, 15);
            txtName.Name = "txtName";
            txtName.Size = new Size(177, 23);
            txtName.TabIndex = 1;
            txtName.Validating += txtName_Validating;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(12, 18);
            labelName.Name = "labelName";
            labelName.Size = new Size(39, 15);
            labelName.TabIndex = 0;
            labelName.Text = "Name";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(191, 118);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 7;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // labelFlagset
            // 
            labelFlagset.AutoSize = true;
            labelFlagset.Location = new Point(12, 47);
            labelFlagset.Name = "labelFlagset";
            labelFlagset.Size = new Size(44, 15);
            labelFlagset.TabIndex = 2;
            labelFlagset.Text = "Flagset";
            // 
            // txtFlagset
            // 
            txtFlagset.Location = new Point(73, 44);
            txtFlagset.Name = "txtFlagset";
            txtFlagset.Size = new Size(274, 23);
            txtFlagset.TabIndex = 3;
            txtFlagset.Validating += txtFlagset_Validating;
            // 
            // labelTooltip
            // 
            labelTooltip.AutoSize = true;
            labelTooltip.Location = new Point(12, 76);
            labelTooltip.Name = "labelTooltip";
            labelTooltip.Size = new Size(43, 15);
            labelTooltip.TabIndex = 4;
            labelTooltip.Text = "Tooltip";
            // 
            // txtToolTip
            // 
            txtToolTip.Location = new Point(73, 73);
            txtToolTip.Name = "txtToolTip";
            txtToolTip.Size = new Size(274, 23);
            txtToolTip.TabIndex = 5;
            txtToolTip.Validating += txtToolTip_Validating;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(272, 118);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnUpdateFlagset
            // 
            btnUpdateFlagset.Location = new Point(12, 118);
            btnUpdateFlagset.Name = "btnUpdateFlagset";
            btnUpdateFlagset.Size = new Size(119, 23);
            btnUpdateFlagset.TabIndex = 6;
            btnUpdateFlagset.Text = "Update Flagset";
            btnUpdateFlagset.UseVisualStyleBackColor = true;
            btnUpdateFlagset.Click += btnUpdateFlagset_Click;
            // 
            // errorProvider1
            // 
            errorProvider1.ContainerControl = this;
            // 
            // CustomiseButtonFlagsetForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(359, 161);
            Controls.Add(btnUpdateFlagset);
            Controls.Add(btnCancel);
            Controls.Add(labelTooltip);
            Controls.Add(txtToolTip);
            Controls.Add(labelFlagset);
            Controls.Add(txtFlagset);
            Controls.Add(btnSave);
            Controls.Add(labelName);
            Controls.Add(txtName);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CustomiseButtonFlagsetForm";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Custom Flagset";
            ((System.ComponentModel.ISupportInitialize)errorProvider1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtName;
        private Label labelName;
        private Button btnSave;
        private Label labelFlagset;
        private TextBox txtFlagset;
        private Label labelTooltip;
        private TextBox txtToolTip;
        private Button btnCancel;
        private Button btnUpdateFlagset;
        private ErrorProvider errorProvider1;
    }
}