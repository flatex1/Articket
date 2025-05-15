using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Extensions.Logging;
using AfishaUno.Presentation.ViewModels;
using AfishaUno.Services;
using AfishaUno.Models;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;

namespace AfishaUno.Presentation.Pages
{
    public sealed partial class CustomerSearchPage : Page
    {
        public CustomerSearchViewModel ViewModel { get; }
        
        private readonly ILogger<CustomerSearchPage> _logger;
        private readonly INavigationService _navigationService;
        private readonly CustomerSelectionManager _customerSelectionManager;
        
        public CustomerSearchPage()
        {
            this.InitializeComponent();
            
            // Получаем сервисы
            _navigationService = App.Current.Services.GetService<INavigationService>();
            _logger = App.Current.Services.GetService<ILogger<CustomerSearchPage>>();
            _customerSelectionManager = App.Current.Services.GetService<CustomerSelectionManager>();
            
            _logger?.LogInformation("CustomerSearchPage: Начало инициализации страницы");
            
            try
            {
                // Создаем и инициализируем ViewModel
                ViewModel = App.Current.Services.GetService<CustomerSearchViewModel>();
                
                if (ViewModel == null)
                {
                    _logger?.LogError("CustomerSearchPage: Не удалось получить CustomerSearchViewModel из DI");
                    ViewModel = new CustomerSearchViewModel(
                        App.Current.Services.GetService<ISupabaseService>(),
                        App.Current.Services.GetService<ILogger<CustomerSearchViewModel>>()
                    );
                }
                
                // Загружаем начальные данные
                Task.Run(async () => await ViewModel.InitializeAsync());
                
                _logger?.LogInformation("CustomerSearchPage: Страница успешно инициализирована");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CustomerSearchPage: Ошибка при инициализации страницы: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"ОШИБКА ИНИЦИАЛИЗАЦИИ: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    _logger?.LogError(ex.InnerException, "Внутренняя ошибка: {ErrorMessage}", 
                        ex.InnerException.Message);
                }
            }
        }
        
        // Обработчик нажатия Enter в поле поиска
        private async void SearchTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    _logger?.LogInformation("CustomerSearchPage: Нажат Enter в поле поиска");
                    Debug.WriteLine("Нажата клавиша Enter в поле поиска");
                    
                    // Выполняем поиск
                    await ViewModel.SearchCustomersAsync();
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CustomerSearchPage: Ошибка при обработке нажатия Enter: {ErrorMessage}", ex.Message);
                ViewModel.ErrorMessage = $"Ошибка при поиске: {ex.Message}";
                Debug.WriteLine($"ОШИБКА при нажатии Enter: {ex.Message}");
            }
        }
        
        // Обработчик нажатия кнопки поиска
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("CustomerSearchPage: Нажата кнопка поиска");
                Debug.WriteLine("Нажата кнопка поиска");
                
                // Показываем индикатор загрузки
                ViewModel.IsLoading = true;
                
                // Закрываем форму создания нового клиента, если она открыта
                ViewModel.ShowCreateNewCustomer = false;
                
                // Выполняем поиск
                await ViewModel.SearchCustomersAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CustomerSearchPage: Ошибка при нажатии кнопки поиска: {ErrorMessage}", ex.Message);
                ViewModel.ErrorMessage = $"Ошибка при поиске: {ex.Message}";
                Debug.WriteLine($"ОШИБКА при нажатии кнопки поиска: {ex.Message}");
            }
            finally
            {
                ViewModel.IsLoading = false;
            }
        }
        
        // Обработчик нажатия кнопки создания нового клиента
        private void CreateNewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("CustomerSearchPage: Нажата кнопка создания нового клиента");
                Debug.WriteLine("Нажата кнопка создания нового клиента");
                
                // Открываем форму создания нового клиента
                ViewModel.ShowCreateNewCustomer = true;
                
                // Обнуляем текущий выбор клиента
                ViewModel.SelectedCustomer = null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CustomerSearchPage: Ошибка при нажатии кнопки создания клиента: {ErrorMessage}", ex.Message);
                ViewModel.ErrorMessage = $"Ошибка при открытии формы создания: {ex.Message}";
                Debug.WriteLine($"ОШИБКА при открытии формы создания: {ex.Message}");
            }
        }
        
        // Обработчик нажатия кнопки Отмена при создании клиента
        private void CancelCreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("CustomerSearchPage: Нажата кнопка отмены создания клиента");
                Debug.WriteLine("Нажата кнопка отмены создания клиента");
                
                // Закрываем форму создания нового клиента
                ViewModel.ShowCreateNewCustomer = false;
                
                // Очищаем поля формы
                ViewModel.ResetNewCustomerForm();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CustomerSearchPage: Ошибка при нажатии кнопки отмены создания: {ErrorMessage}", ex.Message);
                ViewModel.ErrorMessage = $"Ошибка при отмене создания: {ex.Message}";
                Debug.WriteLine($"ОШИБКА при нажатии кнопки отмены создания: {ex.Message}");
            }
        }
        
        // Обработчик нажатия кнопки Отмена
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("CustomerSearchPage: Нажата кнопка Отмена");
                Debug.WriteLine("Нажата кнопка Отмена");
                
                // Проверяем, инициализирован ли сервис навигации
                if (_navigationService == null)
                {
                    _logger?.LogWarning("CustomerSearchPage: NavigationService не инициализирован, используем Frame.GoBack()");
                    // Возвращаемся на предыдущую страницу через Frame
                    if (this.Frame != null && this.Frame.CanGoBack)
                    {
                        this.Frame.GoBack();
                    }
                    else
                    {
                        _logger?.LogWarning("CustomerSearchPage: Невозможно вернуться назад через Frame");
                        throw new InvalidOperationException("Невозможно вернуться назад");
                    }
                }
                else
                {
                    // Возвращаемся на предыдущую страницу через сервис
                    _navigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CustomerSearchPage: Ошибка при нажатии кнопки отмены: {ErrorMessage}", ex.Message);
                ViewModel.ErrorMessage = $"Ошибка при отмене: {ex.Message}";
                Debug.WriteLine($"ОШИБКА при нажатии кнопки отмены: {ex.Message}");
            }
        }
        
        // Обработчик нажатия кнопки создания клиента
        private async void SaveCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("CustomerSearchPage: Нажата кнопка сохранения клиента");
                Debug.WriteLine("Нажата кнопка сохранения нового клиента");
                
                // Показываем индикатор загрузки
                ViewModel.IsLoading = true;
                
                // Метод прямого сохранения данных, минуя механизм привязки
                if (!UpdateViewModelDirectly())
                {
                    ViewModel.IsLoading = false;
                    return;
                }
                
                // Логируем значения полей перед сохранением
                _logger?.LogInformation($"Данные для сохранения: ФИО=[{ViewModel.NewCustomerFullName}], Телефон=[{ViewModel.NewCustomerPhone}]");
                
                // Создаем нового клиента
                await ViewModel.CreateNewCustomerAsync();
                
                // Очищаем поля формы после успешного создания
                if (string.IsNullOrEmpty(ViewModel.ErrorMessage))
                {
                    // Сбрасываем форму
                    ViewModel.ResetNewCustomerForm();
                    
                    // Закрываем форму создания
                    ViewModel.ShowCreateNewCustomer = false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CustomerSearchPage: Ошибка при нажатии кнопки сохранения клиента: {ErrorMessage}", ex.Message);
                ViewModel.ErrorMessage = $"Ошибка при создании клиента: {ex.Message}";
                Debug.WriteLine($"ОШИБКА при создании клиента: {ex.Message}");
            }
            finally
            {
                ViewModel.IsLoading = false;
            }
        }
        
        /// <summary>
        /// Метод для прямого обновления полей ViewModel данными из UI-элементов
        /// </summary>
        private bool UpdateViewModelDirectly()
        {
            try
            {
                // Найдем и получим значения напрямую из UI-элементов по имени
                string fullName = string.Empty;
                string phone = string.Empty;
                string email = string.Empty;
                string documentType = string.Empty;
                string documentNumber = string.Empty;
                string notes = string.Empty;

                // Поиск полей по имени
                var fullNameTextBox = FindName("FullNameTextBox") as TextBox;
                var phoneTextBox = FindName("PhoneTextBox") as TextBox;
                var emailTextBox = FindName("EmailTextBox") as TextBox;
                var documentTypeTextBox = FindName("DocumentTypeTextBox") as TextBox;
                var documentNumberTextBox = FindName("DocumentNumberTextBox") as TextBox;
                var notesTextBox = FindName("NotesTextBox") as TextBox;
                
                // Получение значений
                if (fullNameTextBox != null) fullName = fullNameTextBox.Text;
                if (phoneTextBox != null) phone = phoneTextBox.Text;
                if (emailTextBox != null) email = emailTextBox.Text;
                if (documentTypeTextBox != null) documentType = documentTypeTextBox.Text;
                if (documentNumberTextBox != null) documentNumber = documentNumberTextBox.Text;
                if (notesTextBox != null) notes = notesTextBox.Text;
                
                // Логирование полученных значений
                _logger?.LogInformation($"Получены значения из полей UI: ФИО=[{fullName}], Телефон=[{phone}]");
                
                // Явное присваивание значений во ViewModel
                ViewModel.NewCustomerFullName = fullName?.Trim() ?? string.Empty;
                ViewModel.NewCustomerPhone = phone?.Trim() ?? string.Empty;
                ViewModel.NewCustomerEmail = email?.Trim() ?? string.Empty;
                ViewModel.NewCustomerDocumentType = documentType?.Trim() ?? string.Empty;
                ViewModel.NewCustomerDocumentNumber = documentNumber?.Trim() ?? string.Empty;
                ViewModel.NewCustomerNotes = notes?.Trim() ?? string.Empty;
                
                // Проверка обязательных полей
                if (string.IsNullOrWhiteSpace(ViewModel.NewCustomerFullName))
                {
                    ViewModel.ErrorMessage = "Необходимо указать ФИО клиента";
                    _logger?.LogWarning("CustomerSearchPage: Попытка создать клиента без ФИО");
                    return false;
                }
                
                if (string.IsNullOrWhiteSpace(ViewModel.NewCustomerPhone))
                {
                    ViewModel.ErrorMessage = "Необходимо указать телефон клиента";
                    _logger?.LogWarning("CustomerSearchPage: Попытка создать клиента без телефона");
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка при прямом обновлении полей ViewModel: {ErrorMessage}", ex.Message);
                ViewModel.ErrorMessage = $"Ошибка при сборе данных формы: {ex.Message}";
                return false;
            }
        }
        
        /// <summary>
        /// Вспомогательный метод для поиска всех дочерних элементов указанного типа
        /// </summary>
        private IEnumerable<T> FindChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                yield break;
            
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T childOfType)
                    yield return childOfType;
                
                foreach (var descendant in FindChildren<T>(child))
                    yield return descendant;
            }
        }
        
        // Обработчик нажатия кнопки выбора клиента
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("CustomerSearchPage: Нажата кнопка выбора клиента");
                Debug.WriteLine("Нажата кнопка выбора клиента");
                
                if (ViewModel.SelectedCustomer != null)
                {
                    // Сохраняем выбранного клиента
                    var selectedCustomer = ViewModel.SelectedCustomer;
                    
                    // Устанавливаем выбранного клиента в менеджер
                    if (_customerSelectionManager != null)
                    {
                        _customerSelectionManager.SelectedCustomer = selectedCustomer;
                        _logger?.LogInformation("CustomerSearchPage: Клиент сохранен в CustomerSelectionManager");
                    }
                    else
                    {
                        _logger?.LogWarning("CustomerSearchPage: CustomerSelectionManager не инициализирован, сохранение клиента невозможно");
                    }
                    
                    // Возвращаемся назад
                    if (_navigationService != null)
                    {
                        _navigationService.GoBack();
                    }
                    else if (this.Frame != null && this.Frame.CanGoBack)
                    {
                        this.Frame.GoBack();
                    }
                    else
                    {
                        _logger?.LogWarning("CustomerSearchPage: Невозможно вернуться назад после выбора клиента");
                        throw new InvalidOperationException("Невозможно вернуться назад");
                    }
                }
                else
                {
                    ViewModel.ErrorMessage = "Необходимо выбрать клиента из списка";
                    _logger?.LogWarning("CustomerSearchPage: Попытка выбрать клиента, когда ни один не выбран");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CustomerSearchPage: Ошибка при нажатии кнопки выбора клиента: {ErrorMessage}", ex.Message);
                ViewModel.ErrorMessage = $"Ошибка при выборе клиента: {ex.Message}";
                Debug.WriteLine($"ОШИБКА при выборе клиента: {ex.Message}");
            }
        }
        
        // Обработчик события потери фокуса для поля ФИО
        private void CustomerFullName_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    // Принудительно обновляем значение во ViewModel
                    ViewModel.NewCustomerFullName = textBox.Text?.Trim() ?? string.Empty;
                    _logger?.LogInformation($"CustomerSearchPage: Поле ФИО потеряло фокус, значение: [{ViewModel.NewCustomerFullName}]");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CustomerSearchPage: Ошибка в обработчике потери фокуса поля ФИО: {ErrorMessage}", ex.Message);
                Debug.WriteLine($"ОШИБКА в обработчике потери фокуса: {ex.Message}");
            }
        }
    }
} 