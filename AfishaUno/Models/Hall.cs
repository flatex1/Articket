using System;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace AfishaUno.Models
{
    [Table("halls")]
    public class Hall : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("capacity")]
        public int Capacity { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Hall hall &&
                   Id.Equals(hall.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
} 
