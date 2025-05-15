using AfishaUno.Models;
using AfishaUno.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AfishaUno.Presentation.Pages
{
    public sealed partial class TicketDetailsPage : Page
    {
        public TicketDetailsViewModel ViewModel { get; }
        private readonly ILogger<TicketDetailsPage> _logger;

        public TicketDetailsPage()
        {
            _logger?.LogInformation("TicketDetailsPage: Начало инициализации страницы");
            Debug.WriteLine("TicketDetailsPage: Начало инициализации страницы");
            
            try
            {
                this.InitializeComponent();
                
                // Получаем ViewModel и логгер из сервисов
                ViewModel = App.Current.Services.GetService<TicketDetailsViewModel>();
                _logger = App.Current.Services.GetService<ILogger<TicketDetailsPage>>();
                
                if (ViewModel == null)
                {
                    _logger?.LogError("TicketDetailsPage: Не удалось получить TicketDetailsViewModel из DI");
                    Debug.WriteLine("TicketDetailsPage: Не удалось получить TicketDetailsViewModel из DI");
                    
                    throw new InvalidOperationException("Не удалось получить ViewModel из сервисов");
                }
                
                _logger?.LogInformation("TicketDetailsPage: Страница успешно инициализирована");
                Debug.WriteLine("TicketDetailsPage: Страница успешно инициализирована");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при инициализации страницы: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage ОШИБКА: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    _logger?.LogError(ex.InnerException, "Внутренняя ошибка: {ErrorMessage}", ex.InnerException.Message);
                    Debug.WriteLine($"TicketDetailsPage внутренняя ошибка: {ex.InnerException.Message}");
                }
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                base.OnNavigatedTo(e);
                _logger?.LogInformation("TicketDetailsPage: Навигация на страницу, тип параметра: {ParamType}", 
                    e.Parameter?.GetType().Name ?? "null");
                Debug.WriteLine($"TicketDetailsPage: Навигация на страницу, тип параметра: {e.Parameter?.GetType().Name ?? "null"}");

                // Устанавливаем XamlRoot для диалоговых окон
                if (ViewModel != null)
                {
                    try 
                    {
                        if (this.XamlRoot == null)
                        {
                            _logger?.LogError("TicketDetailsPage: XamlRoot страницы равен null");
                            Debug.WriteLine("TicketDetailsPage: XamlRoot страницы равен null");
                        }
                        else
                        {
                            ViewModel.XamlRoot = this.XamlRoot;
                            _logger?.LogInformation("TicketDetailsPage: Установлен XamlRoot для ViewModel");
                            Debug.WriteLine("TicketDetailsPage: Установлен XamlRoot для ViewModel");
                        }
                        
                        // Подписываемся на событие изменения статуса
                        ViewModel.StatusChanged += ViewModel_StatusChanged;
                        _logger?.LogInformation("TicketDetailsPage: Подписка на событие StatusChanged");
                        Debug.WriteLine("TicketDetailsPage: Подписка на событие StatusChanged");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "TicketDetailsPage: Ошибка при установке XamlRoot: {Error}", ex.Message);
                        Debug.WriteLine($"TicketDetailsPage: Ошибка при установке XamlRoot: {ex.Message}");
                    }
                }

                // Проверяем, что получили нужные параметры
                if (e.Parameter is Dictionary<string, object> parameters)
                {
                    _logger?.LogInformation("TicketDetailsPage: Получены параметры ({Count})", parameters.Count);
                    Debug.WriteLine($"TicketDetailsPage: Получены параметры ({parameters.Count})");
                    
                    foreach (var key in parameters.Keys)
                    {
                        _logger?.LogInformation("TicketDetailsPage: Параметр {Key} типа {Type}", 
                            key, parameters[key]?.GetType().Name ?? "null");
                        Debug.WriteLine($"TicketDetailsPage: Параметр {key} типа {parameters[key]?.GetType().Name ?? "null"}");
                    }
                    
                    if (parameters.TryGetValue("Schedule", out object scheduleObj) &&
                        parameters.TryGetValue("SelectedSeat", out object seatObj))
                    {
                        if (scheduleObj is Schedule schedule && seatObj is SeatViewModel selectedSeat)
                        {
                            _logger?.LogInformation("TicketDetailsPage: Инициализация с расписанием ID: {ScheduleId} и местом: ряд {Row}, место {Seat}",
                                schedule.Id, selectedSeat.RowNumber, selectedSeat.SeatNumber);
                            Debug.WriteLine($"TicketDetailsPage: Инициализация с расписанием ID: {schedule.Id} и местом: ряд {selectedSeat.RowNumber}, место {selectedSeat.SeatNumber}");
                            
                            // Инициализируем ViewModel с данными
                            await ViewModel.InitializeAsync(schedule, selectedSeat);
                        }
                        else
                        {
                            _logger?.LogWarning("TicketDetailsPage: Неверный тип параметров: Schedule = {ScheduleType}, SelectedSeat = {SeatType}", 
                                scheduleObj?.GetType().Name ?? "null", seatObj?.GetType().Name ?? "null");
                            Debug.WriteLine($"TicketDetailsPage: Неверный тип параметров: Schedule = {scheduleObj?.GetType().Name ?? "null"}, SelectedSeat = {seatObj?.GetType().Name ?? "null"}");
                            
                            ViewModel.ErrorMessage = "Ошибка: неверный тип параметров";
                        }
                    }
                    else
                    {
                        _logger?.LogWarning("TicketDetailsPage: Не найдены необходимые параметры");
                        Debug.WriteLine("TicketDetailsPage: Не найдены необходимые параметры");
                        
                        ViewModel.ErrorMessage = "Ошибка: отсутствуют необходимые параметры";
                    }
                }
                else
                {
                    _logger?.LogWarning("TicketDetailsPage: Параметры не являются словарем");
                    Debug.WriteLine("TicketDetailsPage: Параметры не являются словарем");
                    
                    ViewModel.ErrorMessage = "Ошибка: неверный формат параметров";
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при инициализации: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage ОШИБКА при инициализации: {ex.Message}");
                
                ViewModel.ErrorMessage = $"Ошибка при загрузке данных: {ex.Message}";
                
                if (ex.InnerException != null)
                {
                    _logger?.LogError(ex.InnerException, "TicketDetailsPage: Внутренняя ошибка: {ErrorMessage}", 
                        ex.InnerException.Message);
                    Debug.WriteLine($"TicketDetailsPage внутренняя ошибка: {ex.InnerException.Message}");
                }
            }
        }

        // Метод для переключения видимости поиска клиентов
        private void ToggleCustomerSearch(object sender, RoutedEventArgs e)
        {
            try
            {
                bool newValue = !ViewModel.IsCustomerSearchVisible;
                
                _logger?.LogInformation("TicketDetailsPage: Переключение видимости поиска клиентов с {Old} на {New}", 
                    ViewModel.IsCustomerSearchVisible, newValue);
                Debug.WriteLine($"ToggleCustomerSearch: текущее значение = {ViewModel.IsCustomerSearchVisible}, новое значение = {newValue}");
                
                // Устанавливаем новое значение
                ViewModel.IsCustomerSearchVisible = newValue;
                
                // Проверяем, что значение изменилось
                if (ViewModel.IsCustomerSearchVisible != newValue)
                {
                    _logger?.LogWarning("TicketDetailsPage: Значение IsCustomerSearchVisible не изменилось");
                    Debug.WriteLine("TicketDetailsPage: Значение IsCustomerSearchVisible не изменилось");
                }
                
                if (ViewModel.IsCustomerSearchVisible && ViewModel.CustomerSearchViewModel != null)
                {
                    // Если открываем поиск, инициализируем ViewModel для поиска
                    Task.Run(async () => await ViewModel.CustomerSearchViewModel.InitializeAsync());
                    _logger?.LogInformation("TicketDetailsPage: Инициализирован поиск клиентов");
                    Debug.WriteLine("TicketDetailsPage: Инициализирован поиск клиентов");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при переключении видимости поиска: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage ОШИБКА: {ex.Message}");
                
                ViewModel.ErrorMessage = $"Ошибка при открытии поиска клиентов: {ex.Message}";
            }
        }
        
        // Обработчик кнопки поиска клиента
        private void SearchCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("TicketDetailsPage: Нажата кнопка поиска клиента");
                Debug.WriteLine("TicketDetailsPage: Нажата кнопка поиска клиента");
                
                ToggleCustomerSearch(sender, e);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при нажатии кнопки поиска: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage ОШИБКА при нажатии кнопки поиска: {ex.Message}");
                
                ViewModel.ErrorMessage = $"Ошибка при поиске клиента: {ex.Message}";
            }
        }
        
        // Обработчик команды поиска клиента по нажатию на Enter
        private async void CustomerSearchTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    _logger?.LogInformation("TicketDetailsPage: Нажат Enter в поле поиска клиента");
                    Debug.WriteLine("TicketDetailsPage: Нажат Enter в поле поиска клиента");
                    
                    // Запускаем поиск клиента
                    if (ViewModel != null && !string.IsNullOrWhiteSpace(ViewModel.CustomerPhone))
                    {
                        // Выполняем поиск
                        await Task.Run(() => ViewModel.SearchCustomerCommand.Execute(null));
                        e.Handled = true;
                        
                        _logger?.LogInformation("TicketDetailsPage: Поиск клиента запущен");
                        Debug.WriteLine("TicketDetailsPage: Поиск клиента запущен");
                    }
                    else
                    {
                        _logger?.LogWarning("TicketDetailsPage: Невозможно выполнить поиск - пустой телефон");
                        Debug.WriteLine("TicketDetailsPage: Невозможно выполнить поиск - пустой телефон");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при нажатии Enter в поле поиска: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage ОШИБКА при нажатии Enter: {ex.Message}");
                
                ViewModel.ErrorMessage = $"Ошибка при поиске клиента: {ex.Message}";
            }
        }
        
        // Обработчик клика для выбора клиента
        private void SelectCustomerFromSearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("TicketDetailsPage: Нажата кнопка выбора клиента из поиска");
                Debug.WriteLine("TicketDetailsPage: Нажата кнопка выбора клиента из поиска");
                
                if (ViewModel?.CustomerSearchViewModel?.SelectedCustomer != null)
                {
                    // Фиксируем выбранного клиента
                    var selectedCustomer = ViewModel.CustomerSearchViewModel.SelectedCustomer;
                    
                    // Выбираем клиента и закрываем форму поиска
                    ViewModel.SelectCustomerCommand.Execute(selectedCustomer);
                    
                    _logger?.LogInformation("TicketDetailsPage: Выбран клиент: {CustomerName} ({CustomerId})", 
                        selectedCustomer.FullName, selectedCustomer.Id);
                    Debug.WriteLine($"TicketDetailsPage: Выбран клиент: {selectedCustomer.FullName} ({selectedCustomer.Id})");
                }
                else
                {
                    _logger?.LogWarning("TicketDetailsPage: Попытка выбрать клиента, когда ни один не выбран");
                    Debug.WriteLine("TicketDetailsPage: Попытка выбрать клиента, когда ни один не выбран");
                    
                    ViewModel.ErrorMessage = "Необходимо выбрать клиента из списка";
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при выборе клиента: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage ОШИБКА при выборе клиента: {ex.Message}");
                
                ViewModel.ErrorMessage = $"Ошибка при выборе клиента: {ex.Message}";
            }
        }
        
        // Обработчик создания нового клиента
        private void CreateNewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("TicketDetailsPage: Нажата кнопка создания нового клиента");
                Debug.WriteLine("TicketDetailsPage: Нажата кнопка создания нового клиента");
                
                ViewModel.CustomerSearchViewModel.ShowCreateNewCustomer = true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при нажатии на кнопку создания клиента: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage ОШИБКА: {ex.Message}");
                ViewModel.ErrorMessage = $"Ошибка при открытии формы создания клиента: {ex.Message}";
            }
        }
        
        // Обработчик для кнопки сохранения нового клиента
        private async void SaveNewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("TicketDetailsPage: Нажата кнопка сохранения нового клиента");
                Debug.WriteLine("TicketDetailsPage: Нажата кнопка сохранения нового клиента");
                
                // Установка флага загрузки
                ViewModel.IsLoading = true;
                
                // Получаем доступ к полям формы и вручную обновляем значения во ViewModel
                var fullNameTextBox = FindName("FullNameTextBox") as TextBox;
                var phoneTextBox = FindName("PhoneTextBox") as TextBox;
                var emailTextBox = FindName("EmailTextBox") as TextBox;
                
                if (fullNameTextBox != null && phoneTextBox != null && emailTextBox != null)
                {
                    // Логируем значения, которые считываем с полей
                    string fullName = fullNameTextBox.Text?.Trim() ?? string.Empty;
                    string phone = phoneTextBox.Text?.Trim() ?? string.Empty;
                    string email = emailTextBox.Text?.Trim() ?? string.Empty;
                    
                    _logger?.LogInformation("TicketDetailsPage: Считаны значения из полей - ФИО: [{FullName}], Телефон: [{Phone}], Email: [{Email}]",
                        fullName, phone, email);
                    
                    // Принудительно обновляем значения во ViewModel
                    ViewModel.CustomerSearchViewModel.NewCustomerFullName = fullName;
                    ViewModel.CustomerSearchViewModel.NewCustomerPhone = phone;
                    ViewModel.CustomerSearchViewModel.NewCustomerEmail = email;
                }
                
                // Проверяем заполнение обязательных полей
                if (string.IsNullOrWhiteSpace(ViewModel.CustomerSearchViewModel.NewCustomerFullName))
                {
                    _logger?.LogWarning("TicketDetailsPage: Отсутствует обязательное поле ФИО");
                    ViewModel.ErrorMessage = "Необходимо указать ФИО клиента";
                    ViewModel.IsLoading = false;
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(ViewModel.CustomerSearchViewModel.NewCustomerPhone))
                {
                    _logger?.LogWarning("TicketDetailsPage: Отсутствует обязательное поле Телефон");
                    ViewModel.ErrorMessage = "Необходимо указать телефон клиента";
                    ViewModel.IsLoading = false;
                    return;
                }
                
                // Логируем значения полей перед сохранением
                _logger?.LogInformation("TicketDetailsPage: Перед созданием клиента - ФИО: [{FullName}], Телефон: [{Phone}], Email: [{Email}]",
                    ViewModel.CustomerSearchViewModel.NewCustomerFullName,
                    ViewModel.CustomerSearchViewModel.NewCustomerPhone,
                    ViewModel.CustomerSearchViewModel.NewCustomerEmail);
                
                // Создаем нового клиента
                await ViewModel.CustomerSearchViewModel.CreateNewCustomerAsync();
                
                // Если нет ошибок после создания клиента
                if (string.IsNullOrEmpty(ViewModel.CustomerSearchViewModel.ErrorMessage))
                {
                    // Получаем созданного клиента
                    Customer? createdCustomer = ViewModel.CustomerSearchViewModel.CreatedCustomer;
                    
                    if (createdCustomer != null)
                    {
                        _logger?.LogInformation("TicketDetailsPage: Клиент создан, ID: {CustomerId}", 
                            createdCustomer.Id);
                        
                        // Выбираем созданного клиента
                        ViewModel.SelectedCustomer = createdCustomer;
                        
                        // Закрываем форму создания
                        ViewModel.CustomerSearchViewModel.ShowCreateNewCustomer = false;
                        
                        // Закрываем поиск
                        ViewModel.IsCustomerSearchVisible = false;
                    }
                    else
                    {
                        _logger?.LogWarning("TicketDetailsPage: Клиент создан, но объект клиента не получен");
                        ViewModel.ErrorMessage = "Клиент создан, но не выбран автоматически. Пожалуйста, выберите клиента из списка.";
                    }
                }
                else
                {
                    _logger?.LogWarning("TicketDetailsPage: Ошибка при создании клиента: {ErrorMessage}", 
                        ViewModel.CustomerSearchViewModel.ErrorMessage);
                    ViewModel.ErrorMessage = ViewModel.CustomerSearchViewModel.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при сохранении нового клиента: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage ОШИБКА: {ex.Message}");
                ViewModel.ErrorMessage = $"Ошибка при создании клиента: {ex.Message}";
            }
            finally
            {
                ViewModel.IsLoading = false;
            }
        }
        
        // Обработчик для кнопки отмены создания клиента
        private void CancelCreateCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("TicketDetailsPage: Нажата кнопка отмены создания клиента");
                Debug.WriteLine("TicketDetailsPage: Нажата кнопка отмены создания клиента");
                
                // Закрываем форму создания
                ViewModel.CustomerSearchViewModel.ShowCreateNewCustomer = false;
                
                // Очищаем поля формы
                ViewModel.CustomerSearchViewModel.ResetNewCustomerForm();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при отмене создания клиента: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage ОШИБКА: {ex.Message}");
                ViewModel.ErrorMessage = $"Ошибка при отмене создания клиента: {ex.Message}";
            }
        }

        // Обработчик для изменения выбора типа документа
        private void DocumentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                if (comboBox != null && comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string documentType = selectedItem.Tag?.ToString() ?? string.Empty;
                    _logger?.LogInformation($"TicketDetailsPage: Выбран тип документа: {documentType}");
                    
                    // Присваиваем значение Tag в качестве типа документа
                    ViewModel.CustomerSearchViewModel.NewCustomerDocumentType = documentType;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при изменении типа документа: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage ОШИБКА при изменении типа документа: {ex.Message}");
            }
        }

        // Добавим метод для отображения визуального статуса продажи билета
        public void ShowTicketStatus(string message, bool isSuccess)
        {
            try
            {
                // Предполагаем, что в XAML-разметке страницы есть TextBlock с именем StatusTextBlock
                var statusTextBlock = FindName("StatusTextBlock") as TextBlock;
                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = message;
                    statusTextBlock.Foreground = new SolidColorBrush(
                        isSuccess ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
                    statusTextBlock.Visibility = Visibility.Visible;
                    
                    _logger?.LogInformation($"TicketDetailsPage: Отображен статус: {message}");
                    Debug.WriteLine($"TicketDetailsPage: Отображен статус: {message}");
                }
                else
                {
                    _logger?.LogWarning("TicketDetailsPage: StatusTextBlock не найден в разметке");
                    Debug.WriteLine("TicketDetailsPage: StatusTextBlock не найден в разметке");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при отображении статуса: {Error}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage: Ошибка при отображении статуса: {ex.Message}");
            }
        }

        // Обработчик события изменения статуса
        private void ViewModel_StatusChanged(object sender, TicketDetailsViewModel.StatusEventArgs e)
        {
            try
            {
                _logger?.LogInformation("TicketDetailsPage: Получено событие StatusChanged: {Message}, IsSuccess: {IsSuccess}", 
                    e.Message, e.IsSuccess);
                Debug.WriteLine($"TicketDetailsPage: Получено событие StatusChanged: {e.Message}, IsSuccess: {e.IsSuccess}");
                
                // Обновляем статус на UI
                ShowTicketStatus(e.Message, e.IsSuccess);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при обработке события StatusChanged: {Error}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage: Ошибка при обработке события StatusChanged: {ex.Message}");
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            
            try
            {
                // Отписываемся от событий при уходе со страницы
                if (ViewModel != null)
                {
                    ViewModel.StatusChanged -= ViewModel_StatusChanged;
                    _logger?.LogInformation("TicketDetailsPage: Отписка от события StatusChanged");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "TicketDetailsPage: Ошибка при отписке от событий: {Error}", ex.Message);
                Debug.WriteLine($"TicketDetailsPage: Ошибка при отписке от событий: {ex.Message}");
            }
        }
    }
} 