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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Microsoft.VisualBasic.ApplicationServices;
using System.Windows.Forms;



public class TelegramBotService
{
    private static readonly HttpClient httpClient = new HttpClient();
    private readonly mtv.ConfigurationSettings configSettings;

    private static readonly Dictionary<long, bool> waitingForResponse = new();
    private static readonly Dictionary<long, string> pendingBooking = new();
    private static readonly Dictionary<long, DateTime> confirmationStart = new();
    private static readonly Dictionary<long, long> userChatMap = new(); 

    private static readonly Dictionary<long, bool> waitingForGetMeInFirstConfirmation = new();
    private static readonly Dictionary<long, bool> waitingForGetMeInSecondConfirmation = new(); 


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

    public async Task SendText(long chatId, string text)
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


    public async Task ListenForTelegramAsync(bool ignoreOldUpdatesOnStart = true)
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

                if (!response.IsSuccessStatusCode) { await Task.Delay(1000); continue; }
                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("ok", out var okElem) || !okElem.GetBoolean()) { await Task.Delay(500); continue; }

                if (!doc.RootElement.TryGetProperty("result", out var resultElem) || resultElem.GetArrayLength() == 0)
                {
                    continue;
                }

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
                    if (!el.TryGetProperty("update_id", out var updateIdElem) || !updateIdElem.TryGetInt64(out long updateId))
                        continue;
                    offset = updateId + 1;

                    if (!el.TryGetProperty("message", out var messageElem)) continue;

                    string text = "";
                    if (messageElem.TryGetProperty("text", out var textElem))
                        text = textElem.GetString() ?? "";
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    if (!messageElem.TryGetProperty("from", out var fromElem) || !fromElem.TryGetProperty("id", out var fromIdElem) || !fromIdElem.TryGetInt64(out long userId)) continue;
                    if (!messageElem.TryGetProperty("chat", out var chatElem) || !chatElem.TryGetProperty("id", out var chatIdElem) || !chatIdElem.TryGetInt64(out long chatId)) continue;

                    string? chatType = null;
                    if (chatElem.TryGetProperty("type", out var typeElem))
                        chatType = typeElem.GetString();

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
                    userChatMap[userId] = chatId;

                    var textTrim = text.Trim();
                    var textUpper = textTrim.ToUpperInvariant();

                    var firstToken = textTrim.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                    var cmdToken = firstToken.Split('@', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToUpperInvariant() ?? "";


                    bool isGetMeInActive = waitingForGetMeInFirstConfirmation.ContainsKey(userId) || waitingForGetMeInSecondConfirmation.ContainsKey(userId);
                    bool isVacationActive = waitingForResponse.ContainsKey(userId) || pendingBooking.ContainsKey(userId);

                    if (isGetMeInActive)
                    {
                        await ProcessGetMeInReplyAsync(userId, chatId, textUpper, nickname, chatType);
                        continue;
                    }


                    if (pendingBooking.ContainsKey(userId))
                    {
                        var activeUserId = pendingBooking.Keys.First();
                        if (activeUserId != userId)
                        {
                            await SendText(chatId, "🤐 Shut up! Let me finish with the booking guy first.");
                            continue;
                        }

                        await ProcessVacationConfirmationAsync(userId, chatId, textUpper, nickname);
                        continue;
                    }

                    else if (waitingForResponse.ContainsKey(userId) && waitingForResponse[userId])
                    {
                        if (pendingBooking.Any() && pendingBooking.Keys.First() != userId)
                        {
                            await SendText(chatId, "🤐 Shut up! Let me finish with the booking guy first.");
                            continue;
                        }

                        await ProcessVacationScheduleInputAsync(userId, chatId, textTrim, textUpper, nickname);
                        continue;
                    }

                    // Start /VACATION (Step 1)
                    else if (cmdToken == "/VACATION")
                    {
                        await StartVacationFlowAsync(userId, chatId);
                        continue;
                    }

                    // Start /GETMEIN
                    else if (cmdToken == "/GETMEIN")
                    {
                        await StartGetMeInFlowAsync(userId, chatId, nickname, chatType);
                        continue;
                    }

                } // foreach update

                var now = DateTime.UtcNow;
                var expired = confirmationStart.Where(kv => now - kv.Value > TimeSpan.FromMinutes(2)).Select(kv => kv.Key).ToList();
                foreach (var uid in expired)
                {
                    long targetChat = userChatMap[uid];

                    if (waitingForGetMeInFirstConfirmation.ContainsKey(uid) || waitingForGetMeInSecondConfirmation.ContainsKey(uid))
                    {
                        // /getmein timeout
                        await SendText(targetChat, $"⌛ Timeout: Get-in request cancelled (no confirmation).");
                        waitingForGetMeInFirstConfirmation.Remove(uid);
                        waitingForGetMeInSecondConfirmation.Remove(uid);
                    }
                    else if (pendingBooking.ContainsKey(uid) || waitingForResponse.ContainsKey(uid))
                    {
                        // /vacation timeout
                        await SendText(targetChat, $"⌛ Timeout: Booking request cancelled (no confirmation).");
                        waitingForResponse.Remove(uid);
                        pendingBooking.Remove(uid);
                    }

                    // Clear shared state
                    confirmationStart.Remove(uid);
                    userChatMap.Remove(uid);
                }
            }
            catch (Exception ex)
            {
                FuckTheLifeToFindTheLuck.LogMessage("ListenForVacation error: " + ex.ToString(), AppConst.findtheluckLogFile);
                await Task.Delay(1000);
            }
        } // while
    }

    private async Task StartGetMeInFlowAsync(long userId, long chatId, string nickname, string? chatType)
    {
        if (chatType != "private")
        {
            await SendText(chatId, "⚠️ Please use the /getmein command in a private chat to the bot.");
            return;
        }

        if (waitingForResponse.ContainsKey(userId) || pendingBooking.ContainsKey(userId))
        {
            await SendText(chatId, "⏳ Please finish your active /vacation request first.");
            return;
        }

        waitingForGetMeInFirstConfirmation[userId] = true;
        confirmationStart[userId] = DateTime.UtcNow;

        string emailAddress = await FuckTheLifeToFindTheLuck.FindAccountByTelegramAccount(nickname.TrimStart('@'));
        if (!string.IsNullOrEmpty(emailAddress))
        {
            await SendText(chatId, "❓ Confirm login " + emailAddress.Split('@')[0].Replace(".", "") + " ? (Y/N)/(yes/no)");
        }
        else
        {
            await SendText(chatId, $"You need to add telegram nickname into the healing app.");
            waitingForGetMeInFirstConfirmation.Remove(userId);
            confirmationStart.Remove(userId);
        }
        var now = DateTime.UtcNow;
        var expired = confirmationStart.Where(kv => now - kv.Value > TimeSpan.FromMinutes(2)).Select(kv => kv.Key).ToList();
        foreach (var uid in expired)
        {
            long targetChat = userChatMap[uid];

            if (waitingForGetMeInFirstConfirmation.ContainsKey(uid) || waitingForGetMeInSecondConfirmation.ContainsKey(uid))
            {
                // /getmein timeout
                await SendText(targetChat, $"⌛ Timeout: Get-in request cancelled (no confirmation).");
                waitingForGetMeInFirstConfirmation.Remove(uid);
                waitingForGetMeInSecondConfirmation.Remove(uid);
            }
            else if (pendingBooking.ContainsKey(uid) || waitingForResponse.ContainsKey(uid))
            {
                // /vacation timeout
                await SendText(targetChat, $"⌛ Timeout: Booking request cancelled (no confirmation).");
                waitingForResponse.Remove(uid);
                pendingBooking.Remove(uid);
            }

            // Clear shared state
            confirmationStart.Remove(uid);
            userChatMap.Remove(uid);
        }
    }

    private async Task StartVacationFlowAsync(long userId, long chatId)
    {
        // Block if /getmein is active
        if (waitingForGetMeInFirstConfirmation.ContainsKey(userId) || waitingForGetMeInSecondConfirmation.ContainsKey(userId))
        {
            await SendText(chatId, "⏳ Please finish active /getmein request first.");
            return;
        }

        if (pendingBooking.Any() && pendingBooking.Keys.First() != userId)
        {
            await SendText(chatId, "🤐 Shut up! Let me finish with the booking guy first.");
            return;
        }

        waitingForResponse[userId] = true;
        pendingBooking.Remove(userId);
        confirmationStart.Remove(userId);

        await SendText(chatId,
     @"📅 What is your schedule?  
Here is the answer format (default is current year):

❌ Cancel: Type n/N if you want to cancel this request !

👉 Book for today:
 - Today AM: today am  
 - Today PM: today pm  
 - Today Full Day: today

👉 Book for some days (not consecutive):  
 - Book for full day (06/10, 9/10, 9/11): 06/10, 9/10, 9/11  
 - Book for AM/PM/FULL (06/10 am, 9/10 pm, 9/11): 06/10 am, 9/10 pm, 9/11

👉 Book for a range of days (consecutive and always full day):  
 - Book for 06-Oct to 9-Oct: 6-9/10 or 06-09/10
");
        
    }

    private async Task ProcessVacationScheduleInputAsync(long userId, long chatId, string textTrim, string textUpper, string nickname)
    {
        if (textTrim.ToUpper() == "N")
        {
            waitingForResponse.Remove(userId);
            pendingBooking.Remove(userId);
            confirmationStart.Remove(userId);
            userChatMap.Remove(userId);
            await SendText(chatId, $"❌ Request is cancelled. Bye, {nickname}!");
            return;
        }
        if (textTrim.ToUpper() == "/VACATION")
        {
            waitingForResponse.Remove(userId);
            pendingBooking.Remove(userId);
            confirmationStart.Remove(userId);
            userChatMap.Remove(userId);
            await SendText(chatId, "⚠️ The duplicated request, I closed for now and please give me the new request again.");
            return;
        }
        var parseResult = ParseVacationInput(textTrim);
        bool parseFailed = parseResult.Count == 1 && parseResult[0].StartsWith("❌");

        if (parseFailed)
        {
            await SendText(chatId, parseResult[0]);
            return;
        }

        pendingBooking[userId] = textTrim;
        confirmationStart[userId] = DateTime.UtcNow;

        await SendText(chatId, $"Can you confirm this booking (Y/N)/(yes/no)?\n\n👉 {textTrim}");
    }

    private async Task ProcessVacationConfirmationAsync(long userId, long chatId, string textUpper, string nickname)
    {
        if (textUpper == "Y" || textUpper == "YES")
        {
            var bookingRaw = pendingBooking[userId];
            var parsed = ParseVacationInput(bookingRaw);

            if (parsed.Count == 1 && parsed[0].StartsWith("❌"))
            {
                await SendText(chatId, parsed[0]);
            }
            else
            {
                string emailAddress = await FuckTheLifeToFindTheLuck.FindAccountByTelegramAccount(nickname.TrimStart('@'));

                if (!string.IsNullOrEmpty(emailAddress))
                {
                    string emailPrefix = emailAddress.ToLower().Split('@')[0];
                    string userVacationFile = Path.Combine(configSettings.RootDataPath, $"vacation_{emailPrefix}.txt");
                    if (!Directory.Exists(AppConst.vacationFolder)) { Directory.CreateDirectory(AppConst.vacationFolder); }
                    using (StreamWriter writer = new StreamWriter(userVacationFile, append: true))
                    {
                        foreach (var res in parsed)
                        {
                            writer.WriteLine(res.Replace("/", "-"));
                        }
                    }
                    foreach (var res in parsed)
                    {
                        await SendText(chatId, $"{nickname} requested day: {res}");
                    }

                    await SendText(chatId, $"{nickname} booking has been confirmed, bye for now.");
                }
                else
                {
                    await SendText(chatId, $"You need to add telegram nickname into the healing app.");
                    waitingForResponse.Remove(userId);
                    pendingBooking.Remove(userId);
                    confirmationStart.Remove(userId);
                    userChatMap.Remove(userId);
                }
            }

            // ✅ Clear ALL state on success
            waitingForResponse.Remove(userId); // Use .Remove() instead of [userId] = false
            pendingBooking.Remove(userId);
            confirmationStart.Remove(userId);
            userChatMap.Remove(userId);
        }
        else if (textUpper == "N" || textUpper == "NO")
        {
            await SendText(chatId, $"❌ Request is cancelled. Bye, {nickname}!");

            waitingForResponse.Remove(userId);
            pendingBooking.Remove(userId);
            confirmationStart.Remove(userId);
            userChatMap.Remove(userId);
        }
        else
        {
            //await SendText(chatId, "⚠️ Please reply (Y/N).");
            waitingForResponse.Remove(userId);
            pendingBooking.Remove(userId);
            confirmationStart.Remove(userId);
            userChatMap.Remove(userId);
            //confirmationStart[userId] = DateTime.UtcNow;
        }
    }
    private async Task ProcessGetMeInReplyAsync(long userId, long chatId, string textUpper, string nickname, string? chatType)
    {
        if (chatType != "private")
        {
            return;
        }

        if (waitingForGetMeInFirstConfirmation.ContainsKey(userId) && waitingForGetMeInFirstConfirmation[userId])
        {
            if (textUpper == "Y" || textUpper == "YES")
            {
                waitingForGetMeInFirstConfirmation.Remove(userId);
                waitingForGetMeInSecondConfirmation[userId] = true;
                confirmationStart[userId] = DateTime.UtcNow;

                string emailAddress = await FuckTheLifeToFindTheLuck.FindAccountByTelegramAccount(nickname.TrimStart('@'));

                if (!string.IsNullOrEmpty(emailAddress))
                {
                    string accountFileName = Path.Combine(AppConst.personCred, emailAddress.Trim().ToLower().Split('@')[0]);
                    if (File.Exists(accountFileName))
                    {
                        Person aPerson = new Person();

                        aPerson = FuckTheLifeToFindTheLuck.GetLoginCred(accountFileName);
                        await SendText(chatId, $"{nickname} ✅ OK, man. Stay tuned ! I'll give you the code to enter soon.");
                        int userPort = await FuckTheLifeToFindTheLuck.FindChromePortByEmai(emailAddress);
                        if (userPort != 0)
                        {
                            string userName = emailAddress.Split('@')[0].Replace(".", "");
                            string userProfile = Path.Combine(configSettings.ChromeProfiles, userName);
                            string cookiesPath = Path.Combine(userProfile, "Default", "Network", "Cookies");
                            if (File.Exists(cookiesPath))
                            {
                                File.SetAttributes(cookiesPath, FileAttributes.Normal);
                                File.Delete(cookiesPath);
                            }

                            var (loginStatus, sessionId, driver) = await FuckTheLifeToFindTheLuck.CRVLogin(emailAddress, aPerson.accountPassword, userPort, true, true, chatId);

                            if (!string.IsNullOrEmpty(sessionId))
                            {
                                List<Employee> employees = FuckTheLifeToFindTheLuck.ReadUserDetails(configSettings.RootDataPath + @"\" + configSettings.UserAccountFile);

                                for (int i = 0; i < employees.Count; i++)
                                {
                                    Employee employee = employees[i];

                                    if (employee.personName == emailAddress)
                                    {
                                        employee.sessionID = sessionId;
                                        employees[i] = employee;
                                        break;
                                    }
                                }
                                FuckTheLifeToFindTheLuck.WriteUserDetails(configSettings.RootDataPath + @"\" + configSettings.UserAccountFile, employees);
                                driver?.Quit();
                            }
                            else 
                            { 
                                await SendText(chatId, $"❌ No session found {nickname}, let's try again.");
                                waitingForGetMeInFirstConfirmation.Remove(userId);
                                confirmationStart.Remove(userId);
                            }
                        }
                    }
                    else 
                    { 
                        await SendText(chatId, $"❌ No asterisk found {nickname}, find the Set Asterisk Button in healing.");
                        waitingForGetMeInFirstConfirmation.Remove(userId);
                        confirmationStart.Remove(userId);
                    }
                    
                }
                else
                {
                    await SendText(chatId, $"No info about you.");
                    waitingForGetMeInFirstConfirmation.Remove(userId);
                    confirmationStart.Remove(userId);
                }

                
            }
            else if (textUpper == "N" || textUpper == "NO")
            {
                await SendText(chatId, $"❌ Request is cancelled {nickname}. Bye!");
                waitingForGetMeInFirstConfirmation.Remove(userId);
                confirmationStart.Remove(userId);
            }
            else
            {
                await SendText(chatId, "⚠️ Duplicated request, I closed for now and please give me the new request.");
                waitingForGetMeInFirstConfirmation.Remove(userId);
                confirmationStart.Remove(userId);
            }
        }
        else if (waitingForGetMeInSecondConfirmation.ContainsKey(userId) && waitingForGetMeInSecondConfirmation[userId])
        {
            if (textUpper == "Y" || textUpper == "YES")
            {
                //await SendText(chatId, "🎉 Wait a minutes !.");

                waitingForGetMeInSecondConfirmation.Remove(userId);
                confirmationStart.Remove(userId);
                waitingForGetMeInFirstConfirmation.Remove(userId);
                confirmationStart.Remove(userId);
            }
            else if (textUpper == "N" || textUpper == "NO")
            {
                //await SendText(chatId, $"❌ Get-in request cancelled by {nickname} after the second prompt. Bye!");
                waitingForGetMeInSecondConfirmation.Remove(userId);
                confirmationStart.Remove(userId);
                waitingForGetMeInFirstConfirmation.Remove(userId);
                confirmationStart.Remove(userId);
            }
            else
            {
                //await SendText(chatId, "⚠️ Please reply with Y or N to confirm your final request.");
                waitingForGetMeInSecondConfirmation.Remove(userId);
                confirmationStart.Remove(userId);
                waitingForGetMeInFirstConfirmation.Remove(userId);
                confirmationStart.Remove(userId);
            }
        }
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

        // --- Handle range format (6-9/10)
        if (input.Contains("-"))
        {
            var rangeParts = input.Split("-", StringSplitOptions.RemoveEmptyEntries);
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

    public static Person GetLoginCred(string filePath)
    {
        Person people = new Person();

        if (File.Exists(filePath))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                string emailAddress = reader.ReadString();
                string passwordEmail = DecryptString(AppConst.encryptKey, reader.ReadString());

                people.accountEmail = emailAddress;
                people.accountPassword = passwordEmail;

            }
        }
        return people;
    }
    public static void SaveLoginCred(string filePath, Person people)
    {
        try
        {
            if (!string.IsNullOrEmpty(people.accountPassword))
            {
                string passwordEncrypt = EncryptString(AppConst.encryptKey, people.accountPassword);
                people.accountPassword = passwordEncrypt;
            }
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                writer.Write(people.accountEmail ?? string.Empty);
                writer.Write(people.accountPassword ?? string.Empty);
            }
        }
        catch (Exception ex)
        {
            FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
        }
    }

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
            FuckTheLifeToFindTheLuck.LogMessage(ex.Message, AppConst.findtheluckLogFile);
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

    public static async Task<(bool loginStatus, string sessionID, ChromeDriver? driver)> CRVLogin(string emailAddress, string emailPassword, int userPort, bool loginAuth, bool botLogin, long chatId)
    {
        var configuration = mtv.ConfigurationHelper.BuildConfiguration();
        var configSettings = new mtv.ConfigurationSettings(configuration);
        bool iSuccess = false;
        string iSession = string.Empty;
        ChromeDriver? driver = null;
        string userName = emailAddress.Split('@')[0].Replace(".", "");
        string userProfile = Path.Combine(configSettings.ChromeProfiles, userName);

        string chromePath = FuckTheLifeToFindTheLuck.FindChromePath();
        var chromeDriverService = ChromeDriverService.CreateDefaultService();
        chromeDriverService.HideCommandPromptWindow = true;

        var options = new ChromeOptions();
        options.BinaryLocation = chromePath;
        options.AddUserProfilePreference("profile.default_content_settings.geolocation", 1);
        options.AddArgument($"--remote-debugging-port={userPort}");
        options.AddArgument($@"--user-data-dir={userProfile}");
        options.AddArgument("--disable-infobars");
        options.AddArgument("--start-maximized");

        driver = new ChromeDriver(chromeDriverService, options);
        await Task.Delay(TimeSpan.FromSeconds(20));

        driver.Navigate().GoToUrl(AppConst.signInUrl);
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(35));

        try
        {
            // Bot Login
            if (loginAuth && botLogin)
            {
                TelegramBotService telegramService = new TelegramBotService();
                bool success = false;
                string jsessionId = string.Empty;

                await Task.Delay(TimeSpan.FromSeconds(5));

                // Fill the email
                var emailBox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("i0116")));
                emailBox.Clear();

                // Fill email then click Next Button
                emailBox.SendKeys(emailAddress);
                var nextButton = driver.FindElement(By.Id("idSIButton9"));
                nextButton.Click();
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Password Button
                var passwordBox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("i0118")));
                passwordBox.Clear();
                passwordBox.SendKeys(emailPassword);
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Click Sign In
                var signInButton = driver.FindElement(By.Id("idSIButton9"));
                signInButton.Click();

                await Task.Delay(TimeSpan.FromSeconds(10));

                try
                {
                    // Get the MFA code
                    var displaySignElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("idRichContext_DisplaySign")));
                    string mfaCode = displaySignElement.Text.Trim();

                    // Inform to ask put in MFA
                    await telegramService.SendText(chatId, "Here you are: " + mfaCode);
                }

                catch (WebDriverTimeoutException)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        var passwordError = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("passwordError")));
                        string errorMessage = passwordError.Text.Trim();
                        await telegramService.SendText(chatId, $"❌ Update your account in the healing app !" + "error: " + errorMessage);

                    }
                    catch (WebDriverTimeoutException)
                    {
                        await telegramService.SendText(chatId, "⚠️ Webpage is frozen, let's try again or check manually.");

                    }
                }

                // Check to the Do not ask again for 30 days
                var dontAskAgainCheckbox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id("idChkBx_SAOTCAS_TD")));
                ((IJavaScriptExecutor)driver).ExecuteScript("document.getElementById('idChkBx_SAOTCAS_TD').checked = true;");

                await Task.Delay(TimeSpan.FromSeconds(5));

                var dontShowAgain = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("KmsiCheckboxField")));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].checked = true;", dontShowAgain);
                await Task.Delay(TimeSpan.FromSeconds(1));

                var yesButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("idSIButton9")));
                yesButton.Click();
                await Task.Delay(TimeSpan.FromSeconds(5));

                for (int i = 0; i < 120; i++)
                {
                    var cookie = driver.Manage().Cookies.GetCookieNamed("JSESSIONID");
                    if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                    {
                        jsessionId = cookie.Value;
                        success = true; iSuccess = true; iSession = jsessionId;
                        // Inform login successful when catch the session
                        await telegramService.SendText(chatId, "Yours: " + iSession + " Congratulation! Bye");

                        return (success, jsessionId, driver);
                    }
                    else { await Task.Delay(TimeSpan.FromSeconds(1)); } // Sleep 1 second to wait for session available
                }

            }
            // Human Login
            if (loginAuth && botLogin == false)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                string jsessionId = string.Empty;
                bool success = false;

                for (int i = 0; i < 120; i++)
                {
                    var cookie = driver.Manage().Cookies.GetCookieNamed("JSESSIONID");
                    if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                    {
                        jsessionId = cookie.Value;
                        success = true; iSuccess = true; iSession = jsessionId;
                        return (success, jsessionId, driver);
                    }
                    else { await Task.Delay(TimeSpan.FromSeconds(1)); } // Sleep 1 second to wait for session available
                }
                
            }

            // This is for the test session expired
            
            if (loginAuth == false)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                string jsessionId = string.Empty;
                bool success = false;

                try
                {
                    for (int i = 0; i < 120; i++)
                    {
                        var cookie = driver.Manage().Cookies.GetCookieNamed("JSESSIONID");
                        if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                        {
                            jsessionId = cookie.Value;
                            success = true; iSuccess = true; iSession = jsessionId;
                            return (success, jsessionId, driver);
                        }
                        else { await Task.Delay(TimeSpan.FromSeconds(1)); } // Sleep 1 second to wait for session available
                    }
                    if (!string.IsNullOrEmpty(iSession))
                    {
                        TelegramBotService telegramService = new TelegramBotService();
                        await telegramService.SendText(chatId, "Your session is expired please chat to the bot  with /getmein command to login.");
                    }
                }
                catch (Exception ex)
                {
                    TelegramBotService telegramService = new TelegramBotService();
                    LogMessage(ex.ToString(), AppConst.findtheluckLogFile);
                    await telegramService.SendText(chatId, "Your session is expired please chat to the bot  with /getmein command to login.");
                }
            }


            return (iSuccess, iSession, driver);
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

                    await Task.Delay(TimeSpan.FromSeconds(3));

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

                    await Task.Delay(TimeSpan.FromSeconds(3));

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

    public static async Task<int> FindChromePortByEmai(string emailAddress)
    {
        var configuration = mtv.ConfigurationHelper.BuildConfiguration();
        var configSettings = new mtv.ConfigurationSettings(configuration);

        string filePath = Path.Combine(configSettings.RootDataPath, configSettings.UserAccountFile);

        List<Employee>? employees = FuckTheLifeToFindTheLuck.ReadUserDetails(filePath);
        int maxAttempts = 5;
        int attempt = 0;
        int result = 0;
        while (attempt < maxAttempts)
        {
            try
            {
                foreach (Employee employee in employees)
                {
                    if (employee.personName == emailAddress)
                    {
                        result = employee.chromePort;
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