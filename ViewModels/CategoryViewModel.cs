using QC_Management.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class CategoryViewModel : BaseViewModel
    {
        private ObservableCollection<Category> _List;
        public ObservableCollection<Category> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        private Category _SelectedItem;
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public Category SelectedItem
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
        public CategoryViewModel()
        {
            List = new ObservableCollection<Category>(DataProvider.Ins.DB.Categories);

            AddCommand = new RelayCommand<Category>((p) =>
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return false;
                else
                    return true;

            }, (p) =>
            {
                var category = new Category() { Name = DisplayName };
                DataProvider.Ins.DB.Categories.Add(category);
                DataProvider.Ins.DB.SaveChanges();
                List.Add(category);

            });

            EditCommand = new RelayCommand<Category>((p) =>
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return false;

                else

                    return true;

            }, (p) =>
            {
                var category = DataProvider.Ins.DB.Categories.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                category.Name = DisplayName;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                DataProvider.Ins.DB.SaveChanges();

                SelectedItem.Name = DisplayName;

            });

            DeleteCommand = new RelayCommand<Category>((p) =>
            {
                if (SelectedItem == null)
                    return false;

                else

                    return true;

            }, (p) =>
            {
                MessageBoxResult result = MessageBox.Show($"Bạn có muốn xóa thông tin nhóm: {SelectedItem.Name} ?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    DataProvider.Ins.DB.Categories.Remove(SelectedItem);
                    DataProvider.Ins.DB.SaveChanges();
                    List.Remove(SelectedItem);
                }
                else return;

            });

        }
    }
}
