using Microsoft.Extensions.Configuration;
using Supabase;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SupabaseClient = Supabase.Client;
using AfishaUno.Models;

namespace AfishaUno.Services;

public class SupabaseService : ISupabaseService
{
    private readonly SupabaseClient _client;
    private User _currentUser;

    public User CurrentUser => _currentUser;

    public SupabaseService(IConfiguration configuration)
    {
        var url = configuration["Supabase:Url"];
        var key = configuration["Supabase:Key"];

        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Supabase URL и Key должны быть указаны в конфигурации");
        }

        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        };

        _client = new SupabaseClient(url, key, options);
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var session = await _client.Auth.SignIn(email, password);
            
            if (session?.User == null)
                return false;

            var users = await _client.From<User>()
                .Where(u => u.Email == email)
                .Get();

            if (users?.Models?.Count > 0)
            {
                _currentUser = users.Models[0];
                return true;
            }
            else
            {
                // Создаем пользователя, если он есть в Auth, но нет в таблице users
                var newUser = new User
                {
                    Email = email,
                    FullName = email.Split('@')[0],
                    Role = UserRoles.Cashier
                };

                var response = await _client.From<User>().Insert(newUser);
                
                if (response?.Models?.Count > 0)
                {
                    _currentUser = response.Models[0];
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка входа: {ex.Message}");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _client.Auth.SignOut();
        _currentUser = null;
    }

    // Пользователи
    public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            var response = await _client.From<User>().Get();
            return response.Models;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка получения пользователей: {ex.Message}");
            return new List<User>();
        }
    }

    public async Task<int> GetUserCountAsync()
    {
        try
        {
            var response = await _client.From<User>().Get();
            return response.Models.Count;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка получения количества пользователей: {ex.Message}");
            return -1;
        }
    }

    public async Task<bool> RegisterAdminAsync(string email, string password, string fullName)
    {
        try
        {
            // Проверяем, есть ли уже пользователи в системе
            int userCount = await GetUserCountAsync();
            
            if (userCount > 0)
                return false;

            // Создаём пользователя в Supabase Auth
            var authResponse = await _client.Auth.SignUp(email, password);
            
            if (authResponse?.User == null)
                return false;

            // Получаем UUID пользователя из auth
            string authUserId = authResponse.User.Id;
            Guid userId = Guid.Parse(authUserId);
            
            // Создаем пользователя с ролью админа
            var user = new User
            {
                Id = userId,
                Email = email,
                FullName = fullName,
                Role = UserRoles.Admin,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var response = await _client.From<User>().Insert(user);
            
            if (response?.Models?.Count > 0)
            {
                Trace.WriteLine($"Администратор успешно создан: {email}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка регистрации администратора: {ex.Message}");
            if (ex.InnerException != null)
            {
                Trace.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }
            return false;
        }
    }

    public async Task<User> CreateUserAsync(string email, string password, string fullName, string role)
    {
        try
        {
            // Проверяем права текущего пользователя
            if (_currentUser?.Role != UserRoles.Admin)
                return null;

            // Создаём пользователя в Supabase Auth
            var authResponse = await _client.Auth.SignUp(email, password);

            if (authResponse?.User == null)
                return null;

            // Получаем UUID пользователя из auth
            string authUserId = authResponse.User.Id;
            Guid userId = Guid.Parse(authUserId);
            
            var user = new User
            {
                Id = userId,
                Email = email,
                FullName = fullName,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var response = await _client.From<User>().Insert(user);
            
            if (response?.Models?.Count > 0)
            {
                Trace.WriteLine($"Пользователь успешно создан: {email}, роль: {role}");
                return response.Models.FirstOrDefault();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка создания пользователя: {ex.Message}");
            if (ex.InnerException != null)
            {
                Trace.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    // Спектакли
    public async Task<List<Performance>> GetPerformancesAsync()
    {
        try
        {
            var response = await _client.From<Performance>().Get();
            return response.Models;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка получения спектаклей: {ex.Message}");
            return new List<Performance>();
        }
    }

    public async Task<Performance> CreatePerformanceAsync(Performance performance)
    {
        try
        {
            var response = await _client.From<Performance>().Insert(performance);
            return response.Models.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка создания спектакля: {ex.Message}");
            return null;
        }
    }

    // Залы
    public async Task<List<Hall>> GetHallsAsync()
    {
        try
        {
            var response = await _client.From<Hall>().Get();
            return response.Models;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка получения залов: {ex.Message}");
            return new List<Hall>();
        }
    }

    // Места
    public async Task<List<Seat>> GetSeatsAsync(Guid hallId)
    {
        try
        {
            var response = await _client.From<Seat>()
                .Where(s => s.HallId == hallId)
                .Get();
            return response.Models;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка получения мест: {ex.Message}");
            return new List<Seat>();
        }
    }

    // Расписание
    public async Task<List<Schedule>> GetScheduleAsync()
    {
        try
        {
            var response = await _client.From<Schedule>().Get();

            foreach (var schedule in response.Models)
            {
                schedule.Performance = await _client
                    .From<Performance>()
                    .Where(p => p.Id == schedule.PerformanceId)
                    .Single();

                schedule.Hall = await _client
                    .From<Hall>()
                    .Where(h => h.Id == schedule.HallId)
                    .Single();
            }

            return response.Models;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка получения расписания: {ex.Message}");
            return new List<Schedule>();
        }
    }

    // Билеты
    public async Task<List<Ticket>> GetTicketsAsync(Guid scheduleId)
    {
        try
        {
            var response = await _client.From<Ticket>()
                .Where(t => t.ScheduleId == scheduleId)
                .Get();
            return response.Models;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка получения билетов: {ex.Message}");
            return new List<Ticket>();
        }
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        try
        {
            var response = await _client.From<Ticket>().Insert(ticket);
            
            if (response?.Models?.Count > 0)
            {
                Trace.WriteLine($"Билет успешно создан: {ticket.Id}");
                return response.Models.FirstOrDefault();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка создания билета: {ex.Message}");
            if (ex.InnerException != null)
            {
                Trace.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    public async Task<Ticket> UpdateTicketAsync(Ticket ticket)
    {
        try
        {
            var response = await _client.From<Ticket>()
                .Where(t => t.Id == ticket.Id)
                .Update(ticket);
                
            if (response?.Models?.Count > 0)
            {
                Trace.WriteLine($"Билет успешно обновлен: {ticket.Id}");
                return response.Models.FirstOrDefault();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка обновления билета: {ex.Message}");
            if (ex.InnerException != null)
            {
                Trace.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    // Категории скидок
    public async Task<List<DiscountCategory>> GetDiscountCategoriesAsync()
    {
        try
        {
            var response = await _client.From<DiscountCategory>().Get();
            return response.Models;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка получения категорий скидок: {ex.Message}");
            return new List<DiscountCategory>();
        }
    }

    // Сброс базы данных (только для отладки)
    public async Task<bool> ResetDatabaseAsync()
    {
        try
        {
            await _client.From<User>().Delete();
            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка сброса базы данных: {ex.Message}");
            return false;
        }
    }
}

