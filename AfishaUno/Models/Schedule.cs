using System;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AfishaUno.Models
{
    [Table("schedule")]
    public class Schedule : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("performance_id")]
        public Guid PerformanceId { get; set; }

        [Column("hall_id")]
        public Guid HallId { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("base_price")]
        public decimal BasePrice { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Навигационные свойства для соединения с другими объектами
        // Добавляем атрибут [JsonIgnore], чтобы исключить их из сериализации
        [JsonIgnore]
        public Performance Performance { get; set; }

        [JsonIgnore]
        public Hall Hall { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Schedule schedule &&
                   Id.Equals(schedule.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
