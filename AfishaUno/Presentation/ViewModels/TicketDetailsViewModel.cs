using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using AfishaUno.Models;
using AfishaUno.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Devices.Printers;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using AfishaUno.Presentation.Views;

namespace AfishaUno.Presentation.ViewModels
{
    public class TicketDetailsViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;
        private readonly ITicketPrintService _ticketPrintService;
        private readonly ILogger<TicketDetailsViewModel> _logger;

        // XamlRoot для диалоговых окон
        private XamlRoot _xamlRoot;
        public XamlRoot XamlRoot
        {
            get => _xamlRoot;
            set => SetProperty(ref _xamlRoot, value);
        }

        // Информация о спектакле, зале и месте
        private Schedule _schedule;
        private SeatViewModel _selectedSeat;
        private List<DiscountCategory> _discountCategories;
        private DiscountCategory _selectedDiscountCategory;
        private ObservableCollection<string> _ticketTypes;
        private string _selectedTicketType;
        private decimal _finalPrice;
        private bool _isLoading;
        private string _errorMessage;

        // Информация о покупателе
        private string _customerName;
        private string _customerPhone;
        private Customer _selectedCustomer;
        private bool _isCustomerSearchVisible;
        private CustomerSearchViewModel _customerSearchViewModel;

        // Информация о карте лояльности
        private List<LoyaltyCard> _customerLoyaltyCards;
        private LoyaltyCard _selectedLoyaltyCard;
        private bool _showLoyaltyCardInfo;
        private bool _showCreateLoyaltyCard;

        // Команды
        public ICommand CancelCommand { get; }
        public ICommand ConfirmTicketCommand { get; }
        public ICommand SearchCustomerCommand { get; }
        public ICommand CreateLoyaltyCardCommand { get; }
        public ICommand SelectCustomerCommand { get; }
        public ICommand PreviousStepCommand { get; }

        // Свойства
        public Schedule Schedule
        {
            get => _schedule;
            set => SetProperty(ref _schedule, value);
        }

        public SeatViewModel SelectedSeat
        {
            get => _selectedSeat;
            set => SetProperty(ref _selectedSeat, value);
        }

        public List<DiscountCategory> DiscountCategories
        {
            get => _discountCategories;
            set => SetProperty(ref _discountCategories, value);
        }

        public DiscountCategory SelectedDiscountCategory
        {
            get => _selectedDiscountCategory;
            set
            {
                if (SetProperty(ref _selectedDiscountCategory, value))
                {
                    CalculateFinalPrice();
                }
            }
        }

        public ObservableCollection<string> TicketTypes
        {
            get => _ticketTypes;
            set => SetProperty(ref _ticketTypes, value);
        }

        public string SelectedTicketType
        {
            get => _selectedTicketType;
            set => SetProperty(ref _selectedTicketType, value);
        }

        public decimal FinalPrice
        {
            get => _finalPrice;
            set => SetProperty(ref _finalPrice, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        public string CustomerPhone
        {
            get => _customerPhone;
            set => SetProperty(ref _customerPhone, value);
        }

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value) && value != null)
                {
                    // Обновляем поля формы данными выбранного клиента
                    CustomerName = value.FullName;
                    CustomerPhone = value.Phone;
                    // Загружаем карты лояльности клиента
                    LoadCustomerLoyaltyCardsAsync(value.Id).ConfigureAwait(false);
                }
            }
        }

        public bool IsCustomerSearchVisible
        {
            get => _isCustomerSearchVisible;
            set => SetProperty(ref _isCustomerSearchVisible, value);
        }

        public CustomerSearchViewModel CustomerSearchViewModel
        {
            get => _customerSearchViewModel;
            set => SetProperty(ref _customerSearchViewModel, value);
        }

        public List<LoyaltyCard> CustomerLoyaltyCards
        {
            get => _customerLoyaltyCards;
            set => SetProperty(ref _customerLoyaltyCards, value);
        }

        public LoyaltyCard SelectedLoyaltyCard
        {
            get => _selectedLoyaltyCard;
            set
            {
                if (SetProperty(ref _selectedLoyaltyCard, value) && value != null)
                {
                    // Если у карты лояльности есть связанная категория скидки, используем её
                    if (value.DiscountCategoryId.HasValue)
                    {
                        var discountCategory = DiscountCategories.FirstOrDefault(d => d.Id == value.DiscountCategoryId.Value);
                        if (discountCategory != null)
                        {
                            SelectedDiscountCategory = discountCategory;
                        }
                    }
                }
            }
        }

        public bool ShowLoyaltyCardInfo
        {
            get => _showLoyaltyCardInfo;
            set => SetProperty(ref _showLoyaltyCardInfo, value);
        }

        public bool ShowCreateLoyaltyCard
        {
            get => _showCreateLoyaltyCard;
            set => SetProperty(ref _showCreateLoyaltyCard, value);
        }

        // Событие для отображения статуса
        public event EventHandler<StatusEventArgs> StatusChanged;
        
        // Класс для передачи информации о статусе
        public class StatusEventArgs : EventArgs
        {
            public string Message { get; set; }
            public bool IsSuccess { get; set; }
        }

        public TicketDetailsViewModel(
            ISupabaseService supabaseService, 
            INavigationService navigationService, 
            ITicketPrintService ticketPrintService = null, 
            ILogger<TicketDetailsViewModel> logger = null)
        {
            _supabaseService = supabaseService;
            _navigationService = navigationService;
            _ticketPrintService = ticketPrintService;
            _logger = logger;

            // Инициализация команд
            CancelCommand = new AsyncRelayCommand(OnCancel);
            ConfirmTicketCommand = new AsyncRelayCommand(ConfirmTicketAsync);
            SearchCustomerCommand = new AsyncRelayCommand(SearchCustomerAsync);
            CreateLoyaltyCardCommand = new AsyncRelayCommand(CreateLoyaltyCardAsync);
            SelectCustomerCommand = new RelayCommand<Customer?>(OnSelectCustomer);
            PreviousStepCommand = new AsyncRelayCommand(OnCancel);

            // Начальные значения
            TicketTypes = new ObservableCollection<string>
            {
                "Обычный",
                "Электронный"
            };
            SelectedTicketType = TicketTypes[0];
            _discountCategories = new List<DiscountCategory>();
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            CustomerLoyaltyCards = new List<LoyaltyCard>();
            IsCustomerSearchVisible = false;
            ShowLoyaltyCardInfo = false;
            ShowCreateLoyaltyCard = false;
            
            // Инициализируем CustomerSearchViewModel
            try
            {
                CustomerSearchViewModel = App.Current.Services.GetService<CustomerSearchViewModel>();
                
                if (CustomerSearchViewModel == null)
                {
                    _logger?.LogWarning("TicketDetailsViewModel: CustomerSearchViewModel не найден в сервисах, создаем новый экземпляр");
                    CustomerSearchViewModel = new CustomerSearchViewModel(
                        _supabaseService,
                        App.Current.Services.GetService<ILogger<CustomerSearchViewModel>>()
                    );
                }
                
                _logger?.LogInformation("TicketDetailsViewModel: CustomerSearchViewModel инициализирован");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsViewModel: Ошибка при инициализации CustomerSearchViewModel: {Error}", ex.Message);
            }
        }

        // Метод для инициализации представления данными
        public async Task InitializeAsync(Schedule schedule, SeatViewModel selectedSeat)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Сохраняем информацию о выбранном месте
                Schedule = schedule;
                SelectedSeat = selectedSeat;

                try
                {
                    // Загружаем категории скидок из базы данных
                    _logger?.LogInformation("Загрузка категорий скидок для билета");
                    var loadedCategories = await _supabaseService.GetDiscountCategoriesAsync();
                    
                    // Устанавливаем список категорий скидок
                    if (loadedCategories != null && loadedCategories.Count > 0)
                    {
                        _logger?.LogInformation($"Загружено {loadedCategories.Count} категорий скидок");
                        DiscountCategories = loadedCategories;
                        
                        // Ищем категорию "Без скидки" для выбора по умолчанию
                        var noDiscountCategory = DiscountCategories.FirstOrDefault(c => 
                            c.Name.Contains("Без скидки") || 
                            c.DiscountPercent == 0);
                            
                        if (noDiscountCategory != null)
                        {
                            _logger?.LogInformation($"Выбрана категория скидки по умолчанию: {noDiscountCategory.Name} (ID: {noDiscountCategory.Id})");
                            SelectedDiscountCategory = noDiscountCategory;
                        }
                        else
                        {
                            SelectedDiscountCategory = DiscountCategories.First();
                        }
                    }
                    else
                    {
                        _logger?.LogWarning("Категории скидок не загружены из базы данных");
                        
                        // Создаем стандартную категорию "Без скидки"
                        var noDiscountCategory = new DiscountCategory
                        {
                            Id = Guid.Empty,
                            Name = "Без скидки",
                            DiscountPercent = 0,
                            Description = "Стандартный билет без скидки",
                            RequiresVerification = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        
                        DiscountCategories = new List<DiscountCategory> { noDiscountCategory };
                        SelectedDiscountCategory = noDiscountCategory;
                    }
                    
                    // Рассчитываем конечную цену
                    CalculateFinalPrice();
                    
                    // Автоматически инициализируем и показываем поиск клиента
                    await InitializeCustomerSearchAsync();
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Ошибка при загрузке категорий скидок: {ex.Message}");
                    
                    // Создаем стандартную категорию "Без скидки" для случая ошибки
                    var errorCategory = new DiscountCategory
                    {
                        Id = Guid.Empty,
                        Name = "Без скидки (ошибка)",
                        DiscountPercent = 0,
                        Description = "Стандартный билет без скидки (при ошибке)",
                        RequiresVerification = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    DiscountCategories = new List<DiscountCategory> { errorCategory };
                    SelectedDiscountCategory = errorCategory;
                    CalculateFinalPrice();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при инициализации: {ex.Message}";
                _logger?.LogError($"Ошибка инициализации TicketDetailsViewModel: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger?.LogError($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        // Инициализация поиска клиента
        private async Task InitializeCustomerSearchAsync()
        {
            try
            {
                // Показываем панель поиска клиента
                IsCustomerSearchVisible = true;
                
                // Создаем и инициализируем ViewModel для поиска клиентов
                CustomerSearchViewModel = App.Current.Services.GetService<CustomerSearchViewModel>();
                
                if (CustomerSearchViewModel == null)
                {
                    ErrorMessage = "Ошибка при получении CustomerSearchViewModel из сервисов";
                    _logger?.LogError("CustomerSearchViewModel не зарегистрирована в сервисах");
                    return;
                }
                
                await CustomerSearchViewModel.InitializeAsync();
                
                // Проверяем наличие клиента в CustomerSelectionManager
                await CheckSelectionManagerAsync();
                
                _logger?.LogInformation("Панель поиска клиента успешно инициализирована");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при инициализации поиска клиента: {ex.Message}";
                _logger?.LogError($"Исключение в InitializeCustomerSearchAsync: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    _logger?.LogError($"Внутреннее исключение: {ex.InnerException.Message}");
                }
            }
        }

        // Расчет конечной цены билета с учетом скидок
        private void CalculateFinalPrice()
        {
            try
            {
                if (Schedule == null || SelectedSeat == null)
                {
                    FinalPrice = 0;
                    return;
                }

                decimal basePrice = Schedule.BasePrice;
                decimal discountPercentage = SelectedDiscountCategory?.DiscountPercent ?? 0;

                // Расчет цены с учетом скидки
                FinalPrice = basePrice - (basePrice * discountPercentage / 100);

                // Округление до целого рубля
                FinalPrice = Math.Round(FinalPrice, 0);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Ошибка при расчете цены: {ex.Message}");
                FinalPrice = Schedule?.BasePrice ?? 0;
            }
        }

        // Метод для отмены и возврата назад
        private async Task OnCancel()
        {
            await _navigationService.GoBackAsync();
        }

        // Метод для поиска клиента
        private async Task SearchCustomerAsync()
        {
            try
            {
                _logger?.LogInformation("TicketDetailsViewModel: Начат поиск клиента");
                
                // Проверяем CustomerSearchViewModel
                if (CustomerSearchViewModel == null)
                {
                    // Пересоздаем, если он null
                    CustomerSearchViewModel = new CustomerSearchViewModel(
                        _supabaseService,
                        App.Current.Services.GetService<ILogger<CustomerSearchViewModel>>()
                    );
                    
                    _logger?.LogWarning("TicketDetailsViewModel: CustomerSearchViewModel был null, создан новый экземпляр");
                }
                
                // Показываем панель поиска
                IsCustomerSearchVisible = true;
                
                // Инициализируем CustomerSearchViewModel
                await CustomerSearchViewModel.InitializeAsync();
                
                // Если в поле телефона что-то введено, используем это для поиска
                if (!string.IsNullOrWhiteSpace(CustomerPhone))
                {
                    CustomerSearchViewModel.SearchTerm = CustomerPhone;
                    await CustomerSearchViewModel.SearchCustomersAsync();
                    _logger?.LogInformation("TicketDetailsViewModel: Выполнен поиск по телефону: {Phone}", CustomerPhone);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при поиске клиента: {ex.Message}";
                _logger?.LogError(ex, "TicketDetailsViewModel: Исключение в SearchCustomerAsync: {ErrorMessage}", ex.Message);
                
                if (ex.InnerException != null)
                {
                    _logger?.LogError(ex.InnerException, "TicketDetailsViewModel: Внутреннее исключение: {ErrorMessage}", 
                        ex.InnerException.Message);
                }
                
                IsCustomerSearchVisible = false;
            }
        }

        // Обработчик выбора клиента из CustomerSearchViewModel
        private void OnSelectCustomer(Customer? customer)
        {
            try
            {
                if (customer != null)
                {
                    _logger?.LogInformation($"Выбран клиент: {customer.FullName}");
                    
                    // Устанавливаем выбранного клиента
                    SelectedCustomer = customer;
                    
                    // Загружаем карты лояльности для этого клиента
                    _ = LoadCustomerLoyaltyCardsAsync(customer.Id);
                    
                    // Скрываем панель поиска клиентов
                    IsCustomerSearchVisible = false;
                    
                    // Если выбрана категория скидки, требующая верификации, проверяем статус клиента
                    if (SelectedDiscountCategory != null && 
                        SelectedDiscountCategory.RequiresVerification &&
                        !customer.VerificationStatus)
                    {
                        _logger?.LogInformation("Клиент не верифицирован, но требуется верификация для скидки");
                        ErrorMessage = "Для выбранной категории скидки требуется верификация. Создайте карту лояльности для этого клиента.";
                        ShowCreateLoyaltyCard = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Ошибка при выборе клиента: {ex.Message}");
            }
        }

        // Метод для проверки наличия клиента в CustomerSelectionManager
        private async Task CheckSelectionManagerAsync()
        {
            try
            {
                // Получаем сервис
                var customerSelectionManager = App.Current.Services.GetService<CustomerSelectionManager>();

                if (customerSelectionManager != null && customerSelectionManager.SelectedCustomer != null)
                {
                    var customer = customerSelectionManager.SelectedCustomer;
                    _logger?.LogInformation($"TicketDetailsViewModel: Найден сохраненный клиент в CustomerSelectionManager: {customer.FullName}");
                    
                    // Выбираем этого клиента
                    OnSelectCustomer(customer);
                    
                    // Очищаем менеджер
                    customerSelectionManager.SelectedCustomer = null;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsViewModel: Ошибка при проверке CustomerSelectionManager: {ErrorMessage}", ex.Message);
            }
        }

        // Метод для загрузки карт лояльности клиента
        private async Task LoadCustomerLoyaltyCardsAsync(Guid customerId)
        {
            try
            {
                _logger?.LogInformation($"Загрузка карт лояльности для клиента ID: {customerId}");
                var loyaltyCards = await _supabaseService.GetLoyaltyCardsAsync(customerId);
                
                if (loyaltyCards != null && loyaltyCards.Count > 0)
                {
                    CustomerLoyaltyCards = loyaltyCards;
                    _logger?.LogInformation($"Загружено {loyaltyCards.Count} карт лояльности");
                    
                    // Выбираем первую активную карту
                    var activeCard = loyaltyCards.FirstOrDefault(c => c.Status == LoyaltyCardStatuses.Active);
                    if (activeCard != null)
                    {
                        SelectedLoyaltyCard = activeCard;
                        ShowLoyaltyCardInfo = true;
                    }
                    else
                    {
                        SelectedLoyaltyCard = null;
                        ShowLoyaltyCardInfo = false;
                    }
                }
                else
                {
                    _logger?.LogInformation($"У клиента нет карт лояльности");
                    CustomerLoyaltyCards = new List<LoyaltyCard>();
                    SelectedLoyaltyCard = null;
                    ShowLoyaltyCardInfo = false;
                }
                
                // Всегда показываем возможность создания новой карты лояльности
                ShowCreateLoyaltyCard = true;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Ошибка загрузки карт лояльности: {ex.Message}");
                CustomerLoyaltyCards = new List<LoyaltyCard>();
                SelectedLoyaltyCard = null;
                ShowLoyaltyCardInfo = false;
                
                // Даже при ошибке показываем возможность создания новой карты
                ShowCreateLoyaltyCard = true;
            }
        }

        // Метод для создания карты лояльности
        private async Task CreateLoyaltyCardAsync()
        {
            if (SelectedCustomer == null)
            {
                ErrorMessage = "Сначала выберите клиента";
                return;
            }
            
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                // Создаем новую карту лояльности
                var loyaltyCard = new LoyaltyCard
                {
                    CustomerId = SelectedCustomer.Id,
                    Status = LoyaltyCardStatuses.Active,
                    Level = LoyaltyLevels.Bronze,
                    Points = 0,
                    IssueDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddYears(2), // Срок действия 2 года
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                // Всегда связываем карту лояльности с выбранной категорией скидки
                if (SelectedDiscountCategory != null && SelectedDiscountCategory.Id != Guid.Empty)
                {
                    _logger?.LogInformation($"Связывание карты лояльности с категорией скидки: {SelectedDiscountCategory.Name}");
                    loyaltyCard.DiscountCategoryId = SelectedDiscountCategory.Id;
                }
                
                _logger?.LogInformation($"Создание карты лояльности для клиента: {SelectedCustomer.FullName}");
                var createdCard = await _supabaseService.CreateLoyaltyCardAsync(loyaltyCard);
                
                if (createdCard != null)
                {
                    _logger?.LogInformation($"Карта лояльности успешно создана: {createdCard.CardNumber}");
                    
                    // Обновляем список карт лояльности
                    await LoadCustomerLoyaltyCardsAsync(SelectedCustomer.Id);
                    
                    // Выбираем только что созданную карту
                    SelectedLoyaltyCard = createdCard;
                    ShowLoyaltyCardInfo = true;
                    
                    // Уведомляем пользователя об успешном создании карты
                    ErrorMessage = $"Карта лояльности успешно создана: {createdCard.CardNumber}";
                }
                else
                {
                    ErrorMessage = "Не удалось создать карту лояльности";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания карты лояльности: {ex.Message}";
                _logger?.LogError($"Ошибка создания карты лояльности: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Метод для подтверждения оформления билета
        private async Task ConfirmTicketAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Проверка обязательных данных
                if (Schedule == null || SelectedSeat == null)
                {
                    ErrorMessage = "Не выбрано место";
                    return;
                }

                // Получаем текущего пользователя (кассира)
                var user = _supabaseService.CurrentUser;
                if (user == null)
                {
                    ErrorMessage = "Ошибка авторизации. Пожалуйста, войдите в систему снова.";
                    return;
                }

                // Генерируем QR-код для билета (в реальной системе здесь будет более сложная логика)
                string qrCode = $"{Schedule.Id}-{SelectedSeat.Id}-{DateTime.UtcNow.Ticks}";

                // Проверяем, нужна ли верификация для выбранной категории скидки
                Guid? discountCategoryId = null;
                if (SelectedDiscountCategory != null && SelectedDiscountCategory.Id != Guid.Empty)
                {
                    if (SelectedDiscountCategory.RequiresVerification)
                    {
                        // Если выбран клиент и у него есть карта лояльности с этой скидкой, то верификация не нужна
                        bool isVerified = false;
                        
                        if (SelectedCustomer != null && SelectedLoyaltyCard != null && 
                            SelectedLoyaltyCard.DiscountCategoryId == SelectedDiscountCategory.Id)
                        {
                            isVerified = true;
                            _logger?.LogInformation("Клиент верифицирован через существующую карту лояльности");
                        }
                        else if (SelectedCustomer != null && SelectedCustomer.VerificationStatus)
                        {
                            isVerified = true;
                            _logger?.LogInformation("Клиент верифицирован через статус верификации");
                        }
                        
                        // Если клиент не верифицирован, но выбран, создаем карту лояльности
                        if (!isVerified && SelectedCustomer != null)
                        {
                            _logger?.LogInformation("Автоматическое создание карты лояльности для верификации");
                            
                            // Создаем новую карту лояльности
                            var loyaltyCard = new LoyaltyCard
                            {
                                Id = Guid.NewGuid(),
                                CustomerId = SelectedCustomer.Id,
                                Status = LoyaltyCardStatuses.Active,
                                Level = LoyaltyLevels.Bronze,
                                Points = 0,
                                DiscountCategoryId = SelectedDiscountCategory.Id, // Привязываем к выбранной категории скидки
                                IssueDate = DateTime.UtcNow,
                                ExpiryDate = DateTime.UtcNow.AddYears(2),
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            
                            var createdCard = await _supabaseService.CreateLoyaltyCardAsync(loyaltyCard);
                            
                            if (createdCard != null)
                            {
                                _logger?.LogInformation($"Создана карта лояльности для верификации: {createdCard.CardNumber}");
                                SelectedLoyaltyCard = createdCard;
                                isVerified = true;
                            }
                            else
                            {
                                _logger?.LogError("Не удалось создать карту лояльности для верификации");
                            }
                        }
                        
                        if (!isVerified)
                        {
                            ErrorMessage = "Для выбранной категории скидки требуется верификация. Выберите клиента с подтвержденными документами или создайте карту лояльности.";
                            return;
                        }
                    }
                    
                    discountCategoryId = SelectedDiscountCategory.Id;
                }
                
                // Создаем новый билет
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    ScheduleId = Schedule.Id,
                    SeatId = SelectedSeat.Id,
                    Status = TicketStatuses.Sold,
                    Price = FinalPrice,
                    DiscountCategoryId = discountCategoryId,
                    QrCode = qrCode,
                    CreatedBy = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                // Если выбран клиент, связываем билет с ним
                if (SelectedCustomer != null)
                {
                    ticket.CustomerId = SelectedCustomer.Id;
                    
                    // Если выбрана карта лояльности, связываем билет с ней
                    if (SelectedLoyaltyCard != null)
                    {
                        ticket.LoyaltyCardId = SelectedLoyaltyCard.Id;
                        
                        // Добавляем очки на карту лояльности (10% от стоимости билета)
                        int pointsToAdd = (int)(FinalPrice * 0.1m);
                        if (pointsToAdd > 0)
                        {
                            await _supabaseService.AddPointsToLoyaltyCardAsync(SelectedLoyaltyCard.Id, pointsToAdd);
                        }
                    }
                }

                // Продаем билет через сервис
                _logger?.LogInformation($"Продажа билета: Место {SelectedSeat.RowNumber}-{SelectedSeat.SeatNumber} с ценой {FinalPrice}");
                var result = await _supabaseService.SellTicketAsync(ticket);

                if (result != null)
                {
                    _logger?.LogInformation($"Билет успешно продан: {result.Id}");

                    // Печатаем билет в формате PDF
                    if (_ticketPrintService != null)
                    {
                        try
                        {
                            await PrintTicketAsync(result);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError($"Ошибка при печати билета: {ex.Message}");
                            // Продолжаем выполнение даже при ошибке печати
                        }
                    }

                    // Формируем сообщение о продаже для отображения
                    string successMessage = $"Билет успешно продан!\n" +
                        $"Спектакль: {Schedule.Performance.Title}\n" +
                        $"Дата и время: {Schedule.StartTime:dd.MM.yyyy HH:mm}\n" +
                        $"Место: Ряд {SelectedSeat.RowNumber}, Место {SelectedSeat.SeatNumber}\n" +
                        $"Стоимость: {FinalPrice:F2} руб.";
                    
                    if (SelectedCustomer != null)
                    {
                        successMessage += $"\nКлиент: {SelectedCustomer.FullName}";
                    }
                    
                    try
                    {
                        // Проверяем, установлен ли XamlRoot
                        if (XamlRoot == null)
                        {
                            _logger?.LogError("ConfirmTicketAsync: XamlRoot не установлен, невозможно отобразить диалог");
                            ErrorMessage = "Ошибка отображения подтверждения: XamlRoot не установлен";
                            
                            // Вызываем событие для отображения статуса на странице
                            StatusChanged?.Invoke(this, new StatusEventArgs { Message = successMessage, IsSuccess = true });
                            return;
                        }

                        // Отображаем информационное сообщение с использованием DialogHelper
                        _logger?.LogInformation("Отображение диалога с сообщением об успешной продаже билета");
                        var dialogResult = await DialogHelper.ShowSuccessAsync(
                            XamlRoot,
                            "Билет успешно продан",
                            successMessage,
                            "ОК",
                            _logger);
                            
                        _logger?.LogInformation($"Результат диалога: {dialogResult}");
                        
                        // Если диалог закрыт кнопкой "X" или иным образом, выводим статус через элемент страницы
                        if (dialogResult == ContentDialogResult.None)
                        {
                            StatusChanged?.Invoke(this, new StatusEventArgs { Message = successMessage, IsSuccess = true });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Ошибка отображения диалога: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            _logger?.LogError($"Внутренняя ошибка диалога: {ex.InnerException.Message}");
                        }
                        ErrorMessage = $"Ошибка отображения подтверждения: {ex.Message}";
                        
                        // Вызываем событие для отображения статуса на странице
                        StatusChanged?.Invoke(this, new StatusEventArgs { Message = successMessage, IsSuccess = true });
                    }

                    // Симулируем печать билета
                    await Task.Delay(1000);

                    // Переходим к начальному экрану продажи билетов
                    await _navigationService.NavigateToAsync("SchedulePage");
                }
                else
                {
                    ErrorMessage = "Ошибка продажи билета";
                    _logger?.LogError("Ошибка продажи билета: ответ от сервиса был null");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка оформления билета: {ex.Message}";
                _logger?.LogError($"Исключение при оформлении билета: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger?.LogError($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Создаёт PDF-билет и открывает его в браузере
        /// </summary>
        /// <param name="ticket">Проданный билет</param>
        /// <returns>Путь к PDF-файлу</returns>
        private async Task<string> PrintTicketAsync(Ticket ticket)
        {
            try
            {
                _logger?.LogInformation($"Начало печати билета ID: {ticket.Id}");
                
                if (_ticketPrintService == null)
                {
                    _logger?.LogWarning("Сервис печати билетов не доступен");
                    
                    // Отображаем информацию о том, что печать не доступна
                    string printWarning = "Печать билета недоступна: сервис печати не инициализирован.";
                    ErrorMessage = printWarning;
                    
                    // Уведомляем UI через событие
                    StatusChanged?.Invoke(this, new StatusEventArgs { 
                        Message = $"Билет успешно продан, но печать недоступна.\nID билета: {ticket.Id}", 
                        IsSuccess = true 
                    });
                    
                    _logger?.LogWarning(printWarning);
                    return null;
                }
                
                // Получаем необходимые данные для билета
                var seat = await GetSeatByIdAsync(ticket.SeatId);
                if (seat == null)
                {
                    string seatError = $"Не удалось получить информацию о месте с ID: {ticket.SeatId}";
                    _logger?.LogError(seatError);
                    
                    // Уведомляем UI через событие
                    StatusChanged?.Invoke(this, new StatusEventArgs { 
                        Message = $"Билет успешно продан, но произошла ошибка при печати: {seatError}", 
                        IsSuccess = true 
                    });
                    
                    return null;
                }
                
                // Получаем клиента, если билет связан с клиентом
                Customer customer = null;
                if (ticket.CustomerId.HasValue)
                {
                    customer = await _supabaseService.GetCustomerByIdAsync(ticket.CustomerId.Value);
                }
                
                // Генерируем и открываем билет
                string pdfPath = await _ticketPrintService.GenerateAndOpenTicketAsync(
                    ticket,
                    Schedule,
                    seat,
                    customer);
                
                _logger?.LogInformation($"Билет успешно создан и открыт: {pdfPath}");
                
                // Уведомляем UI о печати через событие 
                if (!string.IsNullOrEmpty(pdfPath))
                {
                    StatusChanged?.Invoke(this, new StatusEventArgs { 
                        Message = $"Билет успешно напечатан и сохранен по пути: {pdfPath}", 
                        IsSuccess = true 
                    });
                }
                
                return pdfPath;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка при печати билета: {ex.Message}";
                _logger?.LogError(errorMessage);
                
                if (ex.InnerException != null)
                {
                    _logger?.LogError($"Внутренняя ошибка: {ex.InnerException.Message}");
                    errorMessage += $"\nВнутренняя ошибка: {ex.InnerException.Message}";
                }
                
                // Показываем сообщение об ошибке
                ErrorMessage = errorMessage;
                
                // Уведомляем UI через событие
                StatusChanged?.Invoke(this, new StatusEventArgs { 
                    Message = $"Билет успешно продан, но произошла ошибка при печати: {ex.Message}", 
                    IsSuccess = true 
                });
                
                return null;
            }
        }
        
        /// <summary>
        /// Получает информацию о месте по его ID
        /// </summary>
        /// <param name="seatId">ID места</param>
        /// <returns>Информация о месте</returns>
        private async Task<Seat> GetSeatByIdAsync(Guid seatId)
        {
            try
            {
                // Если уже есть информация о месте в ViewModel, используем её
                if (SelectedSeat != null && SelectedSeat.Id == seatId)
                {
                    return new Seat
                    {
                        Id = SelectedSeat.Id,
                        HallId = Schedule.HallId,
                        RowNumber = SelectedSeat.RowNumber,
                        SeatNumber = SelectedSeat.SeatNumber,
                        Category = SelectedSeat.Category,
                        Status = "Available"
                    };
                }
                
                // Иначе загружаем информацию о месте из базы данных
                var seats = await _supabaseService.GetSeatsAsync(Schedule.HallId);
                return seats.FirstOrDefault(s => s.Id == seatId);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Ошибка при получении информации о месте: {ex.Message}");
                return null;
            }
        }

        // Обновленный метод для проверки наличия созданного клиента
        private void CheckForCreatedCustomer()
        {
            try
            {
                if (CustomerSearchViewModel?.CreatedCustomer != null)
                {
                    _logger?.LogInformation("TicketDetailsViewModel: Обнаружен созданный клиент: {CustomerName} (ID: {CustomerId})",
                        CustomerSearchViewModel.CreatedCustomer.FullName, CustomerSearchViewModel.CreatedCustomer.Id);
                    
                    // Используем созданного клиента
                    SelectedCustomer = CustomerSearchViewModel.CreatedCustomer;
                    
                    // Загружаем карты лояльности
                    _ = LoadCustomerLoyaltyCardsAsync(SelectedCustomer.Id);
                    
                    // Отображаем возможность создания карты лояльности для нового клиента
                    ShowCreateLoyaltyCard = true;
                    
                    // Если у клиента нет карты лояльности и выбрана категория скидки, требующая верификации
                    if (SelectedDiscountCategory != null && 
                        SelectedDiscountCategory.RequiresVerification &&
                        !SelectedCustomer.VerificationStatus)
                    {
                        ErrorMessage = "Рекомендуется создать карту лояльности для этого клиента, чтобы применить льготную категорию билета";
                    }
                    
                    // Очищаем созданного клиента в CustomerSearchViewModel
                    CustomerSearchViewModel.CreatedCustomer = null;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsViewModel: Ошибка в методе CheckForCreatedCustomer: {Error}", ex.Message);
            }
        }

        // Команда отмены (возврат на предыдущую страницу)
        private async Task OnCancelCommandAsync()
        {
            try
            {
                _logger?.LogInformation("TicketDetailsViewModel: Выполнение команды отмены");
                
                // Проверяем, не ищем ли мы клиента
                if (IsCustomerSearchVisible)
                {
                    // Закрываем форму поиска
                    IsCustomerSearchVisible = false;
                    
                    // Проверяем наличие созданного клиента
                    CheckForCreatedCustomer();
                    
                    return;
                }
                
                // Возвращаемся на предыдущую страницу
                await _navigationService.GoBackAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"TicketDetailsViewModel: Ошибка при отмене: {ex.Message}");
                ErrorMessage = $"Ошибка при отмене: {ex.Message}";
            }
        }
    }
} 