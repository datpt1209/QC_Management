using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {

        private bool _IsIndeterminate;
        public bool IsIndeterminate { get => _IsIndeterminate; set { _IsIndeterminate = value; OnPropertyChanged(); } }
        public bool IsLogin { get; set; }
        public User currentUser { get; set; }
        private string _UserName;
        public string UserName { get => _UserName; set { _UserName = value; OnPropertyChanged(); } }
        private string _Password;
        public string Password { get => _Password; set { _Password = value; OnPropertyChanged(); } }

        public ICommand CloseCommand { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand PasswordChangedCommand { get; set; }
        public ICommand RegisterCommand { get; set; }

        public ICommand Window_Loaded { get; set; }

        public ICommand ForgotPasswordCommand { get; set; }

        public ICommand ConfigCommand { get; set; }

        // mọi thứ xử lý sẽ nằm trong này
        public LoginViewModel()
        {
            Password = "";
            UserName = "";
            Window_Loaded = new RelayCommand<Window>((p) =>
            {
                return true;

            }, (p) =>
            {
                Load(p);
            });
            LoginCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { Login(p); });
            RegisterCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { Regis(p); });
            ForgotPasswordCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { ForgotPassword(p); });
            CloseCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { p.Close(); });
            PasswordChangedCommand = new RelayCommand<PasswordBox>((p) => { return true; }, (p) => { Password = p.Password; });
            ConfigCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { Config(p); });
        }

        private async void Login(Window p)
        {
           IsIndeterminate = true;
            var (success, message) = await Task.Run(() =>
            {
                bool success = true;
                string message = "";
                try
                {
                    DataProvider.Ins.DB = new QcManagmentContext();
                    if (DataProvider.Ins.DB.Database.CanConnect())
                    {
                        string passEncode = MD5Hash(Base64Encode(Password));
                        var accCount = DataProvider.Ins.DB.Users.Where(x => x.UserName == UserName && x.Password == passEncode).Count();
                        if (accCount == 0)
                        {
                            success = false;
                            IsIndeterminate = false;
                            message = "Tài khoản hoặc mật khẩu không đúng!";
                        }
                    }
                    else 
                    {        
                        success = false;
                       
                    }
                   
                }
                catch (Exception ex)
                {
                    success = false;
                    message = ex.Message;
                }
                return new Tuple<bool, string>(success, message);
            });
            if (success)
            {
                string passEncode = MD5Hash(Base64Encode(Password));
                IsIndeterminate = false;
                currentUser = new User();
                currentUser = DataProvider.Ins.DB.Users.Where(x => x.UserName == UserName && x.Password == passEncode).FirstOrDefault();
                UserManager.Instance.CurrentUser = currentUser;
                MainWindow view = new MainWindow();
                view.Show();
                p.Close();
        
            }
            else
            {
                IsIndeterminate = false;
                MessageBox.Show($"Cannot login {message}");
            }
        }

        private async void Load(Window p)
        {
       
            //var (success, message) = await Task.Run(() =>
            //{
            //    bool success = true;
            //    string message = "";
            //    try
            //    {
            //        DataProvider.Ins.DB = new QcManagmentContext();
            //        if (!DataProvider.Ins.DB.Database.CanConnect())
            //        {
            //            success = false;
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        success = false;
            //        message = ex.Message;
            //    }
            //    return new Tuple<bool, string>(success, message);
            //});
            //if (success)
            //{
            //    p.Show();
            //}
            //else
            //{
            //    MessageBox.Show($"Connect Database fails: {message}");
            //    var config = new ServerConfig();
            //    config.Show();
            //}
            p.Show();
        }
        void Regis(Window p)
        {
            RegisterView view = new RegisterView();
            view.ShowDialog();
        }

        void ForgotPassword(Window p)
        {
            ChangePassword view = new ChangePassword();
            view.ShowDialog();
        }

        void Config(Window p)
        {
            ServerConfig view = new ServerConfig();
            view.ShowDialog();
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }



        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            XSystem.Security.Cryptography.MD5CryptoServiceProvider md5provider = new XSystem.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }


    }
}
