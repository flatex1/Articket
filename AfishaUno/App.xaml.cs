using AfishaUno.Presentation.Pages;
using AfishaUno.Presentation.ViewModels;
using AfishaUno.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Extensions.Hosting;
using AfishaUno.Models;
using System;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace AfishaUno;
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        this.RequestedTheme = ApplicationTheme.Light;
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    public Window MainWindow2 => _window;
    
    /// <summary>
    /// Получает дескриптор основного окна приложения для использования с инициализацией диалогов WinUI
    /// </summary>
    public IntPtr MainWindowHandle
    {
        get
        {
            if (_window == null)
                return IntPtr.Zero;
            
            var windowHandle = WindowNative.GetWindowHandle(_window);
            return windowHandle;
        }
    }

    #region Fields

    private Window? _window;
    private Frame? _rootFrame;
    private IServiceProvider? _serviceProvider;
    private IConfiguration? _configuration;

    /// <summary>
    /// Gets the current app as a strongly typed reference.
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    public IServiceProvider Services
    {
        get
        {
            if (_serviceProvider == null)
            {
                ConfigureServices();
            }
            return _serviceProvider!;
        }
    }

    #endregion

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private void ConfigureServices()
    {
        // Загрузка конфигурации
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true);

        _configuration = builder.Build();

        var services = new ServiceCollection();

        // Регистрация сервисов
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ISupabaseService, SupabaseService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<ITicketPrintService, TicketPrintService>();
        services.AddSingleton<IReportService, ReportService>();

        // Добавляем логирование
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // Регистрация ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SelectSeatViewModel>();
        services.AddTransient<AddPerformanceViewModel>();
        services.AddTransient<AddScheduleViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<ScheduleViewModel>();
        services.AddTransient<TicketDetailsViewModel>();
        services.AddTransient<CustomerSearchViewModel>();
        services.AddTransient<ReportsViewModel>();
        
        // Регистрация статических сервисов для работы с выбранными объектами
        services.AddSingleton<CustomerSelectionManager>();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.
    /// </summary>
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        // Инициализируем сервисы, если они еще не инициализированы
        if (_serviceProvider == null)
        {
            ConfigureServices();
        }

        // Конфигурация маршрутов навигации
        ConfigureNavigation(_serviceProvider!);
        
        // Создаем окно
        _window = new Window();
        
        // Создаем или получаем корневой фрейм
        _rootFrame = _window.Content as Frame;
        
        // Если у нас еще нет фрейма, создаем его
        if (_rootFrame == null)
        {
            _rootFrame = new Frame();
            _window.Content = _rootFrame;
        }

        // Настраиваем навигационный сервис
        var navigationService = Services.GetService<INavigationService>();
        if (navigationService != null)
        {
            navigationService.Frame = _rootFrame;
        }

        // Активируем окно перед навигацией для предотвращения проблем с ForegroundWindow
        _window.Activate();

        // Навигация на стартовую страницу
        if (_rootFrame.Content == null)
        {
            // Переходим на страницу входа
            _rootFrame.Navigate(typeof(LoginPage));
        }
    }

    private void ConfigureNavigation(IServiceProvider serviceProvider)
    {
        var navigationService = serviceProvider.GetService<INavigationService>();
        if (navigationService == null) return;

        // Регистрируем типы страниц
        navigationService.Configure("LoginPage", typeof(LoginPage));
        navigationService.Configure("MainPage", typeof(MainPage));
        navigationService.Configure("HomePage", typeof(HomePage));
        navigationService.Configure("SchedulePage", typeof(SchedulePage));
        navigationService.Configure("SelectSeatPage", typeof(SelectSeatPage));
        navigationService.Configure("AddSchedulePage", typeof(AddSchedulePage));
        navigationService.Configure("TicketDetailsPage", typeof(TicketDetailsPage));
        navigationService.Configure("CustomerSearchPage", typeof(CustomerSearchPage));
        navigationService.Configure("RegisterPage", typeof(RegisterPage));
        navigationService.Configure("AddPerformancePage", typeof(AddPerformancePage));
        navigationService.Configure("ReportsPage", typeof(ReportsPage));
    }
}

// Класс для хранения выбранного клиента между страницами
public class CustomerSelectionManager
{
    private Customer? _selectedCustomer;

    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set => _selectedCustomer = value;
    }
}
