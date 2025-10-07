using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace mtv_client
{
    public class FindTheLuckThenFuckTheLife
    {
        public static string EncryptString(string key, string plainText)
        {
            try
            {

                byte[] iv = new byte[16];
                byte[] array;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                            {
                                streamWriter.Write(plainText);
                            }

                            array = memoryStream.ToArray();
                        }
                    }
                }

                return Convert.ToBase64String(array);
            }
            catch (Exception ex)
            {
                LogMessage(ex.ToString(), AppSetting.errorLogFile);
                return string.Empty;
            }
        }

        public static string DecryptString(string key, string cipherText)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.ToString(), AppSetting.errorLogFile);
                return string.Empty;
            }

        }
        public static bool IsThePublicHoliday(string holidayDate)
        {
            string listPiblicHolidays = Path.Combine(AppSetting.DataSharedFolder, AppSetting.Holidays);

            List<string> holidayList = new List<string>();
            try
            {
                // Create a FileStream for the whitelist file
                using (FileStream fileStream = File.Open(listPiblicHolidays, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // Pass the FileStream to the StreamReader constructor
                    using (StreamReader reader = new StreamReader(fileStream, Encoding.Default, true))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            holidayList.Add(line.Trim().ToLower());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.ToString(), AppSetting.errorLogFile);
                return false;
            }

            return holidayList.Any(entry => string.Equals(entry, holidayDate, StringComparison.OrdinalIgnoreCase));
        }
        public static bool IsEmailInWhitelist(string email)
        {
            string whitelistFilePath = Path.Combine(AppSetting.DataSharedFolder, AppSetting.MailWhiteList);

            List<string> whitelist = new List<string>();
            try
            {
                // Create a FileStream for the whitelist file
                using (FileStream fileStream = File.Open(whitelistFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // Pass the FileStream to the StreamReader constructor
                    using (StreamReader reader = new StreamReader(fileStream, Encoding.Default, true))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            whitelist.Add(line.Trim().ToLower());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.ToString(), AppSetting.errorLogFile);
                return false;
            }

            return whitelist.Any(entry => string.Equals(entry, email, StringComparison.OrdinalIgnoreCase));
        }

        public static void WriteUserDetails(List<Employee> employees)
        {
            string userAccountFilePath = Path.Combine(AppSetting.DataSharedFolder, AppSetting.UserAccountFile);

            try
            {
                using (FileStream fileStream = new FileStream(userAccountFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    foreach (var emp in employees)
                    {
                        writer.Write(emp.EmailAddress);
                        writer.Write(FindTheLuckThenFuckTheLife.EncryptString(AppSetting.encryptKey, emp.EmailPassword));
                        writer.Write(emp.TelegramUserName);

                    }
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle access denied error
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage(ex.ToString(), AppSetting.errorLogFile);
            }
            catch (IOException ex)
            {
                // Handle other IO errors
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage(ex.ToString(), AppSetting.errorLogFile);
            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage(ex.ToString(), AppSetting.errorLogFile);
            }
        }
        public static void OverWriteUserDetails(List<Employee> employees)
        {
            string userAccountFilePath = Path.Combine(AppSetting.DataSharedFolder, AppSetting.UserAccountFile);
            try
            {
                using (FileStream fileStream = new FileStream(userAccountFilePath, FileMode.Create, FileAccess.Write))
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    foreach (var emp in employees)
                    {
                        writer.Write(emp.EmailAddress);
                        writer.Write(FindTheLuckThenFuckTheLife.EncryptString(AppSetting.encryptKey, emp.EmailPassword));
                        writer.Write(emp.TelegramUserName);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle access denied error
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage(ex.ToString(), AppSetting.errorLogFile);
            }
            catch (IOException ex)
            {
                // Handle other IO errors
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage(ex.ToString(), AppSetting.errorLogFile);
            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage(ex.ToString(), AppSetting.errorLogFile);
            }
        }

        public static bool IsPasswordMatched(string enteredPassword, string enteredEmailAddress)
        {

            bool isPasswordMatched = false;
            string filePath = AppSetting.DataSharedFolder + "\\" + AppSetting.UserAccountFile;
            // Read the binary file
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    string emailAddress = reader.ReadString();
                    string encryptedPassword = reader.ReadString();
                    string decryptedPassword = FindTheLuckThenFuckTheLife.DecryptString(AppSetting.encryptKey, encryptedPassword);
                    string telegramNickName = reader.ReadString();

                    // Compare the decrypted password with the entered password
                    if ((decryptedPassword == enteredPassword) && (emailAddress == enteredEmailAddress))
                    {
                        isPasswordMatched = true;
                        break;
                    }
                }
            }
            return isPasswordMatched;
        }

        public static bool UpdatePassword(string enteredEmailAddress, string oldPassword, string newPassword)
            
        {
            string emailAddress = string.Empty;
            string encryptedPassword = string.Empty;
            string telegramNickName = string.Empty;
            string descryptedPassword = string.Empty;

            string filePath = Path.Combine(AppSetting.DataSharedFolder, AppSetting.UserAccountFile);
            List<Employee> employees = new List<Employee>();

            // Read all existing employee data from the binary file
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    emailAddress = reader.ReadString();
                    encryptedPassword = reader.ReadString();
                    telegramNickName = reader.ReadString();
                    descryptedPassword = FindTheLuckThenFuckTheLife.DecryptString(AppSetting.encryptKey, encryptedPassword);

                    // Add employee to the list
                    employees.Add(new Employee(emailAddress, descryptedPassword, telegramNickName));
                }
            }

            // Find the index of the employee whose email address and password match the old credentials
            int indexToUpdate = employees.FindIndex(emp => emp.EmailAddress == enteredEmailAddress && emp.EmailPassword == oldPassword);

            if (indexToUpdate != -1)
            {
                // Update the password for that employee
                employees[indexToUpdate] = new Employee(enteredEmailAddress, newPassword, telegramNickName);

                // Write the updated employee details to the file
                OverWriteUserDetails(employees);

                // Password update successful
                return true;
            }
            else
            {
                LogMessage("Email address and old password do not match any people", AppSetting.errorLogFile);
                return false;
            }
        }

        public static bool UpdateAccount(string enteredEmailAddress, string enteredPassword, string updateTelegramNickName)

        {
            string emailAddress = string.Empty;
            string encryptedPassword = string.Empty;
            string telegramNickName = string.Empty;
            string descryptedPassword = string.Empty;

            string filePath = Path.Combine(AppSetting.DataSharedFolder, AppSetting.UserAccountFile);
            List<Employee> employees = new List<Employee>();

            // Read all existing employee data from the binary file
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    emailAddress = reader.ReadString();
                    encryptedPassword = reader.ReadString();
                    telegramNickName = reader.ReadString();
                    descryptedPassword = FindTheLuckThenFuckTheLife.DecryptString(AppSetting.encryptKey, encryptedPassword);

                    // Add employee to the list
                    employees.Add(new Employee(emailAddress, descryptedPassword, telegramNickName));
                }
            }

            // Find the index of the employee 
            int indexToUpdate = employees.FindIndex(emp => emp.EmailAddress == enteredEmailAddress && emp.EmailPassword == enteredPassword);

            if (indexToUpdate != -1)
            {
                // Update the account for that employee
                employees[indexToUpdate] = new Employee(enteredEmailAddress, enteredPassword, updateTelegramNickName);

                // Write the updated employee details to the file
                OverWriteUserDetails(employees);

                // Password update successful
                return true;
            }
            else
            {
                LogMessage("Email address and old password do not match any people", AppSetting.errorLogFile);
                return false;
            }
        }
        public static async Task<bool> FindAccount(string enteredEmailAddress)
        {
            bool isAccountExisting = false;
            string filePath = Path.Combine(AppSetting.DataSharedFolder, AppSetting.UserAccountFile);
            int maxAttempts = 5;
            int attempt = 0;

            while (attempt < maxAttempts)
            {
                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            string emailAddress = reader.ReadString();
                            if (emailAddress == enteredEmailAddress)
                            {
                                isAccountExisting = true;
                                return isAccountExisting;
                            }
                        }
                        return isAccountExisting;
                    }
                }
                catch (IOException)
                {
                    attempt++;
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(200 * (int)Math.Pow(2, attempt));
                    }
                    else
                    {
                        MessageBox.Show("Many people are read or write the data, please try again after few seconds.");
                        return false;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Many people are read or write the data, please try again after few seconds.");
                    return false;
                }
            }
            return isAccountExisting;
        }

        public static bool CheckUUID()
        {
            bool result = false;
            // Execute the command to get the system UUID using wmic
            string commandOutput = GetUUID("csproduct get UUID");

            // Extract UUID from the command output
            string[] lines = commandOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string systemUUID = lines.Length > 1 ? lines[1].Trim() : "";

            // Compare the obtained UUID with the expected UUID
            if (systemUUID == AppSetting.App_UUID)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
        public static string GetUUID(string arguments)
        {
            ProcessStartInfo? startInfo = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process? process = Process.Start(startInfo))
            {
                if (process != null)
                {
                    process.WaitForExit();
                    return process.StandardOutput.ReadToEnd();
                }
            }
            return "";
        }
        public static void LogMessage(string message, string fileName)
        {
            string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            // Open the log file for appending text and automatically flush changes
            using (StreamWriter streamWriter = new StreamWriter(logFilePath, append: true))
            {
                // Write the current date and time along with the message to the log file
                streamWriter.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {message}");
            }
        }

        public static void WriteLogForAccount(string filePath, string userName)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(userName + " at " + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")) ;
                }
            }
            catch (IOException ex)
            {
                FindTheLuckThenFuckTheLife.LogMessage(ex.Message, AppSetting.errorLogFile);
            }
            
        }
    }
}
