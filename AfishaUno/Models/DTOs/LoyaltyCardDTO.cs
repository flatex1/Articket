using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AfishaUno.Models.DTOs
{
    [Table("loyalty_cards")]
    public class LoyaltyCardDTO : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("customer_id")]
        public Guid CustomerId { get; set; }

        [Column("card_number")]
        public string CardNumber { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("discount_category_id")]
        public Guid? DiscountCategoryId { get; set; }

        [Column("points")]
        public int Points { get; set; }

        [Column("level")]
        public string Level { get; set; }

        [Column("issue_date")]
        public DateTime IssueDate { get; set; }

        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Создает DTO из модели LoyaltyCard
        public static LoyaltyCardDTO FromLoyaltyCard(LoyaltyCard loyaltyCard)
        {
            return new LoyaltyCardDTO
            {
                Id = loyaltyCard.Id,
                CustomerId = loyaltyCard.CustomerId,
                CardNumber = loyaltyCard.CardNumber,
                Status = loyaltyCard.Status,
                DiscountCategoryId = loyaltyCard.DiscountCategoryId,
                Points = loyaltyCard.Points,
                Level = loyaltyCard.Level,
                IssueDate = loyaltyCard.IssueDate,
                ExpiryDate = loyaltyCard.ExpiryDate,
                CreatedAt = loyaltyCard.CreatedAt,
                UpdatedAt = loyaltyCard.UpdatedAt
            };
        }

        // Создает модель LoyaltyCard из DTO
        public LoyaltyCard ToLoyaltyCard()
        {
            return new LoyaltyCard
            {
                Id = this.Id,
                CustomerId = this.CustomerId,
                CardNumber = this.CardNumber,
                Status = this.Status,
                DiscountCategoryId = this.DiscountCategoryId,
                Points = this.Points,
                Level = this.Level,
                IssueDate = this.IssueDate,
                ExpiryDate = this.ExpiryDate,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }
    }
} 