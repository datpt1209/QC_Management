using QC_Management.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class UnitTableViewModel : BaseViewModel
    {
        private ObservableCollection<UnitTable> _List;
        public ObservableCollection<UnitTable> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        private UnitTable _SelectedItem;
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public UnitTable SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    DisplayName = SelectedItem.Name;
                }
            }
        }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }
        public UnitTableViewModel()
        {
            List = new ObservableCollection<UnitTable>(DataProvider.Ins.DB.UnitTables);

            AddCommand = new RelayCommand<UnitTable>((p) =>
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return false;

                return true;

            }, (p) =>
            {
                var unit = new UnitTable() { Name = DisplayName };
                DataProvider.Ins.DB.UnitTables.Add(unit);
                DataProvider.Ins.DB.SaveChanges();
                List.Add(unit);

            });

            EditCommand = new RelayCommand<UnitTable>((p) =>
            {
                if (SelectedItem == null) return false;
                else if (SelectedItem.Name == DisplayName) return false;

                return true;

            }, (p) =>
            {
                var unit = DataProvider.Ins.DB.UnitTables.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();
                unit.Name = DisplayName;
                DataProvider.Ins.DB.SaveChanges();

                SelectedItem.Name = DisplayName;

            });

            DeleteCommand = new RelayCommand<UnitTable>((p) =>
            {
                if (SelectedItem == null)
                    return false;
                else
                    return true;

            }, (p) =>
            {
                MessageBoxResult result = MessageBox.Show($"Bạn có muốn xóa thông tin đơn vị {SelectedItem.Name} ?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var unit = DataProvider.Ins.DB.UnitTables.Remove(SelectedItem);
                    List.Remove(SelectedItem);
                    DataProvider.Ins.DB.SaveChanges();
                }
                else return;
            });
        }
    }
}
