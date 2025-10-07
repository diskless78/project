using SystemConfig = System.Configuration.ConfigurationManager;
using MicrosoftConfig = Microsoft.Extensions.Configuration;
using OpenQA.Selenium.DevTools.V121.DOM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mtv
{
    public struct Employee
    {
        public string personName;
        public string sessionID;
        public string telegramID;
        public int chromePort;


        public Employee(string emailAddress, string jSessionId, string telegramNickName, int ownerPort)
        {
            personName = emailAddress;
            sessionID = jSessionId;
            telegramID = telegramNickName;
            chromePort = ownerPort;
        }
    }

    public struct Person
    {
        public string accountEmail;
        public string accountPassword;

        public Person(string emailAccount, string passwordAccount)
        {
            accountEmail = emailAccount;
            accountPassword = passwordAccount;
        }
    }

    public class TelegramChat
    {
        public long Id { get; set; }
        public string? Type { get; set; }
    }

    public class TelegramUser
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class TelegramMessage
    {
        public long MessageId { get; set; }
        public TelegramChat? Chat { get; set; }
        public TelegramUser? From { get; set; }
        public string? Text { get; set; }
    }

    public class TelegramUpdate
    {
        public long UpdateId { get; set; }
        public TelegramMessage? Message { get; set; }
    }

    public class TelegramUpdateResponse
    {
        public List<TelegramUpdate>? Result { get; set; }
    }

    public struct Holidays
    {
        public bool PublicHolidayVN;

        public Holidays(bool isTodayPublicHoliday)
        {
            PublicHolidayVN = isTodayPublicHoliday;
        }
    }
    public struct  BookTime
    {
        public int StartCheckInHour;
        public int StartCheckInMinute;
        public int EndCheckInHour;
        public int EndCheckInMinute;

        public int StartCheckOutHour;
        public int StartCheckOutMinute;
        public int EndCheckOutHour;
        public int EndCheckOutMinute;

        public BookTime(int startCheckInHour, int startCheckInMinute, int endCheckInHour, int endCheckInMinute, int startCheckOutHour, int startCheckOutMinute, int endCheckOutHour, int endCheckOutMinute)
        {
            StartCheckInHour = startCheckInHour; StartCheckInMinute = startCheckInMinute;
            EndCheckInHour = endCheckInHour; EndCheckInMinute = endCheckInMinute;

            StartCheckOutHour = startCheckOutHour; StartCheckOutMinute = startCheckOutMinute;
            EndCheckOutHour = endCheckOutHour; EndCheckOutMinute = endCheckOutMinute;
        }

    }

    public static class AppConst
    {
        public const string signInUrl = "https://cnext-time.centralgroup.com";
        public const string clockTimeUrl = "https://central.wta-eu3.wfs.cloud/workforce/Mobile.do";


        // openssl rand -hex 16
        public const string encryptKey = "a0e51106cafe3073f9dd7ab7f7cef7ea";
        // wmic csproduct get UUID

        //public const string SeeMe = "DB480A42-5B0B-169E-31E3-E541EBD7E959";
        //===================================================== Below is server ======================================
        public const string SeeMe = "45882442-4580-A550-8EAE-50FD0A55C78F";

        public const int MilliSecondPerMinute = 60000;
        public const string amTime = "AM";
        public const string pmTime = "PM";
        public const string fullDay = "FULL";
        public const string timerFile = "timer.json";
        public static readonly string findtheluckLogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Logs");
        public static string vacationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vacation");
        public static string personCred = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "person_Creds");
        public static readonly string morningImageFolder = Path.Combine(Directory.GetCurrentDirectory(), "AMImage");
        public static readonly string eveningImageFolder = Path.Combine(Directory.GetCurrentDirectory(), "PMImage");
        public static readonly string findtheluckLogFile = Path.Combine(findtheluckLogDirectory,"findtheluck-log-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".log");
        public static readonly string alreadyCheckOut = Path.Combine(Directory.GetCurrentDirectory(), "checkedOut.log");
        public static readonly string alreadyCheckIn = Path.Combine(Directory.GetCurrentDirectory(), "checkedIn.log");
    }
}
