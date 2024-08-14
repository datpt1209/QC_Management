public class AddResultViewModel : INotifyPropertyChanged
{
    private DateTime _selectedDate;
    private Device _selectedDevice;
    private Level _selectedLevel;
    private ObservableCollection<Result> _newResults;

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            OnPropertyChanged();
        }
    }

    public Device SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            _selectedDevice = value;
            OnPropertyChanged();
        }
    }

    public Level SelectedLevel
    {
        get => _selectedLevel;
        set
        {
            _selectedLevel = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Result> NewResults
    {
        get => _newResults;
        set
        {
            _newResults = value;
            OnPropertyChanged();
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddResultViewModel()
    {
        NewResults = new ObservableCollection<Result>();
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void Save()
    {
        // Save logic here
    }

    private void Cancel()
    {
        // Cancel logic here
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
