using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using XSystem.Security.Cryptography;

namespace QC_Management.ViewModels
{
    public class ChangePasswordViewModel: BaseViewModel
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

        private string _NewPassword;
        public string NewPassword { get => _NewPassword; set { _NewPassword = value; OnPropertyChanged(); } }


        public ICommand CloseCommand { get; set; }
        public ICommand NewPasswordChangedCommand { get; set; }
        public ICommand RepeatPasswordChangedCommand { get; set; }
        public ICommand RegisterCommand { get; set; }
        public ICommand ChangePasswordCommand { get; set; }
        // mọi thứ xử lý sẽ nằm trong này
        public ChangePasswordViewModel()
        {
            Password = "";
            UserName = "";
            CloseCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { p.Close(); });
            NewPasswordChangedCommand = new RelayCommand<PasswordBox>((p) => { return true; }, (p) => { NewPassword = p.Password; });
            RepeatPasswordChangedCommand = new RelayCommand<PasswordBox>((p) => { return true; }, (p) => { RepeatPassword =p.Password; });
            ChangePasswordCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { ChangePassword(p); });
        }
        void ChangePassword(Window p)
        {
            var accCount = DataProvider.Ins.DB.Users.Where(x => x.UserName == UserName).Count();
            if (accCount == 0)
            {
                MessageBox.Show("Tài khoản không tồn tại, vui lòng kiểm tra lại");
            }
            
            else if (NewPassword != RepeatPassword)
            {
                MessageBox.Show("Nhập lại mật khẩu không đúng, Vui lòng kiểm tra lại");
            }
            else
            {
                string passEncode = MD5Hash(Base64Encode(NewPassword));
                var user = DataProvider.Ins.DB.Users.Where(s => s.UserName == UserName).FirstOrDefault();
                user.Password = passEncode;
                try
                {
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    p.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
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
