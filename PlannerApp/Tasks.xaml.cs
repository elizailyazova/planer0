using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Collections.Generic;

using System.Windows.Input;
using MySql.Data.MySqlClient;
using PlannerApp.Models;

namespace PlannerApp
{
    public partial class Tasks : Window, INotifyPropertyChanged
    {
        private TaskItem _task;


        public TaskItem Task
        {
            get => _task;
            set
            {
                _task = value;
                OnPropertyChanged(nameof(Task));
                OnPropertyChanged(nameof(Task.Title));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(DueDate));
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(Category));
                OnPropertyChanged(nameof(Status));
            }
        }


        public string Description => Task?.Description;
        public DateTime DueDate => Task?.Date ?? DateTime.Now;
        public TimeSpan Time => Task?.Time ?? TimeSpan.Zero;
        public string Category => Task?.Category;
        public string Status
        {
            get => Task?.Status;
            set
            {
                if (Task != null)
                {
                    Task.Status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        private bool _isReadOnly = true;

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                OnPropertyChanged(nameof(IsReadOnly));
                OnPropertyChanged(nameof(EditButtonVisibility));
                OnPropertyChanged(nameof(UpdateButtonVisibility));
            }
        }

        public Visibility EditButtonVisibility => IsReadOnly ? Visibility.Visible : Visibility.Collapsed;
        public Visibility UpdateButtonVisibility => !IsReadOnly ? Visibility.Visible : Visibility.Collapsed;

        public ICommand EditCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public List<string> Categories { get; } = new List<string> { "Study", "Work", "Home", "Hobby" };
        public List<string> StatusOptions { get; } = new List<string> { "Not Started", "In Progress", "Completed" };



        public Tasks(TaskItem task)
        {
            InitializeComponent();

            Task = task ?? new TaskItem { Status = "Not Started" };

            this.Title = Task.Title;

            if (string.IsNullOrEmpty(Task.Status))
            {
                Task.Status = "Not Started";
            }

            DataContext = this;

            EditCommand = new RelayCommand(_ => EnableEditing());
            UpdateCommand = new RelayCommand(_ => SaveChanges());
            CancelCommand = new RelayCommand(_ => CancelChanges());
            DeleteCommand = new RelayCommand(_ => DeleteTask());
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void EnableEditing()
        {
            IsReadOnly = false;
            // Создаем копию задачи для возможности отмены изменений
            _originalTask = new TaskItem
            {
                Title = Task.Title,
                Description = Task.Description,
                Date = Task.Date,
                Time = Task.Time,
                Category = Task.Category,
                Status = Task.Status
            };
        }

        private void SaveChanges()
        {
            if (string.IsNullOrWhiteSpace(Task.Title))
            {
                MessageBox.Show("Название задачи не может быть пустым!");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void DeleteTask()
        {
            if (MessageBox.Show("Удалить задачу?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    if (Task.Id > 0)
                    {
                        using (var db = new DatabaseHelper())
                        {
                            string query = "DELETE FROM task WHERE id = @id";

                            using (var command = new MySqlCommand(query, db.Connection))
                            {
                                command.Parameters.AddWithValue("@id", Task.Id);
                                db.Connection.Open();
                                command.ExecuteNonQuery();
                            }
                        }

                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }



        private TaskItem _originalTask;
        private void CancelChanges()
        {
            if (!IsReadOnly)
            {
                Task.Title = _originalTask.Title;
                Task.Description = _originalTask.Description;
                Task.Date = _originalTask.Date;
                Task.Time = _originalTask.Time;
                Task.Category = _originalTask.Category;
                Task.Status = _originalTask.Status;

                OnPropertyChanged(nameof(Task));
                OnPropertyChanged(nameof(Task.Title));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(DueDate));
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(Category));
                OnPropertyChanged(nameof(Status));

                IsReadOnly = true;
            }
            else
            {
               

                
                var taskWindow = new Category();
                taskWindow.Show();
                
                this.Close();
            }
        }


        private void OpenCategory(object sender, RoutedEventArgs e)
        {
            // Сохраняем текущее окно в стек
            WindowHistory.Push(this);

            // Открываем новое окно
            Category categoryWindow = new Category();
            categoryWindow.Show();
            this.Close();
        }

        public static Stack<Window> WindowHistory { get; set; } = new Stack<Window>();

        private void Back(object sender, RoutedEventArgs e)
        {
            if (WindowHistory.Count > 0)
            {
                Window previousWindow = WindowHistory.Pop();
                previousWindow.Show();
                this.Close();
            }
        }




        private void OpenAddTask(object sender, RoutedEventArgs e)
        {
            AddTask addtaskWindow = new AddTask();
            addtaskWindow.Show();
            this.Close();
        }


        private void Logout(object sender, RoutedEventArgs e)
        {
            LoginWindow categoryWindow = new LoginWindow();
            categoryWindow.Show();
            this.Close();
        }

    }
}
