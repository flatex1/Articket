using System;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AfishaUno.Models
{
    public static class LoyaltyCardStatuses
    {
        public const string Active = "active";
        public const string Inactive = "inactive";
        public const string Suspended = "suspended";
        public const string Expired = "expired";
    }

    public static class LoyaltyLevels
    {
        public const string Bronze = "bronze";
        public const string Silver = "silver";
        public const string Gold = "gold";
        public const string Platinum = "platinum";
    }

    [Table("loyalty_cards")]
    public class LoyaltyCard : BaseModel
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

        // Навигационные свойства
        [JsonIgnore]
        public Customer Customer { get; set; }

        [JsonIgnore]
        public DiscountCategory DiscountCategory { get; set; }

        public LoyaltyCard()
        {
            Status = LoyaltyCardStatuses.Active;
            Level = LoyaltyLevels.Bronze;
            Points = 0;
            IssueDate = DateTime.UtcNow;
        }

        public override bool Equals(object obj)
        {
            return obj is LoyaltyCard card &&
                   Id.Equals(card.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
} 