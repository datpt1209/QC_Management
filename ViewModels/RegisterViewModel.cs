using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XSystem.Security.Cryptography;

namespace QC_Management.ViewModels
{
    public class RegisterViewModel: BaseViewModel
    {
        public bool IsLogin { get; set; }
        public User currentUser { get; set; }

        private string _FullName;
        public string FullName { get => _FullName; set { _FullName = value; OnPropertyChanged(); } }

        private UserRole _SelectedRole;
        public UserRole SelectedRole { get => _SelectedRole; set { _SelectedRole = value; OnPropertyChanged(); } }

        private ObservableCollection<UserRole> _ListRole;
        public ObservableCollection<UserRole> ListRole { get => _ListRole; set { _ListRole = value; OnPropertyChanged(); } }

        private string _UserName;
        public string UserName { get => _UserName; set { _UserName = value; OnPropertyChanged(); } }

        private string _Password;
        public string Password { get => _Password; set { _Password = value; OnPropertyChanged(); } }
        private string _RepeatPassword;
        public string RepeatPassword { get => _RepeatPassword; set { _RepeatPassword = value; OnPropertyChanged(); } }

        public ICommand CloseCommand { get; set; }
        public ICommand PasswordChangedCommand { get; set; }
        public ICommand RepeatPasswordChangedCommand { get; set; }
        public ICommand RegisterCommand { get; set; }
        // mọi thứ xử lý sẽ nằm trong này
        public RegisterViewModel()
        {
            ListRole = new ObservableCollection<UserRole>(DataProvider.Ins.DB.UserRoles);
            Password = "";
            UserName = "";
            RegisterCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { Regis(p); });
            CloseCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { p.Close(); });
            PasswordChangedCommand = new RelayCommand<PasswordBox>((p) => { return true; }, (p) => { Password = p.Password; });
            RepeatPasswordChangedCommand = new RelayCommand<PasswordBox>((p) => { return true; }, (p) => { RepeatPassword = p.Password; });
        }

        void Regis(Window p)
        {
            
            var accCount = DataProvider.Ins.DB.Users.Where(x => x.UserName == UserName).Count();

            if (accCount > 0)
            {
                MessageBox.Show("Tài khoản đã tồn tại, vui lòng tạo tài khoản với tên đăng nhập khác");
            }
            else if (Password != RepeatPassword)
            {
                MessageBox.Show("Nhập mật khẩu lặp lại không đúng");
            }
            else
            {
                string passEncode = MD5Hash(Base64Encode(Password));
                var user = new User
                {
                    UserName = UserName,
                    Password = passEncode,
                    DisplayName = FullName,
                    RoleNavigation = SelectedRole,
                    Role = SelectedRole.Id
                };
                try
                {
                    DataProvider.Ins.DB.Add(user);
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Đăng ký tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    p.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Có lỗi:{ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
