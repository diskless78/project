using System.DirectoryServices.ActiveDirectory;
using System.Net;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace mtv_client
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

        }

        private void quitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void registerButton_Click(object sender, EventArgs e)
        {
            bool findResult = await FindTheLuckThenFuckTheLife.FindAccount(txt_EmailAddress.Text.Trim().ToLower());
            if (!findResult)
            {
                // Check if password and confirm password are not empty
                if (!string.IsNullOrWhiteSpace(txt_Password.Text) && !string.IsNullOrWhiteSpace(txt_ConfirmPassword.Text))
                {
                    // Check if password and confirm password match 
                    if (txt_Password.Text == txt_ConfirmPassword.Text)
                    {
                        // Create a list to hold employee details
                        List<Employee> employees = new List<Employee>();
                        string accountName = txt_EmailAddress.Text.Trim().ToLower().Split('@')[0];
                        string newRegisterLogFile = Path.Combine(AppSetting.DataSharedFolder, $"new-register_{accountName}.txt");

                        // Add employee details to the list
                        employees.Add(new Employee(txt_EmailAddress.Text.Trim().ToLower(), txt_Password.Text, txt_Telegram.Text.Trim()));

                        // Write employee details to the file
                        FindTheLuckThenFuckTheLife.WriteUserDetails(employees);
                        MessageBox.Show("Congratulations! You've successfully registered as a member !", "Registration Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        txt_EmailAddress.Clear();
                        txt_Password.Clear();
                        txt_ConfirmPassword.Clear();
                        txt_Telegram.Clear();
                        FindTheLuckThenFuckTheLife.WriteLogForAccount(newRegisterLogFile, accountName);
                    }
                    else
                    {
                        // Display error message if password and confirm password do not match
                        MessageBox.Show("Password and Confirm Password do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txt_Password.Clear();
                        txt_ConfirmPassword.Clear();
                    }
                }
                else
                {
                    // Display error message if password or confirm password is empty
                    MessageBox.Show("Empty Password is not allowed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txt_Password.Clear();
                    txt_ConfirmPassword.Clear();
                }
            }
            else
            {
                MessageBox.Show("This email account already registered please enter the new one !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txt_EmailAddress.Clear();
                txt_Password.Clear();
                txt_ConfirmPassword.Clear();
            }
        }

        private async void txt_EmailAddress_TextChanged(object sender, EventArgs e)
        {
            if (txt_EmailAddress.Text.Trim() != string.Empty)
            {
                bool findResult = await FindTheLuckThenFuckTheLife.FindAccount(txt_EmailAddress.Text.Trim().ToLower());
                if (findResult)
                {
                    MessageBox.Show("This email account already registered, please enter a new one!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txt_EmailAddress.Clear();
                    txt_EmailAddress.Focus();
                }
                else
                {
                    bool isInWhitelist = FindTheLuckThenFuckTheLife.IsEmailInWhitelist(txt_EmailAddress.Text.Trim().ToLower());
                    txt_Telegram.Enabled = isInWhitelist;
                    txt_Password.Enabled = isInWhitelist;
                    txt_ConfirmPassword.Enabled = isInWhitelist;
                    register_Button.Enabled = isInWhitelist;
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (CredentialManager.MapNetworkDrive(AppSetting.DataSharedFolder, AppSetting.UNCUserName, AppSetting.UNCPassword))
            {
                if (FindTheLuckThenFuckTheLife.CheckUUID())
                {
                    string filePath = Path.Combine(AppSetting.DataSharedFolder, AppSetting.UserAccountFile);
                    txt_Password.UseSystemPasswordChar = true;
                    txt_ConfirmPassword.UseSystemPasswordChar = true;
                    txt_Password.Enabled = false;
                    txt_ConfirmPassword.Enabled = false;
                    txt_Telegram.Enabled = false;
                    register_Button.Enabled = false;
                    if (!File.Exists(filePath))
                    {
                        using (FileStream fs = File.Create(filePath)) { }
                    }
                }
                else
                {
                    Application.Exit();
                }
            }
            else
            {
                Application.Exit();
            }
        }

        private void changePasswordToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Hide();

            Change_Password changePasswordForm = new Change_Password();
            changePasswordForm.FormClosed += (s, args) => this.Show();
            changePasswordForm.ShowDialog();
        }

        private void scheduleOfflineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();

            Vacation vacationForm = new Vacation();
            vacationForm.FormClosed += (s, args) => this.Show();
            vacationForm.ShowDialog();
        }

        private void updateAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();

            UpdateAccount updateAccountForm = new UpdateAccount();
            updateAccountForm.FormClosed += (s, args) => this.Show();
            updateAccountForm.ShowDialog();
        }
    }
}
