using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using QC_Management.Views;
using Microsoft.EntityFrameworkCore;

namespace QC_Management.ViewModels
{
    public class Re_ResultViewModel : BaseViewModel
    {
        private ObservableCollection<ReResultGroup> _GroupedReResults;
        public ObservableCollection<ReResultGroup> GroupedReResults
        {
            get => _GroupedReResults;
            set { _GroupedReResults = value; OnPropertyChanged(); }
        }

        private ReResultGroup _SelectedItem;
        public ReResultGroup SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
            }
        }
        public ICommand ShowDetailCommand { get; set; }

        public Re_ResultViewModel()
        {
            GroupedReResults = new ObservableCollection<ReResultGroup>();
            LoadReResults();
            //ShowDetailCommand = new RelayCommand<ReResultGroup>(ShowDetail);
            ShowDetailCommand = new RelayCommand<object>((p) => true, (p) =>
            {
                Re_ResultDetailView reResultWindow = new Re_ResultDetailView(SelectedItem);
                reResultWindow.ShowDialog();
            });
        }

        private void ShowDetail(ReResultGroup reResultGroup)
        {
            if (reResultGroup != null)
            {
                Re_ResultDetailView detailWindow = new Re_ResultDetailView(reResultGroup);
                detailWindow.ShowDialog();
            }
        }

        private void LoadReResults()
        {
            QcManagmentContext DB = DataProvider.Ins.DB;
            var reResults = DB.ReResults.ToList();
            var groupedResults = reResults
                .GroupBy(r => new { r.IdDevice, r.IdLevel, DateTime = r.Date.Date })
                .Select(g => new ReResultGroup
                {
                    DeviceName = DB.Devices.FirstOrDefault(d => d.Id == g.Key.IdDevice)?.Name ?? "Unknown Device",
                    LevelName = DB.LevelQcs.FirstOrDefault(l => l.Id == g.Key.IdLevel)?.Name ?? "Unknown Level",
                    DateTime = g.Key.DateTime,
                    Results = new ObservableCollection<ReResult>(g.ToList())
                })
                .ToList();

            GroupedReResults = new ObservableCollection<ReResultGroup>(groupedResults);
        }
    }

}
