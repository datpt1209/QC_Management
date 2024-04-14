using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
                var config = System.Configuration.ConfigurationManager.AppSettings;
                UserName = config["Username"];
                PassWord = config["Password"];
                Server = config["Server"];
                Database = config["Database"];
            });

            okButton_Click = new RelayCommand<Window>((p) =>
            {
                return true;

            }, (p) =>
            {
                Configuration config = System.Configuration.
                        ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["Server"].Value = Server;
                config.AppSettings.Settings["Database"].Value = Database;
                config.AppSettings.Settings["Password"].Value = PassWord;
                config.AppSettings.Settings["Username"].Value = UserName;

                try
                {
                    config.Save(ConfigurationSaveMode.Full);
                    MessageBox.Show("Luu thanh cong");
                    p.Close();
                    
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                config.Save(ConfigurationSaveMode.Full);
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
              
            });

            TestConnect_click = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, (p) =>
            {
                AppConfig.Password = PassWord;
                AppConfig.Username = UserName;
                AppConfig.Database = Database;
                AppConfig.Server = Server;

                var con = AppConfig.BuildConnectionString();

                var db = new QcManagmentContext();
                try
                {
                    var kq = db.Database.CanConnect();
                    //System.Threading.Thread.Sleep(3000);
                    //__connection.Open();
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
