using System.Diagnostics;
using AfishaUno.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace AfishaUno.Presentation.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<HomeViewModel> _logger;

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

        public IRelayCommand<string> NavigateCommand { get; }

        public HomeViewModel(
            ISupabaseService supabaseService,
            INavigationService navigationService,
            IAuthorizationService authorizationService,
            ILogger<HomeViewModel> logger)
        {
            _supabaseService = supabaseService;
            _navigationService = navigationService;
            _authorizationService = authorizationService;
            _logger = logger;

            NavigateCommand = new RelayCommand<string>(OnNavigate);

            UpdateUserInfo();
        }

        public void UpdateUserInfo()
        {
            var currentUser = _supabaseService.CurrentUser;
            IsLoggedIn = currentUser != null;

            if (IsLoggedIn)
            {
                UserName = currentUser.FullName;
                if (currentUser.Role == UserRoles.Admin)
                {
                    UserRole = "Администратор";
                    ShowAdminMenu = true;
                    ShowCashierMenu = false;
                }
                else
                {
                    UserRole = "Кассир";
                    ShowAdminMenu = false;
                    ShowCashierMenu = true;
                }
                FillMenuItems(currentUser.Role);
            }
            else
            {
                UserName = string.Empty;
                UserRole = string.Empty;
                ShowAdminMenu = false;
                ShowCashierMenu = false;
                MenuItems.Clear();
            }

            _logger.LogInformation($"Обновлена информация о пользователе: IsLoggedIn={IsLoggedIn}, Role={UserRole}");
        }

        private void FillMenuItems(string role)
        {
            MenuItems.Clear();
            if (role == UserRoles.Admin)
            {
                MenuItems.Add(new MenuItem { Title = "Спектакли", Icon = "\uE7F4", PageName = "AddPerformancePage" });
                MenuItems.Add(new MenuItem { Title = "Расписание", Icon = "\uE8BF", PageName = "AddSchedulePage" });
                MenuItems.Add(new MenuItem { Title = "Отчёты", Icon = "\uE9D2", PageName = "ReportsPage" });
                MenuItems.Add(new MenuItem { Title = "Пользователи", Icon = "\uE77B", PageName = "UsersPage" });
            }
            else if (role == UserRoles.Cashier)
            {
                MenuItems.Add(new MenuItem { Title = "Продажа билетов", Icon = "\uE8A7", PageName = "SchedulePage" });
            }
        }

        private void OnNavigate(string pageName)
        {
            if (string.IsNullOrEmpty(pageName))
                return;

            _logger.LogInformation($"Попытка навигации на страницу: {pageName}");

            // Проверка прав доступа перед навигацией
            bool hasAccess = true;

            // Для страниц, требующих прав администратора
            if (pageName == "PerformancesPage" || pageName == "HallsPage" || 
                pageName == "ScheduleManagementPage" || pageName == "ReportsPage" || 
                pageName == "UsersPage")
            {
                hasAccess = _authorizationService.CanAccessAdminSection();
                _logger.LogInformation($"Проверка прав администратора: {hasAccess}");
            }
            
            // Для страниц, требующих авторизации
            if (pageName == "SchedulePage")
            {
                hasAccess = _authorizationService.CanSellTickets();
                _logger.LogInformation($"Проверка прав на продажу билетов: {hasAccess}");
            }

            if (!hasAccess)
            {
                _logger.LogWarning($"Попытка доступа к странице {pageName} без необходимых прав");
                return;
            }

            _logger.LogInformation($"Навигация на страницу: {pageName}");
            _navigationService.NavigateTo(pageName);
        }
    }
} 