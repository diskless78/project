namespace mtv_client
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            label1 = new Label();
            txt_EmailAddress = new TextBox();
            txt_Password = new TextBox();
            label2 = new Label();
            txt_ConfirmPassword = new TextBox();
            label3 = new Label();
            register_Button = new Button();
            quit_Button = new Button();
            menuStrip1 = new MenuStrip();
            configToolStripMenuItem = new ToolStripMenuItem();
            changePasswordToolStripMenuItem1 = new ToolStripMenuItem();
            scheduleOfflineToolStripMenuItem = new ToolStripMenuItem();
            updateAccountToolStripMenuItem = new ToolStripMenuItem();
            txt_Telegram = new TextBox();
            label4 = new Label();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 49);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(81, 15);
            label1.TabIndex = 0;
            label1.Text = "Email Address";
            // 
            // txt_EmailAddress
            // 
            txt_EmailAddress.Location = new Point(179, 49);
            txt_EmailAddress.Margin = new Padding(2, 1, 2, 1);
            txt_EmailAddress.Name = "txt_EmailAddress";
            txt_EmailAddress.Size = new Size(217, 23);
            txt_EmailAddress.TabIndex = 1;
            txt_EmailAddress.TextChanged += txt_EmailAddress_TextChanged;
            // 
            // txt_Password
            // 
            txt_Password.Location = new Point(179, 144);
            txt_Password.Margin = new Padding(2, 1, 2, 1);
            txt_Password.Name = "txt_Password";
            txt_Password.Size = new Size(217, 23);
            txt_Password.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(20, 144);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(57, 15);
            label2.TabIndex = 4;
            label2.Text = "Password";
            // 
            // txt_ConfirmPassword
            // 
            txt_ConfirmPassword.Location = new Point(179, 201);
            txt_ConfirmPassword.Margin = new Padding(2, 1, 2, 1);
            txt_ConfirmPassword.Name = "txt_ConfirmPassword";
            txt_ConfirmPassword.Size = new Size(217, 23);
            txt_ConfirmPassword.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 201);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(104, 15);
            label3.TabIndex = 6;
            label3.Text = "Confirm Password";
            // 
            // register_Button
            // 
            register_Button.Location = new Point(20, 243);
            register_Button.Margin = new Padding(2, 1, 2, 1);
            register_Button.Name = "register_Button";
            register_Button.Size = new Size(109, 34);
            register_Button.TabIndex = 5;
            register_Button.Text = "Register";
            register_Button.UseVisualStyleBackColor = true;
            register_Button.Click += registerButton_Click;
            // 
            // quit_Button
            // 
            quit_Button.Location = new Point(286, 243);
            quit_Button.Margin = new Padding(2, 1, 2, 1);
            quit_Button.Name = "quit_Button";
            quit_Button.Size = new Size(109, 34);
            quit_Button.TabIndex = 9;
            quit_Button.Text = "Exit";
            quit_Button.UseVisualStyleBackColor = true;
            quit_Button.Click += quitButton_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(32, 32);
            menuStrip1.Items.AddRange(new ToolStripItem[] { configToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(3, 1, 0, 1);
            menuStrip1.Size = new Size(424, 24);
            menuStrip1.TabIndex = 10;
            menuStrip1.Text = "menuStrip1";
            // 
            // configToolStripMenuItem
            // 
            configToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { changePasswordToolStripMenuItem1, scheduleOfflineToolStripMenuItem, updateAccountToolStripMenuItem });
            configToolStripMenuItem.Name = "configToolStripMenuItem";
            configToolStripMenuItem.Size = new Size(56, 22);
            configToolStripMenuItem.Text = "Setting";
            // 
            // changePasswordToolStripMenuItem1
            // 
            changePasswordToolStripMenuItem1.Name = "changePasswordToolStripMenuItem1";
            changePasswordToolStripMenuItem1.Size = new Size(168, 22);
            changePasswordToolStripMenuItem1.Text = "Change Password";
            changePasswordToolStripMenuItem1.Click += changePasswordToolStripMenuItem1_Click;
            // 
            // scheduleOfflineToolStripMenuItem
            // 
            scheduleOfflineToolStripMenuItem.Name = "scheduleOfflineToolStripMenuItem";
            scheduleOfflineToolStripMenuItem.Size = new Size(168, 22);
            scheduleOfflineToolStripMenuItem.Text = "Schedule Offline";
            scheduleOfflineToolStripMenuItem.Click += scheduleOfflineToolStripMenuItem_Click;
            // 
            // updateAccountToolStripMenuItem
            // 
            updateAccountToolStripMenuItem.Name = "updateAccountToolStripMenuItem";
            updateAccountToolStripMenuItem.Size = new Size(168, 22);
            updateAccountToolStripMenuItem.Text = "Update Account";
            updateAccountToolStripMenuItem.Click += updateAccountToolStripMenuItem_Click;
            // 
            // txt_Telegram
            // 
            txt_Telegram.Location = new Point(179, 97);
            txt_Telegram.Margin = new Padding(2, 1, 2, 1);
            txt_Telegram.Name = "txt_Telegram";
            txt_Telegram.Size = new Size(217, 23);
            txt_Telegram.TabIndex = 2;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(20, 97);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(117, 15);
            label4.TabIndex = 11;
            label4.Text = "Telegram Nick Name";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(424, 284);
            Controls.Add(txt_Telegram);
            Controls.Add(label4);
            Controls.Add(quit_Button);
            Controls.Add(register_Button);
            Controls.Add(txt_ConfirmPassword);
            Controls.Add(label3);
            Controls.Add(txt_Password);
            Controls.Add(label2);
            Controls.Add(txt_EmailAddress);
            Controls.Add(label1);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new Padding(2, 1, 2, 1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Main";
            Text = "Healing Register";
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txt_EmailAddress;
        private TextBox txt_Password;
        private Label label2;
        private TextBox txt_ConfirmPassword;
        private Label label3;
        private Button register_Button;
        private Button quit_Button;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem configToolStripMenuItem;
        private ToolStripMenuItem changePasswordToolStripMenuItem1;
        private ToolStripMenuItem scheduleOfflineToolStripMenuItem;
        private TextBox txt_Telegram;
        private Label label4;
        private ToolStripMenuItem updateAccountToolStripMenuItem;
    }
}
