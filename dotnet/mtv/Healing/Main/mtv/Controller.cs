using Microsoft.Extensions.Configuration;
using mtv;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Collections.Generic;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;



public class TelegramBotService
{
    private static readonly HttpClient httpClient = new HttpClient();
    private readonly mtv.ConfigurationSettings configSettings;

    private static readonly Dictionary<long, bool> waitingForResponse = new();
    private static readonly Dictionary<long, string> pendingBooking = new();
    private static readonly Dictionary<long, DateTime> confirmationStart = new();
    private static readonly Dictionary<long, long> userChatMap = new();

    public TelegramBotService()
    {
        var configuration = mtv.ConfigurationHelper.BuildConfiguration();
        configSettings = new mtv.ConfigurationSettings(configuration);

    }

    public async Task SendMsgTelegram(bool isAdmin, string fileName, string basedUserName, string captionMessage, string telegramUsername)
    {
        try
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), currentDate);
            string filePath = Path.Combine(directoryPath, fileName);
            string msgChatID = isAdmin ? configSettings.TelegramChatIdOperator : configSettings.TelegramChatIdMember;

            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(msgChatID), "chat_id");
                string message = string.IsNullOrEmpty(telegramUsername) ? captionMessage + basedUserName : $"{captionMessage + basedUserName} @{telegramUsername}";
                form.Add(new StringContent(message), "caption");

                if (!string.IsNullOrEmpty(fileName) && File.Exists(filePath))
                {
                    try
                    {
                        using (var fileStream = File.OpenRead(filePath))
                        {
                            form.Add(new StreamContent(fileStream), "photo", Path.GetFileName(filePath));
                            HttpResponseMessage response = await httpClient.PostAsync($"https://api.telegram.org/bot{configSettings.TelegramBotToken}/sendPhoto", form);
                            if (!response.IsSuccessStatusCode)
                            {
                                FuckTheLifeToFindTheLuck.LogMessage($"Failed to send photo: {await response.Content.ReadAsStringAsync()}", AppConst.findtheluckLogFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FuckTheLifeToFindTheLuck.LogMessage("Failed to send photo: " + ex.Message, AppConst.findtheluckLogFile);
                    }
                }
                else
                {
                    form.Add(new StringContent(message), "text");  // Text message field should be "text" not "caption"
                    HttpResponseMessage response = await httpClient.PostAsync($"https://api.telegram.org/bot{configSettings.TelegramBotToken}/sendMessage", form);
                    if (!response.IsSuccessStatusCode)
                    {
                        FuckTheLifeToFindTheLuck.LogMessage($"Failed to send message: {await response.Content.ReadAsStringAsync()}", AppConst.findtheluckLogFile);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage("Exception: " + ex.Message, AppConst.findtheluckLogFile);
        }
    }

    public async Task SendWelcomeToTelegram(string fileName, string basedUserName, string captionMessage, string telegramUsername)
    {
        try
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), currentDate);
            string filePath = Path.Combine(directoryPath, fileName);

            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(configSettings.TelegramChatIdMember), "chat_id");
                string message = string.IsNullOrEmpty(telegramUsername) ? captionMessage + basedUserName : $"{captionMessage + basedUserName} @{telegramUsername}";
                form.Add(new StringContent(message), "caption");

                if (!string.IsNullOrEmpty(fileName) && File.Exists(filePath))
                {
                    try
                    {
                        using (var fileStream = File.OpenRead(filePath))
                        {
                            form.Add(new StreamContent(fileStream), "document", Path.GetFileName(filePath));
                            HttpResponseMessage response = await httpClient.PostAsync($"https://api.telegram.org/bot{configSettings.TelegramBotToken}/sendDocument", form);
                            if (!response.IsSuccessStatusCode)
                            {
                                FuckTheLifeToFindTheLuck.LogMessage($"Failed to send document: {await response.Content.ReadAsStringAsync()}", AppConst.findtheluckLogFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FuckTheLifeToFindTheLuck.LogMessage("Failed to send document: " + ex.Message, AppConst.findtheluckLogFile);
                    }
                }
                else
                {
                    form.Add(new StringContent(message), "text");  // Text message field should be "text" not "caption"
                    HttpResponseMessage response = await httpClient.PostAsync($"https://api.telegram.org/bot{configSettings.TelegramBotToken}/sendMessage", form);
                    if (!response.IsSuccessStatusCode)
                    {
                        FuckTheLifeToFindTheLuck.LogMessage($"Failed to send message: {await response.Content.ReadAsStringAsync()}", AppConst.findtheluckLogFile);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage("Exception: " + ex.Message, AppConst.findtheluckLogFile);
        }
    }

    private async Task SendText(long chatId, string text)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(chatId.ToString()), "chat_id");
        form.Add(new StringContent(text), "text");

        await httpClient.PostAsync($"https://api.telegram.org/bot{configSettings.TelegramBot8}/sendMessage", form);

    }

    

    private string GetDisplayName(TelegramUser user)
    {
        if (!string.IsNullOrEmpty(user.Username))
            return "@" + user.Username;

        return $"{user.FirstName} {user.LastName}".Trim();
    }


    public async Task ListenForVacationAsync(bool ignoreOldUpdatesOnStart = true)
    {
        long offset = 0;
        bool firstRun = ignoreOldUpdatesOnStart;

        while (true)
        {
            try
            {
                var response = await httpClient.GetAsync(
                    $"https://api.telegram.org/bot{configSettings.TelegramBot8}/getUpdates?offset={offset}&timeout=30"
                );

                if (!response.IsSuccessStatusCode)
                {
                    // small backoff to avoid tight loop on network error
                    await Task.Delay(1000);
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("ok", out var okElem) || !okElem.GetBoolean())
                {
                    await Task.Delay(500);
                    continue;
                }

                if (!doc.RootElement.TryGetProperty("result", out var resultElem) || resultElem.GetArrayLength() == 0)
                {
                    // nothing new
                    continue;
                }

                // If first run and ignoring backlog, jump offset to newest update_id + 1 and skip processing
                if (firstRun)
                {
                    long maxId = 0;
                    foreach (var el in resultElem.EnumerateArray())
                    {
                        if (el.TryGetProperty("update_id", out var upIdElem) && upIdElem.TryGetInt64(out long upId))
                            maxId = Math.Max(maxId, upId);
                    }

                    offset = maxId + 1;
                    firstRun = false;
                    FuckTheLifeToFindTheLuck.LogMessage($"ListenForVacation: skipped backlog, starting offset={offset}", "telegram-message.log");
                    continue; // skip processing old updates
                }

                // Process updates
                foreach (var el in resultElem.EnumerateArray())
                {
                    // read update_id safely
                    if (!el.TryGetProperty("update_id", out var updateIdElem) || !updateIdElem.TryGetInt64(out long updateId))
                        continue;

                    // advance offset so this update won't be returned next time
                    offset = updateId + 1;

                    // ensure message exists
                    if (!el.TryGetProperty("message", out var messageElem))
                        continue;

                    // read text
                    string text = "";
                    if (messageElem.TryGetProperty("text", out var textElem))
                        text = textElem.GetString() ?? "";

                    if (string.IsNullOrWhiteSpace(text))
                        continue;

                    // read from.id
                    if (!messageElem.TryGetProperty("from", out var fromElem) ||
                        !fromElem.TryGetProperty("id", out var fromIdElem) ||
                        !fromIdElem.TryGetInt64(out long userId))
                        continue;

                    // read chat.id
                    if (!messageElem.TryGetProperty("chat", out var chatElem) ||
                        !chatElem.TryGetProperty("id", out var chatIdElem) ||
                        !chatIdElem.TryGetInt64(out long chatId))
                        continue;

                    // build a small TelegramUser for GetDisplayName
                    string? username = null, firstName = null, lastName = null;
                    if (fromElem.TryGetProperty("username", out var u)) username = u.GetString();
                    if (fromElem.TryGetProperty("first_name", out var f)) firstName = f.GetString();
                    if (fromElem.TryGetProperty("last_name", out var l)) lastName = l.GetString();

                    var tmpUser = new TelegramUser
                    {
                        Id = userId,
                        Username = username,
                        FirstName = firstName,
                        LastName = lastName
                    };
                    var nickname = GetDisplayName(tmpUser);

                    // remember chat where this user sent the message (group or private)
                    userChatMap[userId] = chatId;

                    var textTrim = text.Trim();
                    var textUpper = textTrim.ToUpperInvariant();

                    // parse first token to accept /vacation and /vacation@BotName
                    var firstToken = textTrim.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                    var cmdToken = firstToken.Split('@', StringSplitOptions.RemoveEmptyEntries)[0].ToUpperInvariant();

                    // --- Step 1: detect /VACATION command
                    if (cmdToken == "/VACATION")
                    {
                        // if someone else currently has an active pending booking, inform
                        if (pendingBooking.Any())
                        {
                            var activeUserId = pendingBooking.Keys.First();
                            if (activeUserId != userId)
                            {
                                await SendText(chatId, "🤐 Shut up! Let me finish with the booking guy first.");
                                continue;
                            }
                        }

                        waitingForResponse[userId] = true;
                        pendingBooking.Remove(userId);
                        confirmationStart.Remove(userId);

                        await SendText(chatId,
    @"📅 What is your schedule?  
Here is the answer format (default is current year):

👉 Book for today:
 - Today AM: today am  
 - Today PM: today pm  
 - Today Full Day: today

👉 Book for some days (not consecutive):  
 - Book for full day (06/10, 9/10, 9/11): 06/10, 9/10, 9/11  
 - Book for AM/PM/FULL (06/10 am, 9/10 pm, 9/11): 06/10 am, 9/10 pm, 9/11

👉 Book for a range of days (consecutive and always full day):  
 - Book for 06-Oct to 9-Oct: 6->9/10 or 06->09/10
");
                        continue;
                    }

                    // --- Step 2: waiting for schedule input (only same active booking allowed)
                    if (waitingForResponse.ContainsKey(userId) && waitingForResponse[userId] && !pendingBooking.ContainsKey(userId))
                    {
                        // if another user currently holds pendingBooking, block him/her
                        if (pendingBooking.Any())
                        {
                            var activeUserId = pendingBooking.Keys.First();
                            if (activeUserId != userId)
                            {
                                await SendText(chatId, "🤐 Shut up! Let me finish with the booking guy first.");
                                continue;
                            }
                        }

                        // Validate their booking BEFORE storing:
                        var parseResult = ParseVacationInput(textTrim); // your strict parser
                        bool parseFailed = parseResult.Count == 1 && parseResult[0].StartsWith("❌");

                        if (parseFailed)
                        {
                            // respond with the parse error (exact message) and keep waitingForResponse true (let them re-enter)
                            await SendText(chatId, parseResult[0]);
                            // do NOT store pendingBooking; let user retry
                            continue;
                        }

                        // store and ask for confirmation
                        pendingBooking[userId] = textTrim;
                        confirmationStart[userId] = DateTime.UtcNow;

                        await SendText(chatId, $"Can you confirm this booking (Y/N)/(y/n)/(yes/no)/(YES/NO)?\n\n👉 {textTrim}");
                        continue;
                    }

                    // --- Step 3: waiting for confirmation (only active booking user allowed)
                    if (pendingBooking.ContainsKey(userId))
                    {
                        // enforce single active booker
                        var activeUserId = pendingBooking.Keys.First();
                        if (activeUserId != userId)
                        {
                            await SendText(chatId, "🤐 Shut up! Let me finish with the booking guy first.");
                            continue;
                        }

                        // confirm or cancel
                        if (textUpper == "Y" || textUpper == "YES")
                        {
                            var bookingRaw = pendingBooking[userId];

                            // parse to normalized entries for final posting
                            var parsed = ParseVacationInput(bookingRaw);

                            // check parse again (safety)
                            if (parsed.Count == 1 && parsed[0].StartsWith("❌"))
                            {
                                // if parsing now fails (rare), tell user to re-enter
                                await SendText(chatId, parsed[0]);
                            }
                            else
                            {
                                string bookingInfo = string.Join(Environment.NewLine, parsed);
                                string emailAddress = await FuckTheLifeToFindTheLuck.FindAccountByTelegramAccount(nickname.TrimStart('@'));

                                if (!string.IsNullOrEmpty(emailAddress))
                                {
                                    string emailPrefix = emailAddress.ToLower().Split('@')[0];
                                    string userVacationFile = Path.Combine(configSettings.RootDataPath, $"vacation_{emailPrefix}.txt");

                                    // ✅ Ensure folder exists
                                    if (!Directory.Exists(AppConst.vacationFolder)) { Directory.CreateDirectory(AppConst.vacationFolder); }

                                    // ✅ Append if file exists, otherwise create new
                                    using (StreamWriter writer = new StreamWriter(userVacationFile, append: true))
                                    {
                                        foreach (var res in parsed)
                                        {
                                            writer.WriteLine(res.Replace("/", "-"));
                                        }
                                    }

                                    // ✅ Send confirmation messages
                                    foreach (var res in parsed)
                                    {
                                        await SendText(chatId, $"{nickname} requested day: {res}");
                                    }
                                    await SendText(chatId, $"{nickname} booking has been confirmed, bye for now.");
                                }
                                else
                                {
                                    await SendText(chatId, $"Your Telegram nickname is not found in the data, please add it. Otherwise, book in the app.");
                                }


                            }

                            // clear state
                            waitingForResponse[userId] = false;
                            pendingBooking.Remove(userId);
                            confirmationStart.Remove(userId);
                            userChatMap.Remove(userId);
                        }
                        else if (textUpper == "N" || textUpper == "NO")
                        {
                            await SendText(chatId, $"❌ Booking cancelled. Bye, {nickname}!");

                            waitingForResponse[userId] = false;
                            pendingBooking.Remove(userId);
                            confirmationStart.Remove(userId);
                            userChatMap.Remove(userId);
                        }
                        else
                        {
                            // Not Y/N: ask again and reset confirmation timer
                            await SendText(chatId, "⚠️ Please reply Y or N.");
                            confirmationStart[userId] = DateTime.UtcNow;
                        }
                    }
                } // foreach update

                // --- Step 4: check timeouts (5 minutes)
                var now = DateTime.UtcNow;
                var expired = confirmationStart.Where(kv => now - kv.Value > TimeSpan.FromMinutes(2)).Select(kv => kv.Key).ToList();
                foreach (var uid in expired)
                {
                    long targetChat = userChatMap.ContainsKey(uid) ? userChatMap[uid] : long.Parse(configSettings.TelegramChatIdMember);
                    await SendText(targetChat, $"⌛ Timeout: Booking request cancelled (no confirmation in 2 minutes).");

                    waitingForResponse[uid] = false;
                    pendingBooking.Remove(uid);
                    confirmationStart.Remove(uid);
                    userChatMap.Remove(uid);
                }
            }
            catch (Exception ex)
            {
                // log exception and small delay to avoid tight loop
                FuckTheLifeToFindTheLuck.LogMessage("ListenForVacation error: " + ex.ToString(), AppConst.findtheluckLogFile);
                await Task.Delay(1000);
            }
        } // while
    }



    private List<string> ParseVacationInput(string input)
    {
        var results = new List<string>();
        var currentYear = DateTime.Now.Year;

        if (string.IsNullOrWhiteSpace(input))
        {
            results.Add("❌ Do not cheat me, book the correct one as above instruction!");
            return results;
        }

        input = input.Trim();

        // --- Handle "today"
        if (input.StartsWith("today", StringComparison.OrdinalIgnoreCase))
        {
            if (input.ToLower().Contains("am"))
                results.Add($"{DateTime.Now:dd/MM/yyyy}, AM");
            else if (input.ToLower().Contains("pm"))
                results.Add($"{DateTime.Now:dd/MM/yyyy}, PM");
            else
                results.Add($"{DateTime.Now:dd/MM/yyyy}, FULL");

            return results;
        }

        // --- Handle multiple days with commas
        if (input.Contains(","))
        {
            var days = input.Split(",", StringSplitOptions.RemoveEmptyEntries);
            foreach (var day in days)
            {
                var parsed = ParseSingleDay(day.Trim(), currentYear);
                if (parsed == null)
                {
                    return new List<string> { "❌ Do not cheat me, book the correct one as above instruction!" };
                }
                results.Add(parsed);
            }
            return results;
        }

        // --- Handle range format (6->9/10)
        if (input.Contains("->"))
        {
            var rangeParts = input.Split("->", StringSplitOptions.RemoveEmptyEntries);
            if (rangeParts.Length == 2)
            {
                var startDayStr = rangeParts[0].Trim();
                var endPart = rangeParts[1].Trim();

                var endParts = endPart.Split("/");
                if (endParts.Length == 2 &&
                    int.TryParse(startDayStr, out int startDay) &&
                    int.TryParse(endParts[0], out int endDay) &&
                    int.TryParse(endParts[1], out int month))
                {
                    try
                    {
                        for (int d = startDay; d <= endDay; d++)
                        {
                            var date = new DateTime(currentYear, month, d);
                            results.Add($"{date:dd/MM/yyyy}, FULL");
                        }
                        return results;
                    }
                    catch
                    {
                        return new List<string> { "❌ Do not cheat me, book the correct one as above instruction!" };
                    }
                }
            }
            return new List<string> { "❌ Do not cheat me, book the correct one as above instruction!" };
        }

        // --- Handle single day
        var singleParsed = ParseSingleDay(input, currentYear);
        if (singleParsed != null)
        {
            results.Add(singleParsed);
            return results;
        }

        // --- Fallback (invalid input)
        return new List<string> { "❌ Do not cheat me, book the correct one as above instruction!" };
    }

    private string? ParseSingleDay(string input, int currentYear)
    {
        string shift = "FULL";
        var single = input.ToLower();

        if (single.Contains("am"))
        {
            shift = "AM";
            single = single.Replace("am", "").Trim();
        }
        else if (single.Contains("pm"))
        {
            shift = "PM";
            single = single.Replace("pm", "").Trim();
        }

        var parts = single.Split("/");
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int d) &&
            int.TryParse(parts[1], out int m))
        {
            try
            {
                var date = new DateTime(currentYear, m, d);
                return $"{date:dd/MM/yyyy}, {shift}";
            }
            catch
            {
                return null;
            }
        }
        return null;
    }


}

public class FuckTheLifeToFindTheLuck
{
    public static mtv.ConfigurationSettings? configSettings;

    public IWebDriver? driver;

    public static void DataCleansing(string vacationFolder,List<Employee> employees)
    {
        try
        {
            DateTime today = DateTime.Today;
            
            //Clean up vacation file
            foreach (Employee employee in employees)
            {
                int countVacationDay = 0;
                string vacationFilePath = Path.Combine(vacationFolder, $"vacation_{employee.personName.Split('@')[0]}.txt");

                if (File.Exists(vacationFilePath))
                {
                    string[] lines = File.ReadAllLines(vacationFilePath);

                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(',');

                        if (parts.Length == 2)
                        {
                            string dateStr = parts[0].Trim();

                            if (DateTime.TryParseExact(dateStr, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            {
                                if (date >= today)
                                {
                                    countVacationDay++;
                                }
                            }
                        }
                    }

                    if (countVacationDay == 0)
                    {
                        // Delete the file if there are no future vacation dates
                        File.Delete(vacationFilePath);
                    }
                }
            }

            // Clean up the logs folder
            if (Directory.Exists(AppConst.findtheluckLogDirectory))
            {
                Directory.Delete(AppConst.findtheluckLogDirectory, true);
            }
        }
        catch (Exception ex) 
        {
            FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
        }
    }

    public static (int hour, int minute) GetHourAndMinute(string timeTextbox)
    {
        DateTime selectedTime;
        if (DateTime.TryParseExact(timeTextbox, "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedTime))
        {
            // Extract the hour and minute components
            int hour = selectedTime.Hour;
            int minute = selectedTime.Minute;

            return (hour, minute);
        }
        else
        {
            MessageBox.Show("Invalid time format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return (-1, -1);
        }
    }

    public static (bool dayOff, string timeOff) CheckDayOff(string filePath)
    {
        bool dayOff = false;
        string timeOff = "";

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');

                if (parts.Length == 2)
                {
                    // Parse date and time from the text file
                    if (DateTime.TryParseExact(parts[0], "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    {
                        // Compare with today's date
                        if (date.Date == DateTime.Today)
                        {
                            dayOff = true;
                            timeOff = parts[1].Trim().ToUpper();
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            
            FuckTheLifeToFindTheLuck.LogMessage(ex.ToString(), AppConst.findtheluckLogFile);
        }

        return (dayOff, timeOff);
    }

    public static bool CheckPublicHoliday(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                LogMessage("File is not existing", AppConst.findtheluckLogFile);
                return false;
            }

            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (DateTime.TryParseExact(line, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime holidayDate))
                {
                    if (holidayDate.Date == DateTime.Today)
                    {
                        return true;
                    }
                }
            }

            return false; // Today is not a holiday
        }
        catch (Exception ex)
        {
            LogMessage(ex.ToString(), AppConst.findtheluckLogFile);
            return false;
        }
    }

    public static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
    public static List<Employee> ReadUserDetails(string filePath)
    {
        List<Employee> employees = new List<Employee>();

        try
        {
            if (File.Exists(filePath))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        string personName = reader.ReadString();
                        string sessionID = reader.ReadString();
                        string telegramID = reader.ReadString();
                        int chromePort = reader.ReadInt32();

                        Employee emp = new Employee(personName, sessionID, telegramID, chromePort);
                        employees.Add(emp);
                    }
                }
            }
            return employees;
        }
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
            return employees;
        }
    }

    public static void WriteUserDetails(string filePath, List<Employee> employees)
    {
        try
        {

            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                foreach (var emp in employees)
                {
                    writer.Write(emp.personName);
                    writer.Write(emp.sessionID);
                    writer.Write(emp.telegramID);
                    writer.Write(emp.chromePort);
                }
            }
        }
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
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
            LogMessage(ex.ToString(), AppConst.findtheluckLogFile);
            return string.Empty;
        }
        
    }

    public static void LogMessage(string message, string fileName)
    {
        try
        {

            string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            // Open the log file for appending text and automatically flush changes
            using (StreamWriter streamWriter = new StreamWriter(logFilePath, append: true))
            {
                // Write the current date and time along with the message to the log file
                streamWriter.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {message}");
            }
        }
        catch { }
    }

    public static class SleepTimeGenerator
    {
        private static readonly Random random = new Random();

        public static int GenerateRandomSleepTime(int min, int max)
        {
            // Generate a random integer between min (inclusive) and max (exclusive)
            return random.Next(min, max);
        }
    }

    public static void Shuffle<T>(List<T> list)
    {
        Random rng = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static string FindChromePath()
    {
        var paths = new[]
        {
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\Application\\chrome.exe")
            };

        foreach (var path in paths)
        {
            if (File.Exists(path))
                return path;
        }

        return string.Empty;
    }

    public static async Task CaptureScreenshot(IWebDriver driver, string userName, string actionStatus, string captionMessage, string telegramNickName)
    {
        try
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string directoryPath = Path.Combine(Path.GetTempPath(), currentDate);
            string currentTime = DateTime.Now.ToString("HH'h'mm");
            string baseUserName = userName.Split('@')[0];
            string fileName = $"{baseUserName}-{actionStatus}-{currentDate}-{currentTime}.png";

            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, fileName);

            var screenshotDriver = driver as ITakesScreenshot;
            if (screenshotDriver != null)
            {
                try
                {
                    var screenshot = screenshotDriver.GetScreenshot();
                    screenshot.SaveAsFile(filePath);
                    TelegramBotService telegramBotService = new TelegramBotService();
                    await telegramBotService.SendMsgTelegram(true, filePath, baseUserName, captionMessage, telegramNickName);
                }
                catch (Exception ex)
                {
                    FuckTheLifeToFindTheLuck.LogMessage(ex.ToString() + " failed for: " + baseUserName, AppConst.findtheluckLogFile);
                }
            }
            else
            {
                FuckTheLifeToFindTheLuck.LogMessage("Screenshot functionality not supported", AppConst.findtheluckLogFile);
            }
        }
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
        }
    }

    private static (bool isErrorLoginPage, string errorMessage) IsLoginErrorPresent(IWebDriver driver, WebDriverWait wait)
    {
        try
        {
            IWebElement errorElement = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("errorText")));
            string errorMessage = errorElement.Text;
            return (true, errorMessage);
        }
        catch (WebDriverTimeoutException)
        {
            return (false, string.Empty);
        }
    }

    public static async Task<(bool loginStatus, string sessionID, ChromeDriver? driver)> CRVLogin(string chromeUserProfile, int userPort)
    {
        ChromeDriver? driver = null;

        string chromePath = FuckTheLifeToFindTheLuck.FindChromePath();
        var chromeDriverService = ChromeDriverService.CreateDefaultService();
        chromeDriverService.HideCommandPromptWindow = true;

        var options = new ChromeOptions();
        options.BinaryLocation = chromePath;
        options.AddUserProfilePreference("profile.default_content_settings.geolocation", 1);
        options.AddArgument($"--remote-debugging-port={userPort}");
        options.AddArgument($@"--user-data-dir={chromeUserProfile}");
        options.AddArgument("--disable-infobars");
        options.AddArgument("--start-maximized");

        driver = new ChromeDriver(chromeDriverService, options);
        await Task.Delay(TimeSpan.FromSeconds(10));
        driver.Navigate().GoToUrl(AppConst.signInUrl);

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(35));

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(20));
            string jsessionId = string.Empty;
            var cookie = driver.Manage().Cookies.GetCookieNamed("JSESSIONID");
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                jsessionId = cookie.Value;

            bool success = !string.IsNullOrEmpty(jsessionId);
            return (success, jsessionId, driver);
        }
        catch (WebDriverException ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage(ex.ToString(), AppConst.findtheluckLogFile);
            driver?.Quit();
            driver?.Dispose();
            return (false, string.Empty, null);
        }
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage(ex.ToString(), AppConst.findtheluckLogFile);
            driver?.Quit();
            driver?.Dispose();
            return (false, string.Empty, null);
        }
    }

    public static async Task<bool> CRVCheckIn(string chromeUserProfile, string emailAddress, int userPort, string sessionId, string telegramNickID, string testOnly)
    {
        TelegramBotService telegramBotService = new TelegramBotService();

        string chromePath = FuckTheLifeToFindTheLuck.FindChromePath();
        if (string.IsNullOrEmpty(chromePath))
            return (false);

        var chromeDriverService = ChromeDriverService.CreateDefaultService();
        chromeDriverService.HideCommandPromptWindow = true;

        var options = new ChromeOptions();
        options.BinaryLocation = chromePath;
        options.AddUserProfilePreference("profile.default_content_settings.geolocation", 1);
        options.AddArgument($"--remote-debugging-port={userPort}");
        options.AddArgument($@"--user-data-dir={chromeUserProfile}");
        options.AddArgument("--disable-infobars");
        options.AddArgument("--start-maximized");

        var driver = new ChromeDriver(chromeDriverService, options);
        await Task.Delay(TimeSpan.FromSeconds(10));

        //string sessionUrl = $"{AppConst.clockTimeUrl};jsessionid={sessionId}#app";

        driver.Navigate().GoToUrl(AppConst.signInUrl);

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(35));

        await Task.Delay(TimeSpan.FromSeconds(10));

        try
        {
            // Find Settings Button
            IWebElement settingSpan = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[contains(@class, 'x-button-label') and contains(text(), 'Settings')]")));
            string settingSpanId = settingSpan.GetAttribute("id");
            driver.FindElement(By.Id(settingSpanId)).Click();
            await Task.Delay(TimeSpan.FromSeconds(5));
            // Get the username is logged in in Settings
            IWebElement loggedInLabel = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(@class,'x-innerhtml') and contains(text(),'Logged in as')]")));
            string loggedInText = loggedInLabel.Text;

            // Go back to the Clock Area
            IWebElement clockSpan = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[contains(@class, 'x-button-label') and contains(text(), 'Clock')]")));
            string clockSpanId = clockSpan.GetAttribute("id");
            driver.FindElement(By.Id(clockSpanId)).Click();
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Find Clock In Button
            IWebElement clockInSpan = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[contains(@class, 'x-button-label') and contains(text(), 'Clock In')]")));
            string clockInSpanId = clockInSpan.GetAttribute("id");
            if (!string.IsNullOrEmpty(clockInSpanId))
            {
                if (testOnly.ToUpper() == "Y")
                {
                    string welcomeImagePath = FuckTheLifeToFindTheLuck.GetImageFilePath(AppConst.morningImageFolder);
                    await telegramBotService.SendWelcomeToTelegram(welcomeImagePath, loggedInText.Replace("Logged in as ", "").Trim() + " : " + clockInSpanId, "Not Clock Time - Just Test And Button Found ", telegramNickID);
                    FuckTheLifeToFindTheLuck.LogMessage(emailAddress.Split('@')[0] + ":" + clockInSpanId, AppConst.findtheluckLogFile);
                }
                else
                {
                    driver.FindElement(By.Id(clockInSpanId)).Click();

                    await Task.Delay(TimeSpan.FromSeconds(5));

                    // Find OK Button
                    IWebElement okButtonSpan = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[contains(@class, 'x-button-label') and contains(text(), 'OK')]")));
                    string okButtonId = okButtonSpan.GetAttribute("id");
                    if (!string.IsNullOrEmpty(okButtonId))
                    {

                        string welcomeImagePath = FuckTheLifeToFindTheLuck.GetImageFilePath(AppConst.morningImageFolder);
                        driver.FindElement(By.Id(okButtonId)).Click();
                        await telegramBotService.SendWelcomeToTelegram(welcomeImagePath, loggedInText.Replace("Logged in as ", "").Trim(), "Bon Matin ", telegramNickID);
                        FuckTheLifeToFindTheLuck.LogMessage(emailAddress.Split('@')[0] + " : " + clockInSpanId, AppConst.findtheluckLogFile);
                        return (true);
                    }
                    else
                    {
                        FuckTheLifeToFindTheLuck.LogMessage("OK Button Element_ID Missing", AppConst.findtheluckLogFile);
                        return (false);
                    }
                }
            }
            else
            {
                FuckTheLifeToFindTheLuck.LogMessage("Clock In Button element_ID Missing", AppConst.findtheluckLogFile);
                return (false);
            }
            return (true);
        }
        catch (NoSuchElementException ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage(ex.ToString(), AppConst.findtheluckLogFile);
            return (false);
        }
        catch (WebDriverException ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage("WebDriverException: " + ex.Message, AppConst.findtheluckLogFile);
            driver?.Quit();
            driver?.Dispose();
            return (false);
        }
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage("Exception: " + ex.Message, AppConst.findtheluckLogFile);
            driver?.Quit();
            driver?.Dispose();
            return (false);
        }
        finally
        {
            driver.Quit();
        }

    }

    public static async Task<bool> CRVCheckOut(string chromeUserProfile, string emailAddress, int userPort, string sessionId, string telegramNickID, string testOnly)
    {
        TelegramBotService telegramBotService = new TelegramBotService();

        string chromePath = FuckTheLifeToFindTheLuck.FindChromePath();
        if (string.IsNullOrEmpty(chromePath))
            return (false);

        var chromeDriverService = ChromeDriverService.CreateDefaultService();
        chromeDriverService.HideCommandPromptWindow = true;

        var options = new ChromeOptions();
        options.BinaryLocation = chromePath;
        options.AddUserProfilePreference("profile.default_content_settings.geolocation", 1);
        options.AddArgument($"--remote-debugging-port={userPort}");
        options.AddArgument($@"--user-data-dir={chromeUserProfile}");
        options.AddArgument("--disable-infobars");
        options.AddArgument("--start-maximized");

        var driver = new ChromeDriver(chromeDriverService, options);
        await Task.Delay(TimeSpan.FromSeconds(10));
        //string sessionUrl = $"{AppConst.clockTimeUrl};jsessionid={sessionId}#app";
        driver.Navigate().GoToUrl(AppConst.signInUrl);

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(35));

        await Task.Delay(TimeSpan.FromSeconds(10));

        try
        {
            // Find Settings Button
            IWebElement settingSpan = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[contains(@class, 'x-button-label') and contains(text(), 'Settings')]")));
            string settingSpanId = settingSpan.GetAttribute("id");
            driver.FindElement(By.Id(settingSpanId)).Click();
            await Task.Delay(TimeSpan.FromSeconds(5));
            // Get the username is logged in in Settings
            IWebElement loggedInLabel = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(@class,'x-innerhtml') and contains(text(),'Logged in as')]")));
            string loggedInText = loggedInLabel.Text;

            // Go back to the Clock Area
            IWebElement clockSpan = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[contains(@class, 'x-button-label') and contains(text(), 'Clock')]")));
            string clockSpanId = clockSpan.GetAttribute("id");
            driver.FindElement(By.Id(clockSpanId)).Click();
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Find the Clock Out button
            IWebElement clockOutSpan = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[contains(@class, 'x-button-label') and contains(text(), 'Clock Out')]")));
            string clockOutSpanId = clockOutSpan.GetAttribute("id");
            if (!string.IsNullOrEmpty(clockOutSpanId))
            {
                if (testOnly.ToUpper() == "Y")
                {

                    string welcomeImagePath = FuckTheLifeToFindTheLuck.GetImageFilePath(AppConst.eveningImageFolder);
                    await telegramBotService.SendWelcomeToTelegram(welcomeImagePath, loggedInText.Replace("Logged in as ", "").Trim() + " : " + clockOutSpanId, "Not Clock Time - Just Test And Button Found ", telegramNickID);
                    FuckTheLifeToFindTheLuck.LogMessage(emailAddress.Split('@')[0] + " : " + clockOutSpanId, AppConst.findtheluckLogFile);
                }
                else
                {
                    driver.FindElement(By.Id(clockOutSpanId)).Click();

                    await Task.Delay(TimeSpan.FromSeconds(5));

                    //Find OK Button
                    IWebElement okButtonSpan = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[contains(@class, 'x-button-label') and contains(text(), 'OK')]")));
                    string okButtonId = okButtonSpan.GetAttribute("id");
                    if (!string.IsNullOrEmpty(okButtonId))
                    {
                        string welcomeImagePath = FuckTheLifeToFindTheLuck.GetImageFilePath(AppConst.eveningImageFolder);
                        driver.FindElement(By.Id(okButtonId)).Click();
                        await telegramBotService.SendWelcomeToTelegram(welcomeImagePath, loggedInText.Replace("Logged in as ", "").Trim(), "Au Revoir ", telegramNickID);
                        FuckTheLifeToFindTheLuck.LogMessage(emailAddress.Split('@')[0] + " : " + clockOutSpanId, AppConst.findtheluckLogFile);
                        return (true);
                    }
                    else
                    {
                        FuckTheLifeToFindTheLuck.LogMessage("No ID found for OK button", AppConst.findtheluckLogFile);
                        return (false);
                    }
                }
            }
            else
            {
                FuckTheLifeToFindTheLuck.LogMessage("No ID found for Clock Out button", AppConst.findtheluckLogFile);
                return (false);
            }
            return (true);
        }
        catch (WebDriverException ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage("WebDriverException: " + ex.Message, AppConst.findtheluckLogFile);
            return (false);
        }
        
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage("Exception: " + ex.Message, AppConst.findtheluckLogFile);
            driver?.Quit();
            driver?.Dispose();
            return (false);
        }
      
        finally
        {
            driver.Quit();
        }

    }

    public static string GetImageFilePath(string folderPath)
    {
        string imageNamePath = string.Empty;
        try
        {
            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                if (files.Length > 0)
                {
                    Random random = new Random();
                    int randomIndex = random.Next(files.Length);
                    imageNamePath = files[randomIndex];
                }
            }
            else
            {
                imageNamePath = string.Empty;
            }
        }
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
            imageNamePath = string.Empty;
        }

        return imageNamePath;
    }

    public static bool DoYouSeeMe()
    {
        try
        {
            bool result = false;
            // Execute the command: wmic csproduct get UUID
            string commandOutput = GetUUID("csproduct get UUID");

            // Extract UUID from the command output
            string[] lines = commandOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string systemUUID = lines.Length > 1 ? lines[1].Trim() : "";

            // Compare the obtained UUID with the expected UUID
            if (systemUUID == AppConst.SeeMe)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
            return false;
        }
        
    }
    public static string GetUUID(string arguments)
    {
        string resultUUID = string.Empty;
        try
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
                    resultUUID = process.StandardOutput.ReadToEnd();
                }
            }
            return resultUUID;
        }
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
            return string.Empty;
        }
    }

    public static bool IsThePublicHoliday(string holidayDate)
    {
        var configuration = mtv.ConfigurationHelper.BuildConfiguration();
        configSettings = new mtv.ConfigurationSettings(configuration);

        string listPiblicHolidays = Path.Combine(AppConst.vacationFolder, configSettings.PublicHolidaysFile);

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
            FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
            return false;
        }

        return holidayList.Any(entry => string.Equals(entry, holidayDate, StringComparison.OrdinalIgnoreCase));
    }

    public static async Task<bool> FindAccount(string enteredEmailAddress)
    {
        var configuration = mtv.ConfigurationHelper.BuildConfiguration();
        var configSettings = new mtv.ConfigurationSettings(configuration);

        string filePath = Path.Combine(configSettings.RootDataPath, configSettings.UserAccountFile);

        List<Employee>? employees = FuckTheLifeToFindTheLuck.ReadUserDetails(filePath);
        int maxAttempts = 5;
        int attempt = 0;

        while (attempt < maxAttempts)
        {
            try
            {
                foreach (Employee employee in employees)
                {
                    if (employee.personName == enteredEmailAddress)
                    {
                        return true;
                    }
                }

                return false;
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
                    MessageBox.Show("Many people are reading or writing the data, please try again after a few seconds.");
                    return false;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unexpected error occurred while reading the data, please try again after a few seconds.");
                return false;
            }
        }

        return false;
    }

    public static async Task<string> FindAccountByTelegramAccount(string telegramUserName)
    {
        var configuration = mtv.ConfigurationHelper.BuildConfiguration();
        var configSettings = new mtv.ConfigurationSettings(configuration);

        string filePath = Path.Combine(configSettings.RootDataPath, configSettings.UserAccountFile);

        List<Employee>? employees = FuckTheLifeToFindTheLuck.ReadUserDetails(filePath);
        int maxAttempts = 5;
        int attempt = 0;
        string result = string.Empty;
        while (attempt < maxAttempts)
        {
            try
            {
                foreach (Employee employee in employees)
                {
                    if (employee.telegramID == telegramUserName)
                    {
                        result = employee.personName;
                        return result;
                    }
                }

                return result;
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
                    MessageBox.Show("Many people are reading or writing the data, please try again after a few seconds.");
                    return result;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unexpected error occurred while reading the data, please try again after a few seconds.");
                return result;
            }
        }

        return result;
    }
}