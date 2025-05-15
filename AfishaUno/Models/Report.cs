using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AfishaUno.Models
{
    [Table("reports")]
    public class Report : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [Column("title")]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [Column("report_type")]
        [JsonPropertyName("report_type")]
        public string ReportType { get; set; }

        [Column("parameters")]
        [JsonPropertyName("parameters")]
        public string Parameters { get; set; }

        [Column("data")]
        [JsonPropertyName("data")]
        public string Data { get; set; }

        [Column("created_by")]
        [JsonPropertyName("created_by")]
        public string CreatedBy { get; set; }

        [Column("created_at")]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        // Навигационное свойство
        [JsonIgnore]
        public User CreatedByUser { get; set; }

        // Вспомогательные методы для работы с JSON
        public T GetParametersAs<T>() where T : class
        {
            if (string.IsNullOrEmpty(Parameters))
                return null;
                
            return JsonSerializer.Deserialize<T>(Parameters);
        }

        public T GetDataAs<T>() where T : class
        {
            if (string.IsNullOrEmpty(Data))
                return null;
                
            return JsonSerializer.Deserialize<T>(Data);
        }

        public void SetParameters<T>(T value) where T : class
        {
            if (value == null)
                Parameters = null;
            else
                Parameters = JsonSerializer.Serialize(value);
        }

        public void SetData<T>(T value) where T : class
        {
            if (value == null)
                Data = null;
            else
                Data = JsonSerializer.Serialize(value);
        }

        // Вспомогательные методы для отображения
        [JsonIgnore]
        public string DisplayReportType 
        { 
            get 
            {
                return ReportType switch
                {
                    "sales" => "Отчет о продажах",
                    "attendance" => "Отчет о посещаемости",
                    _ => ReportType
                };
            }
        }

        [JsonIgnore]
        public string DisplayCreatedAt
        {
            get { return CreatedAt.ToString("dd.MM.yyyy HH:mm"); }
        }
    }
}
