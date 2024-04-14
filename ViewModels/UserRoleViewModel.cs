using QC_Management.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class UserRoleViewModel : BaseViewModel
    {
        private ObservableCollection<UserRole> _List;
        public ObservableCollection<UserRole> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        private UserRole _SelectedItem;
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public UserRole SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    DisplayName = SelectedItem.DisplayName;
                }
            }
        }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }
        public UserRoleViewModel()
        {
            List = new ObservableCollection<UserRole>(DataProvider.Ins.DB.UserRoles);

            AddCommand = new RelayCommand<UserRole>((p) =>
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return false;

                return true;

            }, (p) =>
            {
                var role = new UserRole() { DisplayName = DisplayName };
                DataProvider.Ins.DB.UserRoles.Add(role);
                DataProvider.Ins.DB.SaveChanges();
                List.Add(role);

            });


            EditCommand = new RelayCommand<UserRole>((p) =>
            {
                if (SelectedItem == null)
                    return false;
                else if (SelectedItem.DisplayName == DisplayName) return false;
                else return true;

            }, (p) =>
            {
                var role = DataProvider.Ins.DB.UserRoles.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();
                role.DisplayName = DisplayName;
                DataProvider.Ins.DB.SaveChanges();

                SelectedItem.DisplayName = DisplayName;

            });

            DeleteCommand = new RelayCommand<UserRole>((p) =>
            {
                if (SelectedItem == null)
                    return false;
                else return true;

            }, (p) =>
            {
                MessageBoxResult result = MessageBox.Show($"Bạn có muốn xóa thông tin phân quyền {SelectedItem.DisplayName} ?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    DataProvider.Ins.DB.UserRoles.Remove(SelectedItem);
                    DataProvider.Ins.DB.SaveChanges();
                    List.Remove(SelectedItem);
                }
                else return;

            });
        }
        public void Reload()
        {
            SelectedItem = null;
        }
    }
}