using System;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Runtime.Serialization;

namespace AfishaUno.Models
{
    // Константы для статусов билетов вместо перечисления
    public static class TicketStatuses
    {
        public const string Sold = "sold";
        public const string Reserved = "reserved";
        public const string Cancelled = "cancelled";
    }

    [Table("tickets")]
    public class Ticket : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("schedule_id")]
        public Guid ScheduleId { get; set; }

        [Column("seat_id")]
        public Guid SeatId { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("discount_category_id")]
        public Guid? DiscountCategoryId { get; set; }

        [Column("customer_id")]
        public Guid? CustomerId { get; set; }

        [Column("loyalty_card_id")]
        public Guid? LoyaltyCardId { get; set; }

        [Column("reserved_until")]
        public DateTime? ReservedUntil { get; set; }

        [Column("qr_code")]
        public string QrCode { get; set; }

        [Column("created_by")]
        public Guid CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Навигационные свойства
        public Schedule Schedule { get; set; }
        public Seat Seat { get; set; }
        public DiscountCategory DiscountCategory { get; set; }
        public User CreatedByUser { get; set; }
        public Customer Customer { get; set; }
        public LoyaltyCard LoyaltyCard { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Ticket ticket &&
                   Id.Equals(ticket.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
} 
