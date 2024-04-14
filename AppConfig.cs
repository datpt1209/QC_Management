using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace QC_Management
{
    public class AppConfig
    {
        public static string Username { get; set; } = "";
        public static string Password { get; set; } = "";
        public static string Server { get; set; } = "";
        public static string Database { get; set; } = "";
        public static string Entropy { get; set; } = "";
        public static string PasswordIn64 { get; set; } = "";


        public static void ReloadSetting()
        {
            var config = System.Configuration.ConfigurationManager.AppSettings;
            Username = config["Username"] ?? "";
            PasswordIn64 = config["Password"] ?? "";
            var entropyIn64 = config["Entropy"] ?? "";
            Server = config["Server"] ?? "";
            Database = config["Database"] ?? "";

            if (PasswordIn64.Length != 0)
            {
                var passwordInBytes = Convert.FromBase64String(PasswordIn64);
                var entropyInBytes = Convert.FromBase64String(entropyIn64);
                var unencryptedPassword = ProtectedData.Unprotect(passwordInBytes, entropyInBytes, DataProtectionScope.CurrentUser);
                Password = Encoding.UTF8.GetString(unencryptedPassword);
            }
        }
        public static void Save()
        {
            Configuration config = System.Configuration.
                        ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Username"].Value = Username;
            config.AppSettings.Settings["Password"].Value = PasswordIn64;
            config.AppSettings.Settings["Entropy"].Value = Entropy;
            config.Save(ConfigurationSaveMode.Full);
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        }
        public static string BuildConnectionString()
        {
            var config = System.Configuration.ConfigurationManager.AppSettings;
            Username = config["Username"];
            Password = config["Password"];
            Server = config["Server"];
            Database = config["Database"];
            //ReloadSetting();
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder();
            builder.DataSource = Server;
            builder.InitialCatalog = Database;
            builder.TrustServerCertificate = true;
            builder.UserID = Username;
            builder.Password = Password;

            string connectionString = builder.ConnectionString;
            return connectionString;

        }

    }
}
