using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
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
            _originalTask = Task.Clone(); 
        }
        
        private void CancelEditing()
        {
            if (_originalTask != null)
            {
                Task = _originalTask;
                OnPropertyChanged(nameof(Task)); 
                IsReadOnly = true;
            }
        }

        private void SaveChanges()
        {
            if (string.IsNullOrWhiteSpace(Task.Title))
            {
                MessageBox.Show("Название задачи не может быть пустым!");
                return;
            }

            try
            {
                Task.Save();
                this.DialogResult = true; // Устанавливаем ДО закрытия
            }
            catch 
            { 
                this.DialogResult = false; // Отмена при ошибке
            }
            finally
            {
                Close(); // Закрываем окно
            }
        }
        private void DeleteTask()
        {
            if (MessageBox.Show("Удалить задачу?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    Task.Delete(); // << вызов удаления из модели
                    DialogResult = true;
                    Close();
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
        
                private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchText = SearchTextBox.Text;

                using (var db = new DatabaseHelper())
                {
                    var conn = db.GetConnection();
                    conn.Open();
                    
                    string query = @"SELECT * FROM task
                                   WHERE title LIKE @searchText 
                                      OR description LIKE @searchText";
                    
                    var results = new List<TaskItem>();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@searchText", $"%{searchText}%");
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                MessageBox.Show("Ничего не найдено");
                                return;
                            }

                            while (reader.Read())
                            {
                                results.Add(new TaskItem
                                {
                                    Id = reader.GetInt32("id"),
                                    Title = reader.GetString("title"),
                                    Status = reader.GetString("status"),
                                    Date = reader.GetDateTime("date"),
                                    Category = reader.GetString("category")
                                });
                            }
                        }
                    }

                    if (results == null || !results.Any())
                    {
                        MessageBox.Show("Нет результатов");
                        return;
                    }

                    var searchWindow = new Search(results);
                    searchWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }

        
        private void ShowCalendar(object sender, RoutedEventArgs e)
        {
            Calendar calendarWindow = new Calendar();
            calendarWindow.Show();
            this.Close();
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
