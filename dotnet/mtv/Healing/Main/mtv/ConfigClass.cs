using Microsoft.Extensions.Configuration;
using System;

namespace mtv
{
    public class ConfigurationSettings
    {
        public IConfiguration Configuration { get; }

        public ConfigurationSettings(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string RootDataPath => Configuration["Settings:DefaultDataPath"] ?? "";
        public string ChromeProfiles => Configuration["Settings:AllChromeProfiles"] ?? "";
        public string UserAccountFile => Configuration["Settings:UserAccountFileName"] ?? "";
        public string PublicHolidaysFile => Configuration["Settings:PublicHolidayFileName"] ?? "";
        public string TelegramBotToken => Configuration["Settings:TelegramBotToken"] ?? "";
        public string TelegramBot8 => Configuration["Settings:TelegramBotChat"] ?? "";
        public string TelegramChatIdMember => Configuration["Settings:TelegramChatId_Member"] ?? "";
        public string TelegramChatIdOperator => Configuration["Settings:TelegramChatId_Operator"] ?? "";
        public string HealthCheckDuration => Configuration["Settings:HealthCheck_Duration_Minute"] ?? "";
        public string Random_Min => Configuration["Settings:Random_Min_Minute"] ?? "";
        public string Random_Max => Configuration["Settings:Random_Max_Minute"] ?? "";
        public string RetryTimes => Configuration["Settings:NumberOfRetry"] ?? "";
        public string TestOnly => Configuration["Settings:Testing"] ?? "";
    }

    public static class ConfigurationHelper
    {
        public static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            return builder.Build();
        }
    }

}
