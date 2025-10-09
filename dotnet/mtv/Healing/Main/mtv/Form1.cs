using Microsoft.Extensions.Configuration;
using mtv_client;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; 
using System;
using System.Configuration;
using System.Globalization;
using System.Net.Mail;
using System.Windows.Forms;
using System.Windows.Forms;
using static FuckTheLifeToFindTheLuck;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace mtv
{

    public partial class Form1 : Form
    {
        private static bool isVacationListenerRunning = false;
        private TelegramBotService telegramBotService;

        private int hoveredRow = -1;
        private int hoveredColumn = -1;
        private Font? boldFont;

        HashSet<int> usedPorts = new HashSet<int>();
        int basePort = 9210;
        int maxPort = 9299;
        int countPersonExpired = 0;

        private int tickCount = 0;
        private readonly mtv.ConfigurationSettings configSettings;
        private DateTime previousDate = DateTime.Now.Date;
        private bool isLogTimeDone = false;
        private bool notifyBeforeCheckIn = false;
        private bool notifyBeforeCheckOut = false;
        private bool notifyDaily = false;
        private bool checkSessionExiredDone = false; // Set = True for testing
        private FileSystemWatcher? watcher;
        public BookTime bookTime = new BookTime();
        public Holidays publicHoliday = new Holidays();
        private HashSet<int> loggedRowsCheckOut = new HashSet<int>();
        private HashSet<int> loggedRowsCheckIn = new HashSet<int>();


        public Form1()
        {
            InitializeComponent();
            boldFont = new Font(dataGridView1.Font, FontStyle.Bold);
            telegramBotService = new TelegramBotService();

            dataGridView1.CellMouseMove += dataGridView1_CellMouseMove;
            dataGridView1.MouseLeave += dataGridView1_MouseLeave;
            this.FormClosing += Form1_FormClosing;

            var configuration = mtv.ConfigurationHelper.BuildConfiguration();
            configSettings = new mtv.ConfigurationSettings(configuration);

            try
            {
                SetupWatcher();
                notifyIcon1.Visible = true;
                notifyIcon1.Text = "Healing application is running in the background";

            }
            catch (Exception ex)
            {
                FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
            }

        }
        private void SetupWatcher()
        {
            watcher = new FileSystemWatcher();
            watcher.Path = configSettings.RootDataPath;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";

            // Add event handlers
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;

            // Enable the watcher
            watcher.EnableRaisingEvents = true;
        }

        private async void OnChanged(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(10000);
            try
            {
                LoadDataAndConfiguration();
                if (registrationNotify.Checked)
                {
                    TelegramBotService telegramBotService = new TelegramBotService();
                    List<Employee> employees = FuckTheLifeToFindTheLuck.ReadUserDetails(configSettings.RootDataPath + @"\" + configSettings.UserAccountFile);

                    foreach (Employee employee in employees)
                    {
                        string userName = employee.personName.Split('@')[0];
                        string userVacationFile = Path.Combine(configSettings.RootDataPath, $"vacation_{userName}.txt");

                        //    FormScreenshot.CaptureRegistrationList(this, appScreenshot);
                        //    await telegramBotService.SendMsgTelegram(true, appScreenshot, userName, "Congratulations! You've successfully registered ", "all");
                        //    await telegramBotService.SendMsgTelegram(false, Directory.GetCurrentDirectory() + @"\WelcomeNewMember.png", userName, "Congratulations! You've successfully registered ", employee.telegramID);
                        //    File.Delete(appScreenshot);
                        //    File.Delete(tempFilePath);
                        //}

                        if (File.Exists(userVacationFile))
                        {
                            string vacationDataFile = AppConst.vacationFolder + @"\vacation_" + userName + ".txt";
                            if (File.Exists(vacationDataFile))
                            {
                                File.Delete(vacationDataFile);
                            }
                            File.Move(userVacationFile, vacationDataFile);

                            string vacationMsg = string.Empty;
                            string[] lines = File.ReadAllLines(vacationDataFile);
                            DateTime today = DateTime.Today;

                            foreach (string line in lines)
                            {
                                string[] parts = line.Split(',');

                                if (parts.Length == 2)
                                {
                                    string dateStr = parts[0].Trim();
                                    string descriptor = parts[1].Trim();

                                    DateTime date;
                                    if (DateTime.TryParseExact(dateStr, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                                    {
                                        if (date >= today)
                                        {
                                            vacationMsg += dateStr + " Party " + descriptor + " Time" + Environment.NewLine;
                                        }
                                    }
                                }
                            }

                            await telegramBotService.SendMsgTelegram(false, Directory.GetCurrentDirectory() + @"\VacationSet.png", userName, "Time to enjoy your life" + Environment.NewLine + vacationMsg, employee.telegramID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FuckTheLifeToFindTheLuck.LogMessage(ex.ToString(), AppConst.findtheluckLogFile);
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (start_Button.Text == "Start")
            {
                (bookTime.StartCheckInHour, bookTime.StartCheckInMinute) = FuckTheLifeToFindTheLuck.GetHourAndMinute(checkin_StartTime.Text);
                (bookTime.EndCheckInHour, bookTime.EndCheckInMinute) = FuckTheLifeToFindTheLuck.GetHourAndMinute(checkin_EndTime.Text);

                (bookTime.StartCheckOutHour, bookTime.StartCheckOutMinute) = FuckTheLifeToFindTheLuck.GetHourAndMinute(checkout_StartTime.Text);
                (bookTime.EndCheckOutHour, bookTime.EndCheckOutMinute) = FuckTheLifeToFindTheLuck.GetHourAndMinute(checkout_EndTime.Text);

                start_Button.Text = "Stop";
                dataGridView1.ReadOnly = false;
                delete_Button.Enabled = false;
                quit_Button.Enabled = false;
                cmd_saveData.Enabled = false;
                notifyBeforeClockTime.Enabled = false;
                registrationNotify.Enabled = false;
                timer_MTV.Interval = 1000;
                tickCount = 0;
                timer_MTV.Start();

                dateTimePicker_StartCheckIn.Enabled = false;
                dateTimePicker_EndCheckIn.Enabled = false;
                dateTimePicker_StartCheckOut.Enabled = false;
                dateTimePicker_EndCheckOut.Enabled = false;

                string json = JsonConvert.SerializeObject(bookTime);
                File.WriteAllText(configSettings.RootDataPath + @"\" + AppConst.timerFile, json);
                this.Hide();
            }
            else if (start_Button.Text == "Stop")
            {
                this.Show();
                start_Button.Text = "Start";
                dataGridView1.ReadOnly = false;
                start_Button.Enabled = true;
                delete_Button.Enabled = true;
                quit_Button.Enabled = true;
                cmd_saveData.Enabled = true;
                notifyBeforeClockTime.Enabled = true;
                registrationNotify.Enabled = true;
                timer_MTV.Stop();
                dateTimePicker_StartCheckIn.Enabled = true;
                dateTimePicker_EndCheckIn.Enabled = true;
                dateTimePicker_StartCheckOut.Enabled = true;
                dateTimePicker_EndCheckOut.Enabled = true;

            }

        }

        private void LoadDataAndConfiguration()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(LoadDataAndConfiguration));
                return;
            }

            if (start_Button.Text == "Start")
            {
                dateTimePicker_StartCheckIn.ShowUpDown = true;
                dateTimePicker_EndCheckIn.ShowUpDown = true;
                dateTimePicker_StartCheckOut.ShowUpDown = true;
                dateTimePicker_EndCheckOut.ShowUpDown = true;

                dateTimePicker_StartCheckIn.Enabled = true;
                dateTimePicker_EndCheckIn.Enabled = true;
                dateTimePicker_StartCheckOut.Enabled = true;
                dateTimePicker_EndCheckOut.Enabled = true;
            }

            if (start_Button.Text == "Stop")
            {
                dateTimePicker_StartCheckIn.ShowUpDown = false;
                dateTimePicker_EndCheckIn.ShowUpDown = false;
                dateTimePicker_StartCheckOut.ShowUpDown = false;
                dateTimePicker_EndCheckOut.ShowUpDown = false;

                dateTimePicker_StartCheckIn.Enabled = false;
                dateTimePicker_EndCheckIn.Enabled = false;
                dateTimePicker_StartCheckOut.Enabled = false;
                dateTimePicker_EndCheckOut.Enabled = false;
            }

            if (!Directory.Exists(AppConst.findtheluckLogDirectory))
                Directory.CreateDirectory(AppConst.findtheluckLogDirectory);

            dataGridView1.Rows.Clear();
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;


            if (File.Exists(configSettings.RootDataPath + @"\" + AppConst.timerFile))
            {
                string json = File.ReadAllText(configSettings.RootDataPath + @"\" + AppConst.timerFile);
                BookTime loadTimer = JsonConvert.DeserializeObject<BookTime>(json);

                dateTimePicker_StartCheckIn.Value = DateTime.Today.Add(new TimeSpan(loadTimer.StartCheckInHour, loadTimer.StartCheckInMinute, 0));
                dateTimePicker_EndCheckIn.Value = DateTime.Today.Add(new TimeSpan(loadTimer.EndCheckInHour, loadTimer.EndCheckInMinute, 0));
                dateTimePicker_StartCheckOut.Value = DateTime.Today.Add(new TimeSpan(loadTimer.StartCheckOutHour, loadTimer.StartCheckOutMinute, 0));
                dateTimePicker_EndCheckOut.Value = DateTime.Today.Add(new TimeSpan(loadTimer.EndCheckOutHour, loadTimer.EndCheckOutMinute, 0));

            }
            else
            {
                dateTimePicker_StartCheckIn.Value = DateTime.Today.Add(new TimeSpan(8, 00, 0));
                dateTimePicker_EndCheckIn.Value = DateTime.Today.Add(new TimeSpan(8, 30, 0));
                dateTimePicker_StartCheckOut.Value = DateTime.Today.Add(new TimeSpan(18, 00, 0));
                dateTimePicker_EndCheckOut.Value = DateTime.Today.Add(new TimeSpan(19, 00, 0));
            }

            checkin_StartTime.Text = dateTimePicker_StartCheckIn.Value.ToString("hh:mm tt");
            checkin_EndTime.Text = dateTimePicker_EndCheckIn.Value.ToString("hh:mm tt");
            checkout_StartTime.Text = dateTimePicker_StartCheckOut.Value.ToString("hh:mm tt");
            checkout_EndTime.Text = dateTimePicker_EndCheckOut.Value.ToString("hh:mm tt");

            List<Employee> employees = FuckTheLifeToFindTheLuck.ReadUserDetails(configSettings.RootDataPath + @"\" + configSettings.UserAccountFile);
            int IdNum = 1;
            publicHoliday.PublicHolidayVN = FuckTheLifeToFindTheLuck.CheckPublicHoliday(configSettings.RootDataPath + @"\" + configSettings.PublicHolidaysFile);
            bool weekendHoliday = FuckTheLifeToFindTheLuck.IsWeekend(DateTime.Today);
            foreach (var emp in employees)
            {
                string userVacationFile = AppConst.vacationFolder + @"\" + "vacation_" + emp.personName.Split('@')[0] + ".txt";
                if (File.Exists(userVacationFile))
                {
                    (bool isDayOff, string timeOff) = FuckTheLifeToFindTheLuck.CheckDayOff(userVacationFile);
                    if (isDayOff)
                    {
                        bool morningOff = false; bool noonOff = false;
                        if (timeOff == AppConst.amTime)
                        {
                            morningOff = true;
                        }
                        if (timeOff == AppConst.pmTime)
                        {
                            noonOff = true;
                        }
                        if (timeOff == AppConst.fullDay)
                        {
                            morningOff = true; noonOff = true;

                        }
                        dataGridView1.Rows.Add(IdNum, emp.personName, emp.sessionID, emp.telegramID, emp.chromePort, morningOff, noonOff, weekendHoliday, publicHoliday.PublicHolidayVN, false);
                    }
                    else
                    {
                        dataGridView1.Rows.Add(IdNum, emp.personName, emp.sessionID, emp.telegramID, emp.chromePort, false, false, weekendHoliday, publicHoliday.PublicHolidayVN, false);
                    }
                }
                else
                {

                    dataGridView1.Rows.Add(IdNum, emp.personName, emp.sessionID, emp.telegramID, emp.chromePort, false, false, weekendHoliday, publicHoliday.PublicHolidayVN, false);
                }

                IdNum++;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!isVacationListenerRunning)
            {
                isVacationListenerRunning = true;
                _ = Task.Run(() => telegramBotService.ListenForTelegramAsync());
            }

            if (FuckTheLifeToFindTheLuck.DoYouSeeMe())
            {
                registrationNotify.Checked = true;
                notifyBeforeClockTime.Checked = true;
                LoadDataAndConfiguration();

                (bookTime.StartCheckInHour, bookTime.StartCheckInMinute) = FuckTheLifeToFindTheLuck.GetHourAndMinute(checkin_StartTime.Text);
                (bookTime.EndCheckInHour, bookTime.EndCheckInMinute) = FuckTheLifeToFindTheLuck.GetHourAndMinute(checkin_EndTime.Text);

                (bookTime.StartCheckOutHour, bookTime.StartCheckOutMinute) = FuckTheLifeToFindTheLuck.GetHourAndMinute(checkout_StartTime.Text);
                (bookTime.EndCheckOutHour, bookTime.EndCheckOutMinute) = FuckTheLifeToFindTheLuck.GetHourAndMinute(checkout_EndTime.Text);

                start_Button.Text = "Stop";
                dataGridView1.ReadOnly = false;
                delete_Button.Enabled = false;
                quit_Button.Enabled = false;
                cmd_saveData.Enabled = false;
                notifyBeforeClockTime.Enabled = false;
                registrationNotify.Enabled = false;
                timer_MTV.Interval = 1000;
                tickCount = 0;
                timer_MTV.Start();

                dateTimePicker_StartCheckIn.Enabled = false;
                dateTimePicker_EndCheckIn.Enabled = false;
                dateTimePicker_StartCheckOut.Enabled = false;
                dateTimePicker_EndCheckOut.Enabled = false;

                string json = JsonConvert.SerializeObject(bookTime);
                File.WriteAllText(configSettings.RootDataPath + @"\" + AppConst.timerFile, json);
                BeginInvoke(new Action(() => this.Hide()));
            }
            else
            {
                Application.Exit();
            }
        }

        private void quitButton_Click(object sender, EventArgs e)
        {
            //List<int> indices = Enumerable.Range(0, dataGridView1.Rows.Count).ToList();
            //FuckTheLifeToFindTheLuck.Shuffle(indices);

            //foreach (int index in indices)
            //{
            //    DataGridViewRow row = dataGridView1.Rows[index];
            //    if (row.IsNewRow || loggedRowsCheckIn.Contains(index)) continue;

            //    string emailAddress = row.Cells["EmailAddress"].Value as string ?? "N/A";
            //    string sessionID = row.Cells["SessionID"].Value as string ?? "N/A";
            //    string telegramNickID = row.Cells["TelegramNickName"].Value as string ?? "N/A";

            //    int userPort = 0;
            //    if (row.Cells["chromePort"].Value != null &&
            //        int.TryParse(row.Cells["chromePort"].Value.ToString(), out int parsedPort))
            //    {
            //        userPort = parsedPort;
            //    }

            //    string userName = emailAddress.Split('@')[0].Replace(".", "");
            //    string userProfile = Path.Combine(configSettings.ChromeProfiles, userName);
            //    if (!string.IsNullOrEmpty(emailAddress) && !string.IsNullOrEmpty(sessionID))
            //    {
            //        var result = await FuckTheLifeToFindTheLuck.CRVCheckIn(userProfile, emailAddress, userPort, sessionID, telegramNickID, configSettings.TestOnly);
            //    }

            //}

            Form prompt = new Form()
            {
                Width = 350,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Enter Password",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Text = "Enter your password:", AutoSize = true };
            System.Windows.Forms.TextBox inputBox = new System.Windows.Forms.TextBox()
            {
                Left = 20,
                Top = 50,
                Width = 280
            };
            inputBox.PasswordChar = '*'; // mask input


            System.Windows.Forms.Button yesButton = new System.Windows.Forms.Button() { Text = "Yes", Left = 60, Width = 80, Top = 80, DialogResult = DialogResult.Yes };
            System.Windows.Forms.Button noButton = new System.Windows.Forms.Button() { Text = "No", Left = 180, Width = 80, Top = 80, DialogResult = DialogResult.No };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(yesButton);
            prompt.Controls.Add(noButton);

            prompt.AcceptButton = yesButton;
            prompt.CancelButton = noButton;

            if (prompt.ShowDialog() == DialogResult.Yes)
            {
                string password = inputBox.Text;

                if (password == "fuckthelife")
                {
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show("Incorrect password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    prompt.Close();
                }
            }
            else
            {
                prompt.Close();
            }

        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            for (int i = dataGridView1.Rows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = dataGridView1.Rows[i];
                if (row.IsNewRow) continue;

                bool isChecked = Convert.ToBoolean(row.Cells["confirmSelected"].Value);
                if (isChecked)
                {
                    dataGridView1.Rows.RemoveAt(i);
                }
                dataGridView1.EndEdit();
                dataGridView1.Refresh();
            }
        }

        private void notifyIcon1_DoubleClick_1(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
            }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                notifyIcon1.Visible = true;
                boldFont = null;
            }
        }
        private async void timer_MTV_Tick(object sender, EventArgs e)
        {
            timer_MTV.Enabled = false;
            DateTime now = DateTime.Now;
            tickCount += 1000;

            DateTime startTimeCheckIn = new DateTime(now.Year, now.Month, now.Day, bookTime.StartCheckInHour, bookTime.StartCheckInMinute, 0);
            DateTime endTimeCheckIn = new DateTime(now.Year, now.Month, now.Day, bookTime.EndCheckInHour, bookTime.EndCheckInMinute, 0);
            // Define Clock Out Timer
            DateTime startTimeCheckOut = new DateTime(now.Year, now.Month, now.Day, bookTime.StartCheckOutHour, bookTime.StartCheckOutMinute, 0);
            DateTime endTimeCheckOut = new DateTime(now.Year, now.Month, now.Day, bookTime.EndCheckOutHour, bookTime.EndCheckOutMinute, 0);

            if (DateTime.Now.Hour >= 21 & checkSessionExiredDone == false)
            {
                List<Employee> employees = FuckTheLifeToFindTheLuck.ReadUserDetails(configSettings.RootDataPath + @"\" + configSettings.UserAccountFile);

                for (int i = 0; i < employees.Count; i++)
                {
                    Employee employee = employees[i];

                    if (!string.IsNullOrEmpty(employee.personName) && !string.IsNullOrEmpty(employee.sessionID))
                    {
                        var (loginStatus, sessionId, driver) = await FuckTheLifeToFindTheLuck.CRVLogin(employee.personName, string.Empty, employee.chromePort, false, false, 0);
                        if (!string.IsNullOrEmpty(sessionId))
                        {
                            employee.sessionID = sessionId;
                            employees[i] = employee;
                        }
                        else
                        {
                            countPersonExpired++;
                            employee.sessionID = string.Empty;
                            employees[i] = employee;

                            await telegramBotService.SendMsgTelegram(false, Directory.GetCurrentDirectory() + @"\SessionExpired.png", employee.personName.Split('@')[0].Replace(".", ""), "Your session is expired, pls login again !", employee.telegramID);
                        }
                        driver?.Quit();
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                }

                FuckTheLifeToFindTheLuck.WriteUserDetails(configSettings.RootDataPath + @"\" + configSettings.UserAccountFile, employees);
                checkSessionExiredDone = true;
            }
            if (now.Date > previousDate)
            {
                tickCount = 0;
                notifyDaily = false;
                isLogTimeDone = false;
                notifyBeforeCheckIn = false;
                notifyBeforeCheckOut = false;
                loggedRowsCheckIn.Clear();
                loggedRowsCheckOut.Clear();

                previousDate = now.Date;
                List<Employee> employees = FuckTheLifeToFindTheLuck.ReadUserDetails(configSettings.RootDataPath + @"\" + configSettings.UserAccountFile);
                FuckTheLifeToFindTheLuck.DataCleansing(AppConst.vacationFolder, employees);
                LoadDataAndConfiguration();
                checkSessionExiredDone = false;
                countPersonExpired = 0;
            }

            // Application HealthCheck
            if (tickCount >= int.Parse(configSettings.HealthCheckDuration) * AppConst.MilliSecondPerMinute)
            {
                string msgContent = string.Empty;
                string msgCaption = string.Empty;
                string timeCapture = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                string fileName = "Registration-List-" + timeCapture + ".png";
                string appScreenshot = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                try
                {
                    if (Directory.Exists(AppConst.findtheluckLogDirectory))
                    {
                        string[] filePaths = Directory.GetFiles(AppConst.findtheluckLogDirectory);
                        if (filePaths.Length > 0)
                        {
                            msgCaption = "Here is the logs in last execution:" + Environment.NewLine;
                            foreach (string filePath in filePaths)
                            {
                                try
                                {
                                    string fileContent = File.ReadAllText(filePath);
                                    msgContent += fileContent + Environment.NewLine;
                                }
                                catch (Exception ex)
                                {
                                    FuckTheLifeToFindTheLuck.LogMessage(ex.ToString(), AppConst.findtheluckLogFile);
                                }
                            }
                        }
                        else
                        {
                            msgCaption = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + " : ";
                            msgContent = "Application is running as normal.";
                        }
                        try
                        {
                            Directory.Delete(AppConst.findtheluckLogDirectory, true);
                        }
                        catch (Exception ex)
                        {
                            FuckTheLifeToFindTheLuck.LogMessage("Error deleting directory: " + ex.ToString(), AppConst.findtheluckLogFile);
                        }
                    }
                    else
                    {
                        msgCaption = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + " : ";
                        msgContent = "Application is running as normal.";
                    }

                    FormScreenshot.CaptureRegistrationList(this, appScreenshot);
                    TelegramBotService telegramBotService = new TelegramBotService();
                    await telegramBotService.SendMsgTelegram(true, appScreenshot, msgContent, msgCaption, "all");
                    File.Delete(appScreenshot); tickCount = 0;
                }
                catch (Exception ex)
                {
                    FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
                }
            }

            try
            {
                // Public holiday
                if (publicHoliday.PublicHolidayVN)
                {
                    if (now >= startTimeCheckIn && now <= startTimeCheckIn.AddMinutes(5))
                    {
                        tickCount = 0;
                        if (!notifyDaily)
                        {
                            LoadDataAndConfiguration();
                            string allAccount = "Let's Party !";
                            string timeCapture = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                            string fileName = "Registration-List-" + timeCapture + ".png";
                            string appScreenshot = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                            FormScreenshot.CaptureRegistrationList(this, appScreenshot);
                            TelegramBotService telegramBotService = new TelegramBotService();
                            await telegramBotService.SendMsgTelegram(true, appScreenshot, allAccount, "Public Holiday ", "all");
                            File.Delete(appScreenshot); notifyDaily = true;
                        }
                    }
                }
                // Weekend
                else if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (now >= startTimeCheckIn && now <= startTimeCheckIn.AddMinutes(5))
                    {
                        tickCount = 0;
                        if (!notifyDaily)
                        {
                            LoadDataAndConfiguration();
                            string allAccount = "Let's Party !";
                            string timeCapture = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                            string fileName = "Registration-List-" + timeCapture + ".png";
                            string appScreenshot = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                            FormScreenshot.CaptureRegistrationList(this, appScreenshot);
                            TelegramBotService telegramBotService = new TelegramBotService();
                            await telegramBotService.SendMsgTelegram(true, appScreenshot, allAccount, "Weekend ", "all");
                            File.Delete(appScreenshot); notifyDaily = true;
                        }
                    }
                }
                // It's time to Check In
                else if (now >= startTimeCheckIn && now <= endTimeCheckIn)
                {
                    LoadDataAndConfiguration();
                    tickCount = 0;
                    if (!isLogTimeDone && !File.Exists(AppConst.alreadyCheckIn))
                    {
                        TelegramBotService telegramBotService = new TelegramBotService();

                        // Send the notification to telegram group before check in
                        if (notifyBeforeClockTime.Checked && !notifyBeforeCheckIn)
                        {
                            string timeCapture = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                            string fileName = "Registration-List-" + timeCapture + ".png";
                            string appScreenshot = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                            FormScreenshot.CaptureRegistrationList(this, appScreenshot);
                            string allAccount = "everyone online, individuals will be retried " + configSettings.RetryTimes + " times in case of failures.";

                            await telegramBotService.SendWelcomeToTelegram(Directory.GetCurrentDirectory() + @"\StartTask.gif", allAccount, "Good morning to ", "all");
                            await telegramBotService.SendMsgTelegram(true, appScreenshot, "action in this morning", "Status before ", "all");
                            notifyBeforeCheckIn = true; File.Delete(appScreenshot);
                        }

                        // Start Clock In
                        List<int> indices = Enumerable.Range(0, dataGridView1.Rows.Count).ToList();
                        FuckTheLifeToFindTheLuck.Shuffle(indices);

                        foreach (int index in indices)
                        {
                            DataGridViewRow row = dataGridView1.Rows[index];
                            if (row.IsNewRow || loggedRowsCheckIn.Contains(index)) continue;

                            string emailAddress = row.Cells["EmailAddress"].Value as string ?? string.Empty;
                            string sessionID = row.Cells["SessionID"].Value as string ?? string.Empty;
                            string telegramNickID = row.Cells["TelegramNickName"].Value as string ?? string.Empty;
                            bool isOffAM = Convert.ToBoolean(row.Cells["OffAM"].Value);

                            int userPort = 0;
                            if (row.Cells["chromePort"].Value != null &&
                                int.TryParse(row.Cells["chromePort"].Value.ToString(), out int parsedPort))
                            {
                                userPort = parsedPort;
                            }

                            string userName = emailAddress.Split('@')[0].Replace(".", "");
                            string userProfile = Path.Combine(configSettings.ChromeProfiles, userName);

                            if (!string.IsNullOrEmpty(emailAddress) && !string.IsNullOrEmpty(sessionID))
                            {
                                if (!isOffAM)
                                {
                                    int attempt = 0;
                                    await Task.Delay(SleepTimeGenerator.GenerateRandomSleepTime((int.Parse(configSettings.Random_Min)) * AppConst.MilliSecondPerMinute, (int.Parse(configSettings.Random_Max)) * AppConst.MilliSecondPerMinute) + 45000);

                                    var result = await FuckTheLifeToFindTheLuck.CRVCheckIn(userProfile, emailAddress, userPort, sessionID, telegramNickID, configSettings.TestOnly);
                                    if (result == false)
                                    {
                                        while (attempt <= Convert.ToInt16(configSettings.RetryTimes))
                                        {
                                            attempt++;
                                            // Retry
                                            result = await FuckTheLifeToFindTheLuck.CRVCheckIn(userProfile, emailAddress, userPort, sessionID, telegramNickID, configSettings.TestOnly);
                                        }
                                        if (attempt >= Convert.ToInt16(configSettings.RetryTimes))
                                        {
                                            LogMessage(emailAddress, AppConst.findtheluckLogFile);
                                            //await telegramBotService.SendMsgTelegram(false, Directory.GetCurrentDirectory() + @"\TaskFail.png", emailAddress.Split('@')[0], "Defeats to " + configSettings.RetryTimes + " times for ", telegramNickID);
                                        }
                                    }
                                }
                                else
                                {
                                    await telegramBotService.SendWelcomeToTelegram(Directory.GetCurrentDirectory() + @"\LifeEnjoy.gif", emailAddress.Split('@')[0], "You are ignored, enjoy your life in this AM time ", telegramNickID);
                                }
                            }
                            else
                            {
                                LogMessage(emailAddress, AppConst.findtheluckLogFile);
                                //await telegramBotService.SendMsgTelegram(false, Directory.GetCurrentDirectory() + @"\WrongLogin.png", emailAddress.Split('@')[0], "WOW, Session is Expired !", telegramNickID);
                            }

                        }
                        await telegramBotService.SendWelcomeToTelegram(Directory.GetCurrentDirectory() + @"\CompletedTask.gif", "Have good day ahead !", "That's all for the morning, ", "all");
                        isLogTimeDone = true; FuckTheLifeToFindTheLuck.LogMessage(isLogTimeDone.ToString(), AppConst.alreadyCheckIn);

                    }
                }
                // It's time to Check Out
                else if (now >= startTimeCheckOut && now <= endTimeCheckOut)
                {
                    LoadDataAndConfiguration();
                    tickCount = 0;

                    if (!isLogTimeDone && !File.Exists(AppConst.alreadyCheckOut))
                    {
                        TelegramBotService telegramBotService = new TelegramBotService();

                        // Send the notification to telegram group before check out
                        if (notifyBeforeClockTime.Checked && !notifyBeforeCheckOut)
                        {
                            string timeCapture = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                            string fileName = "Registration-List-" + timeCapture + ".png";
                            string appScreenshot = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                            FormScreenshot.CaptureRegistrationList(this, appScreenshot);
                            string allAccount = "all those online, individuals will be retried up to " + configSettings.RetryTimes + " times in case of any failures.";

                            await telegramBotService.SendWelcomeToTelegram(Directory.GetCurrentDirectory() + @"\StartTask.gif", allAccount, "Goodbye for ", "all");
                            await telegramBotService.SendMsgTelegram(true, appScreenshot, "close the day", "Status before ", "all");
                            notifyBeforeCheckOut = true; File.Delete(appScreenshot);

                        }

                        // Start Clock Out
                        List<int> indices = Enumerable.Range(0, dataGridView1.Rows.Count).ToList();
                        FuckTheLifeToFindTheLuck.Shuffle(indices);

                        foreach (int index in indices)
                        {

                            DataGridViewRow row = dataGridView1.Rows[index];
                            if (row.IsNewRow || loggedRowsCheckOut.Contains(index)) continue;

                            string emailAddress = row.Cells["EmailAddress"].Value as string ?? string.Empty;
                            string sessionID = row.Cells["SessionID"].Value as string ?? string.Empty;
                            string telegramNickID = row.Cells["TelegramNickName"].Value as string ?? string.Empty;
                            bool isOffPM = Convert.ToBoolean(row.Cells["OffPM"].Value);
                            LogMessage(emailAddress, emailAddress);
                            int userPort = 0;
                            if (row.Cells["chromePort"].Value != null &&
                                int.TryParse(row.Cells["chromePort"].Value.ToString(), out int parsedPort))
                            {
                                userPort = parsedPort;
                            }

                            string userName = emailAddress.Split('@')[0].Replace(".", "");
                            string userProfile = Path.Combine(configSettings.ChromeProfiles, userName);

                            if (!string.IsNullOrEmpty(emailAddress) && !string.IsNullOrEmpty(sessionID))
                            {
                                if (!isOffPM)
                                {
                                    int attempt = 0;
                                    await Task.Delay(SleepTimeGenerator.GenerateRandomSleepTime((int.Parse(configSettings.Random_Min)) * AppConst.MilliSecondPerMinute, (int.Parse(configSettings.Random_Max)) * AppConst.MilliSecondPerMinute) + 45000);

                                    var result = await FuckTheLifeToFindTheLuck.CRVCheckOut(userProfile, emailAddress, userPort, sessionID, telegramNickID, configSettings.TestOnly);
                                    if (result == false)
                                    {
                                        while (attempt <= Convert.ToInt16(configSettings.RetryTimes))
                                        {
                                            attempt++;
                                            // Retry
                                            result = await FuckTheLifeToFindTheLuck.CRVCheckOut(userProfile, emailAddress, userPort, sessionID, telegramNickID, configSettings.TestOnly);
                                        }
                                        if (attempt >= Convert.ToInt16(configSettings.RetryTimes))
                                        {
                                            LogMessage(emailAddress, AppConst.findtheluckLogFile);
                                            //await telegramBotService.SendMsgTelegram(false, Directory.GetCurrentDirectory() + @"\TaskFail.png", emailAddress.Split('@')[0], "Defeats to " + configSettings.RetryTimes + " times for ", telegramNickID);
                                        }
                                    }
                                }
                                else
                                {
                                    await telegramBotService.SendWelcomeToTelegram(Directory.GetCurrentDirectory() + @"\LifeEnjoy.gif", emailAddress.Split('@')[0], "You are ignored, enjoy your life in this PM time ", telegramNickID);
                                }
                            }
                            else
                            {
                                LogMessage(emailAddress, AppConst.findtheluckLogFile);
                                //await telegramBotService.SendMsgTelegram(false, Directory.GetCurrentDirectory() + @"\WrongLogin.png", emailAddress.Split('@')[0], "WOW, Session is Expired !", telegramNickID);
                            }
                        }
                        await telegramBotService.SendWelcomeToTelegram(Directory.GetCurrentDirectory() + @"\CompletedTask.gif", "Goodbye !", "The day has been closed for all, ", "all");
                        isLogTimeDone = true; FuckTheLifeToFindTheLuck.LogMessage(isLogTimeDone.ToString(), AppConst.alreadyCheckOut);
                    }
                }
            }

            finally
            {
                timer_MTV.Enabled = true;

                //Clean up for check in
                if (DateTime.Now >= endTimeCheckIn)
                {
                    loggedRowsCheckIn.Clear();
                    notifyBeforeCheckIn = false;
                    isLogTimeDone = false;
                    if (File.Exists(AppConst.alreadyCheckIn))
                    {
                        File.Delete(AppConst.alreadyCheckIn);
                    }
                }
                // Clean up for check out
                if (DateTime.Now >= endTimeCheckOut)
                {
                    loggedRowsCheckOut.Clear();
                    notifyBeforeCheckOut = false;
                    isLogTimeDone = false;
                    if (File.Exists(AppConst.alreadyCheckOut))
                    {
                        File.Delete(AppConst.alreadyCheckOut);
                    }

                }

            }
        }

        private void dateTimePicker_StartCheckIn_ValueChanged(object? sender, EventArgs e)
        {
            DateTime selectedTime = dateTimePicker_StartCheckIn.Value;
            checkin_StartTime.Text = selectedTime.ToString("hh:mm tt");
        }

        private void dateTimePicker_EndCheckIn_ValueChanged(object sender, EventArgs e)
        {
            DateTime selectedTime = dateTimePicker_EndCheckIn.Value;
            checkin_EndTime.Text = selectedTime.ToString("hh:mm tt");
        }

        private void dateTimePicker_StartCheckOut_ValueChanged(object sender, EventArgs e)
        {
            DateTime selectedTime = dateTimePicker_StartCheckOut.Value;
            checkout_StartTime.Text = selectedTime.ToString("hh:mm tt");
        }

        private void dateTimePicker_EndCheckOut_ValueChanged(object sender, EventArgs e)
        {
            DateTime selectedTime = dateTimePicker_EndCheckOut.Value;
            checkout_EndTime.Text = selectedTime.ToString("hh:mm tt");
        }

        public void SaveAllRowsToFile()
        {
            List<Employee> employees = new List<Employee>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                string email = row.Cells["EmailAddress"].Value?.ToString() ?? "";
                string sessionId = row.Cells["SessionID"].Value?.ToString() ?? "";
                string telegramNick = row.Cells["TelegramNickName"].Value?.ToString() ?? "";

                int userPort = 0;
                if (row.Cells["chromePort"].Value != null &&
                    int.TryParse(row.Cells["chromePort"].Value.ToString(), out int parsedPort))
                {
                    userPort = parsedPort;
                }

                employees.Add(new Employee
                {
                    personName = email,
                    sessionID = sessionId,
                    telegramID = telegramNick,
                    chromePort = userPort
                });
            }

            // save to file
            FuckTheLifeToFindTheLuck.WriteUserDetails(
                Path.Combine(configSettings.RootDataPath, configSettings.UserAccountFile),
                employees);
        }

        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dataGridView1.Rows[e.RowIndex].IsNewRow)
                return;

            if (dataGridView1.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                string emailAddress = row.Cells["EmailAddress"].Value?.ToString() ?? "N/A";

                int userPort = 0;
                if (row.Cells["chromePort"].Value != null &&
                    int.TryParse(row.Cells["chromePort"].Value.ToString(), out int parsedPort))
                {
                    userPort = parsedPort;
                }

                MessageBox.Show(
                "When Chrome is opened, please wait for 10 seconds to set up the profile. Then the login page will load automatically. You need to enter the credentials manually.",
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
                var (loginStatus, sessionId, driver) = await FuckTheLifeToFindTheLuck.CRVLogin(emailAddress, string.Empty, userPort, true, false, 0);

                if (!string.IsNullOrEmpty(sessionId))
                {
                    row.Cells["SessionID"].Value = null;
                    row.Cells["SessionID"].Value = sessionId;

                    List<Employee> employees = new List<Employee>();
                    SaveAllRowsToFile();
                    await Task.Delay(TimeSpan.FromMinutes(1));
                    driver?.Quit();
                }
                else
                {
                    // clear if login failed
                    row.Cells["SessionID"].Value = null;
                }
            }
        }

        private void dataGridView1_CellMouseMove(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dataGridView1.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                if (hoveredRow != e.RowIndex)
                {
                    // Reset old row
                    if (hoveredRow >= 0)
                        ResetRowStyle(hoveredRow);

                    // Highlight new row
                    hoveredRow = e.RowIndex;
                    hoveredColumn = e.ColumnIndex;
                    HighlightRow(hoveredRow);
                }
            }
            else
            {
                // Reset if leaving button column
                if (hoveredRow >= 0)
                {
                    ResetRowStyle(hoveredRow);
                    hoveredRow = -1;
                    hoveredColumn = -1;
                }
            }
        }

        private void HighlightRow(int rowIndex)
        {
            var row = dataGridView1.Rows[rowIndex];
            row.DefaultCellStyle.BackColor = Color.White;       // white background
            row.DefaultCellStyle.ForeColor = Color.Red;         // red text
            row.DefaultCellStyle.Font = boldFont;               // bold font
        }

        private void ResetRowStyle(int rowIndex)
        {
            var row = dataGridView1.Rows[rowIndex];
            row.DefaultCellStyle.BackColor = Color.White;       // back to white
            row.DefaultCellStyle.ForeColor = Color.Black;       // black text
            row.DefaultCellStyle.Font = dataGridView1.Font;     // normal font
        }

        private void dataGridView1_MouseLeave(object? sender, EventArgs e)
        {
            if (hoveredRow >= 0)
            {
                ResetRowStyle(hoveredRow);
                hoveredRow = -1;
                hoveredColumn = -1;
            }
        }

        private void cmd_saveData_Click(object sender, EventArgs e)
        {
            try
            {
                List<Employee> employees = new List<Employee>();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    string email = row.Cells["EmailAddress"].Value?.ToString() ?? "";
                    string sessionId = row.Cells["SessionID"].Value?.ToString() ?? "";
                    string telegramNick = row.Cells["TelegramNickName"].Value?.ToString() ?? "";
                    int userPort = 0;
                    if (row.Cells["ChromePort"].Value != null &&
                        int.TryParse(row.Cells["ChromePort"].Value.ToString(), out int parsedPort))
                    {
                        userPort = parsedPort;
                    }

                    employees.Add(new Employee
                    {
                        personName = email.Trim(),
                        sessionID = sessionId,
                        telegramID = telegramNick,
                        chromePort = userPort
                    });
                }

                FuckTheLifeToFindTheLuck.WriteUserDetails(configSettings.RootDataPath + @"\" + configSettings.UserAccountFile, employees);
                LoadDataAndConfiguration();
            }
            catch (Exception ex)
            {
                FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
            }
        }
        private int GetNextAvailablePort()
        {
            for (int port = basePort; port <= maxPort; port++)
            {
                // Check if already used in memory
                if (usedPorts.Contains(port))
                    continue;

                // Check if already displayed in the DataGridView
                bool existsInGrid = dataGridView1.Rows
                    .Cast<DataGridViewRow>()
                    .Any(r => !r.IsNewRow &&
                              r.Cells["chromePort"].Value != null &&
                              int.TryParse(r.Cells["chromePort"].Value.ToString(), out int p) &&
                              p == port);

                if (!existsInGrid)
                {
                    usedPorts.Add(port);
                    return port;
                }
            }

            throw new Exception("No free ports available in range!");
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var grid = (DataGridView)sender;

            if (grid.Columns[e.ColumnIndex].Name == "EmailAddress")
            {
                string? email = grid.Rows[e.RowIndex].Cells["EmailAddress"].Value?.ToString();

                if (!string.IsNullOrWhiteSpace(email))
                {
                    // Check for duplicate
                    foreach (DataGridViewRow otherRow in grid.Rows)
                    {
                        if (otherRow.Index != e.RowIndex && !otherRow.IsNewRow)
                        {
                            string? otherEmail = otherRow.Cells["EmailAddress"].Value?.ToString();
                            if (string.Equals(email, otherEmail, StringComparison.OrdinalIgnoreCase))
                            {
                                MessageBox.Show($"Email '{email}' is already used in another row.",
                                    "Duplicate Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                grid.Rows[e.RowIndex].Cells["EmailAddress"].Value = "";
                                return;
                            }
                        }
                    }

                    // Assign Id and chromePort if not already set
                    var currentRow = grid.Rows[e.RowIndex];
                    if (currentRow.Cells["Id"].Value == null || string.IsNullOrEmpty(currentRow.Cells["Id"].Value.ToString()))
                    {
                        currentRow.Cells["Id"].Value = e.RowIndex + 1;
                    }

                    if (currentRow.Cells["chromePort"].Value == null || string.IsNullOrEmpty(currentRow.Cells["chromePort"].Value.ToString()))
                    {
                        currentRow.Cells["chromePort"].Value = GetNextAvailablePort();
                    }
                }
                else
                {
                    // If email is cleared → remove the row
                    if (!grid.Rows[e.RowIndex].IsNewRow)
                    {
                        grid.Rows.RemoveAt(e.RowIndex);
                    }
                }
            }
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void cmd_Vacation_Click(object sender, EventArgs e)
        {
            for (int i = dataGridView1.Rows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = dataGridView1.Rows[i];
                if (row.IsNewRow) continue;

                bool isChecked = Convert.ToBoolean(row.Cells["confirmSelected"].Value);
                if (isChecked)
                {
                    string emailAddress = row.Cells["EmailAddress"].Value?.ToString() ?? "";

                    Vacation vacationForm = new Vacation();

                    vacationForm.txt_EmailAddress.Text = emailAddress;

                    vacationForm.Show();
                    this.Hide();
                }
            }
        }

        private void cmd_Creds_Click(object sender, EventArgs e)
        {
            for (int i = dataGridView1.Rows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = dataGridView1.Rows[i];
                if (row.IsNewRow) continue;

                bool isChecked = Convert.ToBoolean(row.Cells["confirmSelected"].Value);
                if (isChecked)
                {
                    string emailAddress = row.Cells["EmailAddress"].Value?.ToString() ?? "";
                    string accountFileName = Path.Combine(AppConst.personCred, emailAddress.Trim().ToLower().Split('@')[0]);

                    // Create a new form
                    Form prompt = new Form()
                    {
                        Width = 400,
                        Height = 250,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        Text = $"Set Password for {emailAddress}",
                        StartPosition = FormStartPosition.CenterScreen
                    };

                    // Labels
                    Label lblPassword = new Label() { Left = 20, Top = 20, Text = "Enter Password:", AutoSize = true };
                    Label lblConfirm = new Label() { Left = 20, Top = 80, Text = "Confirm Password:", AutoSize = true };

                    // Textboxes
                    System.Windows.Forms.TextBox txtPassword = new System.Windows.Forms.TextBox() { Left = 20, Top = 45, Width = 340, PasswordChar = '*' };
                    System.Windows.Forms.TextBox txtConfirm = new System.Windows.Forms.TextBox() { Left = 20, Top = 105, Width = 340, PasswordChar = '*' };

                    // Buttons
                    System.Windows.Forms.Button btnOK = new System.Windows.Forms.Button() { Text = "OK", Left = 90, Width = 100, Top = 160, DialogResult = DialogResult.OK };
                    System.Windows.Forms.Button btnCancel = new System.Windows.Forms.Button() { Text = "Cancel", Left = 210, Width = 100, Top = 160, DialogResult = DialogResult.Cancel };

                    prompt.Controls.Add(lblPassword);
                    prompt.Controls.Add(lblConfirm);
                    prompt.Controls.Add(txtPassword);
                    prompt.Controls.Add(txtConfirm);
                    prompt.Controls.Add(btnOK);
                    prompt.Controls.Add(btnCancel);

                    prompt.AcceptButton = btnOK;
                    prompt.CancelButton = btnCancel;

                    // Show dialog and handle input
                    if (prompt.ShowDialog() == DialogResult.OK)
                    {
                        string password = txtPassword.Text;
                        string confirm = txtConfirm.Text;

                        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirm))
                        {
                            MessageBox.Show("Please fill in both password fields.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            i++;
                            continue;
                        }

                        if (password != confirm)
                        {
                            MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            i++; // recheck same row
                            continue;
                        }

                        // ✅ Password confirmed
                        MessageBox.Show($"Password set for: {emailAddress}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Save the Info which has emailaddres + password

                        Person person = new Person();
                        person.accountEmail = emailAddress;
                        person.accountPassword = password;
                        FuckTheLifeToFindTheLuck.SaveLoginCred(accountFileName, person);

                    }

                    prompt.Dispose();
                    LoadDataAndConfiguration();
                }
            }
        }

    }
}
