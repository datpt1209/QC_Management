using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using XAct.Library.Settings;

namespace QC_Management.ViewModels
{
    public class ServerConfigViewModel:BaseViewModel
    {
        private string _UserName;
        public string UserName
        {
            get => _UserName;
            set
            {
                _UserName = value;
                OnPropertyChanged();
            }
        }
        private string _PassWord;
        public string PassWord
        {
            get => _PassWord;
            set
            {
                _PassWord = value;
                OnPropertyChanged();
            }
        }
        private string _Server;
        public string Server
        {
            get => _Server;
            set
            {
                _Server = value;
                OnPropertyChanged();
            }
        }
        private string _Database;
        public string Database
        {
            get => _Database;
            set
            {
                _Database = value;
                OnPropertyChanged();
            }
        }

        public ICommand Window_Loaded { get; set; }
        public ICommand okButton_Click { get; set; }
        public ICommand TestConnect_click { get; set; }

        public ServerConfigViewModel()
        {
            Window_Loaded = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, (p) =>
            {
                try
                {
                    AppConfig.ReloadSetting();
                    UserName = AppConfig.Username;
                    PassWord = AppConfig.Password;
                    Database = AppConfig.Database;
                    Server = AppConfig.Server;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            okButton_Click = new RelayCommand<Window>((p) =>
            {
                return true;

            }, (p) =>
            {
                try
                {
                    Configuration config = System.Configuration.
                        ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["Server"].Value = Server;
                    config.AppSettings.Settings["Database"].Value = Database;
                    config.AppSettings.Settings["Username"].Value = UserName;
                    var encryptedPassword = AesEncryption.Encrypt(PassWord);
                    config.AppSettings.Settings["Password"].Value = encryptedPassword;
                    config.Save(ConfigurationSaveMode.Full);
                    System.Configuration.ConfigurationManager.RefreshSection("appSettings");

                    MessageBox.Show("Configuration saved successfully.");
                    p.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving configuration: {ex.Message}");
                }
            });

            TestConnect_click = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, (p) =>
            {
                var db = new QcManagmentContext();
                try
                {
                    var kq = db.Database.CanConnect();
            
                    if(kq)
                        MessageBox.Show("Ket noi thanh cong");
                        DataProvider.Ins.DB = new QcManagmentContext();
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            });
        }

    }
}
