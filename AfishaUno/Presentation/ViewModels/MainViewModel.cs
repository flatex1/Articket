using AfishaUno.Models;
using AfishaUno.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AfishaUno.Presentation.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;

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

        public MainViewModel(ISupabaseService supabaseService, INavigationService navigationService)
        {
            _supabaseService = supabaseService;
            _navigationService = navigationService;
            CurrentUser = _supabaseService.CurrentUser;
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
        public async Task LogoutAsync()
        {
            await _supabaseService.LogoutAsync();
            _navigationService.NavigateTo("LoginPage");
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
    }
} 
