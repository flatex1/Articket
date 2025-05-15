using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AfishaUno.Models
{
    [Table("discount_categories")]
    public class DiscountCategory : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("discount_percent")]
        public int DiscountPercent { get; set; }

        [Column("requires_verification")]
        public bool RequiresVerification { get; set; }

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public override bool Equals(object? obj)
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
