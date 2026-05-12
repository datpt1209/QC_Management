using System;
using System.Globalization;
using QC_Management.Models;

namespace QC_Management.ViewModels
{
    // Thin row VM that wraps ExternalResult and raises property notifications.
    public class ExternalResultRow : BaseViewModel
    {
        public ExternalResult Model { get; }

        public ExternalResultRow(ExternalResult model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public int Id => Model.Id;

        public string ExternalProgramName => Model.ExternalProgram?.Name;

        public string Batch
        {
            get => Model.Batch;
            set { Model.Batch = value; OnPropertyChanged(); }
        }

        public DateTime DateRun
        {
            get => Model.DateRun;
            set { Model.DateRun = value; OnPropertyChanged(); }
        }

        // Sample (Mẫu)
        public string? Sample
        {
            get => Model.Sample;
            set
            {
                if (Model.Sample == value) return;
                Model.Sample = value;
                OnPropertyChanged();
            }
        }

        // Received date (shared or seeded from VM)
        public DateTime? ReceivedAt
        {
            get => Model.ReceivedAt;
            set
            {
                if (Model.ReceivedAt == value) return;
                Model.ReceivedAt = value;
                OnPropertyChanged();
            }
        }

        // Read-only: Result saved date (set by VM on save)
        public DateTime? ResultSavedAt => Model.ResultSavedAt;

        // SigmaP stored as string (user input)
        public string? SigmaP
        {
            get => Model.SigmaP;
            set
            {
                if (Model.SigmaP == value) return;
                Model.SigmaP = value;
                // Recompute when sigma changes
                Model.ApplyReferenceEvaluation();
                OnPropertyChanged();
                OnPropertyChanged(nameof(ZScore));
                OnPropertyChanged(nameof(Status));
            }
        }

        // Editable string result - user types here.
        public string? TempResult
        {
            get => Model.TempResult;
            set
            {
                if (Model.TempResult == value) return;
                Model.TempResult = value;
                // Recompute evaluation using reference and sigma
                Model.ApplyReferenceEvaluation();
                OnPropertyChanged();
                OnPropertyChanged(nameof(ZScore));
                OnPropertyChanged(nameof(Status));
            }
        }

        // Reference/target value stored as string
        public string? ReferenceValue
        {
            get => Model.ReferenceValue;
            set
            {
                if (Model.ReferenceValue == value) return;
                Model.ReferenceValue = value;
                Model.ApplyReferenceEvaluation();
                OnPropertyChanged();
                OnPropertyChanged(nameof(ZScore));
                OnPropertyChanged(nameof(Status));
            }
        }

        public double? ZScore => Model.ZScore;

        // Status alias (mirrors IsDefect)
        public bool? Status
        {
            get => Model.IsDefect;
            set
            {
                if (Model.IsDefect == value) return;
                Model.IsDefect = value;
                OnPropertyChanged();
            }
        }

        public string DeviceName => Model.IdDeviceNavigation?.Name;
        public string TestName => Model.IdTestNavigation?.Name;

        public string? Notes
        {
            get => Model.Notes;
            set
            {
                Model.Notes = value;
                OnPropertyChanged();
            }
        }
    }
}
