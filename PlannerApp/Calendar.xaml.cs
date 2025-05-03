using System.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using PlannerApp.Models;

namespace PlannerApp;

public partial class Calendar : Window
{
    
    private DateTime currentMonth;
    private DateTime selectedDate;
    private TaskItem selectedTask;
    
    private List<Button> calendarButtons = new List<Button>();
    private List<TaskItem> allTasks = new List<TaskItem>();
    public ICommand OpenTaskCommand { get; private set; }

    public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();

    public Calendar()
    {
            InitializeComponent();
            
            currentMonth = DateTime.Now;
            selectedDate = DateTime.Today;

            OpenTaskCommand = new RelayCommand(OpenTask);
            Tasks = new ObservableCollection<TaskItem>(); 

            DataContext = this; 

            LoadMonthData(currentMonth.Year, currentMonth.Month);
            GenerateCalendar(currentMonth.Year, currentMonth.Month);
            UpdateTasksForSelectedDate();
            
            UpdateMonthYearText();
        }

    private void OpenTask(object parameter)
    {
        if (parameter is TaskItem task)
        {
            var tasksWindow = new Tasks(task);
            if (tasksWindow.ShowDialog() == true) 
            {
                LoadMonthData(currentMonth.Year, currentMonth.Month);
            }
        }
    }


    // Загрузка задач из базы данных
    private void LoadMonthData(int year, int month)
    {
        allTasks.Clear();
        
        using (var db = new DatabaseHelper())
        {
            var conn = db.GetConnection();
            conn.Open();
            
            string query = @"SELECT id, title, date, 
                            COALESCE(status, 'Not Started') as status,
                            COALESCE(category, 'Uncategorized') as category 
                          FROM task 
                          WHERE YEAR(date) = @year 
                            AND MONTH(date) = @month";
            
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@year", year);
                cmd.Parameters.AddWithValue("@month", month);
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        allTasks.Add(new TaskItem
                        {
                            Id = reader.GetInt32("id"),
                            Title = reader.GetString("title"),
                            Date = reader.GetDateTime("date"),
                            Status = reader.GetString("status"),
                            Category = reader.GetString("category")
                        });
                    }
                }
            }
        }
        
        UpdateTasksForSelectedDate();
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

    private void UpdateTasksForSelectedDate()
    {
        Tasks.Clear();
        var filtered = allTasks
            .Where(t => t.Date.Date == selectedDate.Date)
            .ToList();
            
        foreach (var task in filtered)
        {
            Tasks.Add(task);
        }
        var culture = new CultureInfo("en-US"); 

        TasksDateText.Text = $"To-Do Tasks for {selectedDate.ToString("dddd, dd MMMM", culture)}";

    }

    private void DayButton_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        selectedDate = (DateTime)button.Tag;
        UpdateSelectedDayStyle();
        UpdateTasksForSelectedDate(); 
    }

    private void PrevMonthButton_Click(object sender, RoutedEventArgs e)
    {
        currentMonth = currentMonth.AddMonths(-1);
        LoadMonthData(currentMonth.Year, currentMonth.Month);
        GenerateCalendar(currentMonth.Year, currentMonth.Month);
    }

    private void NextMonthButton_Click(object sender, RoutedEventArgs e)
    {
        currentMonth = currentMonth.AddMonths(1);
        LoadMonthData(currentMonth.Year, currentMonth.Month);
        GenerateCalendar(currentMonth.Year, currentMonth.Month);
    }
        // Генерация календарной сетки
        private void GenerateCalendar(int year, int month)
        {
            // Очистка сетки календаря
            DaysGrid.Children.Clear();
            calendarButtons.Clear();
            
            // Первый день месяца
            var firstDay = new DateTime(year, month, 1);
            int offset = (int)firstDay.DayOfWeek;
            
            // Количество дней в месяце
            int daysInMonth = DateTime.DaysInMonth(year, month);
            
            // Количество дней предыдущего месяца для отображения
            int previousMonth = month == 1 ? 12 : month - 1;
            int previousYear = month == 1 ? year - 1 : year;
            int daysInPreviousMonth = DateTime.DaysInMonth(previousYear, previousMonth);
            
            // Генерация всех ячеек календаря (6 рядов по 7 дней)
            for (int i = 0; i < 42; i++)
            {
                Button dayButton = new Button();
                dayButton.Style = FindResource("CalendarDayButtonStyle") as Style;
                
                // Настройка дня
                int displayDay;
                bool isCurrentMonth = false;
                
                if (i < offset)
                {
                    // Предыдущий месяц
                    displayDay = daysInPreviousMonth - (offset - i - 1);
                    dayButton.Opacity = 0.4;
                }
                else if (i < offset + daysInMonth)
                {
                    // Текущий месяц
                    displayDay = i - offset + 1;
                    isCurrentMonth = true;
                    
                    // Проверяем, есть ли задачи на этот день
                    var currentDate = new DateTime(year, month, displayDay);
                    bool hasTasksOnDay = allTasks.Any(t => t.Date.Date == currentDate.Date);
                    
                    // Если есть задачи, добавляем индикатор
                    if (hasTasksOnDay)
                    {
                        Grid buttonContent = new Grid();
                        
                        buttonContent.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
                        buttonContent.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        
                        // Текст даты
                        TextBlock dayText = new TextBlock
                        {
                            Text = displayDay.ToString(),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetRow(dayText, 0);
                        buttonContent.Children.Add(dayText);
                        
                        // Индикатор задач
                        Ellipse taskIndicator = new Ellipse
                        {
                            Width = 6,
                            Height = 6,
                            Fill = new SolidColorBrush(Colors.Green),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0, 0, 0, 2)
                        };
                        Grid.SetRow(taskIndicator, 1);
                        buttonContent.Children.Add(taskIndicator);
                        
                        dayButton.Content = buttonContent;
                    }
                    else
                    {
                        dayButton.Content = displayDay.ToString();
                    }
                    
                    // Проверка для выбранного дня
                    if (currentDate.Date == selectedDate.Date)
                    {
                        dayButton.Background = new SolidColorBrush(Color.FromRgb(65, 171, 73));
                        dayButton.Foreground = new SolidColorBrush(Colors.White);
                        
                        if (dayButton.Content is Grid grid)
                        {
                            foreach (var child in grid.Children)
                            {
                                if (child is TextBlock tb)
                                    tb.Foreground = new SolidColorBrush(Colors.White);
                                else if (child is Ellipse el)
                                    el.Fill = new SolidColorBrush(Colors.White);
                            }
                        }
                    }
                }
                else
                {
                    // Следующий месяц
                    displayDay = i - (offset + daysInMonth) + 1;
                    dayButton.Opacity = 0.4;
                    dayButton.Content = displayDay.ToString();
                }
                
                if (isCurrentMonth)
                {
                    // Сохраняем дату как тег кнопки для обработки нажатий
                    DateTime buttonDate = new DateTime(year, month, displayDay);
                    dayButton.Tag = buttonDate;
                    dayButton.Click += DayButton_Click;
                    calendarButtons.Add(dayButton);
                }
                else
                {
                    dayButton.IsEnabled = false;
                }
                
                DaysGrid.Children.Add(dayButton);
            }
        }

        // Обновление стилей для выбранного дня
        private void UpdateSelectedDayStyle()
        {
            // Сбрасываем стили всех кнопок
            foreach (var button in calendarButtons)
            {
                DateTime buttonDate = (DateTime)button.Tag;
                bool hasTasksOnDay = allTasks.Any(t => t.Date.Date == buttonDate.Date);
                
                // Устанавливаем стандартный стиль
                button.Background = new SolidColorBrush(Colors.Transparent);
                button.Foreground = new SolidColorBrush(Colors.Black);
                
                if (hasTasksOnDay)
                {
                    // Если кнопка имеет Grid с индикатором
                    if (button.Content is Grid grid)
                    {
                        foreach (var child in grid.Children)
                        {
                            if (child is TextBlock tb)
                                tb.Foreground = new SolidColorBrush(Colors.Black);
                            else if (child is Ellipse el)
                                el.Fill = new SolidColorBrush(Color.FromRgb(65, 171, 73));
                        }
                    }
                }
                
                // Если это выбранный день, применяем выбранный стиль
                if (buttonDate.Date == selectedDate.Date)
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(65, 171, 73));
                    button.Foreground = new SolidColorBrush(Colors.White);
                    
                    if (button.Content is Grid grid)
                    {
                        foreach (var child in grid.Children)
                        {
                            if (child is TextBlock tb)
                                tb.Foreground = new SolidColorBrush(Colors.White);
                            else if (child is Ellipse el)
                                el.Fill = new SolidColorBrush(Colors.White);
                        }
                    }
                }
            }
        }
        
        // Обновление текста заголовка месяца
        private void UpdateMonthYearText()
        {
            var culture = new CultureInfo("en-US");
            MonthYearText.Text = currentMonth.ToString("MMMM yyyy", culture);
        }

        // Обработчики кнопок навигации по месяцам
        
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            // Открытие окна добавления задачи
            var addTaskWindow = new AddTask();
            if (addTaskWindow.ShowDialog() == true)
            {
                // Перезагружаем задачи и обновляем отображение
                LoadMonthData(currentMonth.Year, currentMonth.Month);
                GenerateCalendar(currentMonth.Year, currentMonth.Month);
                UpdateTasksForSelectedDate ();
            }
        }
        
        
        private void OpenCategory(object sender, RoutedEventArgs e)
        {
            Category categoryWindow = new Category();
            categoryWindow.Show();
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
    
