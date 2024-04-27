using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public bool Isloaded = false;
        public ICommand LoadedWindowCommand { get; set; }
        private User _currentUser { get; set; }

        private BaseViewModel _currentView;

        private Visibility _visibility1;
        public Visibility visibility1 { get => _visibility1; set { _visibility1 = value; OnPropertyChanged(); } }

        private Visibility _visibility2;
        public Visibility visibility2 { get => _visibility2; set { _visibility2 = value; OnPropertyChanged(); } }
        public ICommand LoadHomePageCommand { get; set; }
        public ICommand LoadResultViewCommand { get; set; }
        public ICommand LoadViewResultViewCommand { get; set; }
        public ICommand LoadCategoryViewCommand { get; set; }
        public ICommand LoadQC_InformationViewCommand { get; set; }
        public ICommand LoadQC_DetailViewCommand { get; set; }
        public ICommand LoadUserViewCommand { get; set; }
        public ICommand LoadUserRoleViewCommand { get; set; }
        public ICommand LoadUnitViewCommand { get; set; }
        public ICommand LoadTestViewCommand { get; set; }
        public ICommand LoadDeviceViewCommand { get; set; }
        public BaseViewModel CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }
        public User currentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(currentUser));
            }
        }
        public MainViewModel()
        {
            LoadedWindowCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
             
               if(UserManager.Instance.CurrentUser.Role == 1)
                    {
                        visibility1 = Visibility.Visible;
                        visibility2 = Visibility.Collapsed;
                    }
               else if(UserManager.Instance.CurrentUser.Role == 2)
                {
                    visibility1 = Visibility.Collapsed;
                    visibility2 = Visibility.Collapsed;
                }
               else
                {
                    visibility1 = Visibility.Visible;
                    visibility2 = Visibility.Visible;
                }
           
            });
            currentUser = UserManager.Instance.CurrentUser;
            CurrentView = new HomeViewModel_V2();
            LoadHomePageCommand = new RelayCommand<Object>((p) => { return true; }, (p) => { CurrentView = new HomeViewModel_V2(); OnPropertyChanged(); });
            LoadResultViewCommand = new RelayCommand<Object>((p) => { return true; }, (p) => { CurrentView = new ResultViewModel(); OnPropertyChanged(); });
            LoadViewResultViewCommand = new RelayCommand<Object>((p) => { return true; }, (p) => { CurrentView = new ViewResultViewModel(); OnPropertyChanged(); });
            LoadUserViewCommand = new RelayCommand<System.Object>((p) => { return true; }, (p) => { CurrentView = new UserViewModel(); OnPropertyChanged(); });
            LoadUserRoleViewCommand = new RelayCommand<System.Object>((p) => { return true; }, (p) => { CurrentView = new UserRoleViewModel(); OnPropertyChanged(); });
            LoadCategoryViewCommand = new RelayCommand<System.Object>((p) => { return true; }, (p) => { CurrentView = new CategoryViewModel(); OnPropertyChanged(); });
            LoadUnitViewCommand = new RelayCommand<System.Object>((p) => { return true; }, (p) => { CurrentView = new UnitTableViewModel(); OnPropertyChanged(); });
            LoadQC_InformationViewCommand = new RelayCommand<System.Object>((p) => { return true; }, (p) => { CurrentView = new QC_InformationViewModel(); OnPropertyChanged(); });
            LoadQC_DetailViewCommand = new RelayCommand<System.Object>((p) => { return true; }, (p) => { CurrentView = new QC_DetailViewModel(); OnPropertyChanged(); });
            LoadTestViewCommand = new RelayCommand<System.Object>((p) => { return true; }, (p) => { CurrentView = new TestViewModel(); OnPropertyChanged(); });
            LoadDeviceViewCommand = new RelayCommand<System.Object>((p) => { return true; }, (p) => { CurrentView = new DeviceViewModel(); OnPropertyChanged(); });
        }

    }
}
