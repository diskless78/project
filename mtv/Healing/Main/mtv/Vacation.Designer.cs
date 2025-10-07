namespace mtv_client
{
    partial class Vacation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Vacation));
            txt_EmailAddress = new TextBox();
            label1 = new Label();
            timeOff = new ComboBox();
            dateTimeOffPicker = new DateTimePicker();
            listDayOff = new ListBox();
            addDayOff = new Button();
            saveDateOff = new Button();
            closeButton = new Button();
            label2 = new Label();
            removeDayOff = new Button();
            SuspendLayout();
            // 
            // txt_EmailAddress
            // 
            txt_EmailAddress.Location = new Point(142, 22);
            txt_EmailAddress.Margin = new Padding(2, 1, 2, 1);
            txt_EmailAddress.Name = "txt_EmailAddress";
            txt_EmailAddress.ReadOnly = true;
            txt_EmailAddress.Size = new Size(217, 23);
            txt_EmailAddress.TabIndex = 17;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ImeMode = ImeMode.NoControl;
            label1.Location = new Point(20, 22);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(81, 15);
            label1.TabIndex = 19;
            label1.Text = "Email Address";
            // 
            // timeOff
            // 
            timeOff.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            timeOff.FormattingEnabled = true;
            timeOff.Location = new Point(245, 131);
            timeOff.Margin = new Padding(2, 1, 2, 1);
            timeOff.Name = "timeOff";
            timeOff.Size = new Size(57, 25);
            timeOff.TabIndex = 21;
            timeOff.KeyPress += timeOff_KeyPress;
            // 
            // dateTimeOffPicker
            // 
            dateTimeOffPicker.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dateTimeOffPicker.Location = new Point(21, 131);
            dateTimeOffPicker.Margin = new Padding(2, 1, 2, 1);
            dateTimeOffPicker.Name = "dateTimeOffPicker";
            dateTimeOffPicker.Size = new Size(217, 25);
            dateTimeOffPicker.TabIndex = 22;
            dateTimeOffPicker.ValueChanged += dateTimeOffPicker_ValueChanged;
            // 
            // listDayOff
            // 
            listDayOff.FormattingEnabled = true;
            listDayOff.ItemHeight = 15;
            listDayOff.Location = new Point(438, 6);
            listDayOff.Margin = new Padding(2, 1, 2, 1);
            listDayOff.Name = "listDayOff";
            listDayOff.Size = new Size(173, 199);
            listDayOff.TabIndex = 23;
            listDayOff.SelectedIndexChanged += listDayOff_SelectedIndexChanged;
            // 
            // addDayOff
            // 
            addDayOff.Location = new Point(334, 109);
            addDayOff.Margin = new Padding(2, 1, 2, 1);
            addDayOff.Name = "addDayOff";
            addDayOff.Size = new Size(60, 29);
            addDayOff.TabIndex = 24;
            addDayOff.Text = "+DayOff";
            addDayOff.UseVisualStyleBackColor = true;
            addDayOff.Click += addDayOff_Click;
            // 
            // saveDateOff
            // 
            saveDateOff.Location = new Point(23, 230);
            saveDateOff.Margin = new Padding(2, 1, 2, 1);
            saveDateOff.Name = "saveDateOff";
            saveDateOff.Size = new Size(93, 40);
            saveDateOff.TabIndex = 25;
            saveDateOff.Text = "Save DayOff";
            saveDateOff.UseVisualStyleBackColor = true;
            saveDateOff.Click += saveDateOff_Click;
            // 
            // closeButton
            // 
            closeButton.Location = new Point(511, 230);
            closeButton.Margin = new Padding(2, 1, 2, 1);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(93, 40);
            closeButton.TabIndex = 26;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.Red;
            label2.Location = new Point(18, 203);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(358, 15);
            label2.TabIndex = 27;
            label2.Text = "Date will be in format DD-MM-YYYY ( Example 31-01-2024)";
            // 
            // removeDayOff
            // 
            removeDayOff.Location = new Point(334, 156);
            removeDayOff.Margin = new Padding(2, 1, 2, 1);
            removeDayOff.Name = "removeDayOff";
            removeDayOff.Size = new Size(60, 29);
            removeDayOff.TabIndex = 28;
            removeDayOff.Text = "-DayOff";
            removeDayOff.UseVisualStyleBackColor = true;
            removeDayOff.Click += removeDayOff_Click_1;
            // 
            // Vacation
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(629, 274);
            Controls.Add(removeDayOff);
            Controls.Add(label2);
            Controls.Add(closeButton);
            Controls.Add(saveDateOff);
            Controls.Add(addDayOff);
            Controls.Add(listDayOff);
            Controls.Add(dateTimeOffPicker);
            Controls.Add(timeOff);
            Controls.Add(txt_EmailAddress);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2, 1, 2, 1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Vacation";
            Text = "Vacation";
            Load += Vacation_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private ComboBox timeOff;
        private DateTimePicker dateTimeOffPicker;
        private ListBox listDayOff;
        private Button addDayOff;
        private Button saveDateOff;
        private Button closeButton;
        private Label label2;
        private Button removeDayOff;
        public TextBox txt_EmailAddress;
    }
}