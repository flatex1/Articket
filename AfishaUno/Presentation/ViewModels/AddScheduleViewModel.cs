using AfishaUno.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AfishaUno.Presentation.ViewModels
{
    public partial class AddScheduleViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private ObservableCollection<Performance> _performances = new();

        [ObservableProperty]
        private Hall _selectedHall;

        [ObservableProperty]
        private string _hallInfo = "Загрузка информации о зале...";

        [ObservableProperty]
        private bool _canInitializeSeats = false;

        [ObservableProperty]
        private Performance _selectedPerformance;

        [ObservableProperty]
        private DateTimeOffset _selectedDate = DateTimeOffset.Now;

        [ObservableProperty]
        private TimeSpan _selectedTime = new TimeSpan(19, 0, 0); // 19:00

        [ObservableProperty]
        private double _basePrice = 500.0;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        partial void OnSelectedPerformanceChanged(Performance value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }

        partial void OnSelectedHallChanged(Hall value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }

        partial void OnBasePriceChanged(double value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }

        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand InitializeSeatsCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public AddScheduleViewModel(ISupabaseService supabaseService, INavigationService navigationService)
        {
            _supabaseService = supabaseService;
            _navigationService = navigationService;

            SaveCommand = new AsyncRelayCommand(SaveScheduleAsync, CanSaveSchedule);
            InitializeSeatsCommand = new AsyncRelayCommand(InitializeSeatsAsync);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private bool CanSaveSchedule()
        {
            return SelectedPerformance != null &&
            BasePrice > 0;
        }

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

                // Загрузка или создание зала
                var halls = await _supabaseService.GetHallsAsync();

                if (halls.Count == 0)
                {
                    // Создаем стандартный зал автоматически
                    var defaultHall = new Hall
                    {
                        Id = Guid.NewGuid(),
                        Name = "Главный зал",
                        Capacity = 200,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var createdHall = await _supabaseService.CreateHallAsync(defaultHall);
                    if (createdHall != null)
                    {
                        SelectedHall = createdHall;
                        HallInfo = $"Выбран зал: {SelectedHall.Name} (создан автоматически)";
                    }
                    else
                    {
                        ErrorMessage = "Не удалось создать зал";
                        HallInfo = "Ошибка создания зала";
                        return;
                    }
                }
                else
                {
                    // Выбираем первый зал
                    SelectedHall = halls[0];
                    HallInfo = $"Выбран зал: {SelectedHall.Name}";
                }

                // Проверяем, можно ли инициализировать места
                var existingSeats = await _supabaseService.GetSeatsAsync(SelectedHall.Id);
                CanInitializeSeats = existingSeats.Count == 0;

                if (!CanInitializeSeats)
                {
                    HallInfo = $"Выбран зал: {SelectedHall.Name} (уже имеет {existingSeats.Count} мест)";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки данных: {ex.Message}";
                Trace.WriteLine($"[AddScheduleViewModel] Ошибка: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveScheduleAsync()
        {
            if (SelectedPerformance == null || SelectedHall == null)
            {
                ErrorMessage = "Выберите спектакль";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var schedule = new Schedule
                {
                    Id = Guid.NewGuid(),
                    PerformanceId = SelectedPerformance.Id,
                    HallId = SelectedHall.Id,
                    StartTime = SelectedDate.DateTime.Add(SelectedTime),
                    BasePrice = (decimal)BasePrice,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                Trace.WriteLine($"[SaveScheduleAsync] Создаем расписание: Performance={SelectedPerformance.Title}, Hall={SelectedHall.Name}, Time={schedule.StartTime}");

                var result = await _supabaseService.CreateScheduleAsync(
                    schedule.PerformanceId,
                    schedule.HallId,
                    schedule.StartTime,
                    schedule.BasePrice);

                if (result != null)
                {
                    Trace.WriteLine($"[SaveScheduleAsync] Расписание успешно создано с Id={result.Id}");
                    _navigationService.GoBack();
                }
                else
                {
                    ErrorMessage = "Не удалось создать расписание";
                    Trace.WriteLine("[SaveScheduleAsync] Ошибка: Результат создания расписания = null");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                Trace.WriteLine($"[SaveScheduleAsync] Исключение: {ex.Message}");

                if (ex.InnerException != null)
                {
                    Trace.WriteLine($"[SaveScheduleAsync] Внутреннее исключение: {ex.InnerException.Message}");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task InitializeSeatsAsync()
        {
            try
            {
                if (SelectedHall == null)
                {
                    ErrorMessage = "Зал не найден";
                    return;
                }

                IsLoading = true;
                ErrorMessage = string.Empty;

                Trace.WriteLine($"[InitializeSeatsAsync] Инициализация мест для зала: {SelectedHall.Name}");

                // Проверяем, есть ли уже места в этом зале
                var existingSeats = await _supabaseService.GetSeatsAsync(SelectedHall.Id);
                if (existingSeats.Count > 0)
                {
                    ErrorMessage = $"В зале уже есть {existingSeats.Count} мест. Инициализация невозможна.";
                    return;
                }

                // Создаем места для разных категорий
                var allSeats = new List<Seat>();

                // Партер (10 рядов по 15 мест)
                for (int row = 1; row <= 10; row++)
                {
                    for (int seatNum = 1; seatNum <= 15; seatNum++)
                    {
                        allSeats.Add(new Seat
                        {
                            Id = Guid.NewGuid(),
                            HallId = SelectedHall.Id,
                            RowNumber = row,
                            SeatNumber = seatNum,
                            Category = "Партер",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // Балкон (5 рядов по 10 мест)
                for (int row = 1; row <= 5; row++)
                {
                    for (int seatNum = 1; seatNum <= 10; seatNum++)
                    {
                        allSeats.Add(new Seat
                        {
                            Id = Guid.NewGuid(),
                            HallId = SelectedHall.Id,
                            RowNumber = row,
                            SeatNumber = seatNum,
                            Category = "Балкон",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // Амфитеатр (3 ряда по 8 мест)
                for (int row = 1; row <= 3; row++)
                {
                    for (int seatNum = 1; seatNum <= 8; seatNum++)
                    {
                        allSeats.Add(new Seat
                        {
                            Id = Guid.NewGuid(),
                            HallId = SelectedHall.Id,
                            RowNumber = row,
                            SeatNumber = seatNum,
                            Category = "Амфитеатр",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // Ложи (4 ложи по 2 места в каждой)
                for (int row = 1; row <= 2; row++)
                {
                    for (int seatNum = 1; seatNum <= 8; seatNum++)
                    {
                        allSeats.Add(new Seat
                        {
                            Id = Guid.NewGuid(),
                            HallId = SelectedHall.Id,
                            RowNumber = row,
                            SeatNumber = seatNum,
                            Category = "Ложа",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                Trace.WriteLine($"[InitializeSeatsAsync] Подготовлено {allSeats.Count} мест для создания");

                // Сохраняем все места
                var result = await _supabaseService.CreateSeatsAsync(allSeats);

                if (result)
                {
                    ErrorMessage = $"Успешно создано {allSeats.Count} мест в зале";
                    Trace.WriteLine($"[InitializeSeatsAsync] Успех: Создано {allSeats.Count} мест");
                    CanInitializeSeats = false;
                    HallInfo = $"Выбран зал: {SelectedHall.Name} (инициализировано {allSeats.Count} мест)";
                }
                else
                {
                    ErrorMessage = "Не удалось создать места в зале";
                    Trace.WriteLine("[InitializeSeatsAsync] Ошибка: Не удалось создать места");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                Trace.WriteLine($"[InitializeSeatsAsync] Исключение: {ex.Message}");

                if (ex.InnerException != null)
                {
                    Trace.WriteLine($"[InitializeSeatsAsync] Внутреннее исключение: {ex.InnerException.Message}");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnCancel()
        {
            _navigationService.GoBack();
        }
    }
}