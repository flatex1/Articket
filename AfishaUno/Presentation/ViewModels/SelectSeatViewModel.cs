using AfishaUno.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace AfishaUno.Presentation.ViewModels
{
    public partial class SelectSeatViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private Schedule _schedule;

        [ObservableProperty]
        private ObservableCollection<SeatViewModel> _seats = new();

        [ObservableProperty]
        private SeatViewModel _selectedSeat;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private decimal _ticketPrice;

        [ObservableProperty]
        private ObservableCollection<SeatViewModel> _balconySeats = new();

        [ObservableProperty]
        private ObservableCollection<SeatViewModel> _parterSeats = new();

        [ObservableProperty]
        private ObservableCollection<SeatViewModel> _amphitheaterSeats = new();

        [ObservableProperty]
        private ObservableCollection<SeatViewModel> _leftBoxSeats = new();

        [ObservableProperty]
        private ObservableCollection<SeatViewModel> _rightBoxSeats = new();

        public IRelayCommand SelectSeatCommand { get; }
        public IAsyncRelayCommand SellTicketCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public SelectSeatViewModel(ISupabaseService supabaseService, INavigationService navigationService)
        {
            _supabaseService = supabaseService;
            _navigationService = navigationService;

            SelectSeatCommand = new RelayCommand<SeatViewModel>(OnSelectSeat);
            SellTicketCommand = new AsyncRelayCommand(SellTicketAsync, CanSellTicket);
            CancelCommand = new RelayCommand(OnCancel);
        }

        public async Task InitializeAsync(Guid scheduleId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Получаем информацию о расписании
                Schedule = await _supabaseService.GetScheduleDetailsAsync(scheduleId);
                if (Schedule == null)
                {
                    ErrorMessage = "Не удалось загрузить информацию о сеансе";
                    return;
                }

                // Устанавливаем цену билета из расписания
                TicketPrice = Schedule.BasePrice;

                // Получаем все места в зале
                var allSeats = await _supabaseService.GetSeatsAsync(Schedule.HallId);

                // Получаем проданные билеты
                var tickets = await _supabaseService.GetTicketsAsync(scheduleId);

                // Создаем модели для отображения
                Seats.Clear();

                // Загружаем схему зала
                LoadHallScheme(allSeats, tickets);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                Trace.WriteLine($"Ошибка инициализации: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadHallScheme(IEnumerable<Seat> seats, IEnumerable<Ticket> tickets)
        {
            var soldTickets = tickets.Where(t => t.Status == TicketStatuses.Sold).ToList();

            // Очищаем все коллекции
            Seats.Clear();
            BalconySeats.Clear();
            ParterSeats.Clear();
            AmphitheaterSeats.Clear();
            LeftBoxSeats.Clear();
            RightBoxSeats.Clear();

            // Группируем места по категориям
            var seatsByCategory = seats.GroupBy(s => s.Category.ToLower()).ToDictionary(g => g.Key, g => g.ToList());

            // Выводим отладочную информацию о местах
            Trace.WriteLine($"Загружено мест: {seats.Count()}");
            foreach (var group in seatsByCategory)
            {
                Trace.WriteLine($"Категория {group.Key}: {group.Value.Count} мест");
                foreach (var row in group.Value.GroupBy(s => s.RowNumber).OrderBy(r => r.Key))
                {
                    Trace.WriteLine($"  Ряд {row.Key}: {row.Count()} мест");
                }
            }

            // Обработка балкона
            if (seatsByCategory.TryGetValue("балкон", out var balconySeats))
            {
                foreach (var rowGroup in balconySeats.GroupBy(s => s.RowNumber).OrderBy(g => g.Key))
                {
                    var rowSeats = rowGroup.OrderBy(s => s.SeatNumber).ToList();

                    foreach (var seat in rowSeats)
            {
                        var seatVM = new SeatViewModel(seat);
                        seatVM.Status = soldTickets.Any(t => t.SeatId == seat.Id) ? "Sold" : "Available";

                        BalconySeats.Add(seatVM);
                Seats.Add(seatVM);
            }
                }
            }

            // Обработка партера
            if (seatsByCategory.TryGetValue("партер", out var parterSeats))
            {
                foreach (var rowGroup in parterSeats.GroupBy(s => s.RowNumber).OrderBy(g => g.Key))
                {
                    var rowSeats = rowGroup.OrderBy(s => s.SeatNumber).ToList();

                    foreach (var seat in rowSeats)
            {
                        var seatVM = new SeatViewModel(seat);
                        seatVM.Status = soldTickets.Any(t => t.SeatId == seat.Id) ? "Sold" : "Available";

                        ParterSeats.Add(seatVM);
                Seats.Add(seatVM);
            }
                }
            }

            // Обработка амфитеатра
            if (seatsByCategory.TryGetValue("амфитеатр", out var amphiSeats))
            {
                foreach (var rowGroup in amphiSeats.GroupBy(s => s.RowNumber).OrderBy(g => g.Key))
                {
                    var rowSeats = rowGroup.OrderBy(s => s.SeatNumber).ToList();

                    foreach (var seat in rowSeats)
            {
                        var seatVM = new SeatViewModel(seat);
                        seatVM.Status = soldTickets.Any(t => t.SeatId == seat.Id) ? "Sold" : "Available";

                        AmphitheaterSeats.Add(seatVM);
                Seats.Add(seatVM);
                    }
                }
            }

            // Обработка лож
            if (seatsByCategory.TryGetValue("ложа", out var boxSeats))
            {
                var leftBoxSeats = boxSeats.Where(s => s.SeatNumber <= 4).ToList();
                var rightBoxSeats = boxSeats.Where(s => s.SeatNumber > 4).ToList();

                // Левые ложи
                foreach (var rowGroup in leftBoxSeats.GroupBy(s => s.RowNumber).OrderBy(g => g.Key))
                {
                    var rowSeats = rowGroup.OrderBy(s => s.SeatNumber).ToList();

                    foreach (var seat in rowSeats)
                    {
                        var seatVM = new SeatViewModel(seat);
                        seatVM.Status = soldTickets.Any(t => t.SeatId == seat.Id) ? "Sold" : "Available";

                        LeftBoxSeats.Add(seatVM);
                    Seats.Add(seatVM);
                    }
                }

                // Правые ложи
                foreach (var rowGroup in rightBoxSeats.GroupBy(s => s.RowNumber).OrderBy(g => g.Key))
                {
                    var rowSeats = rowGroup.OrderBy(s => s.SeatNumber).ToList();

                    foreach (var seat in rowSeats)
                    {
                        var seatVM = new SeatViewModel(seat);
                        seatVM.Status = soldTickets.Any(t => t.SeatId == seat.Id) ? "Sold" : "Available";

                        RightBoxSeats.Add(seatVM);
                    Seats.Add(seatVM);
                }
            }
            }

            // Выводим информацию о количестве мест в коллекциях
            Trace.WriteLine($"Всего мест в коллекциях:");
            Trace.WriteLine($"Балкон: {BalconySeats.Count}");
            Trace.WriteLine($"Партер: {ParterSeats.Count}");
            Trace.WriteLine($"Амфитеатр: {AmphitheaterSeats.Count}");
            Trace.WriteLine($"Левая ложа: {LeftBoxSeats.Count}");
            Trace.WriteLine($"Правая ложа: {RightBoxSeats.Count}");
            Trace.WriteLine($"Общая коллекция: {Seats.Count}");
        }

        private void OnSelectSeat(SeatViewModel seat)
        {
            try
            {
                Trace.WriteLine($"Выбор места: {seat?.Id}, статус: {seat?.Status}");
                
                if (seat == null)
                {
                    Trace.WriteLine("Выбрано пустое место (null)");
                return;
                }
                    
                if (seat.Status == "Sold")
                {
                    Trace.WriteLine("Место уже продано");
                    return;
                }

            // Сбрасываем выбор с предыдущего места
                if (SelectedSeat != null && !SelectedSeat.Equals(seat))
            {
                    Trace.WriteLine($"Сброс статуса с предыдущего места: {SelectedSeat.Id}");
                SelectedSeat.Status = "Available";
            }

            // Устанавливаем новое выбранное место
            if (seat.Status == "Selected")
            {
                    Trace.WriteLine($"Отмена выбора места: {seat.Id}");
                seat.Status = "Available";
                SelectedSeat = null;
            }
            else
            {
                    Trace.WriteLine($"Выбор нового места: {seat.Id}");
                seat.Status = "Selected";
                SelectedSeat = seat;
            }

            // Обновляем доступность команды продажи
            SellTicketCommand.NotifyCanExecuteChanged();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ОШИБКА при выборе места: {ex.GetType().Name}: {ex.Message}");
                Trace.WriteLine($"StackTrace: {ex.StackTrace}");
                ErrorMessage = $"Ошибка при выборе места: {ex.Message}";
            }
        }

        private bool CanSellTicket()
        {
            return SelectedSeat != null;
        }

        private async Task SellTicketAsync()
        {
            try
            {
                if (SelectedSeat == null)
                {
                    ErrorMessage = "Не выбрано место";
                    return;
                }

                // Вместо прямой продажи билета, переходим к форме оформления билета
                await _navigationService.NavigateToAsync("TicketDetailsPage", 
                    new Dictionary<string, object>
                    {
                        { "Schedule", Schedule },
                        { "SelectedSeat", SelectedSeat }
                    });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                Trace.WriteLine($"Исключение при переходе к оформлению билета: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Trace.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
            }
        }

        private async void OnCancel()
        {
            await _navigationService.GoBackAsync();
        }
    }
}
