using AfishaUno.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.Input;

namespace AfishaUno.Presentation.ViewModels
{
    public partial class SelectSeatViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;
        private readonly IAuthorizationService _authorizationService;

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

        [ObservableProperty]
        private Ticket _selectedTicket;

        [ObservableProperty]
        private decimal _refundAmount;

        [ObservableProperty]
        private string _refundMessage;

        [ObservableProperty]
        private bool _canRefund;

        public IRelayCommand SelectSeatCommand { get; }
        public IAsyncRelayCommand SellTicketCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IRelayCommand RefundTicketCommand { get; }
        public IAsyncRelayCommand ReserveTicketCommand { get; }

        public bool CanReserveTicket
        {
            get
            {
                var can = ReserveTicketCommand.CanExecute(null);
                Trace.WriteLine($"[CanReserveTicket] CanExecute(null) = {can}, Schedule: {(Schedule == null ? "null" : Schedule.StartTime.ToString())}, SelectedSeat: {(SelectedSeat == null ? "null" : SelectedSeat.Id.ToString())}, Status: {(SelectedSeat == null ? "null" : SelectedSeat.Status)}");
                return can;
            }
        }

        partial void OnScheduleChanged(Schedule value)
        {
            Trace.WriteLine($"[OnScheduleChanged] Schedule изменён: {(value == null ? "null" : value.StartTime.ToString())}");
            OnPropertyChanged(nameof(CanReserveTicket));
        }

        partial void OnSelectedSeatChanged(SeatViewModel value)
        {
            Trace.WriteLine($"[OnSelectedSeatChanged] SelectedSeat изменён: {(value == null ? "null" : value.Id.ToString())}, Status: {(value == null ? "null" : value.Status)}");
            OnPropertyChanged(nameof(CanReserveTicket));
        }

        public SelectSeatViewModel(ISupabaseService supabaseService, INavigationService navigationService, IAuthorizationService authorizationService)
        {
            _supabaseService = supabaseService;
            _navigationService = navigationService;
            _authorizationService = authorizationService;

            SelectSeatCommand = new RelayCommand<SeatViewModel>(OnSelectSeat);
            SellTicketCommand = new AsyncRelayCommand(SellTicketAsync, CanSellTicket);
            CancelCommand = new RelayCommand(OnCancel);
            RefundTicketCommand = new RelayCommand(OnRefundTicket, () => CanRefund);
            ReserveTicketCommand = new AsyncRelayCommand(ReserveTicketAsync, CanReserve);
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

                // Получаем все билеты (проданные и забронированные)
                var tickets = await _supabaseService.GetTicketsAsync(scheduleId);

                // --- Автоматическая отмена просроченных броней ---
                var now = DateTime.UtcNow;
                var daysToShow = (Schedule.StartTime.Date - now.Date).TotalDays;
                if (daysToShow <= 3)
                {
                    var expiredReservations = tickets.Where(t => t.Status == TicketStatuses.Reserved).ToList();
                    foreach (var reservation in expiredReservations)
                    {
                        await CancelReservationAsync(reservation);
                    }
                    // После отмены обновить список билетов
                    tickets = await _supabaseService.GetTicketsAsync(scheduleId);
                }

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
            var reservedTickets = tickets.Where(t => t.Status == TicketStatuses.Reserved).ToList();

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
                        var ticket = soldTickets.FirstOrDefault(t => t.SeatId == seat.Id);
                        var reserved = reservedTickets.FirstOrDefault(t => t.SeatId == seat.Id);
                        if (ticket != null)
                        {
                            seatVM.Status = "Sold";
                            seatVM.Ticket = ticket;
                        }
                        else if (reserved != null)
                        {
                            seatVM.Status = "Reserved";
                            seatVM.Ticket = reserved;
                        }
                        else
                        {
                            seatVM.Status = "Available";
                        }
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
                        var ticket = soldTickets.FirstOrDefault(t => t.SeatId == seat.Id);
                        var reserved = reservedTickets.FirstOrDefault(t => t.SeatId == seat.Id);
                        if (ticket != null)
                        {
                            seatVM.Status = "Sold";
                            seatVM.Ticket = ticket;
                        }
                        else if (reserved != null)
                        {
                            seatVM.Status = "Reserved";
                            seatVM.Ticket = reserved;
                        }
                        else
                        {
                            seatVM.Status = "Available";
                        }
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
                        var ticket = soldTickets.FirstOrDefault(t => t.SeatId == seat.Id);
                        var reserved = reservedTickets.FirstOrDefault(t => t.SeatId == seat.Id);
                        if (ticket != null)
                        {
                            seatVM.Status = "Sold";
                            seatVM.Ticket = ticket;
                        }
                        else if (reserved != null)
                        {
                            seatVM.Status = "Reserved";
                            seatVM.Ticket = reserved;
                        }
                        else
                        {
                            seatVM.Status = "Available";
                        }
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
                        var ticket = soldTickets.FirstOrDefault(t => t.SeatId == seat.Id);
                        var reserved = reservedTickets.FirstOrDefault(t => t.SeatId == seat.Id);
                        if (ticket != null)
                        {
                            seatVM.Status = "Sold";
                            seatVM.Ticket = ticket;
                        }
                        else if (reserved != null)
                        {
                            seatVM.Status = "Reserved";
                            seatVM.Ticket = reserved;
                        }
                        else
                        {
                            seatVM.Status = "Available";
                        }
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
                        var ticket = soldTickets.FirstOrDefault(t => t.SeatId == seat.Id);
                        var reserved = reservedTickets.FirstOrDefault(t => t.SeatId == seat.Id);
                        if (ticket != null)
                        {
                            seatVM.Status = "Sold";
                            seatVM.Ticket = ticket;
                        }
                        else if (reserved != null)
                        {
                            seatVM.Status = "Reserved";
                            seatVM.Ticket = reserved;
                        }
                        else
                        {
                            seatVM.Status = "Available";
                        }
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
                Trace.WriteLine($"[OnSelectSeat] seat: {(seat == null ? "null" : seat.Id.ToString())}, Status: {(seat == null ? "null" : seat.Status)}");

                if (seat == null)
                {
                    Trace.WriteLine("Выбрано пустое место (null)");
                    return;
                }

                if (seat.Status == "Sold" && seat.Ticket != null)
                {
                    SelectedSeat = seat;
                    SelectedTicket = seat.Ticket;
                    CalculateRefund(seat.Ticket);
                    // Открытие диалога произойдет через PropertyChanged
                    return;
                }
                if (seat.Status == "Reserved" && seat.Ticket != null)
                {
                    SelectedSeat = seat;
                    SelectedTicket = seat.Ticket;
                    // Для забронированных билетов не открываем диалог возврата, просто выделяем
                    return;
                }

                // Сбрасываем выбор с предыдущего места, только если оно было Selected
                if (SelectedSeat != null && !SelectedSeat.Equals(seat))
                {
                    Trace.WriteLine($"Сброс статуса с предыдущего места: {SelectedSeat.Id}");
                    if (SelectedSeat.Status == "Selected")
                        SelectedSeat.Status = "Available";
                }

                // Устанавливаем новое выбранное место
                if (seat.Status == "Selected")
                {
                    Trace.WriteLine($"Отмена выбора места: {seat.Id}");
                    seat.Status = "Available";
                    SelectedSeat = null;
                }
                else if (seat.Status == "Available")
                {
                    Trace.WriteLine($"Выбор нового места: {seat.Id}");
                    seat.Status = "Selected";
                    SelectedSeat = seat;
                }
                else
                {
                    // Для Sold/Reserved просто выделяем, но не меняем статус
                    SelectedSeat = seat;
                }

                // Обновляем доступность команды продажи
                SellTicketCommand.NotifyCanExecuteChanged();

                // Сброс возврата
                SelectedTicket = null;
                RefundAmount = 0;
                RefundMessage = string.Empty;
                CanRefund = false;

                OnPropertyChanged(nameof(CanReserveTicket));
                ReserveTicketCommand.NotifyCanExecuteChanged();
                Trace.WriteLine($"[OnSelectSeat] После обновления: CanReserveTicket = {CanReserveTicket}");
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

        private void CalculateRefund(Ticket ticket)
        {
            var now = DateTime.UtcNow;
            var days = (Schedule.StartTime.Date - now.Date).TotalDays;
            if (days > 10)
            {
                RefundAmount = ticket.Price;
                RefundMessage = "Вернется полная стоимость билета.";
                CanRefund = true;
            }
            else if (days >= 6)
            {
                RefundAmount = ticket.Price * 0.5m;
                RefundMessage = "Вернется 50% стоимости билета.";
                CanRefund = true;
            }
            else if (days >= 4)
            {
                RefundAmount = ticket.Price * 0.3m;
                RefundMessage = "Вернется 30% стоимости билета.";
                CanRefund = true;
            }
            else
            {
                RefundAmount = 0;
                RefundMessage = "До спектакля осталось 3 дня или меньше — возврат невозможен.";
                CanRefund = false;
            }
        }

        private async void OnRefundTicket()
        {
            if (SelectedTicket == null || !CanRefund)
                return;
            // Удаляем билет из базы данных
            await _supabaseService.DeleteTicketAsync(SelectedTicket.Id);
            // После возврата обновить схему зала и UI
            await InitializeAsync(Schedule.Id);
        }

        private bool CanReserve()
        {
            if (Schedule == null)
            {
                Trace.WriteLine("[CanReserve] Schedule == null");
                return false;
            }
            var days = (Schedule.StartTime.Date - DateTime.UtcNow.Date).TotalDays;
            var canReserve = days > 3 && SelectedSeat != null && SelectedSeat.Status == "Selected";
            Trace.WriteLine($"[CanReserve] days: {days}, SelectedSeat: {(SelectedSeat == null ? "null" : SelectedSeat.Id.ToString())}, Status: {(SelectedSeat == null ? "null" : SelectedSeat.Status)}, canReserve: {canReserve}");
            return canReserve;
        }

        private async Task ReserveTicketAsync()
        {
            Trace.WriteLine($"[ReserveTicketAsync] Попытка бронирования. CanReserve = {CanReserve()}, SelectedSeat: {(SelectedSeat == null ? "null" : SelectedSeat.Id.ToString())}, Schedule: {(Schedule == null ? "null" : Schedule.StartTime.ToString())}");
            if (!CanReserve()) return;

            var currentUser = _supabaseService.CurrentUser;
            if (currentUser == null)
            {
                ErrorMessage = "Ошибка авторизации. Пожалуйста, войдите в систему снова.";
                Trace.WriteLine("[ReserveTicketAsync] Ошибка: пользователь не авторизован");
                return;
            }

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                ScheduleId = Schedule.Id,
                SeatId = SelectedSeat.Id,
                Status = TicketStatuses.Reserved,
                Price = TicketPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUser.Id
            };
            await _supabaseService.CreateTicketAsync(ticket);
            await InitializeAsync(Schedule.Id);
            OnPropertyChanged(nameof(CanReserveTicket));
            ReserveTicketCommand.NotifyCanExecuteChanged();
            Trace.WriteLine("[ReserveTicketAsync] Бронирование завершено");
        }

        // Метод для отмены брони (аналог возврата билета, но без расчёта возврата)
        private async Task CancelReservationAsync(Ticket reservation)
        {
            if (reservation == null || reservation.Status != TicketStatuses.Reserved)
                return;
            await _supabaseService.DeleteTicketAsync(reservation.Id);
        }
    }
}
