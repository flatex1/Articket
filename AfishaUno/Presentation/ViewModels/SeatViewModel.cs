using CommunityToolkit.Mvvm.ComponentModel;

namespace AfishaUno.Presentation.ViewModels
{
    [ObservableObject]
    public partial class SeatViewModel
    {
        private readonly Seat _seat;

        [ObservableProperty]
        private double _x;

        [ObservableProperty]
        private double _y;

        [ObservableProperty]
        private double _absoluteX;

        [ObservableProperty]
        private double _absoluteY;

        [ObservableProperty]
        private string _status;

        public Guid Id => _seat.Id;
        public int RowNumber => _seat.RowNumber;
        public int SeatNumber => _seat.SeatNumber;
        public string Category => _seat.Category;

        public Ticket Ticket { get; set; }

        public SeatViewModel(Seat seat)
        {
            _seat = seat;
            Status = "Available";
        }

        public SeatViewModel()
        {
        }
    }
}
