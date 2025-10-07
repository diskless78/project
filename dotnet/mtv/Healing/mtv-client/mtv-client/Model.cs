using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace mtv_client
{
    public struct Employee
    {
        public string EmailAddress;
        public string EmailPassword;
        public string TelegramUserName;


        public Employee(string emailAddress, string emailPassword, string telegramNickName)
        {
            EmailAddress = emailAddress;
            EmailPassword = emailPassword;
            TelegramUserName = telegramNickName;
        }
    }

    public class AppSetting
    {
        public const string encryptKey = "a0e51106cafe3073f9dd7ab7f7cef7ea";
        public static readonly string errorLogFile = Path.Combine(Path.GetTempPath(), "client-error-logs" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".log");
        public static string DataSharedFolder { get; private set; } = ConfigurationManager.AppSettings["UNC_DataFolder"] ?? "";
        public static string UNCUserName { get; private set; } = ConfigurationManager.AppSettings["UNC_UserName"] ?? "";
        public static string UNCPassword { get; private set; } = ConfigurationManager.AppSettings["UNC_Password"] ?? "";
        public static string App_UUID { get; private set; } = ConfigurationManager.AppSettings["Machine_UUID"] ?? "";
        public static string UserAccountFile { get; private set; } = ConfigurationManager.AppSettings["BinaryAccountFile"] ?? "";
        public static string MailWhiteList { get; private set; } = ConfigurationManager.AppSettings["EmailWhiteList"] ?? "";
        public static string Holidays { get; private set; } = ConfigurationManager.AppSettings["PublicHolidays"] ?? "";
        public static string UserVacationFolder { get; private set; } = ConfigurationManager.AppSettings["RootVacationFolder"] ?? "";

        public static void LoadConfiguration()
        {
            DataSharedFolder = ConfigurationManager.AppSettings["UNC_DataFolder"] ?? "";
            UNCUserName = ConfigurationManager.AppSettings["UNC_UserName"] ?? "";
            UNCPassword = ConfigurationManager.AppSettings["UNC_Password"] ?? "";
            App_UUID = ConfigurationManager.AppSettings["Machine_UUID"] ?? "";
            UserAccountFile = ConfigurationManager.AppSettings["BinaryAccountFile"] ?? "";
            MailWhiteList = ConfigurationManager.AppSettings["EmailWhiteList"] ?? "";
            Holidays = ConfigurationManager.AppSettings["PublicHolidays"] ?? "";
            UserVacationFolder = ConfigurationManager.AppSettings["RootVacationFolder"] ?? "";

        }

    }

    public class CredentialManager
    {
        [DllImport("mpr.dll")]
        private static extern int WNetUseConnection(
     IntPtr hwndOwner,
     NETRESOURCE lpNetResource,
     string lpPassword,
     string lpUserID,
     int dwFlags,
     string? lpAccessName,
     ref int lpBufferSize,
     out int lpResult);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(
            string lpName,
            int dwFlags,
            bool fForce);

        [StructLayout(LayoutKind.Sequential)]
        public class NETRESOURCE
        {
            public int dwScope = 0;
            public int dwType = 1; // RESOURCETYPE_DISK
            public int dwDisplayType = 0;
            public int dwUsage = 0;
            public string lpLocalName = "";
            public string lpRemoteName = "";
            public string lpComment = "";
            public string lpProvider = "";
        }

        public static bool MapNetworkDrive(string networkName, string username, string password)
        {
            NETRESOURCE netResource = new NETRESOURCE()
            {
                dwType = 1, // Disk Drive
                lpRemoteName = networkName
            };

            int result;
            int bufferSize = 0;
            result = WNetUseConnection(IntPtr.Zero, netResource, password, username, 0, null, ref bufferSize, out result);

            if (result != 0)
            {
                throw new IOException("Error connecting to remote share", result);
            }
            else
            {
                return true;
            }
        }

    }

}
