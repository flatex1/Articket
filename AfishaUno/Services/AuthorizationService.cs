using Microsoft.Extensions.Logging;

namespace AfishaUno.Services
{
    public interface IAuthorizationService
    {
        bool CanManagePerformances();
        bool CanManageHalls();
        bool CanManageSchedule();
        bool CanSellTickets();
        bool CanViewReports();
        bool CanManageUsers();
        bool CanAccessAdminSection();
        bool IsUserLoggedIn();
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly ISupabaseService _supabaseService;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(ISupabaseService supabaseService, ILogger<AuthorizationService> logger)
        {
            _supabaseService = supabaseService;
            _logger = logger;
        }

        /// <summary>
        /// Проверяет, может ли текущий пользователь управлять спектаклями
        /// </summary>
        public bool CanManagePerformances()
        {
            return IsAdmin();
        }

        /// <summary>
        /// Проверяет, может ли текущий пользователь управлять залами
        /// </summary>
        public bool CanManageHalls()
        {
            return IsAdmin();
        }

        /// <summary>
        /// Проверяет, может ли текущий пользователь управлять расписанием
        /// </summary>
        public bool CanManageSchedule()
        {
            return IsAdmin();
        }

        /// <summary>
        /// Проверяет, может ли текущий пользователь продавать билеты
        /// </summary>
        public bool CanSellTickets()
        {
            // Проверяем, что пользователь авторизован и является кассиром
            var currentUser = _supabaseService.CurrentUser;
            if (currentUser == null)
            {
                _logger.LogWarning("Попытка проверки прав на продажу билетов без авторизации");
                return false;
            }

            return currentUser.Role == UserRoles.Cashier; // Только кассир может продавать билеты
        }

        /// <summary>
        /// Проверяет, может ли текущий пользователь просматривать отчеты
        /// </summary>
        public bool CanViewReports()
        {
            return IsAdmin();
        }

        /// <summary>
        /// Проверяет, может ли текущий пользователь управлять учетными записями
        /// </summary>
        public bool CanManageUsers()
        {
            return IsAdmin();
        }

        /// <summary>
        /// Проверяет, может ли текущий пользователь входить в административный раздел
        /// </summary>
        public bool CanAccessAdminSection()
        {
            return IsAdmin();
        }

        /// <summary>
        /// Проверяет, вошел ли пользователь в систему
        /// </summary>
        public bool IsUserLoggedIn()
        {
            return _supabaseService.CurrentUser != null;
        }

        /// <summary>
        /// Проверяет, является ли текущий пользователь администратором
        /// </summary>
        private bool IsAdmin()
        {
            var currentUser = _supabaseService.CurrentUser;
            if (currentUser == null)
            {
                _logger.LogWarning("Попытка проверки прав без авторизации");
                return false;
            }

            return currentUser.Role == UserRoles.Admin;
        }
    }
} 