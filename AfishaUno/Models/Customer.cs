using System;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Collections.Generic;

namespace AfishaUno.Models
{
    [Table("customers")]
    public class Customer : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Column("phone")]
        public string Phone { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("birth_date")]
        public DateTime? BirthDate { get; set; }

        [Column("document_type")]
        public string DocumentType { get; set; } = string.Empty;

        [Column("document_number")]
        public string DocumentNumber { get; set; } = string.Empty;

        [Column("verification_status")]
        public bool VerificationStatus { get; set; }

        [Column("notes")]
        public string Notes { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Навигационные свойства (не включаются в операции с базой данных)
        [JsonIgnore]
        public List<LoyaltyCard> LoyaltyCards { get; set; }

        public Customer()
        {
            LoyaltyCards = new List<LoyaltyCard>();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Customer customer)
                return false;
            
            if (Id == Guid.Empty && customer.Id == Guid.Empty)
                return ReferenceEquals(this, customer);
            
            return Id.Equals(customer.Id);
        }

        public override int GetHashCode()
        {
            return Id == Guid.Empty ? base.GetHashCode() : HashCode.Combine(Id);
        }
    }
} 