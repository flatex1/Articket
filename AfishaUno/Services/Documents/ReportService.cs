using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AfishaUno.Models;
using AfishaUno.Services.Documents;
using QuestPDF.Fluent;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AfishaUno.Services
{
    /// <summary>
    /// Сервис для генерации различных типов отчетов
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly ISupabaseService _supabaseService;
        private readonly ILogger<ReportService> _logger;

        public ReportService(ISupabaseService supabaseService, ILogger<ReportService> logger)
        {
            _supabaseService = supabaseService ?? throw new ArgumentNullException(nameof(supabaseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Активируем QuestPDF (бесплатная лицензия для open-source проектов)
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        /// <summary>
        /// Генерация отчета о продажах билетов
        /// </summary>
        /// <param name="startDate">Начальная дата периода</param>
        /// <param name="endDate">Конечная дата периода</param>
        /// <param name="generatedBy">Пользователь, формирующий отчет</param>
        /// <returns>Массив байтов с PDF-документом</returns>
        public async Task<byte[]> GenerateSalesReportAsync(DateTime startDate, DateTime endDate, User generatedBy)
        {
            // Получение данных о билетах за указанный период
            var tickets = await _supabaseService.GetTicketsByDateRangeAsync(startDate, endDate);
            
            if (tickets == null || !tickets.Any())
            {
                throw new InvalidOperationException($"Нет данных о билетах за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}");
            }

            // Создание и настройка документа отчета
            var reportDocument = new SalesReportDocument(tickets, startDate, endDate, generatedBy);
            
            // Преобразование документа в массив байтов
            return reportDocument.GeneratePdf();
        }

        /// <summary>
        /// Генерация отчета о посещаемости спектаклей
        /// </summary>
        /// <param name="startDate">Начальная дата периода</param>
        /// <param name="endDate">Конечная дата периода</param>
        /// <param name="generatedBy">Пользователь, формирующий отчет</param>
        /// <returns>Массив байтов с PDF-документом</returns>
        public async Task<byte[]> GeneratePerformanceReportAsync(DateTime startDate, DateTime endDate, User generatedBy)
        {
            // Получение данных о расписании за указанный период
            var schedules = await _supabaseService.GetScheduleByDateRangeAsync(startDate, endDate);
            
            if (schedules == null || !schedules.Any())
            {
                throw new InvalidOperationException($"Нет данных о сеансах за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}");
            }

            // Получение данных о билетах для этих сеансов
            var scheduleIds = schedules.Select(s => s.Id.ToString()).ToList();
            var tickets = await _supabaseService.GetTicketsByScheduleIdsAsync(scheduleIds);

            // Создание и настройка документа отчета
            var reportDocument = new PerformanceReportDocument(schedules, tickets, startDate, endDate, generatedBy);
            
            // Преобразование документа в массив байтов
            return reportDocument.GeneratePdf();
        }

        /// <summary>
        /// Сохранение отчета в базу данных
        /// </summary>
        /// <param name="title">Заголовок отчета</param>
        /// <param name="reportType">Тип отчета</param>
        /// <param name="parameters">Параметры отчета в JSON формате</param>
        /// <param name="data">Данные отчета в JSON формате</param>
        /// <param name="userId">Идентификатор пользователя, создавшего отчет</param>
        /// <returns>Идентификатор созданного отчета</returns>
        public async Task<string> SaveReportAsync(string title, string reportType, string parameters, string data, string userId)
        {
            var report = new Report
            {
                Title = title,
                ReportType = reportType,
                Parameters = parameters,
                Data = data,
                CreatedBy = userId,
                CreatedAt = DateTime.Now
            };

            var createdReport = await _supabaseService.CreateReportAsync(report);
            return createdReport.Id;
        }

        /// <summary>
        /// Получение списка сохраненных отчетов
        /// </summary>
        /// <returns>Список отчетов</returns>
        public async Task<List<Report>> GetSavedReportsAsync()
        {
            return await _supabaseService.GetReportsAsync();
        }

        /// <summary>
        /// Генерация отчета по сохраненным данным
        /// </summary>
        /// <param name="reportId">Идентификатор сохраненного отчета</param>
        /// <param name="currentUser">Текущий пользователь</param>
        /// <returns>Массив байтов с PDF-документом</returns>
        public async Task<byte[]> GenerateSavedReportAsync(string reportId, User currentUser)
        {
            var report = await _supabaseService.GetReportByIdAsync(reportId);
            
            if (report == null)
            {
                throw new InvalidOperationException($"Отчет с ID {reportId} не найден");
            }

            // В зависимости от типа отчета, формируем соответствующий документ
            switch (report.ReportType)
            {
                case "sales":
                    // Восстанавливаем параметры из JSON
                    // Предполагается, что параметры содержат start_date и end_date
                    var salesParameters = System.Text.Json.JsonDocument.Parse(report.Parameters);
                    var salesStartDate = DateTime.Parse(salesParameters.RootElement.GetProperty("start_date").GetString());
                    var salesEndDate = DateTime.Parse(salesParameters.RootElement.GetProperty("end_date").GetString());

                    // Получаем данные билетов из базы заново, чтобы иметь актуальную информацию
                    var tickets = await _supabaseService.GetTicketsByDateRangeAsync(salesStartDate, salesEndDate);
                    
                    // Создаем документ отчета
                    var salesReport = new SalesReportDocument(tickets, salesStartDate, salesEndDate, currentUser, report.Title);
                    return salesReport.GeneratePdf();

                case "attendance":
                    // Параметры для отчета о посещаемости
                    var attendanceParameters = System.Text.Json.JsonDocument.Parse(report.Parameters);
                    var attendanceStartDate = DateTime.Parse(attendanceParameters.RootElement.GetProperty("start_date").GetString());
                    var attendanceEndDate = DateTime.Parse(attendanceParameters.RootElement.GetProperty("end_date").GetString());

                    // Получаем данные о расписании и билетах
                    var schedules = await _supabaseService.GetScheduleByDateRangeAsync(attendanceStartDate, attendanceEndDate);
                    var scheduleIds = schedules.Select(s => s.Id.ToString()).ToList();
                    var attendanceTickets = await _supabaseService.GetTicketsByScheduleIdsAsync(scheduleIds);
                    
                    // Создаем документ отчета
                    var attendanceReport = new PerformanceReportDocument(schedules, attendanceTickets, attendanceStartDate, attendanceEndDate, currentUser, report.Title);
                    return attendanceReport.GeneratePdf();

                // Здесь можно добавить обработку других типов отчетов

                default:
                    throw new NotSupportedException($"Тип отчета '{report.ReportType}' не поддерживается");
            }
        }

        public async Task<Report> GenerateSalesReportAsync(DateTime startDate, DateTime endDate, List<Guid> performanceIds, User generatedBy)
        {
            try
            {
                _logger.LogInformation($"Создание отчета по продажам с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}");

                var parameters = new Dictionary<string, object>
                {
                    { "startDate", startDate.ToString("yyyy-MM-dd") },
                    { "endDate", endDate.ToString("yyyy-MM-dd") },
                    { "performanceIds", performanceIds.Select(id => id.ToString()).ToList() }
                };

                // Получаем билеты за указанный период
                var tickets = await _supabaseService.GetTicketsForPeriodAsync(startDate, endDate, performanceIds);

                // Создаем отчет
                var document = new SalesReportDocument(tickets, startDate, endDate, generatedBy);
                var pdfBytes = document.GeneratePdf();

                // Сохраняем отчет
                var report = new Report
                {
                    Title = $"Отчет по продажам с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}",
                    ReportType = "sales",
                    Parameters = JsonSerializer.Serialize(parameters),
                    Data = Convert.ToBase64String(pdfBytes),
                    CreatedBy = generatedBy.Id.ToString(),
                    CreatedAt = DateTime.Now,
                    CreatedByUser = generatedBy
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании отчета по продажам");
                throw;
            }
        }

        public async Task<Report> GeneratePerformanceReportAsync(DateTime startDate, DateTime endDate, List<Guid> performanceIds, User generatedBy)
        {
            try
            {
                _logger.LogInformation($"Создание отчета по спектаклям с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}");

                var parameters = new Dictionary<string, object>
                {
                    { "startDate", startDate.ToString("yyyy-MM-dd") },
                    { "endDate", endDate.ToString("yyyy-MM-dd") },
                    { "performanceIds", performanceIds.Select(id => id.ToString()).ToList() }
                };

                // Получаем билеты за указанный период
                var tickets = await _supabaseService.GetTicketsForPeriodAsync(startDate, endDate, performanceIds);
                
                // Получаем расписания для отчета
                var schedules = await _supabaseService.GetScheduleByDateRangeAsync(startDate, endDate);
                if (performanceIds != null && performanceIds.Any())
                {
                    schedules = schedules.Where(s => performanceIds.Contains(s.PerformanceId)).ToList();
                }

                // Создаем отчет
                var document = new PerformanceReportDocument(schedules, tickets, startDate, endDate, generatedBy, null);
                var pdfBytes = document.GeneratePdf();

                // Сохраняем отчет
                var report = new Report
                {
                    Title = $"Отчет по спектаклям с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}",
                    ReportType = "performance",
                    Parameters = JsonSerializer.Serialize(parameters),
                    Data = Convert.ToBase64String(pdfBytes),
                    CreatedBy = generatedBy.Id.ToString(),
                    CreatedAt = DateTime.Now,
                    CreatedByUser = generatedBy
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании отчета по спектаклям");
                throw;
            }
        }
    }

    /// <summary>
    /// Интерфейс сервиса для генерации отчетов
    /// </summary>
    public interface IReportService
    {
        Task<byte[]> GenerateSalesReportAsync(DateTime startDate, DateTime endDate, User generatedBy);
        Task<byte[]> GeneratePerformanceReportAsync(DateTime startDate, DateTime endDate, User generatedBy);
        Task<string> SaveReportAsync(string title, string reportType, string parameters, string data, string userId);
        Task<List<Report>> GetSavedReportsAsync();
        Task<byte[]> GenerateSavedReportAsync(string reportId, User currentUser);
        Task<Report> GenerateSalesReportAsync(DateTime startDate, DateTime endDate, List<Guid> performanceIds, User generatedBy);
        Task<Report> GeneratePerformanceReportAsync(DateTime startDate, DateTime endDate, List<Guid> performanceIds, User generatedBy);
    }
} 