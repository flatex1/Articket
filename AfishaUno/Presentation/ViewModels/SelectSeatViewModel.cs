using AfishaUno.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

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

        private void LoadHallScheme(List<Seat> allSeats, List<Ticket> tickets)
        {
            // Определяем занятые места
            var soldSeatIds = tickets
                .Where(t => t.Status is TicketStatuses.Sold or TicketStatuses.Reserved)
                .Select(t => t.SeatId)
                .ToHashSet();

            // Базовые параметры для отображения зала
            double startX = 50;
            double startY = 100;
            double seatWidth = 30;
            double seatHeight = 30;
            double rowSpacing = 40;
            double seatSpacing = 35;

            // Формируем места для разных секций зала
            // Партер
            var parterSeats = allSeats.Where(s => s.Category == "Партер").ToList();
            foreach (var seat in parterSeats)
            {
                var seatVM = new SeatViewModel(seat)
                {
                    X = startX + (seat.SeatNumber - 1) * seatSpacing,
                    Y = startY + (seat.RowNumber - 1) * rowSpacing,
                    Status = soldSeatIds.Contains(seat.Id) ? "Sold" : "Available"
                };

                Seats.Add(seatVM);
            }

            // Балкон (выше партера)
            startY = 20;
            var balconySeats = allSeats.Where(s => s.Category == "Балкон").ToList();
            foreach (var seat in balconySeats)
            {
                var seatVM = new SeatViewModel(seat)
                {
                    X = startX + (seat.SeatNumber - 1) * seatSpacing,
                    Y = startY + (seat.RowNumber - 1) * rowSpacing,
                    Status = soldSeatIds.Contains(seat.Id) ? "Sold" : "Available"
                };

                Seats.Add(seatVM);
            }

            // Амфитеатр (позади партера)
            startY = 300;
            var amphitheaterSeats = allSeats.Where(s => s.Category == "Амфитеатр").ToList();
            foreach (var seat in amphitheaterSeats)
            {
                var seatVM = new SeatViewModel(seat)
                {
                    X = startX + (seat.SeatNumber - 1) * seatSpacing,
                    Y = startY + (seat.RowNumber - 1) * rowSpacing,
                    Status = soldSeatIds.Contains(seat.Id) ? "Sold" : "Available"
                };

                Seats.Add(seatVM);
            }

            // Ложи (по бокам)
            var boxSeats = allSeats.Where(s => s.Category == "Ложа").ToList();
            foreach (var seat in boxSeats)
            {
                // Левые ложи
                if (seat.SeatNumber <= 4)
                {
                    var seatVM = new SeatViewModel(seat)
                    {
                        X = 10 + (seat.SeatNumber - 1) * seatSpacing,
                        Y = 100 + (seat.RowNumber - 1) * rowSpacing,
                        Status = soldSeatIds.Contains(seat.Id) ? "Sold" : "Available"
                    };

                    Seats.Add(seatVM);
                }
                // Правые ложи
                else
                {
                    var seatVM = new SeatViewModel(seat)
                    {
                        X = 500 + ((seat.SeatNumber - 5) % 4) * seatSpacing,
                        Y = 100 + (seat.RowNumber - 1) * rowSpacing,
                        Status = soldSeatIds.Contains(seat.Id) ? "Sold" : "Available"
                    };

                    Seats.Add(seatVM);
                }
            }
        }

        private void OnSelectSeat(SeatViewModel seat)
        {
            if (seat == null || seat.Status == "Sold")
                return;

            // Сбрасываем выбор с предыдущего места
            if (SelectedSeat != null && SelectedSeat != seat)
            {
                SelectedSeat.Status = "Available";
            }

            // Устанавливаем новое выбранное место
            if (seat.Status == "Selected")
            {
                seat.Status = "Available";
                SelectedSeat = null;
            }
            else
            {
                seat.Status = "Selected";
                SelectedSeat = seat;
            }

            // Обновляем доступность команды продажи
            SellTicketCommand.NotifyCanExecuteChanged();
        }

        private bool CanSellTicket()
        {
            return SelectedSeat != null;
        }

        private async Task SellTicketAsync()
        {
            if (SelectedSeat == null)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Создаем билет
                var ticket = new Ticket
                {
                    ScheduleId = Schedule.Id,
                    SeatId = SelectedSeat.Id,
                    Status = TicketStatuses.Sold,
                    Price = TicketPrice,
                    CreatedBy = _supabaseService.CurrentUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Сохраняем билет
                var result = await _supabaseService.SellTicketAsync(ticket);

                if (result != null)
                {
                    // Обновляем статус места
                    SelectedSeat.Status = "Sold";
                    SelectedSeat = null;

                    // TODO: Показать сообщение об успешной продаже или перейти к странице билета

                    // Возвращаемся к списку расписаний
                    _navigationService.GoBack();
                }
                else
                {
                    ErrorMessage = "Не удалось продать билет";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                Trace.WriteLine($"Ошибка продажи билета: {ex.Message}");
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
