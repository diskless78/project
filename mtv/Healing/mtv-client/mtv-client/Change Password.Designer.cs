namespace mtv_client
{
    partial class Change_Password
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Change_Password));
            change_Button = new Button();
            txt_ConfirmNewPassword = new TextBox();
            label3 = new Label();
            txt_NewPassword = new TextBox();
            label2 = new Label();
            txt_EmailAddress = new TextBox();
            label1 = new Label();
            txt_OldPassword = new TextBox();
            label4 = new Label();
            closeButton = new Button();
            SuspendLayout();
            // 
            // change_Button
            // 
            resources.ApplyResources(change_Button, "change_Button");
            change_Button.Name = "change_Button";
            change_Button.UseVisualStyleBackColor = true;
            change_Button.Click += change_Button_Click;
            // 
            // txt_ConfirmNewPassword
            // 
            resources.ApplyResources(txt_ConfirmNewPassword, "txt_ConfirmNewPassword");
            txt_ConfirmNewPassword.Name = "txt_ConfirmNewPassword";
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.Name = "label3";
            // 
            // txt_NewPassword
            // 
            resources.ApplyResources(txt_NewPassword, "txt_NewPassword");
            txt_NewPassword.Name = "txt_NewPassword";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // txt_EmailAddress
            // 
            resources.ApplyResources(txt_EmailAddress, "txt_EmailAddress");
            txt_EmailAddress.Name = "txt_EmailAddress";
            txt_EmailAddress.TextChanged += txt_EmailAddress_TextChanged;
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // txt_OldPassword
            // 
            resources.ApplyResources(txt_OldPassword, "txt_OldPassword");
            txt_OldPassword.Name = "txt_OldPassword";
            txt_OldPassword.TextChanged += txt_OldPassword_TextChanged;
            // 
            // label4
            // 
            resources.ApplyResources(label4, "label4");
            label4.Name = "label4";
            // 
            // closeButton
            // 
            resources.ApplyResources(closeButton, "closeButton");
            closeButton.Name = "closeButton";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // Change_Password
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(closeButton);
            Controls.Add(txt_OldPassword);
            Controls.Add(label4);
            Controls.Add(change_Button);
            Controls.Add(txt_ConfirmNewPassword);
            Controls.Add(label3);
            Controls.Add(txt_NewPassword);
            Controls.Add(label2);
            Controls.Add(txt_EmailAddress);
            Controls.Add(label1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Change_Password";
            Load += Change_Password_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button change_Button;
        private TextBox txt_ConfirmNewPassword;
        private Label label3;
        private TextBox txt_NewPassword;
        private Label label2;
        private TextBox txt_EmailAddress;
        private Label label1;
        private TextBox txt_OldPassword;
        private Label label4;
        private Button closeButton;
    }
}