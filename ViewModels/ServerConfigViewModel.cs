using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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

                    string connectionString = AppConfig.GetConnectionString("QC_ManagmentDB");
                    var parameters = connectionString.Split(';')
                                                     .Select(part => part.Split('='))
                                                     .ToDictionary(split => split[0].Trim(), split => split[1].Trim());

                    if (parameters.ContainsKey("Trust Server Certificate"))
                    {
                        // Handle the 'trust server certificate' parameter as needed
                        parameters.Remove("Trust Server Certificate");
                    }

                    var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
                    foreach (var param in parameters)
                    {
                        builder[param.Key] = param.Value;
                    }

                    Server = builder.DataSource;
                    Database = builder.InitialCatalog;
                    UserName = builder.UserID;
                    PassWord = builder.Password;
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
                string serverName = Server;
                string databaseName = Database;
                string userId = UserName;
                string password = PassWord;

               
                string connectionString = $"Data Source = {serverName}; Initial Catalog = {databaseName}; User ID = {userId}; Password = {password}; Trust Server Certificate = True";
                
                AppConfig.SaveConnectionString("QC_ManagmentDB", connectionString);
                AppConfig.EncryptConfigSection("connectionStrings");
                MessageBox.Show("Configuration saved and encrypted successfully!");
                p.Close();

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
