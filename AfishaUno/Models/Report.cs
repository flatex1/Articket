using System.Text.Json;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AfishaUno.Models
{
    [Table("reports")]
    public class Report : BaseModel
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("report_type")]
        public string ReportType { get; set; }

        [JsonPropertyName("parameters")]
        public JsonDocument Parameters { get; set; }

        [JsonPropertyName("data")]
        public JsonDocument Data { get; set; }

        [JsonPropertyName("created_by")]
        public Guid CreatedBy { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        // Навигационное свойство
        [JsonIgnore]
        public User CreatedByUser { get; set; }

        // Переопределяем свойство PrimaryKey
        [PrimaryKey("id")]
        public string PrimaryKeyId { get; set; }
    }
}
