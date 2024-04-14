using QC_Management.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class UserViewModel : BaseViewModel
    {
        private ObservableCollection<User> _List;
        public ObservableCollection<User> List { get => _List; set { _List = value; OnPropertyChanged(); } }
        private ObservableCollection<UserRole> _Role;
        public ObservableCollection<UserRole> Role { get => _Role; set { _Role = value; OnPropertyChanged(); } }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }

        private string _UserName;
        public string UserName { get => _UserName; set { _UserName = value; OnPropertyChanged(); } }

        private UserRole _UserRole;
        public UserRole UserRole { get => _UserRole; set { _UserRole = value; OnPropertyChanged(); } }

        private User _SelectedItem;
        public User SelectedItem
        {

            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    DisplayName = SelectedItem.DisplayName;
                    UserName = SelectedItem.UserName;
                    UserRole = SelectedItem.RoleNavigation;
                }
            }
        }
        public UserViewModel()
        {

            AddCommand = new RelayCommand<User>((p) =>
            {

                return true;

            }, (p) =>
            {
                //RegistUser wd = new RegistUser();
                //wd.ShowDialog();
                //var isRegist = wd.DataContext as RegistUserViewModel;
                //if (isRegist.isRegist)
                //{
                //    List = new ObservableCollection<User>(DataProvider.Ins.DB.Users);
                //}
            });

            LoadedCommand = new RelayCommand<Object>((p) =>
            {

                return true;

            }, (p) =>
            {
                Role = new ObservableCollection<UserRole>(DataProvider.Ins.DB.UserRoles);
                List = new ObservableCollection<User>(DataProvider.Ins.DB.Users);
            });

            EditCommand = new RelayCommand<User>((p) =>
            {
                if (SelectedItem == null) return false;
                else if (SelectedItem.DisplayName == DisplayName && SelectedItem.RoleNavigation == UserRole) return false;
                return true;

            }, (p) =>
            {
                var user = DataProvider.Ins.DB.Users.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();
                user.DisplayName = DisplayName;
                user.UserName = UserName;
                user.Role = UserRole.Id;
                user.RoleNavigation = UserRole;
                DataProvider.Ins.DB.SaveChanges();

            });

            DeleteCommand = new RelayCommand<User>((p) =>
            {
                if (SelectedItem == null) return false;
                return true;

            }, (p) =>
            {
                //MessageBoxResult result = MessageBox.Show($"Bạn có muốn xóa thông tin người dùng {SelectedItem1.DisplayName} ?", "Confirmation", MessageBoxButton.YesNo);
                //if (result == MessageBoxResult.Yes)
                //{
                //    var output = DataProvider.Ins.DB.Inputs.Where(p => p.IdUser == SelectedItem1.Id);

                //    if (output.Count() > 0)
                //    {
                //        foreach (var item in output)
                //        {
                //            item.IdUserNavigation = null;
                //            item.IdUser = 1;
                //        }
                //    }
                //    DataProvider.Ins.DB.Users.Remove(SelectedItem1);
                //    DataProvider.Ins.DB.SaveChanges();
                //    List.Remove(SelectedItem1);
                //}
                //else return;

            });
        }
    }
}