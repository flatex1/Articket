using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AfishaUno.Models;
using AfishaUno.Services.Documents;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using Windows.System;

namespace AfishaUno.Services
{
    /// <summary>
    /// Сервис для печати билетов
    /// </summary>
    public class TicketPrintService : ITicketPrintService
    {
        private readonly ILogger<TicketPrintService> _logger;

        public TicketPrintService(ILogger<TicketPrintService> logger = null)
        {
            _logger = logger;

            // Активируем QuestPDF (бесплатная лицензия для open-source проектов)
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        /// <summary>
        /// Генерирует PDF-билет и открывает его в браузере
        /// </summary>
        /// <param name="ticket">Билет</param>
        /// <param name="schedule">Расписание (сеанс)</param>
        /// <param name="seat">Место</param>
        /// <param name="customer">Клиент (может быть null для анонимного билета)</param>
        /// <returns>Путь к созданному PDF-файлу</returns>
        public async Task<string> GenerateAndOpenTicketAsync(Ticket ticket, Schedule schedule, Seat seat, Customer customer = null)
        {
            try
            {
                _logger?.LogInformation($"Генерация и открытие PDF-билета для билета ID: {ticket.Id}");

                // Генерируем PDF-файл
                string filePath = await GenerateTicketPdfAsync(ticket, schedule, seat, customer);
                
                // Открываем файл в браузере (асинхронно)
                var uri = new Uri(filePath);
                await Launcher.LaunchUriAsync(uri);

                _logger?.LogInformation($"PDF-билет успешно открыт: {filePath}");
                
                return filePath;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Ошибка при генерации и открытии PDF-билета: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger?.LogError($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        /// <summary>
        /// Генерирует PDF-билет и возвращает путь к файлу
        /// </summary>
        /// <param name="ticket">Билет</param>
        /// <param name="schedule">Расписание (сеанс)</param>
        /// <param name="seat">Место</param>
        /// <param name="customer">Клиент (может быть null для анонимного билета)</param>
        /// <returns>Путь к созданному PDF-файлу</returns>
        public Task<string> GenerateTicketPdfAsync(Ticket ticket, Schedule schedule, Seat seat, Customer customer = null)
        {
            try
            {
                _logger?.LogInformation($"Генерация PDF-билета для билета ID: {ticket.Id}");
                
                // Создаем документ билета
                var document = new TicketDocument(ticket, schedule, seat, customer);
                
                // Создаем директорию для билетов, если она не существует
                string ticketsDir = Path.Combine(Path.GetTempPath(), "Articket", "Tickets");
                Directory.CreateDirectory(ticketsDir);
                
                // Создаем уникальное имя файла
                string fileName = $"Ticket_{ticket.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string filePath = Path.Combine(ticketsDir, fileName);
                
                // Генерируем PDF и сохраняем в файл
                document.GeneratePdf(filePath);
                
                _logger?.LogInformation($"PDF-билет успешно создан: {filePath}");
                
                return Task.FromResult(filePath);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Ошибка при генерации PDF-билета: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger?.LogError($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
} 