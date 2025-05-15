using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace AfishaUno.Models.DTOs
{
    [Table("customers")]
    public class CustomerDTO : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [Column("full_name")]
        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Column("phone")]
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [Column("email")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Column("birth_date")]
        [JsonPropertyName("birth_date")]
        public DateTime? BirthDate { get; set; }

        [Column("document_type")]
        [JsonPropertyName("document_type")]
        public string DocumentType { get; set; } = string.Empty;

        [Column("document_number")]
        [JsonPropertyName("document_number")]
        public string DocumentNumber { get; set; } = string.Empty;

        [Column("verification_status")]
        [JsonPropertyName("verification_status")]
        public bool VerificationStatus { get; set; }

        [Column("notes")]
        [JsonPropertyName("notes")]
        public string Notes { get; set; } = string.Empty;

        [Column("created_at")]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Преобразование из модели Customer в DTO
        public static CustomerDTO FromCustomer(Customer customer)
        {
            var dto = new CustomerDTO
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Phone = customer.Phone,
                Email = customer.Email ?? string.Empty,
                BirthDate = customer.BirthDate,
                DocumentType = customer.DocumentType ?? string.Empty,
                DocumentNumber = customer.DocumentNumber ?? string.Empty,
                VerificationStatus = customer.VerificationStatus,
                Notes = customer.Notes ?? string.Empty,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };

            // Добавляем отладочную информацию
            Trace.WriteLine($"DTO DocumentType: '{dto.DocumentType}'");
            
            return dto;
        }

        // Преобразование из DTO в модель Customer
        public Customer ToCustomer()
        {
            var customer = new Customer
            {
                Id = this.Id,
                FullName = this.FullName,
                Phone = this.Phone,
                Email = this.Email,
                BirthDate = this.BirthDate,
                DocumentType = this.DocumentType,
                DocumentNumber = this.DocumentNumber,
                VerificationStatus = this.VerificationStatus,
                Notes = this.Notes,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };

            // Добавляем отладочную информацию
            Trace.WriteLine($"Customer DocumentType: '{customer.DocumentType}'");
            
            return customer;
        }
    }
} 