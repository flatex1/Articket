using System;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace AfishaUno.Models
{
    [Table("seats")]
    public class Seat : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("hall_id")]
        public Guid HallId { get; set; }

        [Column("row_number")]
        public int RowNumber { get; set; }

        [Column("seat_number")]
        public int SeatNumber { get; set; }

        [Column("category")]
        public string Category { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Available";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Seat seat &&
                   Id.Equals(seat.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
} 
