using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AfishaUno.Presentation.Pages;
using AfishaUno.Services;
using AfishaUno.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace AfishaUno.Presentation.ViewModels
{
    public partial class AddPerformanceViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                {
                    SaveCommand.NotifyCanExecuteChanged();
                }
            }
        }

        [ObservableProperty]
        private string _description;

        private int _duration;
        public int Duration
        {
            get => _duration;
            set
            {
                if (SetProperty(ref _duration, value))
                {
                    SaveCommand.NotifyCanExecuteChanged();
                }
            }
        }

        [ObservableProperty]
        private string _posterUrl;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        public IAsyncRelayCommand SaveCommand { get; }

        public AddPerformanceViewModel(ISupabaseService supabaseService, INavigationService navigationService)
        {
            _supabaseService = supabaseService;
            _navigationService = navigationService;
            SaveCommand = new AsyncRelayCommand(SavePerformanceAsync, CanSavePerformance);
            
            Trace.WriteLine("[AddPerformanceViewModel] Инициализирован");
        }

        private bool CanSavePerformance() => !string.IsNullOrEmpty(Title) && Duration > 0;

        private async Task SavePerformanceAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                Trace.WriteLine($"[SavePerformanceAsync] Начало создания спектакля: Title='{Title}', Duration={Duration}");

                if (string.IsNullOrWhiteSpace(Title))
                {
                    ErrorMessage = "Введите название спектакля";
                    Trace.WriteLine("[SavePerformanceAsync] Ошибка: Пустое название спектакля");
                    return;
                }

                if (Duration <= 0)
                {
                    ErrorMessage = "Продолжительность должна быть больше нуля";
                    Trace.WriteLine("[SavePerformanceAsync] Ошибка: Неверная продолжительность");
                    return;
                }

                var performance = new Performance
            {
                    Id = Guid.NewGuid(), // Генерируем новый идентификатор
                Title = Title,
                    Description = Description ?? string.Empty,
                Duration = Duration,
                    PosterUrl = PosterUrl ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

                Trace.WriteLine($"[SavePerformanceAsync] Создан объект Performance: Id={performance.Id}, Title='{performance.Title}', Description.Length={performance.Description?.Length ?? 0}");

                // Проверяем, что сервис инициализирован
                if (_supabaseService == null)
                {
                    ErrorMessage = "Ошибка: сервис Supabase не инициализирован";
                    Trace.WriteLine("[SavePerformanceAsync] Ошибка: _supabaseService == null");
                    return;
                }
                
                Trace.WriteLine("[SavePerformanceAsync] Вызов CreatePerformanceAsync...");
            var result = await _supabaseService.CreatePerformanceAsync(performance);
                Trace.WriteLine($"[SavePerformanceAsync] Результат CreatePerformanceAsync: {(result != null ? "OK" : "NULL")}");

            if (result != null)
            {
                    Trace.WriteLine($"[SavePerformanceAsync] Спектакль успешно создан с Id={result.Id}");
                    // Перейти к списку спектаклей
                _navigationService.GoBack();
                }
                else
                {
                    ErrorMessage = "Не удалось создать спектакль. Проверьте введенные данные.";
                    Trace.WriteLine("[SavePerformanceAsync] Ошибка: результат создания спектакля = null");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                Trace.WriteLine($"[SavePerformanceAsync] Исключение: {ex.Message}");
                Trace.WriteLine($"[SavePerformanceAsync] StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Trace.WriteLine($"[SavePerformanceAsync] InnerException: {ex.InnerException.Message}");
                    Trace.WriteLine($"[SavePerformanceAsync] InnerException StackTrace: {ex.InnerException.StackTrace}");
                    ErrorMessage += $"\nВнутренняя ошибка: {ex.InnerException.Message}";
                }
            }
            finally
            {
                IsLoading = false;
                Trace.WriteLine("[SavePerformanceAsync] Завершено");
            }
        }
    }
}
