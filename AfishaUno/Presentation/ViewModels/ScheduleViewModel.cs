using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AfishaUno.Models;
using AfishaUno.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AfishaUno.Presentation.ViewModels
{
    public partial class ScheduleViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<ScheduleViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<Schedule> _schedules = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _isEmptySchedule;

        public IAsyncRelayCommand<Guid> SelectSeatsCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ScheduleViewModel(
            ISupabaseService supabaseService, 
            INavigationService navigationService,
            ILogger<ScheduleViewModel> logger)
        {
            _supabaseService = supabaseService;
            _navigationService = navigationService;
            _logger = logger;

            SelectSeatsCommand = new AsyncRelayCommand<Guid>(SelectSeatsAsync);
            CancelCommand = new RelayCommand(OnCancel);
        }

        public async Task LoadSchedulesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Загрузка расписания с подробностями о спектакле и зале
                var schedules = await _supabaseService.GetScheduleWithDetailsAsync();
                
                // Сортировка расписания по дате (ближайшие сеансы сверху)
                var sortedSchedules = schedules
                    .Where(s => s.StartTime > DateTime.Now) // Только будущие сеансы
                    .OrderBy(s => s.StartTime)
                    .ToList();

                Schedules.Clear();
                foreach (var schedule in sortedSchedules)
                {
                    Schedules.Add(schedule);
                }

                // Проверка на пустое расписание
                IsEmptySchedule = Schedules.Count == 0;

                _logger.LogInformation($"Загружено {Schedules.Count} записей расписания");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при загрузке расписания: {ex.Message}";
                _logger.LogError(ex, "Ошибка при загрузке расписания");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SelectSeatsAsync(Guid scheduleId)
        {
            try
            {
                if (scheduleId == Guid.Empty)
                {
                    _logger.LogWarning("Выбрано пустое значение scheduleId");
                    return;
                }

                _logger.LogInformation($"Переход к выбору мест для расписания с Id={scheduleId}");
                _navigationService.NavigateTo("SelectSeatPage", scheduleId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при выборе мест: {ex.Message}";
                _logger.LogError(ex, $"Ошибка при выборе мест для расписания с Id={scheduleId}");
            }
        }

        private void OnCancel()
        {
            _logger.LogInformation("Возврат с экрана расписания");
            _navigationService.GoBack();
        }
    }
} 