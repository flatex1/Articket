using AfishaUno.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace AfishaUno.Presentation.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ISupabaseService _supabaseService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<LoginViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private string _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    public LoginViewModel(ISupabaseService supabaseService, INavigationService navigationService, ILogger<LoginViewModel> logger, IServiceProvider serviceProvider)
    {
        _supabaseService = supabaseService;
        _navigationService = navigationService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Пожалуйста, введите email и пароль";
                return;
            }

            var result = await _supabaseService.LoginAsync(Email, Password);

            if (result)
            {
                // Успешный вход - перенаправляем на главную страницу
                _logger.LogInformation($"Пользователь {Email} успешно вошел в систему");
                _navigationService.NavigateTo("HomePage");
                
                // Обновляем главную модель приложения с информацией о пользователе
                var mainViewModel = _serviceProvider.GetService<MainViewModel>();
                mainViewModel?.UpdateUserInfo();
                
                var homeViewModel = _serviceProvider.GetService<HomeViewModel>();
                homeViewModel?.UpdateUserInfo();
            }
            else
            {
                ErrorMessage = "Неверный email или пароль";
                _logger.LogWarning($"Неудачная попытка входа для {Email}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка входа: {ex.Message}";
            _logger.LogError(ex, $"Ошибка входа: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NavigateToRegister()
    {
        _navigationService.NavigateTo("RegisterPage");
    }
}
