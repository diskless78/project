using mtv;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mtv_client
{
    public partial class Vacation : Form
    {
        private readonly mtv.ConfigurationSettings configSettings;
        private List<string> originalDayOffItems = new List<string>();


        public Vacation()
        {
            InitializeComponent();
            var configuration = mtv.ConfigurationHelper.BuildConfiguration();
            configSettings = new mtv.ConfigurationSettings(configuration);
        }

        private void AddVacationToListBox()
        {

            string selectedDate = dateTimeOffPicker.Value.ToString("dd-MM-yyyy");
            if (!FuckTheLifeToFindTheLuck.IsThePublicHoliday(selectedDate))
            {
                if (timeOff.SelectedItem == null)
                {
                    MessageBox.Show("Please select a time option (AM, PM, FULL).", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string? selectedTime = timeOff.SelectedItem.ToString();
                string combinedData = selectedDate + ", " + selectedTime;

                // Construct identifiers for the list entries
                string dateAM = selectedDate + ", AM";
                string datePM = selectedDate + ", PM";
                string dateFull = selectedDate + ", FULL";

                bool hasAM = listDayOff.Items.Contains(dateAM);
                bool hasPM = listDayOff.Items.Contains(datePM);
                bool hasFull = listDayOff.Items.Contains(dateFull);

                if (hasFull && selectedTime != "FULL")
                {
                    MessageBox.Show("A full day is already registered for this date.", "Invalid Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if ((selectedTime == "AM" && hasPM) || (selectedTime == "PM" && hasAM))
                {
                    // If either AM or PM is added and the other half is present, replace with FULL
                    listDayOff.Items.Remove(dateAM);
                    listDayOff.Items.Remove(datePM);
                    if (!hasFull) // Check if FULL isn't already added to avoid duplicates
                    {
                        listDayOff.Items.Add(dateFull);
                    }
                }
                else if (selectedTime == "FULL")
                {
                    // If FULL is added, remove AM and PM if they exist and add FULL
                    if (hasAM || hasPM)
                    {
                        listDayOff.Items.Remove(dateAM);
                        listDayOff.Items.Remove(datePM);
                    }
                    if (!hasFull) // Again, ensure no duplicate FULL entry is added
                    {
                        listDayOff.Items.Add(combinedData);
                    }
                }
                else if (!listDayOff.Items.Contains(combinedData))
                {
                    // Add the time if neither AM nor PM is present for the selected day and FULL isn't either
                    listDayOff.Items.Add(combinedData);
                }
                else
                {
                    MessageBox.Show("This vacation time is already added.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Vacation is not counted on public holidays.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadDayOff(string enteredEmailAddress)
        {
            string filePath = AppConst.vacationFolder + @"\vacation_" + enteredEmailAddress.Split('@')[0] + ".txt";
            listDayOff.Items.Clear();
            originalDayOffItems.Clear();

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
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
                                string itemToAdd = dateStr + ", " + descriptor;
                                listDayOff.Items.Add(itemToAdd);
                                originalDayOffItems.Add(itemToAdd);
                            }
                        }
                        else
                        {
                            FuckTheLifeToFindTheLuck.LogMessage("Invalid date format: " + dateStr, AppConst.findtheluckLogFile);
                        }
                    }
                }
            }
            SortAndLoadListBoxItems();
        }

        private async void Vacation_Load(object sender, EventArgs e)
        {
            bool findResult = await FuckTheLifeToFindTheLuck.FindAccount(txt_EmailAddress.Text.Trim().ToLower());
            if (findResult)
            {
                dateTimeOffPicker.Enabled = true;
                timeOff.Enabled = true;
                addDayOff.Enabled = true;
                saveDateOff.Enabled = true;
                removeDayOff.Enabled = true;
                LoadDayOff(txt_EmailAddress.Text.Trim().ToLower());

                dateTimeOffPicker.MinDate = DateTime.Today;
                dateTimeOffPicker.Format = DateTimePickerFormat.Custom;
                dateTimeOffPicker.CustomFormat = "ddd, dd-MM-yyyy";
                dateTimeOffPicker.ShowUpDown = false;

                // Set the DateTimePicker to show today's date by default
                dateTimeOffPicker.Value = DateTime.Now;

                timeOff.Items.AddRange(new string[] { "AM", "PM", "FULL" });

                // Set DropDownStyle
                timeOff.DropDownStyle = ComboBoxStyle.DropDownList;

                // Subscribe to KeyPress event
                timeOff.KeyPress += timeOff_KeyPress;

            }
            else
            {
                dateTimeOffPicker.Enabled = false;
                timeOff.Enabled = false;
                addDayOff.Enabled = false;
                removeDayOff.Enabled = false;
                saveDateOff.Enabled = false;
                listDayOff.Items.Clear();
            }
        }

        private void RemoveDayOff_Click(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Vacation_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }

        private void timeOff_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Ignore non-letter characters
            if (!char.IsLetter(e.KeyChar))
            {
                e.Handled = true; // Do not process the key further
                return;
            }

            // Convert to uppercase since items are in uppercase
            char upperChar = char.ToUpper(e.KeyChar);

            switch (upperChar)
            {
                case 'A':
                    timeOff.SelectedIndex = timeOff.Items.IndexOf("AM");
                    e.Handled = true; // Handle the key press to prevent further processing
                    break;
                case 'P':
                    timeOff.SelectedIndex = timeOff.Items.IndexOf("PM");
                    e.Handled = true;
                    break;
                case 'F':
                    timeOff.SelectedIndex = timeOff.Items.IndexOf("FULL");
                    e.Handled = true;
                    break;
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 mainForm = new Form1();
            mainForm.Show();
        }


        private void addDayOff_Click(object sender, EventArgs e)
        {
            AddVacationToListBox();
            SortAndLoadListBoxItems();

        }

        private void listDayOff_SelectedIndexChanged(object sender, EventArgs e)
        {
            removeDayOff.Enabled = listDayOff.SelectedIndex != -1;

        }

        private void removeDayOff_Click_1(object sender, EventArgs e)
        {
            if (listDayOff.SelectedIndex != -1)
            {
                listDayOff.Items.RemoveAt(listDayOff.SelectedIndex);

                if (listDayOff.Items.Count > 0)
                {
                    // Set the selection to a new item
                    if (listDayOff.Items.Count > listDayOff.SelectedIndex)
                    {
                        listDayOff.SelectedIndex = listDayOff.SelectedIndex;
                    }
                    else
                    {
                        listDayOff.SelectedIndex = listDayOff.Items.Count - 1;
                    }
                }
            }
        }

        private void SortAndLoadListBoxItems()
        {
            List<string> items = new List<string>();

            foreach (var item in listDayOff.Items)
            {
                string itemString = item?.ToString() ?? "";
                if (!string.IsNullOrEmpty(itemString))
                {
                    items.Add(itemString);
                }
            }

            items.Sort((x, y) => DateTime.ParseExact(x.Split(',')[0].Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture)
                                   .CompareTo(DateTime.ParseExact(y.Split(',')[0].Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture)));

            listDayOff.Items.Clear();
            foreach (var item in items)
            {
                listDayOff.Items.Add(item);
            }
        }

        private void saveDateOff_Click(object sender, EventArgs e)
        {
            // Create a set from the current ListBox items for easy comparison
            HashSet<string> currentItems = new HashSet<string>(listDayOff.Items.Cast<string>());

            // Check if there are changes by comparing count and individual items
            if (currentItems.SetEquals(originalDayOffItems))
            {
                // Inform the user that there are no changes to save
                MessageBox.Show("No changes to save.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (!Directory.Exists(AppConst.vacationFolder)) { Directory.CreateDirectory(AppConst.vacationFolder); }
                    
                // If there are changes, proceed with saving
                string emailPrefix = txt_EmailAddress.Text.Trim().ToLower().Split('@')[0];
                string userVacationDataFolder = Path.Combine(configSettings.RootDataPath, $"vacation_{emailPrefix}.txt");
                string userVacationDestFolder = Path.Combine(configSettings.RootDataPath, $"vacation_{emailPrefix}.txt");

                try
                {
                    if (File.Exists(userVacationDestFolder)) File.Delete(userVacationDestFolder);
                    using (StreamWriter writer = new StreamWriter(userVacationDataFolder))
                    {
                        foreach (var item in listDayOff.Items)
                        {
                            writer.WriteLine(item.ToString());
                        }
                    }

                    MessageBox.Show("Data saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Update the original items list after saving
                    originalDayOffItems = new List<string>(currentItems);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dateTimeOffPicker_ValueChanged(object sender, EventArgs e)
        {
            //DateTime selectedDate = dateTimeOffPicker.Value;

            // Check if selected date is Saturday or Sunday
            //if (selectedDate.DayOfWeek == DayOfWeek.Saturday || selectedDate.DayOfWeek == DayOfWeek.Sunday)
            //{
            //    MessageBox.Show("Please select a weekday (Monday to Friday). Saturdays and Sundays are not allowed.", "Invalid Selection");
            //    // Reset the DateTimePicker value
            //    dateTimeOffPicker.Value = DateTime.Today; // Or any other appropriate value
            //}
        }

    }
}
