using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using QC_Management.Views;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        public User? currentUser { get; set; } // Make nullable
        private string _UserName;
        public string UserName { get => _UserName; set { _UserName = value; OnPropertyChanged(); } }
        private string _Password;
        public string Password { get => _Password; set { _Password = value; OnPropertyChanged(); } }

        private bool _RememberMe;
        public bool RememberMe { get => _RememberMe; set { _RememberMe = value; OnPropertyChanged(); } }

        public ICommand CloseCommand { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand PasswordChangedCommand { get; set; }
        public ICommand RegisterCommand { get; set; }
        public ICommand Window_Loaded { get; set; }
        public ICommand ForgotPasswordCommand { get; set; }
        public ICommand ConfigCommand { get; set; }

        // file used to store encrypted credentials
        private static string CredentialFilePath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QC_Management", "credentials.dat");

        // mọi thứ xử lý sẽ nằm trong này
        public LoginViewModel()
        {
            Password = string.Empty; // Initialize with empty string
            UserName = string.Empty; // Initialize with empty string
            Window_Loaded = new RelayCommand<Window>((p) => true, (p) => Load(p));
            LoginCommand = new RelayCommand<Window>((p) => true, (p) => Login(p));
            RegisterCommand = new RelayCommand<Window>((p) => true, (p) => Regis(p));
            ForgotPasswordCommand = new RelayCommand<Window>((p) => true, (p) => ForgotPassword(p));
            CloseCommand = new RelayCommand<Window>((p) => true, (p) => p.Close());

            // Keep PasswordBox handler as before but robustly accept object cast
            PasswordChangedCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    if (p is PasswordBox pb)
                    {
                        Password = pb.Password ?? string.Empty;
                    }
                });

            ConfigCommand = new RelayCommand<Window>((p) => true, (p) => Config(p));

            // try to restore saved credentials (if available)
            TryLoadSavedCredentials();
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
                    using (var context = new QcManagmentContext())
                    {
                        if (context.Database.CanConnect())
                        {
                            string passEncode = MD5Hash(Base64Encode(Password));
                            // Include RoleNavigation so we can resolve role at login
                            var user = context.Users
                                              .Include(u => u.RoleNavigation)
                                              .FirstOrDefault(x => x.UserName == UserName && x.Password == passEncode);
                            if (user == null)
                            {
                                success = false;
                                message = "Tài khoản hoặc mật khẩu không đúng!";
                            }
                            else
                            {
                                // resolve and store IsAdmin flag once at login
                                var roleName = user.RoleNavigation?.DisplayName;
                                if (string.IsNullOrEmpty(roleName) && user.Role != 0)
                                {
                                    var role = context.UserRoles.AsNoTracking().FirstOrDefault(r => r.Id == user.Role);
                                    roleName = role?.DisplayName;
                                }
                                user.IsAdmin = !string.IsNullOrEmpty(roleName) && string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase);

                                currentUser = user;
                            }
                        }
                        else
                        {
                            success = false;
                            message = "Cannot connect to the database.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    message = ex.Message;
                }
                return new Tuple<bool, string>(success, message);
            });

            IsIndeterminate = false;
            if (success)
            {
                if (currentUser != null)
                {
                    UserManager.Instance.CurrentUser = currentUser;

                    // Persist or clear credentials based on RememberMe
                    try
                    {
                        if (RememberMe)
                            SaveCredentials();
                        else
                            ClearSavedCredentials();
                    }
                    catch
                    {
                        // Fail silently — login should not be blocked by persistence errors
                    }

                    MainWindow view = new MainWindow();
                    view.Show();
                    p.Close();
                }
                else
                {
                    MessageBox.Show("User not found.");
                }
            }
            else
            {
                MessageBox.Show($"Cannot login: {message}");
            }
        }

        private void Load(Window p)
        {
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
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            using (var md5provider = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));
                for (int i = 0; i < bytes.Length; i++)
                {
                    hash.Append(bytes[i].ToString("x2"));
                }
            }
            return hash.ToString();
        }

        // --- Persistence helpers (uses DPAPI for encryption, stored under %APPDATA%\QC_Management\credentials.dat) ---

        private void TryLoadSavedCredentials()
        {
            try
            {
                if (!File.Exists(CredentialFilePath)) return;

                var encrypted = File.ReadAllBytes(CredentialFilePath);
                var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                var text = Encoding.UTF8.GetString(decrypted);
                // stored format: username\npassword
                var parts = text.Split(new[] { '\n' }, 2);
                if (parts.Length >= 1)
                {
                    UserName = parts[0];
                }
                if (parts.Length == 2)
                {
                    Password = parts[1];
                }
                RememberMe = true;
            }
            catch
            {
                // ignore failures — don't block startup
            }
        }

        private void SaveCredentials()
        {
            try
            {
                var folder = Path.GetDirectoryName(CredentialFilePath);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder!);

                var text = $"{UserName}\n{Password}";
                var bytes = Encoding.UTF8.GetBytes(text);
                var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(CredentialFilePath, encrypted);
            }
            catch
            {
                // ignore persistence errors
            }
        }

        private void ClearSavedCredentials()
        {
            try
            {
                if (File.Exists(CredentialFilePath))
                    File.Delete(CredentialFilePath);
            }
            catch
            {
                // ignore
            }
        }
    }
}
