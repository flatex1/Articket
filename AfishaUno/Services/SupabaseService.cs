using Microsoft.Extensions.Configuration;
using Supabase;
using System.Diagnostics;
using SupabaseClient = Supabase.Client;
using Microsoft.Extensions.Logging;
using AfishaUno.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AfishaUno.Models;
using Supabase.Postgrest;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net.Http;

namespace AfishaUno.Services;

public class SupabaseService : ISupabaseService
{
    private readonly SupabaseClient _client;
    private AfishaUno.Models.User _currentUser;
    private readonly ILogger<SupabaseService> _logger;

    public AfishaUno.Models.User CurrentUser => _currentUser;

    public SupabaseService(IConfiguration configuration, ILogger<SupabaseService> logger)
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
        _logger = logger;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var session = await _client.Auth.SignIn(email, password);

            if (session?.User == null)
                return false;

            var users = await _client.From<AfishaUno.Models.User>()
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
                var newUser = new AfishaUno.Models.User
                {
                    Email = email,
                    FullName = email.Split('@')[0],
                    Role = UserRoles.Cashier
                };

                var response = await _client.From<AfishaUno.Models.User>().Insert(newUser);

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
    public async Task<List<AfishaUno.Models.User>> GetUsersAsync()
    {
        try
        {
            var response = await _client.From<AfishaUno.Models.User>().Get();
            return response.Models;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка получения пользователей: {ex.Message}");
            return new List<AfishaUno.Models.User>();
        }
    }

    public async Task<int> GetUserCountAsync()
    {
        try
        {
            var response = await _client.From<AfishaUno.Models.User>().Get();
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
            var user = new AfishaUno.Models.User
            {
                Id = userId,
                Email = email,
                FullName = fullName,
                Role = UserRoles.Admin,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var response = await _client.From<AfishaUno.Models.User>().Insert(user);

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

    public async Task<AfishaUno.Models.User> CreateUserAsync(string email, string password, string fullName, string role)
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
            
            var user = new AfishaUno.Models.User
            {
                Id = userId,
                Email = email,
                FullName = fullName,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var response = await _client.From<AfishaUno.Models.User>().Insert(user);
                
                if (response?.Models?.Count > 0)
                {
                Trace.WriteLine($"Пользователь успешно создан: {email}");
                    return response.Models[0];
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
            Trace.WriteLine($"[CreatePerformanceAsync] Начало создания спектакля с Id={performance.Id}, Title='{performance.Title}'");

            // Проверяем права пользователя
            if (_currentUser == null)
            {
                Trace.WriteLine("[CreatePerformanceAsync] Ошибка: Пользователь не авторизован");
                return null;
            }

            // Проверяем корректность данных
            if (performance == null || string.IsNullOrEmpty(performance.Title))
            {
                Trace.WriteLine("[CreatePerformanceAsync] Ошибка: Некорректные данные спектакля");
                return null;
            }

            // Логируем детали запроса
            Trace.WriteLine($"[CreatePerformanceAsync] Отправка запроса в Supabase: {performance.Id}, '{performance.Title}', Duration={performance.Duration}");

            var response = await _client.From<Performance>().Insert(performance);

            if (response == null)
            {
                Trace.WriteLine("[CreatePerformanceAsync] Ошибка: Ответ от API равен null");
                return null;
            }

            Trace.WriteLine($"[CreatePerformanceAsync] Получен ответ от API: Count={response.Models?.Count ?? 0}");

            if (response.Models?.Count > 0)
            {
                var createdPerformance = response.Models.FirstOrDefault();
                Trace.WriteLine($"[CreatePerformanceAsync] Спектакль успешно создан: Id={createdPerformance.Id}, Title='{createdPerformance.Title}'");
                return createdPerformance;
            }

            Trace.WriteLine("[CreatePerformanceAsync] Ошибка: В ответе отсутствуют модели");
            return null;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[CreatePerformanceAsync] Исключение: {ex.Message}");
            Trace.WriteLine($"[CreatePerformanceAsync] StackTrace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Trace.WriteLine($"[CreatePerformanceAsync] InnerException: {ex.InnerException.Message}");
                Trace.WriteLine($"[CreatePerformanceAsync] InnerException StackTrace: {ex.InnerException.StackTrace}");
            }

            // Логируем детали объекта performance
            if (performance != null)
            {
                Trace.WriteLine($"[CreatePerformanceAsync] Детали объекта Performance:");
                Trace.WriteLine($"  - Id: {performance.Id}");
                Trace.WriteLine($"  - Title: '{performance.Title}'");
                Trace.WriteLine($"  - Description: '{(performance.Description?.Length > 50 ? performance.Description.Substring(0, 50) + "..." : performance.Description)}'");
                Trace.WriteLine($"  - Duration: {performance.Duration}");
                Trace.WriteLine($"  - PosterUrl: '{performance.PosterUrl}'");
                Trace.WriteLine($"  - CreatedAt: {performance.CreatedAt}");
                Trace.WriteLine($"  - UpdatedAt: {performance.UpdatedAt}");
            }

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
            _logger.LogInformation($"Fetching tickets for schedule {scheduleId}");
            
            var response = await _client.From<TicketDTO>()
                .Where(t => t.ScheduleId == scheduleId)
                .Get();
            
            if (response?.Models == null)
            {
                _logger.LogWarning("No tickets found or error occurred");
                return new List<Ticket>();
            }
            
            // Преобразуем список DTO в список моделей
            var tickets = response.Models.Select(dto => dto.ToTicket()).ToList();
            _logger.LogInformation($"Retrieved {tickets.Count} tickets for schedule {scheduleId}");
            
            return tickets;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving tickets: {ex.Message}");
            return new List<Ticket>();
        }
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        try
        {
            _logger.LogInformation($"Attempting to create ticket for schedule {ticket.ScheduleId}, seat {ticket.SeatId}");
            
            // Создаем DTO для работы с базой данных
            var ticketDto = TicketDTO.FromTicket(ticket);
            
            var response = await _client.From<TicketDTO>().Insert(ticketDto);

            if (response?.Models?.Count > 0)
            {
                _logger.LogInformation($"Ticket successfully created: {ticketDto.Id}");
                return response.Models.First().ToTicket();
            }

            _logger.LogError("Failed to create ticket: No response from database");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating ticket: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    public async Task<Ticket> UpdateTicketAsync(Ticket ticket)
    {
        try
        {
            _logger.LogInformation($"Attempting to update ticket {ticket.Id}");
            
            // Создаем DTO для работы с базой данных
            var ticketDto = TicketDTO.FromTicket(ticket);
            
            var response = await _client.From<TicketDTO>()
                .Where(t => t.Id == ticketDto.Id)
                .Update(ticketDto);

            if (response?.Models?.Count > 0)
            {
                _logger.LogInformation($"Ticket successfully updated: {ticketDto.Id}");
                return response.Models.First().ToTicket();
            }

            _logger.LogError($"Failed to update ticket {ticket.Id}: No response from database");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating ticket: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    // Категории скидок
    public async Task<List<DiscountCategory>> GetDiscountCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Загрузка категорий скидок из базы данных");
            
            // Пытаемся получить категории из базы данных
            try 
            {
                _logger.LogInformation("Попытка получить категории скидок из базы данных");
                var response = await _client.From<DiscountCategory>().Get();
                
                if (response?.Models != null && response.Models.Count > 0)
                {
                    _logger.LogInformation($"Успешно загружено {response.Models.Count} категорий скидок из базы данных");
                    
                    // Выведем все загруженные категории для отладки
                    foreach (var category in response.Models)
                    {
                        _logger.LogInformation($"Категория из БД: ID={category.Id}, Название='{category.Name}', Скидка={category.DiscountPercent}%, Требует проверки={category.RequiresVerification}");
                    }
                    
                    // Добавляем категорию "Без скидки", если её нет в списке
                    bool hasNoDiscountCategory = response.Models.Any(c => c.Name == "Без скидки" || c.DiscountPercent == 0);
                    
                    if (!hasNoDiscountCategory)
                    {
                        var noDiscountCategory = new DiscountCategory
                        {
                            Id = Guid.NewGuid(),
                            Name = "Без скидки",
                            DiscountPercent = 0,
                            RequiresVerification = false,
                            Description = "Стандартный билет без скидки",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        
                        _logger.LogInformation("Добавляем категорию 'Без скидки', так как она отсутствует в базе данных");
                        var insertResponse = await _client.From<DiscountCategory>().Insert(noDiscountCategory);
                        
                        if (insertResponse?.Models?.Count > 0)
                        {
                            _logger.LogInformation($"Категория 'Без скидки' успешно добавлена с ID={insertResponse.Models[0].Id}");
                            response.Models.Add(insertResponse.Models[0]);
                        }
                    }
                    
                    return response.Models;
                }
                else
                {
                    _logger.LogWarning("В базе данных нет категорий скидок. Создаем стандартные категории.");
                    return await CreateDefaultDiscountCategoriesAsync();
                }
        }
        catch (Exception ex)
        {
                _logger.LogWarning($"Ошибка при получении категорий скидок из базы данных: {ex.Message}");
                _logger.LogWarning("Используем стандартные категории скидок");
                return await CreateDefaultDiscountCategoriesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении категорий скидок");
            return new List<DiscountCategory>
            {
                new DiscountCategory
                {
                    Id = Guid.Empty,
                    Name = "Без скидки (резервная)",
                    DiscountPercent = 0,
                    RequiresVerification = false,
                    Description = "Стандартный билет без скидки (резервная категория)",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }
    }
    
    // Создание стандартных категорий скидок в базе данных
    private async Task<List<DiscountCategory>> CreateDefaultDiscountCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Создание стандартных категорий скидок в базе данных");
            
            var categories = new List<DiscountCategory>
            {
                new DiscountCategory
                {
                    Id = Guid.NewGuid(),
                    Name = "Без скидки",
                    DiscountPercent = 0,
                    RequiresVerification = false,
                    Description = "Стандартный билет без скидки",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DiscountCategory
                {
                    Id = Guid.NewGuid(),
                    Name = "Студенческий",
                    DiscountPercent = 30,
                    RequiresVerification = true,
                    Description = "Доступна для студентов дневной формы обучения при предъявлении студенческого билета.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DiscountCategory
                {
                    Id = Guid.NewGuid(),
                    Name = "Пенсионный",
                    DiscountPercent = 25,
                    RequiresVerification = true,
                    Description = "Доступна для пенсионеров при предъявлении пенсионного удостоверения.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DiscountCategory
                {
                    Id = Guid.NewGuid(),
                    Name = "Детский",
                    DiscountPercent = 50,
                    RequiresVerification = true,
                    Description = "Доступна для детей до 12 лет. Требуется подтверждение возраста.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            
            var response = await _client.From<DiscountCategory>().Insert(categories);
            
            if (response?.Models?.Count > 0)
            {
                _logger.LogInformation($"Успешно создано {response.Models.Count} стандартных категорий скидок в базе данных");
                
                // Выведем созданные категории для отладки
                foreach (var category in response.Models)
                {
                    _logger.LogInformation($"Созданная категория: ID={category.Id}, Название='{category.Name}', Скидка={category.DiscountPercent}%");
                }
                
                return response.Models;
            }
            
            _logger.LogWarning("Не удалось создать стандартные категории скидок в базе данных");
            
            // Возвращаем локальный список категорий с пустым GUID для категории "Без скидки"
            return new List<DiscountCategory>
            {
                new DiscountCategory
                {
                    Id = Guid.Empty,
                    Name = "Без скидки (локальная)",
                    DiscountPercent = 0,
                    RequiresVerification = false,
                    Description = "Стандартный билет без скидки (локальная категория)",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании стандартных категорий скидок");
            
            // Возвращаем только одну категорию "Без скидки" с пустым GUID
            return new List<DiscountCategory>
            {
                new DiscountCategory
                {
                    Id = Guid.Empty,
                    Name = "Без скидки (ошибка)",
                    DiscountPercent = 0,
                    RequiresVerification = false,
                    Description = "Стандартный билет без скидки (при ошибке создания категорий)",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }
    }

    // Сброс базы данных (только для отладки)
    public async Task<bool> ResetDatabaseAsync()
    {
        try
        {
            await _client.From<AfishaUno.Models.User>().Delete();
            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка сброса базы данных: {ex.Message}");
            return false;
        }
    }

    // Детали расписания
    public async Task<Schedule> GetScheduleDetailsAsync(Guid scheduleId)
    {
        try
        {
            var response = await _client.From<Schedule>()
                .Where(s => s.Id == scheduleId)
                .Get();

            if (response?.Models?.Count > 0)
            {
                var schedule = response.Models.First();

                // Загружаем связанные данные
                schedule.Performance = await _client
                    .From<Performance>()
                    .Where(p => p.Id == schedule.PerformanceId)
                    .Single();

                schedule.Hall = await _client
                    .From<Hall>()
                    .Where(h => h.Id == schedule.HallId)
                    .Single();

                return schedule;
            }

            return null;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка получения расписания: {ex.Message}");
            return null;
        }
    }

    // Доступные места
    public async Task<List<Seat>> GetAvailableSeatsAsync(Guid scheduleId)
    {
        try
        {
            // Получаем информацию о расписании
            var schedule = await GetScheduleDetailsAsync(scheduleId);
            if (schedule == null)
                return new List<Seat>();

            // Получаем все места в зале
            var allSeats = await GetSeatsAsync(schedule.HallId);

            // Получаем все билеты для этого сеанса
            var tickets = await GetTicketsAsync(scheduleId);

            // Отфильтровываем занятые места
            var soldSeatIds = tickets
                .Where(t => t.Status is TicketStatuses.Sold or TicketStatuses.Reserved)
                .Select(t => t.SeatId)
                .ToHashSet();

            return allSeats.Where(s => !soldSeatIds.Contains(s.Id)).ToList();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Ошибка получения доступных мест: {ex.Message}");
            return new List<Seat>();
        }
    }

    // Продажа билета
    public async Task<Ticket> SellTicketAsync(Ticket ticket)
    {
        try
        {
            _logger.LogInformation($"Attempting to sell ticket for schedule {ticket.ScheduleId}, seat {ticket.SeatId}");
            
            // Дополняем билет необходимыми данными
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            // Заглушка для QR-кода (TODO: реализовать генерацию QR-кода)
            ticket.QrCode = $"TICKET-{Guid.NewGuid()}";

            // Создаем DTO для работы с базой данных
            var ticketDto = TicketDTO.FromTicket(ticket);

            // Сохраняем билет через DTO
            var response = await _client.From<TicketDTO>().Insert(ticketDto);

            if (response?.Models?.Count > 0)
            {
                _logger.LogInformation($"Ticket successfully sold: {ticketDto.Id}");
                // Преобразуем DTO обратно в модель
                return response.Models.First().ToTicket();
            }

            _logger.LogError("Failed to sell ticket: No response from database");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error selling ticket: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    public async Task<Hall> CreateHallAsync(Hall hall)
    {
        try
        {
            Trace.WriteLine($"[CreateHallAsync] Создание зала: {hall.Name}");
            var response = await _client.From<Hall>().Insert(hall);

            if (response?.Models?.Count > 0)
            {
                Trace.WriteLine($"[CreateHallAsync] Зал успешно создан: Id={response.Models[0].Id}");
                return response.Models[0];
            }

            Trace.WriteLine("[CreateHallAsync] Ошибка: Результат равен null");
            return null;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[CreateHallAsync] Исключение: {ex.Message}");
            return null;
        }
    }

    public async Task<Schedule> CreateScheduleAsync(Guid performanceId, Guid hallId, DateTime startTime, decimal basePrice)
    {
        try
        {
            _logger.LogInformation($"Attempting to create schedule for performance {performanceId} in hall {hallId} at {startTime}");
            
            var scheduleDto = new ScheduleDTO
            {
                Id = Guid.NewGuid(),
                PerformanceId = performanceId,
                HallId = hallId,
                StartTime = startTime,
                BasePrice = basePrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var response = await _client.From<ScheduleDTO>()
                .Insert(scheduleDto);
            
            if (response?.Models == null || response.Models.Count == 0)
            {
                _logger.LogError("Failed to create schedule: No models returned from database");
                return null;
            }
            
            // Получаем первый объект DTO из результата
            var resultDto = response.Models[0];
            _logger.LogInformation($"Successfully created schedule {resultDto.Id}");
            
            // Преобразовываем DTO в полную модель Schedule
            var schedule = resultDto.ToSchedule();
            
            return schedule;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to create schedule: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> CreateSeatsAsync(List<Seat> seats)
    {
        try
        {
            Trace.WriteLine($"[CreateSeatsAsync] Создание {seats.Count} мест");

            // Создаем места пакетами по 50, чтобы избежать проблем с большими пакетами данных
            const int batchSize = 50;

            for (int i = 0; i < seats.Count; i += batchSize)
            {
                var batch = seats.Skip(i).Take(batchSize).ToList();
                Trace.WriteLine($"[CreateSeatsAsync] Обработка пакета {i / batchSize + 1}: {batch.Count} мест");

                var response = await _client.From<Seat>().Insert(batch);

                if (response == null || response.Models.Count == 0)
                {
                    Trace.WriteLine($"[CreateSeatsAsync] Ошибка при создании пакета {i / batchSize + 1}");
                    return false;
                }
            }

            Trace.WriteLine($"[CreateSeatsAsync] Успешно создано {seats.Count} мест");
                return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[CreateSeatsAsync] Исключение: {ex.Message}");
            return false;
        }
    }

    // Получение расписания с полными деталями о спектаклях и залах
    public async Task<List<Schedule>> GetScheduleWithDetailsAsync()
    {
        try
        {
            _logger.LogInformation("Загрузка расписания с деталями...");
            
            // Получаем все записи расписания
            var response = await _client.From<Schedule>().Get();
            
            // Загружаем связанные данные для каждой записи расписания
            foreach (var schedule in response.Models)
            {
                try
                {
                    // Загружаем спектакль
                    schedule.Performance = await _client
                        .From<Performance>()
                        .Where(p => p.Id == schedule.PerformanceId)
                        .Single();

                    // Загружаем зал
                    schedule.Hall = await _client
                        .From<Hall>()
                        .Where(h => h.Id == schedule.HallId)
                        .Single();
                    
                    _logger.LogInformation($"Загружен сеанс: {schedule.Id}, спектакль: {schedule.Performance?.Title}, зал: {schedule.Hall?.Name}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Ошибка при загрузке связанных данных для сеанса {schedule.Id}: {ex.Message}");
                }
            }

            _logger.LogInformation($"Загружено {response.Models.Count} записей расписания с деталями");
            return response.Models;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при загрузке расписания с деталями: {ex.Message}");
            return new List<Schedule>();
        }
    }

    // Клиенты
    public async Task<List<Customer>> GetCustomersAsync()
    {
        try
        {
            _logger.LogInformation("Loading customers from database");
            
            // Используем типизированный API с DTO для получения всех клиентов
            var response = await _client.From<Models.DTOs.CustomerDTO>().Get();
            
            if (response?.Models == null || response.Models.Count == 0)
            {
                _logger.LogWarning("No customers found in database");
                return new List<Customer>();
            }

            // Преобразуем список DTO в список моделей Customer
            var customers = response.Models.Select(dto => dto.ToCustomer()).ToList();
            _logger.LogInformation($"Successfully loaded {customers.Count} customers");
            return customers;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting customers: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return new List<Customer>();
        }
    }

    public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
    {
        try
        {
            _logger.LogInformation($"Searching customers with term: {searchTerm}");
            
            // Если поисковый запрос пустой, возвращаем первые 10 клиентов
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                _logger.LogInformation("Empty search term - returning first 10 customers");
                
                var allResponse = await _client.From<Models.DTOs.CustomerDTO>()
                    .Limit(10)
                    .Get();
                
                if (allResponse?.Models == null || allResponse.Models.Count == 0)
                {
                    _logger.LogWarning("No customers found in database");
                    return new List<Customer>();
                }
                
                var allCustomers = allResponse.Models.Select(dto => dto.ToCustomer()).ToList();
                _logger.LogInformation($"Found {allCustomers.Count} customers");
                return allCustomers;
            }
            
            // Поиск по номеру телефона (использует ILike для приблизительного соответствия)
            var phoneResponse = await _client.From<Models.DTOs.CustomerDTO>()
                .Filter("phone", Supabase.Postgrest.Constants.Operator.ILike, $"%{searchTerm}%")
                .Get();
            
            // Поиск по полному имени
            var nameResponse = await _client.From<Models.DTOs.CustomerDTO>()
                .Filter("full_name", Supabase.Postgrest.Constants.Operator.ILike, $"%{searchTerm}%")
                .Get();
            
            // Собираем результаты, преобразуя DTO в модели Customer
            var phoneCustomers = phoneResponse?.Models != null 
                ? phoneResponse.Models.Select(dto => dto.ToCustomer()).ToList() 
                : new List<Customer>();
                
            var nameCustomers = nameResponse?.Models != null 
                ? nameResponse.Models.Select(dto => dto.ToCustomer()).ToList() 
                : new List<Customer>();
            
            // Объединяем результаты и убираем дубликаты
            var result = phoneCustomers
                .Union(nameCustomers, new CustomerEqualityComparer())
                .ToList();
                
            _logger.LogInformation($"Found {result.Count} customers matching search term: {searchTerm}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error searching for customers: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return new List<Customer>();
        }
    }

    public async Task<Customer> GetCustomerByPhoneAsync(string phone)
    {
        try
        {
            _logger.LogInformation($"Looking for customer with phone: {phone}");
            
            if (string.IsNullOrWhiteSpace(phone))
            {
                _logger.LogWarning("Phone number is empty");
                return null;
            }

            // Используем типизированный API с DTO для поиска клиента
            var response = await _client.From<Models.DTOs.CustomerDTO>()
                .Filter("phone", Supabase.Postgrest.Constants.Operator.Equals, phone)
                .Get();
            
            if (response?.Models == null || response.Models.Count == 0)
            {
                _logger.LogInformation($"No customer found with phone: {phone}");
                return null;
            }

            // Преобразуем DTO в модель Customer
            var customer = response.Models.First().ToCustomer();
            _logger.LogInformation($"Found customer: {customer.FullName}");
            return customer;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting customer by phone: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    public async Task<Customer> GetCustomerByIdAsync(Guid customerId)
    {
        try
        {
            _logger.LogInformation($"Getting customer by id: {customerId}");
            
            if (customerId == Guid.Empty)
            {
                _logger.LogWarning("Customer ID is empty");
                return null;
            }

            // Используем типизированный API с DTO для поиска клиента
            // Преобразуем Guid в строку, чтобы избежать ошибки типа критерия
            var response = await _client.From<Models.DTOs.CustomerDTO>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, customerId.ToString())
                .Get();
            
            if (response?.Models == null || response.Models.Count == 0)
            {
                _logger.LogInformation($"No customer found with id: {customerId}");
                return null;
            }

            // Преобразуем DTO в модель Customer
            var customer = response.Models.First().ToCustomer();
            _logger.LogInformation($"Found customer: {customer.FullName}");
            return customer;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting customer by id: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        try
        {
            if (customer == null)
            {
                _logger.LogError("Cannot create customer: customer object is null");
                return null;
            }
            
            _logger.LogInformation($"Creating customer: Id={customer.Id}, FullName={customer.FullName}, " +
                $"Phone={customer.Phone}, Email={customer.Email}, DocumentType={customer.DocumentType}, " +
                $"DocumentNumber={customer.DocumentNumber}");

            // Преобразуем Customer в CustomerDTO для безопасной отправки в Supabase
            var customerDto = Models.DTOs.CustomerDTO.FromCustomer(customer);
            
            _logger.LogInformation($"Sending customer DTO to Supabase: {customerDto.Id}");
            
            // Используем типизированный API для вставки данных через DTO
            var response = await _client.From<Models.DTOs.CustomerDTO>().Insert(customerDto);
            
            if (response?.Models == null || response.Models.Count == 0)
            {
                _logger.LogError("Failed to create customer: No response models returned");
                return null;
            }
            
            // Преобразуем DTO обратно в Customer
            var createdCustomer = response.Models[0].ToCustomer();
            
            _logger.LogInformation($"Customer {createdCustomer.Id} created successfully");
            return createdCustomer;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating customer: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        try
        {
            _logger.LogInformation($"Updating customer: Id={customer.Id}, FullName={customer.FullName}, " +
                $"Phone={customer.Phone}, Email={customer.Email}, DocumentType={customer.DocumentType}, " +
                $"DocumentNumber={customer.DocumentNumber}");

            // Обновляем время изменения
            customer.UpdatedAt = DateTime.UtcNow;
            
            // Преобразуем Customer в CustomerDTO
            var customerDto = Models.DTOs.CustomerDTO.FromCustomer(customer);
            
            // Используем типизированный API для обновления данных через DTO
            var response = await _client.From<Models.DTOs.CustomerDTO>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, customerDto.Id)
                .Update(customerDto);
            
            if (response?.Models == null || response.Models.Count == 0)
            {
                _logger.LogError("Failed to update customer: No response models returned");
                return null;
            }
            
            // Преобразуем DTO обратно в Customer
            var updatedCustomer = response.Models[0].ToCustomer();
            
            _logger.LogInformation($"Customer {updatedCustomer.Id} updated successfully");
            return updatedCustomer;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating customer: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    // Карты лояльности
    public async Task<List<LoyaltyCard>> GetLoyaltyCardsAsync(Guid customerId)
    {
        try
        {
            _logger.LogInformation($"Getting loyalty cards for customer ID: {customerId}");
            
            var response = await _client.From<Models.DTOs.LoyaltyCardDTO>()
                .Filter("customer_id", Supabase.Postgrest.Constants.Operator.Equals, customerId.ToString())
                .Get();

            if (response?.Models == null || response.Models.Count == 0)
            {
                _logger.LogInformation($"No loyalty cards found for customer ID: {customerId}");
                return new List<LoyaltyCard>();
            }

            // Преобразуем список DTO в список моделей
            var loyaltyCards = response.Models.Select(dto => dto.ToLoyaltyCard()).ToList();
            _logger.LogInformation($"Found {loyaltyCards.Count} loyalty cards for customer ID: {customerId}");
            return loyaltyCards;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting loyalty cards: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return new List<LoyaltyCard>();
        }
    }

    public async Task<LoyaltyCard> GetLoyaltyCardByNumberAsync(string cardNumber)
    {
        try
        {
            _logger.LogInformation($"Looking for loyalty card with number: {cardNumber}");
            
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                _logger.LogWarning("Card number is empty");
                return null;
            }

            var response = await _client.From<Models.DTOs.LoyaltyCardDTO>()
                .Filter("card_number", Supabase.Postgrest.Constants.Operator.Equals, cardNumber)
                .Get();

            if (response?.Models == null || response.Models.Count == 0)
            {
                _logger.LogInformation($"No loyalty card found with number: {cardNumber}");
                return null;
            }

            // Преобразуем DTO в модель
            var loyaltyCard = response.Models.First().ToLoyaltyCard();
            _logger.LogInformation($"Found loyalty card for customer ID: {loyaltyCard.CustomerId}");
            return loyaltyCard;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting loyalty card by number: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    public async Task<LoyaltyCard> CreateLoyaltyCardAsync(LoyaltyCard loyaltyCard)
    {
        try
        {
            _logger.LogInformation($"Creating new loyalty card for customer ID: {loyaltyCard.CustomerId}");
            
            // Генерируем уникальный номер карты, если он не указан
            if (string.IsNullOrWhiteSpace(loyaltyCard.CardNumber))
            {
                loyaltyCard.CardNumber = GenerateUniqueCardNumber();
                _logger.LogInformation($"Generated card number: {loyaltyCard.CardNumber}");
            }

            // Генерируем новый ID, если он не был установлен
            if (loyaltyCard.Id == Guid.Empty)
            {
                loyaltyCard.Id = Guid.NewGuid();
            }

            // Преобразуем модель в DTO для отправки в Supabase
            var loyaltyCardDto = Models.DTOs.LoyaltyCardDTO.FromLoyaltyCard(loyaltyCard);
            
            var response = await _client.From<Models.DTOs.LoyaltyCardDTO>().Insert(loyaltyCardDto);

            if (response?.Models == null || response.Models.Count == 0)
            {
                _logger.LogError("Failed to create loyalty card: No response from database");
                return null;
            }

            // Преобразуем полученный DTO в модель для возврата
            var createdCard = response.Models.First().ToLoyaltyCard();
            _logger.LogInformation($"Loyalty card created successfully with ID: {createdCard.Id}");
            return createdCard;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating loyalty card: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    public async Task<LoyaltyCard> UpdateLoyaltyCardAsync(LoyaltyCard loyaltyCard)
    {
        try
        {
            _logger.LogInformation($"Updating loyalty card ID: {loyaltyCard.Id}");
            
            // Преобразуем модель в DTO для отправки в Supabase
            var loyaltyCardDto = Models.DTOs.LoyaltyCardDTO.FromLoyaltyCard(loyaltyCard);
            
            var response = await _client.From<Models.DTOs.LoyaltyCardDTO>()
                .Where(c => c.Id == loyaltyCardDto.Id)
                .Update(loyaltyCardDto);

            if (response?.Models == null || response.Models.Count == 0)
            {
                _logger.LogError($"Failed to update loyalty card ID: {loyaltyCard.Id}");
                return null;
            }

            // Преобразуем полученный DTO в модель для возврата
            var updatedCard = response.Models.First().ToLoyaltyCard();
            _logger.LogInformation($"Loyalty card updated successfully, ID: {updatedCard.Id}");
            return updatedCard;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating loyalty card: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    public async Task<bool> AddPointsToLoyaltyCardAsync(Guid loyaltyCardId, int points)
    {
        try
        {
            _logger.LogInformation($"Adding {points} points to loyalty card ID: {loyaltyCardId}");
            
            // Получаем карту лояльности через DTO
            var response = await _client.From<Models.DTOs.LoyaltyCardDTO>()
                .Where(c => c.Id == loyaltyCardId)
                .Get();

            if (response?.Models == null || response.Models.Count == 0)
            {
                _logger.LogError($"Loyalty card not found: {loyaltyCardId}");
                return false;
            }

            // Преобразуем DTO в модель для работы
            var loyaltyCardDto = response.Models.First();
            var loyaltyCard = loyaltyCardDto.ToLoyaltyCard();
            
            // Добавляем очки
            loyaltyCard.Points += points;
            
            // Проверяем, нужно ли повысить уровень
            UpdateLoyaltyLevel(loyaltyCard);
            
            // Обновляем DTO новыми данными из модели
            loyaltyCardDto = Models.DTOs.LoyaltyCardDTO.FromLoyaltyCard(loyaltyCard);

            // Обновляем карту в базе данных через DTO
            var updateResponse = await _client.From<Models.DTOs.LoyaltyCardDTO>()
                .Where(c => c.Id == loyaltyCardId)
                .Update(loyaltyCardDto);

            if (updateResponse?.Models == null || updateResponse.Models.Count == 0)
            {
                _logger.LogError($"Failed to update loyalty card with new points");
                return false;
            }

            _logger.LogInformation($"Successfully added {points} points to card ID: {loyaltyCardId}, new total: {loyaltyCard.Points}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding points to loyalty card: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            return false;
        }
    }

    // Вспомогательные методы
    private string GenerateUniqueCardNumber()
    {
        // Генерируем 12-значный номер карты
        // Формат: XXXX-XXXX-XXXX
        Random random = new Random();
        string part1 = random.Next(1000, 9999).ToString();
        string part2 = random.Next(1000, 9999).ToString();
        string part3 = random.Next(1000, 9999).ToString();
        
        return $"{part1}-{part2}-{part3}";
    }

    private void UpdateLoyaltyLevel(LoyaltyCard loyaltyCard)
    {
        // Обновляем уровень лояльности на основе накопленных очков
        if (loyaltyCard.Points >= 5000)
        {
            loyaltyCard.Level = LoyaltyLevels.Platinum;
        }
        else if (loyaltyCard.Points >= 2000)
        {
            loyaltyCard.Level = LoyaltyLevels.Gold;
        }
        else if (loyaltyCard.Points >= 500)
        {
            loyaltyCard.Level = LoyaltyLevels.Silver;
        }
        else
        {
            loyaltyCard.Level = LoyaltyLevels.Bronze;
        }
    }

    // Класс для сравнения клиентов и удаления дубликатов при объединении коллекций
    private class CustomerEqualityComparer : IEqualityComparer<Customer>
    {
        public bool Equals(Customer x, Customer y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            
            return x.Id == y.Id;
        }

        public int GetHashCode(Customer obj)
        {
            if (obj == null)
                return 0;
                
            return obj.Id.GetHashCode();
        }
    }
}

