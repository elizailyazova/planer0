using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PlannerApp.Models;
using System.Threading.Tasks;
using System;

namespace PlannerApp.ViewModels
{
    public partial class CategoryViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TaskItem> _taskItems = new ObservableCollection<TaskItem>();

        public ICommand NavigateToCategoryPage { get; set; }
        public ICommand OpenTaskCommand { get; set; } // Команда для открытия задачи

        public ObservableCollection<TaskItem> TaskItems
        {
            get { return _taskItems; }
            set
            {
                _taskItems = value;
                OnPropertyChanged(nameof(TaskItems));
            }
        }

        public class RelayCommand<T> : ICommand
        {
            private readonly Action<T> _execute;
            private readonly Func<T, bool> _canExecute;

            public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter)
            {
                return _canExecute?.Invoke((T)parameter) ?? true;
            }

            public void Execute(object parameter)
            {
                _execute((T)parameter);
            }
        }


        public CategoryViewModel()
        {
            NavigateToCategoryPage = new RelayCommand<string>(LoadTasksByCategory);
            // Инициализация команды для открытия задачи
            OpenTaskCommand = new RelayCommand<TaskItem>(OpenTask);
        }

        // Обработчик команды для открытия задачи
        private void OpenTask(TaskItem task)
        {
            // Открытие окна Tasks и передача выбранной задачи
            var tasksWindow = new Tasks(task);
            tasksWindow.Show(); // Показываем окно с деталями задачи
        }
        
        
        // Загрузка задач по категории
        private async void LoadTasksByCategory(string categoryName)
        {
            try
            {
                using (var db = new DatabaseHelper())  // Создаём объект базы
                {
                    var tasks = await Task.Run(() => db.GetTasks()); // Загружаем задачи
                    var filteredTasks = tasks.Where(t => t.Category == categoryName).ToList(); // Фильтруем по категории

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TaskItems.Clear();
                        foreach (var task in filteredTasks)
                        {
                            TaskItems.Add(task);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач по категории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Оповещение об изменении свойств
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
