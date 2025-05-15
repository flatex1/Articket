using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using AfishaUno.Models;
using AfishaUno.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace AfishaUno.Presentation.ViewModels
{
    public partial class CustomerSearchViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly ILogger<CustomerSearchViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<Customer> _customers = new();

        // Свойство для удобной привязки результатов поиска в XAML
        public ObservableCollection<Customer> SearchResults => Customers;

        [ObservableProperty]
        private Customer _selectedCustomer;

        [ObservableProperty]
        private string _searchTerm = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isSearching;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        // Свойство для отображения формы нового клиента с немедленной нотификацией UI
        private bool _showCreateNewCustomer;
        public bool ShowCreateNewCustomer
        {
            get => _showCreateNewCustomer;
            set
            {
                if (_showCreateNewCustomer != value)
                {
                    _showCreateNewCustomer = value;
                    Debug.WriteLine($"ShowCreateNewCustomer изменено на: {value}");
                    _logger?.LogInformation($"ShowCreateNewCustomer изменено на: {value}");
                    OnPropertyChanged(nameof(ShowCreateNewCustomer));
                }
            }
        }

        // Свойства для нового клиента
        [ObservableProperty]
        private string _newCustomerFullName = string.Empty;

        [ObservableProperty]
        private string _newCustomerPhone = string.Empty;

        [ObservableProperty]
        private string _newCustomerEmail = string.Empty;

        private DateTime? _birthDate;
        
        // Свойство, которое будет привязано к DatePicker напрямую
        [ObservableProperty]
        private DateTimeOffset? _newCustomerBirthDate;
        
        // Метод обновления, вызываемый MVVM Toolkit при изменении NewCustomerBirthDate
        partial void OnNewCustomerBirthDateChanged(DateTimeOffset? value)
        {
            _birthDate = value?.DateTime;
            _logger?.LogInformation($"Установлена дата рождения: {value?.DateTime}");
        }

        [ObservableProperty]
        private string _newCustomerDocumentType = string.Empty;

        [ObservableProperty]
        private object _selectedDocumentTypeItem = null;

        [ObservableProperty]
        private string _newCustomerDocumentNumber = string.Empty;

        [ObservableProperty]
        private string _newCustomerNotes = string.Empty;

        // Свойство для доступа к последнему созданному клиенту
        [ObservableProperty]
        private Customer? _createdCustomer;

        // Команды
        public IAsyncRelayCommand SearchCustomersCommand { get; }
        public IAsyncRelayCommand LoadInitialCustomersCommand { get; }
        public IRelayCommand ClearSearchCommand { get; }
        public IRelayCommand ShowCreateNewCustomerFormCommand { get; }
        public IRelayCommand HideCreateNewCustomerFormCommand { get; }
        public IAsyncRelayCommand CreateNewCustomerCommand { get; }
        public IRelayCommand<Customer> SelectCustomerCommand { get; }

        // Свойство для определения пустого результата
        public bool IsEmptyResults => !IsLoading && !ShowCreateNewCustomer && Customers.Count == 0;

        // Оповещение о пустых результатах при изменении соответствующих свойств
        partial void OnIsLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(IsEmptyResults));
        }

        partial void OnCustomersChanged(ObservableCollection<Customer> value)
        {
            OnPropertyChanged(nameof(IsEmptyResults));
        }

        public CustomerSearchViewModel(ISupabaseService supabaseService, ILogger<CustomerSearchViewModel> logger = null)
        {
            _supabaseService = supabaseService ?? throw new ArgumentNullException(nameof(supabaseService));
            _logger = logger;

            // Логирование инициализации ViewModel
            _logger?.LogInformation("CustomerSearchViewModel: Конструктор вызван");
            Debug.WriteLine("CustomerSearchViewModel: Конструктор вызван");

            try
            {
                // Инициализация команд
                SearchCustomersCommand = new AsyncRelayCommand(SearchCustomersAsync);
                LoadInitialCustomersCommand = new AsyncRelayCommand(LoadInitialCustomersAsync);
                ClearSearchCommand = new RelayCommand(ClearSearch);
                ShowCreateNewCustomerFormCommand = new RelayCommand(ShowCreateNewCustomerForm);
                HideCreateNewCustomerFormCommand = new RelayCommand(HideCreateNewCustomerForm);
                CreateNewCustomerCommand = new AsyncRelayCommand(CreateNewCustomerAsync);
                SelectCustomerCommand = new RelayCommand<Customer>(OnSelectCustomer);
                
                _logger?.LogInformation("CustomerSearchViewModel: Команды инициализированы");
                Debug.WriteLine("CustomerSearchViewModel: Команды инициализированы");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Ошибка инициализации команд в CustomerSearchViewModel: {ex.Message}");
                Debug.WriteLine($"Ошибка инициализации команд в CustomerSearchViewModel: {ex.Message}");
                throw;
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                // Инициализация даты рождения
                _birthDate = null;
                NewCustomerBirthDate = null;
                
                // Загружаем начальный список клиентов
                await LoadInitialCustomersAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка инициализации: {ex.Message}";
                _logger?.LogError($"Ошибка инициализации CustomerSearchViewModel: {ex.Message}");
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

        // Загрузка начального списка клиентов (10 последних)
        public async Task LoadInitialCustomersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                _logger?.LogInformation("Загрузка начального списка клиентов");
                var loadedCustomers = await _supabaseService.GetCustomersAsync();
                
                // Берем только последние 10 клиентов
                var recentCustomers = loadedCustomers.OrderByDescending(c => c.UpdatedAt).Take(10).ToList();
                
                // Отображаем клиентов
                Customers.Clear();
                foreach (var customer in recentCustomers)
                {
                    Customers.Add(customer);
                }
                
                _logger?.LogInformation($"Загружено {Customers.Count} начальных клиентов");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки клиентов: {ex.Message}";
                _logger?.LogError($"Ошибка загрузки начальных клиентов: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Поиск клиентов по поисковому запросу
        public async Task SearchCustomersAsync()
        {
            try
            {
                Debug.WriteLine("SearchCustomersAsync: начато выполнение");
                _logger?.LogInformation("SearchCustomersAsync: начато выполнение");
                
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                if (string.IsNullOrWhiteSpace(SearchTerm))
                {
                    Debug.WriteLine("SearchCustomersAsync: поисковый запрос пуст, загрузка всех клиентов");
                    _logger?.LogInformation("SearchCustomersAsync: поисковый запрос пуст, загрузка всех клиентов");
                    await LoadInitialCustomersAsync();
                    return;
                }
                
                IsSearching = true;
                
                Debug.WriteLine($"SearchCustomersAsync: выполняется поиск по запросу '{SearchTerm}'");
                _logger?.LogInformation($"SearchCustomersAsync: выполняется поиск по запросу '{SearchTerm}'");
                var searchResults = await _supabaseService.SearchCustomersAsync(SearchTerm);
                
                // Отображаем результаты
                Customers.Clear();
                foreach (var customer in searchResults)
                {
                    Customers.Add(customer);
                }
                
                Debug.WriteLine($"SearchCustomersAsync: найдено {Customers.Count} клиентов");
                _logger?.LogInformation($"SearchCustomersAsync: найдено {Customers.Count} клиентов");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SearchCustomersAsync: ошибка - {ex.Message}");
                ErrorMessage = $"Ошибка поиска: {ex.Message}";
                _logger?.LogError($"SearchCustomersAsync: ошибка - {ex.Message}");
            }
            finally
            {
                IsSearching = false;
                IsLoading = false;
                Debug.WriteLine("SearchCustomersAsync: завершено выполнение");
                _logger?.LogInformation("SearchCustomersAsync: завершено выполнение");
            }
        }

        // Очистка поискового запроса и сброс результатов
        private void ClearSearch()
        {
            try
            {
                _logger?.LogInformation("CustomerSearchViewModel: Очистка поискового запроса");
                
                // Очищаем поисковый запрос
                SearchTerm = string.Empty;
                ErrorMessage = string.Empty;
                
                // Загружаем начальные данные
                _ = LoadInitialCustomersAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка очистки: {ex.Message}";
                _logger?.LogError(ex, $"CustomerSearchViewModel: Ошибка очистки поискового запроса: {ex.Message}");
            }
        }

        // Показать форму создания нового клиента
        private void ShowCreateNewCustomerForm()
        {
            try
            {
                _logger?.LogInformation("CustomerSearchViewModel: Показ формы создания нового клиента");
                
                // Сбрасываем форму перед показом
                ResetNewCustomerForm();
                
                // Показываем форму
                ShowCreateNewCustomer = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка отображения формы: {ex.Message}";
                _logger?.LogError(ex, $"CustomerSearchViewModel: Ошибка при отображении формы создания клиента: {ex.Message}");
            }
        }

        // Скрыть форму создания нового клиента
        private void HideCreateNewCustomerForm()
        {
            try
            {
                _logger?.LogInformation("CustomerSearchViewModel: Скрытие формы создания нового клиента");
                
                // Скрываем форму
                ShowCreateNewCustomer = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка скрытия формы: {ex.Message}";
                _logger?.LogError(ex, $"CustomerSearchViewModel: Ошибка при скрытии формы создания клиента: {ex.Message}");
            }
        }

        // Создание нового клиента
        public async Task CreateNewCustomerAsync()
        {
            try
            {
                _logger?.LogInformation("Начало процесса создания нового клиента");
                
                // Проверяем заполнение обязательных полей
                if (string.IsNullOrWhiteSpace(NewCustomerFullName))
                {
                    ErrorMessage = "Необходимо указать ФИО клиента";
                    _logger?.LogWarning("Не заполнено обязательное поле: ФИО клиента");
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(NewCustomerPhone))
                {
                    ErrorMessage = "Необходимо указать телефон клиента";
                    _logger?.LogWarning("Не заполнено обязательное поле: Телефон клиента");
                    return;
                }
                
                // Создаем нового клиента
                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    FullName = NewCustomerFullName.Trim(),
                    Phone = NewCustomerPhone.Trim(),
                    Email = NewCustomerEmail?.Trim() ?? string.Empty,
                    BirthDate = _birthDate,
                    DocumentType = NewCustomerDocumentType?.Trim() ?? string.Empty,
                    DocumentNumber = NewCustomerDocumentNumber?.Trim() ?? string.Empty,
                    VerificationStatus = false, // По умолчанию статус проверки - false
                    Notes = NewCustomerNotes?.Trim() ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Добавляем логирование для проверки значений полей
                _logger?.LogInformation("Данные для создания после OnPropertyChanged: ФИО=[{FullName}], Телефон=[{Phone}], Email=[{Email}]",
                    customer.FullName, customer.Phone, customer.Email);
                
                // Специальное логирование для типа документа
                _logger?.LogInformation("Тип документа перед созданием клиента: [{DocumentType}]", customer.DocumentType);
                
                _logger?.LogInformation("Подготовлен новый клиент для создания: {FullName}, телефон: {Phone}", 
                    customer.FullName, customer.Phone);
                
                // Создаем клиента через сервис
                var createdCustomer = await _supabaseService.CreateCustomerAsync(customer);
                
                if (createdCustomer != null)
                {
                    _logger?.LogInformation("Клиент успешно создан с ID: {CustomerId}", createdCustomer.Id);
                    
                    // Сохраняем созданного клиента для использования в других компонентах
                    CreatedCustomer = createdCustomer;
                    
                    // Добавляем созданного клиента в список результатов
                    if (!Customers.Any(c => c.Id == createdCustomer.Id))
                    {
                        Customers.Add(createdCustomer);
                    }
                    
                    // Сбрасываем форму
                    ResetNewCustomerForm();
                    
                    // Скрываем форму создания
                    ShowCreateNewCustomer = false;
                    
                    // Выбираем созданного клиента
                    SelectedCustomer = createdCustomer;
                }
                else
                {
                    _logger?.LogError("Сервер вернул null при создании клиента");
                    ErrorMessage = "Ошибка при создании клиента: сервер вернул пустой результат";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при создании клиента: {ex.Message}";
                _logger?.LogError("Ошибка при создании клиента: {ErrorMessage}", ex.Message);
                
                if (ex.InnerException != null)
                {
                    _logger?.LogError("Внутренняя ошибка: {InnerErrorMessage}", ex.InnerException.Message);
                }
            }
            finally
            {
                _logger?.LogInformation("Завершение процесса создания нового клиента");
            }
        }

        // Обработка выбора клиента
        private void OnSelectCustomer(Customer? customer)
        {
            Debug.WriteLine($"OnSelectCustomer вызван, customer = {customer?.FullName ?? "null"}");
            _logger?.LogInformation($"OnSelectCustomer вызван, customer = {customer?.FullName ?? "null"}");
            
            if (customer != null)
            {
                SelectedCustomer = customer;
                ErrorMessage = string.Empty;
                Debug.WriteLine($"Выбран клиент: {customer.FullName}");
                _logger?.LogInformation($"Выбран клиент: {customer.FullName}");
            }
            else
            {
                Debug.WriteLine("OnSelectCustomer вызван с null параметром");
                _logger?.LogWarning("OnSelectCustomer вызван с null параметром");
            }
        }

        // Сброс формы создания нового клиента
        public void ResetNewCustomerForm()
        {
            // Сбрасываем все поля формы
            NewCustomerFullName = string.Empty;
            NewCustomerPhone = string.Empty;
            NewCustomerEmail = string.Empty;
            NewCustomerBirthDate = null;
            _birthDate = null;
            NewCustomerDocumentType = string.Empty;
            SelectedDocumentTypeItem = null;
            NewCustomerDocumentNumber = string.Empty;
            NewCustomerNotes = string.Empty;
            
            // Сбрасываем сообщение об ошибке
            ErrorMessage = string.Empty;
            
            _logger?.LogInformation("Форма создания клиента сброшена");
        }
    }
} 