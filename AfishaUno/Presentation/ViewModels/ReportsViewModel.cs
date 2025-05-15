using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AfishaUno.Models;
using AfishaUno.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using WinRT.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AfishaUno.Presentation.ViewModels
{
    /// <summary>
    /// ViewModel для страницы отчетов
    /// </summary>
    public partial class ReportsViewModel : ObservableObject
    {
        private readonly IReportService _reportService;
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private int _selectedReportTypeIndex;

        [ObservableProperty]
        private DateTimeOffset _startDate;

        [ObservableProperty]
        private DateTimeOffset _endDate;

        [ObservableProperty]
        private string _reportTitle;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private bool _isStatusMessageVisible;

        [ObservableProperty]
        private SolidColorBrush _statusMessageColor;

        [ObservableProperty]
        private Report _selectedReport;

        [ObservableProperty]
        private ObservableCollection<Report> _savedReports;

        [ObservableProperty]
        private bool _isGenerating;

        /// <summary>
        /// Индикатор возможности создания отчета
        /// </summary>
        public bool CanGenerateReport => !IsGenerating && StartDate < EndDate;

        partial void OnSelectedReportTypeIndexChanged(int value)
        {
            // Сбрасываем заголовок отчета при изменении типа
            ReportTitle = string.Empty;
        }

        partial void OnIsGeneratingChanged(bool value) => OnPropertyChanged(nameof(CanGenerateReport));
        partial void OnStartDateChanged(DateTimeOffset value) => OnPropertyChanged(nameof(CanGenerateReport));
        partial void OnEndDateChanged(DateTimeOffset value) => OnPropertyChanged(nameof(CanGenerateReport));
        partial void OnStatusMessageChanged(string value) => IsStatusMessageVisible = !string.IsNullOrEmpty(value);

        public ReportsViewModel(
            IReportService reportService,
            ISupabaseService supabaseService,
            INavigationService navigationService)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _supabaseService = supabaseService ?? throw new ArgumentNullException(nameof(supabaseService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // Инициализация свойств
            _selectedReportTypeIndex = 0;
            _startDate = DateTimeOffset.Now.AddDays(-30);
            _endDate = DateTimeOffset.Now;
            _reportTitle = string.Empty;
            _statusMessage = string.Empty;
            _isStatusMessageVisible = false;
            _statusMessageColor = new SolidColorBrush(Colors.Black);
            _savedReports = new ObservableCollection<Report>();
            _isGenerating = false;

            // Инициализация команд
            GenerateReportCommand = new AsyncRelayCommand(GenerateReportAsync, () => CanGenerateReport);
            GenerateAndSaveReportCommand = new AsyncRelayCommand(GenerateAndSaveReportAsync, () => CanGenerateReport);
            RefreshSavedReportsCommand = new AsyncRelayCommand(LoadSavedReportsAsync);
            OpenSavedReportCommand = new AsyncRelayCommand<string>(OpenSavedReportAsync);
            DeleteSavedReportCommand = new AsyncRelayCommand<string>(DeleteSavedReportAsync);
        }

        /// <summary>
        /// Команда для генерации отчета
        /// </summary>
        public IAsyncRelayCommand GenerateReportCommand { get; }

        /// <summary>
        /// Команда для генерации и сохранения отчета
        /// </summary>
        public IAsyncRelayCommand GenerateAndSaveReportCommand { get; }

        /// <summary>
        /// Команда для обновления списка сохраненных отчетов
        /// </summary>
        public IAsyncRelayCommand RefreshSavedReportsCommand { get; }

        /// <summary>
        /// Команда для открытия сохраненного отчета
        /// </summary>
        public IAsyncRelayCommand<string> OpenSavedReportCommand { get; }

        /// <summary>
        /// Команда для удаления сохраненного отчета
        /// </summary>
        public IAsyncRelayCommand<string> DeleteSavedReportCommand { get; }

        /// <summary>
        /// Генерация отчета без сохранения в базу данных
        /// </summary>
        private async Task GenerateReportAsync()
        {
            try
            {
                IsGenerating = true;

                SetStatus("Формирование отчета...", Colors.Blue);

                byte[] reportData = await GenerateReportDataAsync();
                if (reportData == null)
                {
                    SetStatus("Не удалось сформировать отчет. Проверьте параметры и попробуйте снова.", Colors.Red);
                    return;
                }

                await SaveReportToFileAsync(reportData);
                SetStatus("Отчет успешно сформирован и сохранен на диск.", Colors.Green);
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка при формировании отчета: {ex.Message}", Colors.Red);
            }
            finally
            {
                IsGenerating = false;
            }
        }

        /// <summary>
        /// Генерация и сохранение отчета в базу данных
        /// </summary>
        private async Task GenerateAndSaveReportAsync()
        {
            try
            {
                IsGenerating = true;

                SetStatus("Формирование и сохранение отчета...", Colors.Blue);

                // Генерация данных отчета
                byte[] reportData = await GenerateReportDataAsync();
                if (reportData == null)
                {
                    SetStatus("Не удалось сформировать отчет. Проверьте параметры и попробуйте снова.", Colors.Red);
                    return;
                }

                // Формирование параметров отчета в JSON
                var reportParameters = new
                {
                    start_date = StartDate.Date.ToString("yyyy-MM-dd"),
                    end_date = EndDate.Date.ToString("yyyy-MM-dd")
                };

                // Создание объекта для сохранения данных
                string title = !string.IsNullOrEmpty(ReportTitle)
                    ? ReportTitle
                    : GetDefaultReportTitle();

                string reportType = SelectedReportTypeIndex == 0 ? "sales" : "attendance";
                string parametersJson = JsonSerializer.Serialize(reportParameters);
                string dataJson = Convert.ToBase64String(reportData); // Преобразуем PDF в base64
                string userId = _supabaseService.CurrentUser?.Id.ToString();

                if (string.IsNullOrEmpty(userId))
                {
                    SetStatus("Не удалось сохранить отчет: пользователь не авторизован.", Colors.Red);
                    return;
                }

                // Сохранение отчета в базу данных
                await _reportService.SaveReportAsync(title, reportType, parametersJson, dataJson, userId);

                // Обновление списка отчетов
                await LoadSavedReportsAsync();

                // Сохранение на диск
                await SaveReportToFileAsync(reportData);

                SetStatus("Отчет успешно сформирован и сохранен.", Colors.Green);
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка при формировании и сохранении отчета: {ex.Message}", Colors.Red);
            }
            finally
            {
                IsGenerating = false;
            }
        }

        /// <summary>
        /// Открытие сохраненного отчета по его ID
        /// </summary>
        private async Task OpenSavedReportAsync(string reportId)
        {
            if (string.IsNullOrEmpty(reportId))
                return;

            try
            {
                SetStatus("Загрузка отчета...", Colors.Blue);

                // Получение текущего пользователя
                var currentUser = _supabaseService.CurrentUser;
                if (currentUser == null)
                {
                    SetStatus("Не удалось загрузить отчет: пользователь не авторизован.", Colors.Red);
                    return;
                }

                // Генерация PDF на основе сохраненных данных
                byte[] reportData = await _reportService.GenerateSavedReportAsync(reportId, currentUser);
                if (reportData == null || reportData.Length == 0)
                {
                    SetStatus("Не удалось загрузить отчет. Возможно, он был удален или повреждён.", Colors.Red);
                    return;
                }

                // Сохранение PDF на диск
                await SaveReportToFileAsync(reportData);

                SetStatus("Отчет успешно загружен и сохранен на диск.", Colors.Green);
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка при загрузке отчета: {ex.Message}", Colors.Red);
            }
        }

        /// <summary>
        /// Удаление сохраненного отчета
        /// </summary>
        private async Task DeleteSavedReportAsync(string reportId)
        {
            if (string.IsNullOrEmpty(reportId))
                return;

            try
            {
                // Запрос подтверждения
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Удаление отчета",
                    Content = "Вы уверены, что хотите удалить этот отчет? Это действие нельзя отменить.",
                    PrimaryButtonText = "Удалить",
                    CloseButtonText = "Отмена",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = App.Current.MainWindow2.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                    return;

                // Удаление отчета из базы данных
                await _supabaseService.DeleteReportAsync(reportId);

                // Обновление списка отчетов
                await LoadSavedReportsAsync();

                SetStatus("Отчет успешно удален.", Colors.Green);
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка при удалении отчета: {ex.Message}", Colors.Red);
            }
        }

        /// <summary>
        /// Загрузка списка сохраненных отчетов
        /// </summary>
        public async Task LoadSavedReportsAsync()
        {
            try
            {
                var reports = await _reportService.GetSavedReportsAsync();
                SavedReports = new ObservableCollection<Report>(reports.OrderByDescending(r => r.CreatedAt));
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка при загрузке списка отчетов: {ex.Message}", Colors.Red);
            }
        }

        /// <summary>
        /// Генерация данных отчета в зависимости от выбранного типа
        /// </summary>
        private async Task<byte[]> GenerateReportDataAsync()
        {
            try
            {
                // Получение текущего пользователя
                var currentUser = _supabaseService.CurrentUser;
                if (currentUser == null)
                    throw new InvalidOperationException("Пользователь не авторизован");

                // Преобразование DateTimeOffset в DateTime для запросов к базе данных
                DateTime startDate = StartDate.Date;
                DateTime endDate = EndDate.Date.AddDays(1).AddTicks(-1); // Конец дня

                // Генерация соответствующего отчета
                if (SelectedReportTypeIndex == 0) // Отчет о продажах
                {
                    return await _reportService.GenerateSalesReportAsync(startDate, endDate, currentUser);
                }
                else // Отчет о посещаемости
                {
                    return await _reportService.GeneratePerformanceReportAsync(startDate, endDate, currentUser);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка при формировании отчета: {ex.Message}", Colors.Red);
                return null;
            }
        }

        /// <summary>
        /// Сохранение отчета в файл
        /// </summary>
        private async Task SaveReportToFileAsync(byte[] reportData)
        {
            try
            {
                // Создание диалога сохранения файла
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("PDF документ", new List<string>() { ".pdf" });
                savePicker.SuggestedFileName = GetDefaultFileName();

                // Получение дескриптора для WinUI 3
                nint hwnd = App.Current.MainWindowHandle;

                // Инициализация FileSavePicker для WinUI 3
                InitializeWithWindow.Initialize(savePicker, hwnd);

                // Получение файла для сохранения
                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Запись данных в файл
                    CachedFileManager.DeferUpdates(file);
                    await FileIO.WriteBytesAsync(file, reportData);
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                    if (status == FileUpdateStatus.Complete)
                    {
                        SetStatus($"Отчет сохранен в файл: {file.Path}", Colors.Green);
                    }
                    else
                    {
                        SetStatus("Не удалось сохранить отчет в файл.", Colors.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка при сохранении файла: {ex.Message}", Colors.Red);
            }
        }

        /// <summary>
        /// Установка статусного сообщения
        /// </summary>
        private void SetStatus(string message, Windows.UI.Color color)
        {
            StatusMessage = message;
            StatusMessageColor = new SolidColorBrush(color);
        }

        /// <summary>
        /// Получение стандартного имени файла для отчета
        /// </summary>
        private string GetDefaultFileName()
        {
            string reportTypeText = SelectedReportTypeIndex == 0 ? "Sales" : "Attendance";
            string dateRange = $"{StartDate:yyyyMMdd}-{EndDate:yyyyMMdd}";
            return $"Report_{reportTypeText}_{dateRange}.pdf";
        }

        /// <summary>
        /// Получение стандартного заголовка отчета
        /// </summary>
        private string GetDefaultReportTitle()
        {
            string reportType = SelectedReportTypeIndex == 0 ? "Отчет о продажах" : "Отчет о посещаемости";
            return $"{reportType} за период {StartDate:dd.MM.yyyy}-{EndDate:dd.MM.yyyy}";
        }
    }
} 