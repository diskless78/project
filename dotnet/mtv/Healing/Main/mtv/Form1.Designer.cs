namespace mtv
{
    partial class Form1
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle13 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle14 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle8 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle9 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle10 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle11 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle12 = new DataGridViewCellStyle();
            start_Button = new Button();
            quit_Button = new Button();
            dataGridView1 = new DataGridView();
            delete_Button = new Button();
            notifyIcon1 = new NotifyIcon(components);
            timer_MTV = new System.Windows.Forms.Timer(components);
            group_checkin = new GroupBox();
            label1 = new Label();
            dateTimePicker_EndCheckIn = new DateTimePicker();
            dateTimePicker_StartCheckIn = new DateTimePicker();
            checkin_EndTime = new TextBox();
            label3 = new Label();
            checkin_StartTime = new TextBox();
            label2 = new Label();
            groupBox1 = new GroupBox();
            label6 = new Label();
            dateTimePicker_EndCheckOut = new DateTimePicker();
            dateTimePicker_StartCheckOut = new DateTimePicker();
            checkout_EndTime = new TextBox();
            label4 = new Label();
            checkout_StartTime = new TextBox();
            label5 = new Label();
            groupBox2 = new GroupBox();
            notifyBeforeClockTime = new CheckBox();
            registrationNotify = new CheckBox();
            groupBox3 = new GroupBox();
            cmd_saveData = new Button();
            cmd_Vacation = new Button();
            ID = new DataGridViewTextBoxColumn();
            EmailAddress = new DataGridViewTextBoxColumn();
            SessionID = new DataGridViewTextBoxColumn();
            TelegramNickName = new DataGridViewTextBoxColumn();
            chromePort = new DataGridViewTextBoxColumn();
            OffAM = new DataGridViewCheckBoxColumn();
            OffPM = new DataGridViewCheckBoxColumn();
            Weekend = new DataGridViewCheckBoxColumn();
            PublicHolidays = new DataGridViewCheckBoxColumn();
            confirmSelected = new DataGridViewCheckBoxColumn();
            cmd_Login = new DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            group_checkin.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // start_Button
            // 
            start_Button.Location = new Point(5, 8);
            start_Button.Name = "start_Button";
            start_Button.Size = new Size(79, 32);
            start_Button.TabIndex = 0;
            start_Button.Text = "Start";
            start_Button.UseVisualStyleBackColor = true;
            start_Button.Click += startButton_Click;
            // 
            // quit_Button
            // 
            quit_Button.Location = new Point(726, 8);
            quit_Button.Margin = new Padding(2, 1, 2, 1);
            quit_Button.Name = "quit_Button";
            quit_Button.Size = new Size(79, 32);
            quit_Button.TabIndex = 1;
            quit_Button.Text = "Exit";
            quit_Button.UseVisualStyleBackColor = true;
            quit_Button.Click += quitButton_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.ColumnHeadersHeight = 55;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { ID, EmailAddress, SessionID, TelegramNickName, chromePort, OffAM, OffPM, Weekend, PublicHolidays, confirmSelected, cmd_Login });
            dataGridViewCellStyle13.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle13.BackColor = SystemColors.Window;
            dataGridViewCellStyle13.Font = new Font("Segoe UI", 10.125F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle13.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle13.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle13.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle13;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.EditMode = DataGridViewEditMode.EditOnKeystroke;
            dataGridView1.Location = new Point(2, 17);
            dataGridView1.Name = "dataGridView1";
            dataGridViewCellStyle14.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle14.BackColor = SystemColors.Control;
            dataGridViewCellStyle14.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle14.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle14.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle14.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle14.WrapMode = DataGridViewTriState.True;
            dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle14;
            dataGridView1.RowHeadersWidth = 20;
            dataGridView1.Size = new Size(1081, 490);
            dataGridView1.TabIndex = 2;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            dataGridView1.CellMouseMove += dataGridView1_CellMouseMove;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            dataGridView1.CurrentCellDirtyStateChanged += dataGridView1_CurrentCellDirtyStateChanged;
            dataGridView1.MouseLeave += dataGridView1_MouseLeave;
            // 
            // delete_Button
            // 
            delete_Button.Location = new Point(257, 8);
            delete_Button.Name = "delete_Button";
            delete_Button.Size = new Size(79, 32);
            delete_Button.TabIndex = 3;
            delete_Button.Text = "Delete";
            delete_Button.UseVisualStyleBackColor = true;
            delete_Button.Click += deleteButton_Click;
            // 
            // notifyIcon1
            // 
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick_1;
            // 
            // timer_MTV
            // 
            timer_MTV.Tick += timer_MTV_Tick;
            // 
            // group_checkin
            // 
            group_checkin.Controls.Add(label1);
            group_checkin.Controls.Add(dateTimePicker_EndCheckIn);
            group_checkin.Controls.Add(dateTimePicker_StartCheckIn);
            group_checkin.Controls.Add(checkin_EndTime);
            group_checkin.Controls.Add(label3);
            group_checkin.Controls.Add(checkin_StartTime);
            group_checkin.Controls.Add(label2);
            group_checkin.Location = new Point(5, 570);
            group_checkin.Name = "group_checkin";
            group_checkin.Size = new Size(260, 97);
            group_checkin.TabIndex = 6;
            group_checkin.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe Script", 11.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Teal;
            label1.Location = new Point(51, -6);
            label1.Name = "label1";
            label1.Size = new Size(136, 25);
            label1.TabIndex = 13;
            label1.Text = "Rise and Shine";
            // 
            // dateTimePicker_EndCheckIn
            // 
            dateTimePicker_EndCheckIn.CustomFormat = "\"hh:mm tt\"";
            dateTimePicker_EndCheckIn.Format = DateTimePickerFormat.Custom;
            dateTimePicker_EndCheckIn.Location = new Point(165, 62);
            dateTimePicker_EndCheckIn.Name = "dateTimePicker_EndCheckIn";
            dateTimePicker_EndCheckIn.Size = new Size(89, 23);
            dateTimePicker_EndCheckIn.TabIndex = 10;
            dateTimePicker_EndCheckIn.ValueChanged += dateTimePicker_EndCheckIn_ValueChanged;
            // 
            // dateTimePicker_StartCheckIn
            // 
            dateTimePicker_StartCheckIn.CustomFormat = "\"hh:mm tt\"";
            dateTimePicker_StartCheckIn.Format = DateTimePickerFormat.Custom;
            dateTimePicker_StartCheckIn.Location = new Point(165, 26);
            dateTimePicker_StartCheckIn.Name = "dateTimePicker_StartCheckIn";
            dateTimePicker_StartCheckIn.Size = new Size(89, 23);
            dateTimePicker_StartCheckIn.TabIndex = 9;
            dateTimePicker_StartCheckIn.ValueChanged += dateTimePicker_StartCheckIn_ValueChanged;
            // 
            // checkin_EndTime
            // 
            checkin_EndTime.BackColor = SystemColors.GrayText;
            checkin_EndTime.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            checkin_EndTime.ForeColor = Color.White;
            checkin_EndTime.Location = new Point(73, 62);
            checkin_EndTime.Name = "checkin_EndTime";
            checkin_EndTime.ReadOnly = true;
            checkin_EndTime.Size = new Size(85, 23);
            checkin_EndTime.TabIndex = 3;
            checkin_EndTime.TextAlign = HorizontalAlignment.Center;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(4, 65);
            label3.Name = "label3";
            label3.Size = new Size(58, 15);
            label3.TabIndex = 2;
            label3.Text = "End Time";
            // 
            // checkin_StartTime
            // 
            checkin_StartTime.BackColor = SystemColors.GrayText;
            checkin_StartTime.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            checkin_StartTime.ForeColor = Color.White;
            checkin_StartTime.Location = new Point(73, 26);
            checkin_StartTime.Name = "checkin_StartTime";
            checkin_StartTime.ReadOnly = true;
            checkin_StartTime.Size = new Size(86, 23);
            checkin_StartTime.TabIndex = 1;
            checkin_StartTime.TextAlign = HorizontalAlignment.Center;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(3, 29);
            label2.Name = "label2";
            label2.Size = new Size(66, 15);
            label2.TabIndex = 0;
            label2.Text = "Start Time";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(dateTimePicker_EndCheckOut);
            groupBox1.Controls.Add(dateTimePicker_StartCheckOut);
            groupBox1.Controls.Add(checkout_EndTime);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(checkout_StartTime);
            groupBox1.Controls.Add(label5);
            groupBox1.Location = new Point(551, 570);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(260, 97);
            groupBox1.TabIndex = 11;
            groupBox1.TabStop = false;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe Script", 11.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label6.ForeColor = Color.Teal;
            label6.Location = new Point(53, -6);
            label6.Name = "label6";
            label6.Size = new Size(135, 25);
            label6.TabIndex = 12;
            label6.Text = "Settle and Dim";
            // 
            // dateTimePicker_EndCheckOut
            // 
            dateTimePicker_EndCheckOut.CustomFormat = "\"hh:mm tt\"";
            dateTimePicker_EndCheckOut.Format = DateTimePickerFormat.Custom;
            dateTimePicker_EndCheckOut.Location = new Point(165, 62);
            dateTimePicker_EndCheckOut.Name = "dateTimePicker_EndCheckOut";
            dateTimePicker_EndCheckOut.Size = new Size(89, 23);
            dateTimePicker_EndCheckOut.TabIndex = 10;
            dateTimePicker_EndCheckOut.ValueChanged += dateTimePicker_EndCheckOut_ValueChanged;
            // 
            // dateTimePicker_StartCheckOut
            // 
            dateTimePicker_StartCheckOut.CustomFormat = "\"hh:mm tt\"";
            dateTimePicker_StartCheckOut.Format = DateTimePickerFormat.Custom;
            dateTimePicker_StartCheckOut.Location = new Point(165, 26);
            dateTimePicker_StartCheckOut.Name = "dateTimePicker_StartCheckOut";
            dateTimePicker_StartCheckOut.Size = new Size(89, 23);
            dateTimePicker_StartCheckOut.TabIndex = 9;
            dateTimePicker_StartCheckOut.ValueChanged += dateTimePicker_StartCheckOut_ValueChanged;
            // 
            // checkout_EndTime
            // 
            checkout_EndTime.BackColor = SystemColors.GrayText;
            checkout_EndTime.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            checkout_EndTime.ForeColor = Color.White;
            checkout_EndTime.Location = new Point(73, 62);
            checkout_EndTime.Name = "checkout_EndTime";
            checkout_EndTime.ReadOnly = true;
            checkout_EndTime.Size = new Size(85, 23);
            checkout_EndTime.TabIndex = 3;
            checkout_EndTime.TextAlign = HorizontalAlignment.Center;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(4, 65);
            label4.Name = "label4";
            label4.Size = new Size(58, 15);
            label4.TabIndex = 2;
            label4.Text = "End Time";
            // 
            // checkout_StartTime
            // 
            checkout_StartTime.BackColor = SystemColors.GrayText;
            checkout_StartTime.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            checkout_StartTime.ForeColor = Color.White;
            checkout_StartTime.Location = new Point(73, 26);
            checkout_StartTime.Name = "checkout_StartTime";
            checkout_StartTime.ReadOnly = true;
            checkout_StartTime.Size = new Size(86, 23);
            checkout_StartTime.TabIndex = 1;
            checkout_StartTime.TextAlign = HorizontalAlignment.Center;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(3, 29);
            label5.Name = "label5";
            label5.Size = new Size(66, 15);
            label5.TabIndex = 0;
            label5.Text = "Start Time";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(notifyBeforeClockTime);
            groupBox2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox2.ForeColor = SystemColors.ControlText;
            groupBox2.Location = new Point(330, 570);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(161, 97);
            groupBox2.TabIndex = 12;
            groupBox2.TabStop = false;
            // 
            // notifyBeforeClockTime
            // 
            notifyBeforeClockTime.AutoSize = true;
            notifyBeforeClockTime.Location = new Point(17, 45);
            notifyBeforeClockTime.Name = "notifyBeforeClockTime";
            notifyBeforeClockTime.Size = new Size(125, 19);
            notifyBeforeClockTime.TabIndex = 16;
            notifyBeforeClockTime.Text = "Notify New Event";
            notifyBeforeClockTime.UseVisualStyleBackColor = true;
            // 
            // registrationNotify
            // 
            registrationNotify.AutoSize = true;
            registrationNotify.Enabled = false;
            registrationNotify.Location = new Point(965, 648);
            registrationNotify.Name = "registrationNotify";
            registrationNotify.Size = new Size(125, 19);
            registrationNotify.TabIndex = 14;
            registrationNotify.Text = "Notify Registration";
            registrationNotify.UseVisualStyleBackColor = true;
            registrationNotify.Visible = false;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(dataGridView1);
            groupBox3.Location = new Point(7, 40);
            groupBox3.Margin = new Padding(2, 1, 2, 1);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(2, 1, 2, 1);
            groupBox3.Size = new Size(1085, 508);
            groupBox3.TabIndex = 13;
            groupBox3.TabStop = false;
            // 
            // cmd_saveData
            // 
            cmd_saveData.Location = new Point(510, 8);
            cmd_saveData.Name = "cmd_saveData";
            cmd_saveData.Size = new Size(79, 32);
            cmd_saveData.TabIndex = 14;
            cmd_saveData.Text = "Save";
            cmd_saveData.UseVisualStyleBackColor = true;
            cmd_saveData.Click += cmd_saveData_Click;
            // 
            // cmd_Vacation
            // 
            cmd_Vacation.Location = new Point(975, 599);
            cmd_Vacation.Name = "cmd_Vacation";
            cmd_Vacation.Size = new Size(106, 41);
            cmd_Vacation.TabIndex = 15;
            cmd_Vacation.Text = "Book Vacation";
            cmd_Vacation.UseVisualStyleBackColor = true;
            cmd_Vacation.Click += cmd_Vacation_Click;
            // 
            // ID
            // 
            ID.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = Color.Black;
            ID.DefaultCellStyle = dataGridViewCellStyle2;
            ID.FillWeight = 63.4517822F;
            ID.HeaderText = "Id";
            ID.MinimumWidth = 10;
            ID.Name = "ID";
            ID.Resizable = DataGridViewTriState.True;
            ID.Width = 42;
            // 
            // EmailAddress
            // 
            EmailAddress.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle3.ForeColor = Color.Black;
            EmailAddress.DefaultCellStyle = dataGridViewCellStyle3;
            EmailAddress.FillWeight = 226.009186F;
            EmailAddress.HeaderText = "Email Address";
            EmailAddress.MinimumWidth = 10;
            EmailAddress.Name = "EmailAddress";
            EmailAddress.Resizable = DataGridViewTriState.False;
            EmailAddress.Width = 260;
            // 
            // SessionID
            // 
            SessionID.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle4.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            SessionID.DefaultCellStyle = dataGridViewCellStyle4;
            SessionID.HeaderText = "SessionID";
            SessionID.MinimumWidth = 10;
            SessionID.Name = "SessionID";
            SessionID.ReadOnly = true;
            SessionID.Resizable = DataGridViewTriState.False;
            SessionID.Width = 250;
            // 
            // TelegramNickName
            // 
            TelegramNickName.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle5.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            TelegramNickName.DefaultCellStyle = dataGridViewCellStyle5;
            TelegramNickName.FillWeight = 113.004593F;
            TelegramNickName.HeaderText = "Telegram";
            TelegramNickName.MinimumWidth = 10;
            TelegramNickName.Name = "TelegramNickName";
            TelegramNickName.Resizable = DataGridViewTriState.False;
            TelegramNickName.Width = 140;
            // 
            // chromePort
            // 
            chromePort.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle6.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chromePort.DefaultCellStyle = dataGridViewCellStyle6;
            chromePort.HeaderText = "Port";
            chromePort.Name = "chromePort";
            chromePort.Resizable = DataGridViewTriState.False;
            chromePort.Visible = false;
            chromePort.Width = 80;
            // 
            // OffAM
            // 
            OffAM.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle7.NullValue = false;
            OffAM.DefaultCellStyle = dataGridViewCellStyle7;
            OffAM.FillWeight = 226.009186F;
            OffAM.HeaderText = "Off-AM";
            OffAM.MinimumWidth = 10;
            OffAM.Name = "OffAM";
            OffAM.ReadOnly = true;
            OffAM.Resizable = DataGridViewTriState.False;
            OffAM.SortMode = DataGridViewColumnSortMode.Automatic;
            OffAM.Width = 50;
            // 
            // OffPM
            // 
            OffPM.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle8.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle8.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle8.NullValue = false;
            OffPM.DefaultCellStyle = dataGridViewCellStyle8;
            OffPM.FillWeight = 226.009186F;
            OffPM.HeaderText = "Off-PM";
            OffPM.MinimumWidth = 10;
            OffPM.Name = "OffPM";
            OffPM.ReadOnly = true;
            OffPM.Resizable = DataGridViewTriState.False;
            OffPM.SortMode = DataGridViewColumnSortMode.Automatic;
            OffPM.Width = 50;
            // 
            // Weekend
            // 
            Weekend.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle9.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle9.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle9.NullValue = false;
            Weekend.DefaultCellStyle = dataGridViewCellStyle9;
            Weekend.HeaderText = "Weekend";
            Weekend.MinimumWidth = 10;
            Weekend.Name = "Weekend";
            Weekend.ReadOnly = true;
            Weekend.Resizable = DataGridViewTriState.False;
            Weekend.SortMode = DataGridViewColumnSortMode.Automatic;
            Weekend.Width = 60;
            // 
            // PublicHolidays
            // 
            PublicHolidays.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle10.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle10.NullValue = false;
            dataGridViewCellStyle10.WrapMode = DataGridViewTriState.True;
            PublicHolidays.DefaultCellStyle = dataGridViewCellStyle10;
            PublicHolidays.FillWeight = 169.506882F;
            PublicHolidays.HeaderText = "Public Holiday";
            PublicHolidays.MinimumWidth = 10;
            PublicHolidays.Name = "PublicHolidays";
            PublicHolidays.ReadOnly = true;
            PublicHolidays.Resizable = DataGridViewTriState.False;
            PublicHolidays.SortMode = DataGridViewColumnSortMode.Automatic;
            PublicHolidays.Width = 60;
            // 
            // confirmSelected
            // 
            confirmSelected.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle11.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle11.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle11.NullValue = false;
            confirmSelected.DefaultCellStyle = dataGridViewCellStyle11;
            confirmSelected.FillWeight = 226.009186F;
            confirmSelected.HeaderText = "Select ?";
            confirmSelected.MinimumWidth = 10;
            confirmSelected.Name = "confirmSelected";
            confirmSelected.Resizable = DataGridViewTriState.False;
            confirmSelected.Width = 50;
            // 
            // cmd_Login
            // 
            cmd_Login.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle12.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle12.BackColor = Color.White;
            cmd_Login.DefaultCellStyle = dataGridViewCellStyle12;
            cmd_Login.FlatStyle = FlatStyle.System;
            cmd_Login.HeaderText = "Sign-In";
            cmd_Login.Name = "cmd_Login";
            cmd_Login.Resizable = DataGridViewTriState.False;
            cmd_Login.Text = "";
            cmd_Login.ToolTipText = "Login";
            cmd_Login.Width = 95;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(1088, 669);
            Controls.Add(cmd_Vacation);
            Controls.Add(registrationNotify);
            Controls.Add(cmd_saveData);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(group_checkin);
            Controls.Add(delete_Button);
            Controls.Add(quit_Button);
            Controls.Add(start_Button);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "Healing";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            Resize += Form1_Resize;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            group_checkin.ResumeLayout(false);
            group_checkin.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button start_Button;
        private Button quit_Button;
        private DataGridView dataGridView1;
        private Button delete_Button;
        private NotifyIcon notifyIcon1;
        private System.Windows.Forms.Timer timer_MTV;
        private GroupBox group_checkin;
        private Label label2;
        private TextBox checkin_StartTime;
        private TextBox checkin_EndTime;
        private Label label3;
        private DateTimePicker dateTimePicker_StartCheckIn;
        private DateTimePicker dateTimePicker_EndCheckIn;
        private GroupBox groupBox1;
        private Label label6;
        private DateTimePicker dateTimePicker_EndCheckOut;
        private DateTimePicker dateTimePicker_StartCheckOut;
        private TextBox checkout_EndTime;
        private Label label4;
        private TextBox checkout_StartTime;
        private Label label5;
        private GroupBox groupBox2;
        private CheckBox registrationNotify;
        private CheckBox notifyBeforeClockTime;
        private Label label1;
        private GroupBox groupBox3;
        private Button cmd_saveData;
        private Button cmd_Vacation;
        private DataGridViewTextBoxColumn ID;
        private DataGridViewTextBoxColumn EmailAddress;
        private DataGridViewTextBoxColumn SessionID;
        private DataGridViewTextBoxColumn TelegramNickName;
        private DataGridViewTextBoxColumn chromePort;
        private DataGridViewCheckBoxColumn OffAM;
        private DataGridViewCheckBoxColumn OffPM;
        private DataGridViewCheckBoxColumn Weekend;
        private DataGridViewCheckBoxColumn PublicHolidays;
        private DataGridViewCheckBoxColumn confirmSelected;
        private DataGridViewButtonColumn cmd_Login;
    }
}
