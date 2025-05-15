using AfishaUno.Models;
using AfishaUno.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AfishaUno.Presentation.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<MainViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<Performance> _performances = new ObservableCollection<Performance>();

        [ObservableProperty]
        private ObservableCollection<Schedule> _schedules = new ObservableCollection<Schedule>();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private User _currentUser;

        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _userRole;

        [ObservableProperty]
        private bool _isLoggedIn;

        [ObservableProperty]
        private bool _showAdminMenu;

        [ObservableProperty]
        private bool _showCashierMenu;

        [ObservableProperty]
        private ObservableCollection<MenuItem> _menuItems = new();

        public IRelayCommand LogoutCommand { get; }
        public IRelayCommand<string> NavigateCommand { get; }

        public MainViewModel(
            ISupabaseService supabaseService, 
            INavigationService navigationService, 
            IAuthorizationService authorizationService,
            ILogger<MainViewModel> logger)
        {
            _supabaseService = supabaseService;
            _navigationService = navigationService;
            _authorizationService = authorizationService;
            _logger = logger;

            LogoutCommand = new AsyncRelayCommand(LogoutAsync);
            NavigateCommand = new RelayCommand<string>(OnNavigate);

            InitializeMenu();
            UpdateUserInfo();
        }

        private void InitializeMenu()
        {
            MenuItems.Clear();

            // Общие пункты меню
            MenuItems.Add(new MenuItem { Title = "Главная", Icon = "\uE80F", PageName = "HomePage" });
            
            // Только кассир имеет доступ к продаже билетов
            if (_authorizationService.CanSellTickets())
            {
                MenuItems.Add(new MenuItem { Title = "Продажа билетов", Icon = "\uE8A7", PageName = "SchedulePage" });
            }

            // Только администратор имеет доступ к управлению системой
            if (_authorizationService.CanAccessAdminSection())
            {
                MenuItems.Add(new MenuItem { Title = "Спектакли", Icon = "\uE7F4", PageName = "PerformancesPage" });
                MenuItems.Add(new MenuItem { Title = "Залы", Icon = "\uE8F4", PageName = "HallsPage" });
                MenuItems.Add(new MenuItem { Title = "Расписание", Icon = "\uE8BF", PageName = "ScheduleManagementPage" });
                MenuItems.Add(new MenuItem { Title = "Отчеты", Icon = "\uE9D2", PageName = "ReportsPage" });
                MenuItems.Add(new MenuItem { Title = "Пользователи", Icon = "\uE77B", PageName = "UsersPage" });
            }

            if (!_authorizationService.IsUserLoggedIn())
            {
                // Если пользователь не авторизован, показываем только вход
                MenuItems.Clear();
                MenuItems.Add(new MenuItem { Title = "Вход", Icon = "\uE77B", PageName = "LoginPage" });
            }

            UpdateUserInterface();
        }

        public void UpdateUserInfo()
        {
            var currentUser = _supabaseService.CurrentUser;
            IsLoggedIn = currentUser != null;

            if (IsLoggedIn)
            {
                UserName = currentUser.FullName;
                UserRole = currentUser.Role == UserRoles.Admin ? "Администратор" : "Кассир";
                
                ShowAdminMenu = currentUser.Role == UserRoles.Admin;
                ShowCashierMenu = true; // Оба типа пользователей могут продавать билеты
            }
            else
            {
                UserName = string.Empty;
                UserRole = string.Empty;
                ShowAdminMenu = false;
                ShowCashierMenu = false;
            }

            InitializeMenu();
        }

        private void UpdateUserInterface()
        {
            // Обновление интерфейса для отражения прав доступа
            ShowAdminMenu = _authorizationService.CanAccessAdminSection();
            ShowCashierMenu = _authorizationService.CanSellTickets();
        }

        private async Task LogoutAsync()
        {
            try
            {
                await _supabaseService.LogoutAsync();
                UpdateUserInfo();
                _navigationService.NavigateTo("LoginPage");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при выходе из системы: {ex.Message}");
                Trace.WriteLine($"Ошибка при выходе из системы: {ex.Message}");
            }
        }

        private void OnNavigate(string pageName)
        {
            if (string.IsNullOrEmpty(pageName))
                return;

            // Проверка прав доступа перед навигацией
            bool hasAccess = true;

            // Для страниц, требующих прав администратора
            if (pageName == "PerformancesPage" || pageName == "HallsPage" || 
                pageName == "ScheduleManagementPage" || pageName == "ReportsPage" || 
                pageName == "UsersPage")
            {
                hasAccess = _authorizationService.CanAccessAdminSection();
            }
            
            // Для страниц, требующих авторизации
            if (pageName == "SchedulePage")
            {
                hasAccess = _authorizationService.CanSellTickets();
            }

            if (!hasAccess)
            {
                _logger.LogWarning($"Попытка доступа к странице {pageName} без необходимых прав");
                return;
            }

            _navigationService.NavigateTo(pageName);
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Загрузка спектаклей
                var performances = await _supabaseService.GetPerformancesAsync();
                Performances.Clear();
                foreach (var performance in performances)
                {
                    Performances.Add(performance);
                }

                // Загрузка расписания
                var schedules = await _supabaseService.GetScheduleAsync();
                Schedules.Clear();
                foreach (var schedule in schedules)
                {
                    Schedules.Add(schedule);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при загрузке данных: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void NavigateToPerformanceDetails(Performance performance)
        {
            _navigationService.NavigateTo("PerformanceDetailPage", performance);
        }

        [RelayCommand]
        public void NavigateToScheduleDetails(Schedule schedule)
        {
            _navigationService.NavigateTo("ScheduleDetailPage", schedule);
        }

        [RelayCommand]
        public void NavigateToAddPerformance()
        {
            _navigationService.NavigateTo("AddPerformancePage");
        }

        [RelayCommand]
        public void NavigateToSelectSeat(Schedule schedule)
        {
            _navigationService.NavigateTo("SelectSeatPage", schedule.Id);
        }

        [RelayCommand]
        public void NavigateToAddSchedule()
        {
            _navigationService.NavigateTo("AddSchedulePage");
        }
    }
} 
