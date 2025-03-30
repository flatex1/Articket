using System;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace AfishaUno.Models
{
    [Table("discount_categories")]
    public class DiscountCategory : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("discount_percent")]
        public int DiscountPercent { get; set; }

        [Column("requires_verification")]
        public bool RequiresVerification { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DiscountCategory category &&
                   Id.Equals(category.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
} 
