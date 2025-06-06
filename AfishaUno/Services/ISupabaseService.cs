using AfishaUno.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AfishaUno.Services
{
    public interface ISupabaseService
    {
        User CurrentUser { get; }
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<List<User>> GetUsersAsync();
        Task<bool> RegisterAdminAsync(string email, string password, string fullName);
        Task<User> CreateUserAsync(string email, string? password, string fullName, string role);
        Task<List<Performance>> GetPerformancesAsync();
        Task<Performance> CreatePerformanceAsync(Performance performance);
        Task<List<Hall>> GetHallsAsync();
        Task<List<Seat>> GetSeatsAsync(Guid hallId);
        Task<List<Schedule>> GetScheduleAsync();
        Task<List<Ticket>> GetTicketsAsync(Guid scheduleId);
        Task<Ticket> CreateTicketAsync(Ticket ticket);
        Task<Ticket> UpdateTicketAsync(Ticket ticket);
        Task<List<DiscountCategory>> GetDiscountCategoriesAsync();
        Task<int> GetUserCountAsync();
        Task<bool> ResetDatabaseAsync();
        Task<Schedule> GetScheduleDetailsAsync(Guid scheduleId);
        Task<List<Seat>> GetAvailableSeatsAsync(Guid scheduleId);
        Task<Ticket> SellTicketAsync(Ticket ticket);
        Task<Hall> CreateHallAsync(Hall hall);
        Task<Schedule> CreateScheduleAsync(Guid performanceId, Guid hallId, DateTime startTime, decimal basePrice);
        Task<bool> CreateSeatsAsync(List<Seat> seats);
        Task<List<Schedule>> GetScheduleWithDetailsAsync();
        Task<Customer> GetCustomerByIdAsync(Guid customerId);
        Task<Customer> GetCustomerByPhoneAsync(string phone);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<List<LoyaltyCard>> GetLoyaltyCardsAsync(Guid customerId);
        Task<LoyaltyCard> CreateLoyaltyCardAsync(LoyaltyCard loyaltyCard);
        Task<bool> AddPointsToLoyaltyCardAsync(Guid cardId, int points);
        Task<List<Customer>> GetCustomersAsync();
        Task<List<Customer>> SearchCustomersAsync(string searchTerm);
        Task<Report> CreateReportAsync(Report report);
        Task<List<Report>> GetReportsAsync();
        Task<Report> GetReportByIdAsync(string reportId);
        Task DeleteReportAsync(string reportId);
        Task<List<Ticket>> GetTicketsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Schedule>> GetScheduleByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Ticket>> GetTicketsByScheduleIdsAsync(List<string> scheduleIds);
        
        /// <summary>
        /// Получает все билеты за указанный период времени
        /// </summary>
        /// <param name="startDate">Начальная дата периода</param>
        /// <param name="endDate">Конечная дата периода</param>
        /// <param name="performanceIds">Опциональный список идентификаторов спектаклей для фильтрации</param>
        /// <returns>Список билетов за указанный период</returns>
        Task<List<Ticket>> GetTicketsForPeriodAsync(DateTime startDate, DateTime endDate, List<Guid>? performanceIds = null);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid userId);
        Task<User> AddUserAsync(string email, string password, string fullName, string role);
        Task DeleteTicketAsync(Guid ticketId);
    }
} 
