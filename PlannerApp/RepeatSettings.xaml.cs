
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PlannerApp.Models;

namespace PlannerApp
{
    public partial class RepeatSettings : Window, INotifyPropertyChanged
    {
        private int _interval = 1;
        private string _selectedFrequency = "Day";
        private bool _isWeekSelected;
        private bool _isYearSelected;
        private RecurrenceModel recurrence;

        public RecurrenceModel Recurrence { get; set; } = new RecurrenceModel();

        private string _repeatButtonText = "Do not repeat";
        public string RepeatButtonText
        {
            get => _repeatButtonText;
            set
            {
                _repeatButtonText = value;
                OnPropertyChanged();
            }
        }
        public RepeatSettings()
        {
            InitializeComponent();
            DataContext = this;
            recurrence = new RecurrenceModel();
            Recurrence = new RecurrenceModel();
            UpdateVisibility();
        }

        public int Interval
        {
            get => _interval;
            set { _interval = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> FrequencyOptions { get; } = new ObservableCollection<string>
        {
            "Daily", "Weekly", "Monthly", "Yearly"
        };

        public string SelectedFrequency
        {
            get => _selectedFrequency;
            set
            {
                _selectedFrequency = value;
                OnPropertyChanged();
                recurrence.Frequency = _selectedFrequency;
            }
        }

        public ObservableCollection<bool> SelectedDays { get; } = new ObservableCollection<bool> { false, false, false, false, false, false, false };
        public ObservableCollection<bool> SelectedMonths { get; } = new ObservableCollection<bool> { false, false, false, false, false, false, false, false, false, false, false, false };

        public bool IsWeekSelected
        {
            get => _isWeekSelected;
            set { _isWeekSelected = value; OnPropertyChanged(); }
        }

        public bool IsYearSelected
        {
            get => _isYearSelected;
            set { _isYearSelected = value; OnPropertyChanged(); }
        }

        private void UpdateVisibility()
        {
            IsWeekSelected = SelectedFrequency == "Week";
            IsYearSelected = SelectedFrequency == "Year";
        }

        public bool EndsOn
        {
            get => _endsOn;
            set
            {
                _endsOn = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEndsOnEnabled));
            }
        }
        private bool _endsOn;

        public DateTime? EndsOnDate
        {
            get => _endsOnDate;
            set
            {
                _endsOnDate = value;
                OnPropertyChanged();
            }
        }
        private DateTime? _endsOnDate;

        public bool EndsAfter
        {
            get => _endsAfter;
            set
            {
                _endsAfter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEndsAfterEnabled));
            }
        }
        private bool _endsAfter;

        public int? EndsAfterOccurrences
        {
            get => _endsAfterOccurrences;
            set
            {
                _endsAfterOccurrences = value;
                OnPropertyChanged();
            }
        }
        private int? _endsAfterOccurrences;

        public bool IsEndsOnEnabled => EndsOn;
        public bool IsEndsAfterEnabled => EndsAfter;

        public ICommand ApplyCommand => new RelayCommand(_ => ApplySettings());
        public ICommand CancelCommand => new RelayCommand(_ => CloseWindow());

        private void ApplySettings()
        {
            Recurrence.RepeatInterval = Interval;
            Recurrence.Frequency = SelectedFrequency;
            Recurrence.SelectedDays = string.Join("", SelectedDays.Select(b => b ? "1" : "0"));
            Recurrence.SelectedMonths = string.Join("", SelectedMonths.Select(b => b ? "1" : "0"));
            Recurrence.EndsOn = EndsOn;
            Recurrence.EndsOnDate = EndsOn ? EndsOnDate : null;
            Recurrence.EndsAfter = EndsAfter;
            Recurrence.EndsAfterOccurrences = EndsAfter ? EndsAfterOccurrences : null;
            RepeatButtonText = GetRepeatDescription(Recurrence);

            DialogResult = true;
            Close();
        }

        private string GetRepeatDescription(RecurrenceModel recurrence)
        {
            if (recurrence.Frequency == "None")
                return "Do not repeat";

            string description = $"Repeat every {recurrence.RepeatInterval} ";

            switch (recurrence.Frequency)
            {
                case "Daily": description += "day(s)"; break;
                case "Weekly": description += "week(s)"; break;
                case "Monthly": description += "month(s)"; break;
                case "Yearly": description += "year(s)"; break;
            }

            if (recurrence.EndsAfterOccurrences.HasValue)
                description += $", {recurrence.EndsAfterOccurrences} times";
            else if (recurrence.EndsOnDate.HasValue)
                description += $", until {recurrence.EndsOnDate.Value.ToShortDateString()}";

            return description;
        }

        private void CloseWindow() => Close();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void FrequencyChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateVisibility();
        }
    }
}
