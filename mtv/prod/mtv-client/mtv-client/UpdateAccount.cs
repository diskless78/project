using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mtv_client
{
    public partial class UpdateAccount : Form
    {
        public UpdateAccount()
        {
            InitializeComponent();
        }

        private void quit_Button_Click(object sender, EventArgs e)
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
                    txt_Password.Enabled = true;
                }
                else
                {
                    txt_Password.Enabled = false;
                    txt_Password.Clear();
                    txt_Telegram.Clear();
                }
            }
        }


        private void UpdateAccount_Load(object sender, EventArgs e)
        {
            txt_Password.UseSystemPasswordChar = true;
            txt_Password.Enabled = false;
            txt_Telegram.Enabled = false;
            update_Button.Enabled = false;
        }

        private void txt_Password_TextChanged(object sender, EventArgs e)
        {
            bool getPassword = FindTheLuckThenFuckTheLife.IsPasswordMatched(txt_Password.Text, txt_EmailAddress.Text.Trim().ToLower());
            if (getPassword)
            {
                txt_Telegram.Enabled = true;
                update_Button.Enabled = true;
                txt_EmailAddress.Enabled = false;
                txt_Password.Enabled = false;
            }
            else
            {
                txt_Telegram.Enabled = false;
                update_Button.Enabled = false;
                txt_EmailAddress.Enabled = true;
                txt_Password.Enabled = true;


            }
        }

        private void update_Button_Click(object sender, EventArgs e)
        {
            string telegramNickID = txt_Telegram.Text.Trim();
            DialogResult result = MessageBox.Show("Do you want to update your account ?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                string emailUserName = txt_EmailAddress.Text.Trim().ToLower().Split('@')[0];
                string filePath = Path.Combine(AppSetting.DataSharedFolder, $"telegram_{emailUserName}.txt");
                FindTheLuckThenFuckTheLife.UpdateAccount(txt_EmailAddress.Text.Trim().ToLower(), txt_Password.Text, telegramNickID);
                FindTheLuckThenFuckTheLife.WriteLogForAccount(filePath, emailUserName);
                txt_Password.Clear();
                txt_Password.Enabled = true;
                txt_EmailAddress.Enabled = true;
                txt_EmailAddress.Clear();
                txt_Telegram.Clear();
            }
        }

        private void txt_Telegram_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                // If it's not, handle the event by setting e.Handled to true
                e.Handled = true;

                // Show a message box informing the user
                MessageBox.Show("Telegram chatID is numbers only.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
