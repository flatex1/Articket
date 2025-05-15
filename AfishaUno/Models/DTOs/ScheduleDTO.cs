using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AfishaUno.Models.DTOs
{
    [Table("schedule")]
    public class ScheduleDTO : BaseModel
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

        // Создает DTO из модели Schedule
        public static ScheduleDTO FromSchedule(Schedule schedule)
        {
            return new ScheduleDTO
            {
                Id = schedule.Id,
                PerformanceId = schedule.PerformanceId,
                HallId = schedule.HallId,
                StartTime = schedule.StartTime,
                BasePrice = schedule.BasePrice,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt
            };
        }

        // Создает модель Schedule из DTO
        public Schedule ToSchedule()
        {
            return new Schedule
            {
                Id = this.Id,
                PerformanceId = this.PerformanceId,
                HallId = this.HallId,
                StartTime = this.StartTime,
                BasePrice = this.BasePrice,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }
    }
} 