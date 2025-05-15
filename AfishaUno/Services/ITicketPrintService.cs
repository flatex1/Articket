using System;
using System.Threading.Tasks;
using AfishaUno.Models;

namespace AfishaUno.Services
{
    /// <summary>
    /// Интерфейс сервиса для печати билетов
    /// </summary>
    public interface ITicketPrintService
    {
        /// <summary>
        /// Генерирует PDF-билет и открывает его в браузере
        /// </summary>
        /// <param name="ticket">Билет</param>
        /// <param name="schedule">Расписание (сеанс)</param>
        /// <param name="seat">Место</param>
        /// <param name="customer">Клиент (может быть null для анонимного билета)</param>
        /// <returns>Путь к созданному PDF-файлу</returns>
        Task<string> GenerateAndOpenTicketAsync(Ticket ticket, Schedule schedule, Seat seat, Customer customer = null);
        
        /// <summary>
        /// Генерирует PDF-билет и возвращает путь к файлу
        /// </summary>
        /// <param name="ticket">Билет</param>
        /// <param name="schedule">Расписание (сеанс)</param>
        /// <param name="seat">Место</param>
        /// <param name="customer">Клиент (может быть null для анонимного билета)</param>
        /// <returns>Путь к созданному PDF-файлу</returns>
        Task<string> GenerateTicketPdfAsync(Ticket ticket, Schedule schedule, Seat seat, Customer customer = null);
    }
} 