using AfishaUno.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace AfishaUno.Presentation.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ISupabaseService _supabaseService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private string _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    public LoginViewModel(ISupabaseService supabaseService, INavigationService navigationService)
    {
        _supabaseService = supabaseService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите email и пароль";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var success = await _supabaseService.LoginAsync(Email, Password);

            if (success)
            {
                // Навигация на главную страницу
                _navigationService.NavigateTo("MainPage");
            }
            else
            {
                ErrorMessage = "Неверный email или пароль";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка при входе: {ex.Message}";
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
