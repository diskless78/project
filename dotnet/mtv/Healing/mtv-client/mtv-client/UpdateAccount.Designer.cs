namespace mtv_client
{
    partial class UpdateAccount
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateAccount));
            txt_Telegram = new TextBox();
            label4 = new Label();
            txt_Password = new TextBox();
            label2 = new Label();
            txt_EmailAddress = new TextBox();
            label1 = new Label();
            quit_Button = new Button();
            update_Button = new Button();
            SuspendLayout();
            // 
            // txt_Telegram
            // 
            txt_Telegram.Location = new Point(193, 143);
            txt_Telegram.Margin = new Padding(2, 1, 2, 1);
            txt_Telegram.Name = "txt_Telegram";
            txt_Telegram.Size = new Size(217, 23);
            txt_Telegram.TabIndex = 18;
            txt_Telegram.KeyPress += txt_Telegram_KeyPress;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(34, 143);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(117, 15);
            label4.TabIndex = 17;
            label4.Text = "Telegram Nick Name";
            // 
            // txt_Password
            // 
            txt_Password.Location = new Point(193, 88);
            txt_Password.Margin = new Padding(2, 1, 2, 1);
            txt_Password.Name = "txt_Password";
            txt_Password.Size = new Size(217, 23);
            txt_Password.TabIndex = 16;
            txt_Password.TextChanged += txt_Password_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(34, 88);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(57, 15);
            label2.TabIndex = 15;
            label2.Text = "Password";
            // 
            // txt_EmailAddress
            // 
            txt_EmailAddress.Location = new Point(193, 30);
            txt_EmailAddress.Margin = new Padding(2, 1, 2, 1);
            txt_EmailAddress.Name = "txt_EmailAddress";
            txt_EmailAddress.Size = new Size(217, 23);
            txt_EmailAddress.TabIndex = 14;
            txt_EmailAddress.TextChanged += txt_EmailAddress_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(34, 30);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(81, 15);
            label1.TabIndex = 13;
            label1.Text = "Email Address";
            // 
            // quit_Button
            // 
            quit_Button.Location = new Point(301, 200);
            quit_Button.Margin = new Padding(2, 1, 2, 1);
            quit_Button.Name = "quit_Button";
            quit_Button.Size = new Size(109, 34);
            quit_Button.TabIndex = 20;
            quit_Button.Text = "Exit";
            quit_Button.UseVisualStyleBackColor = true;
            quit_Button.Click += quit_Button_Click;
            // 
            // update_Button
            // 
            update_Button.Location = new Point(35, 200);
            update_Button.Margin = new Padding(2, 1, 2, 1);
            update_Button.Name = "update_Button";
            update_Button.Size = new Size(109, 34);
            update_Button.TabIndex = 19;
            update_Button.Text = "Update";
            update_Button.UseVisualStyleBackColor = true;
            update_Button.Click += update_Button_Click;
            // 
            // UpdateAccount
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(453, 239);
            Controls.Add(quit_Button);
            Controls.Add(update_Button);
            Controls.Add(txt_Telegram);
            Controls.Add(label4);
            Controls.Add(txt_Password);
            Controls.Add(label2);
            Controls.Add(txt_EmailAddress);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UpdateAccount";
            Text = "UpdateAccount";
            Load += UpdateAccount_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txt_Telegram;
        private Label label4;
        private TextBox txt_Password;
        private Label label2;
        private TextBox txt_EmailAddress;
        private Label label1;
        private Button quit_Button;
        private Button update_Button;
    }
}