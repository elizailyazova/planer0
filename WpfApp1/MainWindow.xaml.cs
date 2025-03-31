using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _currentDay;
        private string _currentDate;
        public ObservableCollection<TaskModel> Tasks { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string RepeatSummary { get; set; } = "Do Not Repeat";

        private string _taskTitle;


        public string TaskTitle
        {
            get => _taskTitle;
            set
            {
                _taskTitle = value;
                OnPropertyChanged(nameof(TaskTitle));
            }
        }

        private string _description;

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }


        private DateTime _dueDate = DateTime.Now;

        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                OnPropertyChanged(nameof(DueDate));
            }
        }

        public string FormattedDueDate => DueDate.ToString("yyyy-MM-dd");


        private TimeSpan _time = TimeSpan.Zero;

        public TimeSpan Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        private string _category;

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }


        public List<string> Categories { get; } = new List<string> { "Study", "Work", "Home", "Hobby" };

        public ICommand SaveCommand { get; }
        public string RepeatType { get; set; }
        public RecurrenceModel Recurrence { get; set; }

        private TaskService _taskService = new TaskService();

        public MainWindow()
        {
            InitializeComponent();
            
            Tasks = new ObservableCollection<TaskModel>();
            DataContext = this;
    
            Loaded += (s, e) => 
            {
                LoadTasks();
            };
            
            SaveCommand = new RelayCommand(_ => AddTask(null, null));

            UpdateDateTime();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => UpdateDateTime();
            timer.Start();
        }

        private void UpdateDateTime()
        {
            CurrentDay = DateTime.Now.ToString("dddd", new CultureInfo("en-US"));
            CurrentDate = DateTime.Now.ToString("dd.MM.yyyy");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string CurrentDay
        {
            get => _currentDay;
            set
            {
                if (_currentDay != value)
                {
                    _currentDay = value;
                    OnPropertyChanged(nameof(CurrentDay));
                }
            }
        }

        public string CurrentDate
        {
            get => _currentDate;
            set
            {
                if (_currentDate != value)
                {
                    _currentDate = value;
                    OnPropertyChanged(nameof(CurrentDate));
                }
            }
        }


        public ICommand OpenRepeatSettingsCommand => new RelayCommand(_ => OpenRepeatSettings());


        private void OpenRepeatSettings()
        {
            var repeatWindow = new RepeatSettings();
            if (Recurrence != null)
            {
                repeatWindow.Recurrence = Recurrence;
                repeatWindow.SelectedFrequency = Recurrence.Frequency;
                repeatWindow.Interval = Recurrence.RepeatInterval;
                repeatWindow.EndsOn = Recurrence.EndsOn;
                repeatWindow.EndsAfter = Recurrence.EndsAfter;
                repeatWindow.EndsOnDate = Recurrence.EndsOnDate;
                repeatWindow.EndsAfterOccurrences = Recurrence.EndsAfterOccurrences;
            }


            if (repeatWindow.ShowDialog() == true)
            {
                Recurrence = repeatWindow.Recurrence;
                RepeatType = Recurrence.Frequency;
                RepeatSummary = repeatWindow.RepeatButtonText;
                OnPropertyChanged(nameof(RepeatSummary));
            }
        }

        private void UpdateRepeatSummary()
        {
            if (Recurrence == null)
            {
                RepeatSummary = "Do Not Repeat";
                return;
            }

            if (Recurrence.Frequency == "Day")
            {
                RepeatSummary = $"Every {Recurrence.RepeatInterval} day(s)";
            }
            else if (Recurrence.Frequency == "Week")
            {
                RepeatSummary = $"Every {Recurrence.RepeatInterval} week(s)";
            }
            else if (Recurrence.Frequency == "Month")
            {
                RepeatSummary = $"Every {Recurrence.RepeatInterval} month(s)";
            }
            else if (Recurrence.Frequency == "Year")
            {
                RepeatSummary = $"Every {Recurrence.RepeatInterval} year(s)";
            }

            OnPropertyChanged(nameof(RepeatSummary));
        }

        private void AddTask(object sender, RoutedEventArgs e)
        {
            var task = new TaskModel
            {
                Title = TaskTitle,
                Description = Description,
                Date = DueDate.Date + Time,
                Time = Time,
                Category = Category,
                Status = "Not Started",
                RepeatType = RepeatType
            };

            try
            {
                _taskService.AddTask(task, Recurrence);
                
                TaskTitle = Description = Category = string.Empty;
                Time = TimeSpan.Zero;
                DueDate = DateTime.Now;
        
                OpenTaskWindow(task);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании задачи: {ex.Message}");
            }
        }
        
        private void OpenTaskWindow(TaskModel task)
        {
            if (task.Id > 0)
            {
                using (var db = new AppDbContext())
                {
                    task = db.Tasks.Find(task.Id); 
                }
            }
            
            var window = new TaskWindow(task)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                SaveTaskChanges(task);
                LoadTasks();
            }
        }
        
        private void SaveTaskChanges(TaskModel task)
        {
            using (var db = new AppDbContext())
            {
                var existingTask = db.Tasks.Find(task.Id);
                if (existingTask != null)
                {
                    existingTask.Title = task.Title;
                    existingTask.Description = task.Description;
                    existingTask.Date = task.Date;
                    existingTask.Time = task.Time;
                    existingTask.Category = task.Category;
                    existingTask.Status = task.Status;

                    db.SaveChanges();
                }
            }
        }

        private void LoadTasks()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var tasks = db.Tasks.ToList();
            
                    Tasks.Clear();
                    foreach (var task in tasks)
                    {
                        Tasks.Add(task);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }        
    }
     
}

