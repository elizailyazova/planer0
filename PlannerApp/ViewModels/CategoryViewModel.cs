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
        private ObservableCollection<TaskItem> _taskItems;
        public ICommand NavigateToCategoryPage { get; set; }
        public ObservableCollection<TaskItem> TaskItems
        {
            get { return _taskItems; }
            set
            {
                _taskItems = value;
                OnPropertyChanged(nameof(TaskItems));
            }
        }


        // Команда для перехода на страницу категории
        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Func<object, bool> _canExecute;

            public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
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
                return _canExecute?.Invoke(parameter) ?? true;
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }
        }


        // Загрузка задач по категории
        private async void LoadTasksByCategory(string categoryName)
        {
            try
            {
                var tasks = await Task.Run(() => DatabaseHelper.GetTasks()); // Загрузка задач из базы данных
                var filteredTasks = tasks.Where(t => t.Category == categoryName).ToList(); // Фильтрация по имени категории

                Application.Current.Dispatcher.Invoke(() =>
                {
                    TaskItems.Clear();
                    foreach (var task in filteredTasks)
                    {
                        TaskItems.Add(task);
                    }
                });
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
