using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AfishaUno.Models.DTOs
{
    [Table("tickets")]
    public class TicketDTO : BaseModel
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

        // Создает DTO из модели Ticket
        public static TicketDTO FromTicket(Ticket ticket)
        {
            return new TicketDTO
            {
                Id = ticket.Id,
                ScheduleId = ticket.ScheduleId,
                SeatId = ticket.SeatId,
                Status = ticket.Status,
                Price = ticket.Price,
                DiscountCategoryId = ticket.DiscountCategoryId,
                CustomerId = ticket.CustomerId,
                LoyaltyCardId = ticket.LoyaltyCardId,
                ReservedUntil = ticket.ReservedUntil,
                QrCode = ticket.QrCode,
                CreatedBy = ticket.CreatedBy,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt
            };
        }

        // Создает модель Ticket из DTO
        public Ticket ToTicket()
        {
            return new Ticket
            {
                Id = this.Id,
                ScheduleId = this.ScheduleId,
                SeatId = this.SeatId,
                Status = this.Status,
                Price = this.Price,
                DiscountCategoryId = this.DiscountCategoryId,
                CustomerId = this.CustomerId,
                LoyaltyCardId = this.LoyaltyCardId,
                ReservedUntil = this.ReservedUntil,
                QrCode = this.QrCode,
                CreatedBy = this.CreatedBy,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }
    }
} 