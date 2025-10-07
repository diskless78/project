using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mtv_client
{
    public partial class Change_Password : Form
    {
        public Change_Password()
        {
            InitializeComponent();
        }

        private void Change_Password_Load(object sender, EventArgs e)
        {
            txt_OldPassword.UseSystemPasswordChar = true;
            txt_NewPassword.UseSystemPasswordChar = true;
            txt_ConfirmNewPassword.UseSystemPasswordChar = true;

            txt_OldPassword.Enabled = false;
            txt_NewPassword.Enabled = false;
            txt_ConfirmNewPassword.Enabled = false;
            change_Button.Enabled = false;

        }

        private void Change_Password_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void txt_EmailAddress_TextChanged(object sender, EventArgs e)
        {
            if (txt_EmailAddress.Text.Trim() != string.Empty)
            {
                bool findResult = await FindTheLuckThenFuckTheLife.FindAccount(txt_EmailAddress.Text.Trim().ToLower());
                if (findResult)
                {
                    txt_OldPassword.Enabled = true;
                }
                else
                {
                    txt_OldPassword.Enabled = false;
                    txt_OldPassword.Clear();
                    txt_NewPassword.Clear();
                    txt_ConfirmNewPassword.Clear();
                }
            }

        }

        private void txt_OldPassword_TextChanged(object sender, EventArgs e)
        {
            bool getPassword = FindTheLuckThenFuckTheLife.IsPasswordMatched(txt_OldPassword.Text, txt_EmailAddress.Text.Trim().ToLower());
            if (getPassword)
            {
                txt_NewPassword.Enabled = true;
                txt_ConfirmNewPassword.Enabled = true;
                change_Button.Enabled = true;
                txt_EmailAddress.Enabled = false;
                txt_OldPassword.Enabled = false;
            }
            else
            {
                txt_EmailAddress.Enabled = true;
                txt_OldPassword.Enabled = true;

                txt_NewPassword.Enabled = false;
                txt_ConfirmNewPassword.Enabled = false;
                change_Button.Enabled = false;
                txt_NewPassword.Clear();
                txt_ConfirmNewPassword?.Clear();
            }
        }

        private void change_Button_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txt_NewPassword.Text) && !string.IsNullOrWhiteSpace(txt_ConfirmNewPassword.Text))
            {
                if (txt_NewPassword.Text == txt_ConfirmNewPassword.Text)
                {
                    string emailUserName = txt_EmailAddress.Text.Trim().ToLower().Split('@')[0];
                    if (FindTheLuckThenFuckTheLife.UpdatePassword(txt_EmailAddress.Text.Trim().ToLower(), txt_OldPassword.Text, txt_NewPassword.Text))
                    {
                        DialogResult result = MessageBox.Show("Password has been changed successfully. Do you want to view your password again ?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (result == DialogResult.Yes)
                        {
                            string viewPassword = txt_NewPassword.Text;
                            MessageBox.Show($"Your New Password: {viewPassword}", "Password", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txt_OldPassword.Clear();
                            txt_NewPassword.Clear();
                            txt_ConfirmNewPassword.Clear();
                            txt_EmailAddress.Clear();
                            txt_EmailAddress.Enabled = true;
                        }
                        else
                        {
                            txt_EmailAddress.Clear();
                            txt_EmailAddress.Enabled = true;
                            txt_OldPassword.Clear();
                            txt_NewPassword.Clear();
                            txt_ConfirmNewPassword.Clear();
                        }

                       
                        string filePath = Path.Combine(AppSetting.DataSharedFolder, $"asterisk_{emailUserName}.txt");
                        FindTheLuckThenFuckTheLife.WriteLogForAccount(filePath, emailUserName);
                    }
                    else
                    {
                        MessageBox.Show("Failed to update password !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txt_EmailAddress.Clear();
                        txt_EmailAddress.Enabled = true;
                        txt_OldPassword.Clear();
                        txt_NewPassword.Clear();
                        txt_ConfirmNewPassword.Clear();
                    }
                }
                else
                {
                    MessageBox.Show("Password and Confirm Password do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txt_EmailAddress.Clear();
                    txt_EmailAddress.Enabled = true;
                    txt_OldPassword.Clear();
                    txt_NewPassword.Clear();
                    txt_ConfirmNewPassword.Clear();

                }
            }
            else
            {
                MessageBox.Show("Empty Password is not allowed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txt_EmailAddress.Clear();
                txt_EmailAddress.Enabled = true;
                txt_OldPassword.Clear();
                txt_NewPassword.Clear();
                txt_ConfirmNewPassword.Clear();
            }
        }
    }
}
