using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Z2Randomizer.WinFormUI
{
    public partial class CustomiseButtonFlagsetForm : Form
    {
        Button? customButton;
        string mainFormFlagset;

        public CustomiseButtonFlagsetForm()
        {
            InitializeComponent();
            btnCancel.CausesValidation = false;
            AcceptButton = btnSave;
            CancelButton = btnCancel;

        }
        public CustomiseButtonFlagsetForm(Button caller, string flagset) : this()
        {
            customButton = caller;
            mainFormFlagset = flagset;

            SetupForm();
        }

        private void SetupForm()
        {
            btnUpdateFlagset.Enabled = false;

            if (customButton != null)
            {
                CustomisedButtonSettings? settings = customButton.Tag as CustomisedButtonSettings;
                if (settings != null)
                {
                    if (settings.IsCustomised)
                    {
                        this.txtName.Text = settings.Name;
                        txtFlagset.Text = settings.Flagset;
                        txtToolTip.Text = settings.Tooltip;
                    }
                    else
                    {
                        txtName.PlaceholderText = settings.Name;

                    }
                }
            }

            // setup the flagset from the main form if we don't have a value already.
            if (string.IsNullOrWhiteSpace(txtFlagset.Text) && !string.IsNullOrWhiteSpace(mainFormFlagset))
            {
                txtFlagset.Text = mainFormFlagset;
            }

            if (mainFormFlagset != txtFlagset.Text && !string.IsNullOrWhiteSpace(mainFormFlagset))
            {
                btnUpdateFlagset.Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            bool isvalid = ValidateChildren(ValidationConstraints.Enabled);
            if (!errorProvider1.HasErrors)
            {
                CustomisedButtonSettings? settings = customButton.Tag as CustomisedButtonSettings;
                if (settings != null)
                {
                    settings.Name = txtName.Text;
                    settings.Flagset = txtFlagset.Text;
                    settings.Tooltip = txtToolTip.Text;
                    settings.IsCustomised = true;
                }
                else
                {
                    settings = new CustomisedButtonSettings(txtName.Text, txtFlagset.Text, txtToolTip.Text);
                    customButton.Tag = settings;
                }

                this.DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void txtName_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorProvider1.SetError(txtName, "Name cannot be blank!");
            }
            if (txtName.Text.Contains("|"))
            {
                errorProvider1.SetError(txtName, "Pipe | cannot be used in Name!");
            }
        }

        private void txtFlagset_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFlagset.Text))
            {
                errorProvider1.SetError(txtFlagset, "Flagset cannot be blank!");
            }
            if (txtFlagset.Text.Contains("|"))
            {
                errorProvider1.SetError(txtFlagset, "Pipe | cannot be used in Flagset!");
            }
        }

        private void btnUpdateFlagset_Click(object sender, EventArgs e)
        {
            txtFlagset.Text = mainFormFlagset;
        }

        private void txtToolTip_Validating(object sender, CancelEventArgs e)
        {
            if (txtToolTip.Text.Contains("|"))
            {
                errorProvider1.SetError(txtToolTip, "Pipe | cannot be used in Tooltip!");
            }
        }
    }
}
