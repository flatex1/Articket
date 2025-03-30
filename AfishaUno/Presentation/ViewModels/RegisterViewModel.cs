using AfishaUno.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace AfishaUno.Presentation.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _confirmPassword;

        [ObservableProperty]
        private string _fullName;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _logOutput = "Логи будут отображаться здесь...";

        public RegisterViewModel(ISupabaseService supabaseService, INavigationService navigationService)
        {
            _supabaseService = supabaseService;
            _navigationService = navigationService;
            
            // Подписываемся на события отладки
            System.Diagnostics.Trace.Listeners.Add(new DebugTraceListener(this));
        }

        // Класс для перехвата сообщений отладки
        private class DebugTraceListener : System.Diagnostics.TraceListener
        {
            private readonly RegisterViewModel _viewModel;
            
            public DebugTraceListener(RegisterViewModel viewModel)
            {
                _viewModel = viewModel;
            }
            
            public override void Write([AllowNull] string message)
            {
                _viewModel.AppendToLog(message ?? string.Empty);
            }
            
            public override void WriteLine([AllowNull] string message)
            {
                _viewModel.AppendToLog((message ?? string.Empty) + Environment.NewLine);
            }
        }
        
        private void AppendToLog(string message)
        {
            // Ограничиваем размер лога, чтобы избежать проблем с производительностью
            const int maxLength = 10000;
            
            // Добавляем новое сообщение в начало (чтобы новые логи были видны)
            LogOutput = message + Environment.NewLine + LogOutput;
            
            // Обрезаем лог, если он слишком длинный
            if (LogOutput.Length > maxLength)
            {
                LogOutput = LogOutput.Substring(0, maxLength);
            }
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword) || string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "Заполните все поля";
                System.Diagnostics.Trace.WriteLine("[ERROR] Не все поля заполнены при регистрации");
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                System.Diagnostics.Trace.WriteLine("[ERROR] Пароли не совпадают при регистрации");
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                System.Diagnostics.Trace.WriteLine($"[INFO] Начало регистрации администратора: {Email}");

                var success = await _supabaseService.RegisterAdminAsync(Email, Password, FullName);

                if (success)
                {
                    System.Diagnostics.Trace.WriteLine("[INFO] Регистрация успешна, пытаемся войти в систему");
                    // Автоматически входим в систему
                    await _supabaseService.LoginAsync(Email, Password);
                    
                    System.Diagnostics.Trace.WriteLine("[INFO] Вход успешен, переходим на главную страницу");
                    // Навигация на главную страницу
                    _navigationService.NavigateTo("MainPage");
                }
                else
                {
                    ErrorMessage = "Не удалось создать администратора. Возможно, в системе уже есть пользователи.";
                    System.Diagnostics.Trace.WriteLine("[ERROR] Не удалось создать администратора");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при регистрации: {ex.Message}";
                System.Diagnostics.Trace.WriteLine($"[ERROR] Исключение при регистрации: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Trace.WriteLine($"[ERROR] Внутреннее исключение: {ex.InnerException.Message}");
                }
                System.Diagnostics.Trace.WriteLine($"[ERROR] Стек вызовов: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Trace.WriteLine("[INFO] Завершение процесса регистрации");
            }
        }

        [RelayCommand]
        private void NavigateToLogin()
        {
            System.Diagnostics.Trace.WriteLine("[INFO] Переход на страницу входа");
            _navigationService.NavigateTo("LoginPage");
        }

        // Отладочные методы - только для разработки
        [RelayCommand]
        private async Task CheckUserCountAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                System.Diagnostics.Trace.WriteLine("[DEBUG] Проверка количества пользователей...");

                var count = await _supabaseService.GetUserCountAsync();
                
                ErrorMessage = $"Количество пользователей в системе: {count}";
                System.Diagnostics.Trace.WriteLine($"[DEBUG] Найдено пользователей: {count}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка проверки: {ex.Message}";
                System.Diagnostics.Trace.WriteLine($"[ERROR] Исключение при проверке пользователей: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Trace.WriteLine($"[ERROR] Внутреннее исключение: {ex.InnerException.Message}");
                }
                System.Diagnostics.Trace.WriteLine($"[ERROR] Стек вызовов: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Trace.WriteLine("[DEBUG] Завершение проверки пользователей");
            }
        }

        [RelayCommand]
        private async Task ResetDatabaseAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                System.Diagnostics.Trace.WriteLine("[DEBUG] Начало процесса сброса базы данных...");

                var success = await _supabaseService.ResetDatabaseAsync();
                
                ErrorMessage = success ? 
                    "База данных успешно сброшена. Теперь вы можете зарегистрировать администратора." : 
                    "Не удалось сбросить базу данных.";
                
                System.Diagnostics.Trace.WriteLine($"[DEBUG] Результат сброса базы данных: {(success ? "успешно" : "неудачно")}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка сброса базы данных: {ex.Message}";
                System.Diagnostics.Trace.WriteLine($"[ERROR] Исключение при сбросе базы данных: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Trace.WriteLine($"[ERROR] Внутреннее исключение: {ex.InnerException.Message}");
                }
                System.Diagnostics.Trace.WriteLine($"[ERROR] Стек вызовов: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Trace.WriteLine("[DEBUG] Завершение процесса сброса базы данных");
            }
        }
    }
} 