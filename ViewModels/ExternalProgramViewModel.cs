using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using QC_Management.Models;

namespace QC_Management.ViewModels
{
    public class ExternalProgramViewModel : BaseViewModel
    {
        public ObservableCollection<ExternalProgram> Programs { get; set; } = new ObservableCollection<ExternalProgram>();
        private ExternalProgram _selectedProgram;
        public ExternalProgram SelectedProgram
        {
            get => _selectedProgram;
            set { _selectedProgram = value; OnPropertyChanged(); }
        }

        // fields for quick add
        public int NewYear { get; set; } = DateTime.UtcNow.Year;
        public string NewName { get; set; }
        public string NewVendor { get; set; }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadedCommand { get; }  // added

        public ExternalProgramViewModel()
        {
            AddCommand = new RelayCommand<object>((p) => true, (p) => Add());
            SaveCommand = new RelayCommand<object>((p) => Programs.Any(), async (p) => await SaveAsync());
            DeleteCommand = new RelayCommand<object>((p) => SelectedProgram != null, async (p) => await DeleteAsync());
            RefreshCommand = new RelayCommand<object>((p) => true, async (p) => await LoadAsync());

            // LoadedCommand will be invoked by the view when it is shown
            LoadedCommand = new RelayCommand<object>((p) => true, async (p) => await LoadAsync());

            // DO NOT call LoadAsync() here to avoid DB access during resource initialization
            // _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                using var db = new QcManagmentContext();
                var list = await db.ExternalPrograms.AsNoTracking().OrderByDescending(e => e.Year).ThenBy(e => e.Name).ToListAsync();
                Programs.Clear();
                foreach (var e in list) Programs.Add(e);
            }
            catch (Exception ex)
            {
                // handle gracefully (log or non-modal notification preferred)
                MessageBox.Show($"Load external programs failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Add()
        {
            var p = new ExternalProgram
            {
                Year = NewYear,
                Name = NewName ?? string.Empty,
                Vendor = NewVendor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = UserManager.Instance?.CurrentUser?.DisplayName
            };
            Programs.Insert(0, p);
            SelectedProgram = p;
            NewName = string.Empty;
            NewVendor = string.Empty;
            OnPropertyChanged(nameof(NewName));
            OnPropertyChanged(nameof(NewVendor));
        }

        private async Task SaveAsync()
        {
            try
            {
                using var db = new QcManagmentContext();
                foreach (var p in Programs)
                {
                    if (p.Id == 0)
                    {
                        db.ExternalPrograms.Add(p);
                    }
                    else
                    {
                        db.ExternalPrograms.Attach(p);
                        db.Entry(p).State = EntityState.Modified;
                    }
                }
                await db.SaveChangesAsync();
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedProgram == null) return;
            if (MessageBox.Show($"Delete program '{SelectedProgram.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                using var db = new QcManagmentContext();
                if (SelectedProgram.Id != 0)
                {
                    var ent = await db.ExternalPrograms.FindAsync(SelectedProgram.Id);
                    if (ent != null)
                    {
                        db.ExternalPrograms.Remove(ent);
                        await db.SaveChangesAsync();
                    }
                }
                Programs.Remove(SelectedProgram);
                SelectedProgram = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
