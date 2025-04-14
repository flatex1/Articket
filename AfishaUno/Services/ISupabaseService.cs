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
        Task<User> CreateUserAsync(string email, string password, string fullName, string role);
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
        Task<Schedule> CreateScheduleAsync(Schedule schedule);
        Task<bool> CreateSeatsAsync(List<Seat> seats);
    }
} 
