using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfishaUno.Presentation.ViewModels
{
    public class HallSchemaViewModel(Seat seat, double x, double y) : ObservableObject
    {
        private Seat _seat = seat;
        private string _status = "Available"; // Available, Selected, Sold
        private double _x = x;
        private double _y = y;

        public Guid Id => _seat.Id;
        public Guid HallId => _seat.HallId;
        public int RowNumber => _seat.RowNumber;
        public int SeatNumber => _seat.SeatNumber;
        public string Category => _seat.Category;

        public double X => _x;
        public double Y => _y;

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
    }
}
