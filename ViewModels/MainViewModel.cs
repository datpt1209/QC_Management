using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public bool IsLoaded { get; set; } = false;
        public ICommand LoadedWindowCommand { get; set; }
        private User _currentUser;

        private BaseViewModel _currentView;

        private Visibility _visibility1;
        public Visibility Visibility1 { get => _visibility1; set { _visibility1 = value; OnPropertyChanged(); } }

        private Visibility _visibility2;
        public Visibility Visibility2 { get => _visibility2; set { _visibility2 = value; OnPropertyChanged(); } }

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
        public ICommand LoadCALViewCommand { get; set; }
        public ICommand LoadCALDetailViewCommand { get; set; }

        // New command to open Incident / CorrectiveAction management
        public ICommand LoadIncidentManagementViewCommand { get; set; }

        public BaseViewModel CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        public MainViewModel()
        {
            LoadedWindowCommand = new RelayCommand<Window>((p) => true, (p) => LoadWindow(p));
            CurrentUser = UserManager.Instance.CurrentUser;
            CurrentView = new HomeViewModel_V2();

            LoadHomePageCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new HomeViewModel_V2()));
            LoadResultViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new ResultViewModel()));
            LoadViewResultViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new ViewResultViewModel()));
            LoadUserViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new UserViewModel()));
            LoadUserRoleViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new UserRoleViewModel()));
            LoadCategoryViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new CategoryViewModel()));
            LoadUnitViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new UnitTableViewModel()));
            LoadQC_InformationViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new QC_InformationViewModel()));
            LoadQC_DetailViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new QC_DetailViewModel()));
            LoadTestViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new TestViewModel()));
            LoadDeviceViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new DeviceViewModel()));
            LoadCALViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new CAL_InforViewModel()));

            // Initialize new incident management command
            LoadIncidentManagementViewCommand = new RelayCommand<object>((p) => true, (p) => LoadView(new IncidentManagementViewModel()));
        }

        private void LoadWindow(Window window)
        {
            if (UserManager.Instance.CurrentUser.Role == 1)
            {
                Visibility1 = Visibility.Visible;
                Visibility2 = Visibility.Collapsed;
            }
            else if (UserManager.Instance.CurrentUser.Role == 2)
            {
                Visibility1 = Visibility.Collapsed;
                Visibility2 = Visibility.Collapsed;
            }
            else
            {
                Visibility1 = Visibility.Visible;
                Visibility2 = Visibility.Visible;
            }
        }

        private void LoadView(BaseViewModel viewModel)
        {
            CurrentView = viewModel;
        }
    }
}
