namespace AfishaUno.Presentation.ViewModels
{
    public partial class SeatViewModel : ObservableObject
    {
        private readonly Seat _seat;
        private string _status;

        public SeatViewModel(Seat seat)
        {
            _seat = seat;
            _status = "Available";
        }

        public Guid Id => _seat.Id;
        public Guid HallId => _seat.HallId;
        public int RowNumber => _seat.RowNumber;
        public int SeatNumber => _seat.SeatNumber;
        public string Category => _seat.Category;

        // Координаты для отображения в UI
        public double X { get; set; }
        public double Y { get; set; }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
    }
}
