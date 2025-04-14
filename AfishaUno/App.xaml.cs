using AfishaUno.Presentation.Pages;
using AfishaUno.Presentation.ViewModels;
using AfishaUno.Services;
using Microsoft.Extensions.Configuration;

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
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    #region Fields

    private Window _window;
    private IServiceProvider _serviceProvider;
    private IConfiguration _configuration;

    /// <summary>
    /// Gets the current app as a strongly typed reference.
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    public IServiceProvider Services => _serviceProvider;

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

        // Регистрация ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<SelectSeatViewModel>();
        services.AddTransient<AddPerformanceViewModel>();
        services.AddTransient<AddScheduleViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.
    /// </summary>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        ConfigureServices();

        var rootFrame = new Frame();
        _window = new Window();
        _window.Content = rootFrame;
        _window.Activate();

        // Настройка навигации
        var navigationService = Services.GetService<INavigationService>();
        navigationService.Initialize(rootFrame);
        navigationService.Configure("LoginPage", typeof(LoginPage));
        navigationService.Configure("MainPage", typeof(MainPage));
        navigationService.Configure("RegisterPage", typeof(RegisterPage));
        navigationService.Configure("SelectSeatPage", typeof(SelectSeatPage));
        navigationService.Configure("AddPerformancePage", typeof(AddPerformancePage));
        navigationService.Configure("AddSchedulePage", typeof(AddSchedulePage));
        // Закомментируем функциональность, которая еще не реализована
        // navigationService.Configure("PerformanceDetailPage", typeof(PerformanceDetailPage));

        // По умолчанию открываем страницу авторизации
        navigationService.NavigateTo("LoginPage");
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<LoginPage, LoginViewModel>(),
            new ViewMap<MainPage, MainViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new ("Login", View: views.FindByViewModel<LoginViewModel>()),
                    new ("Main", View: views.FindByViewModel<MainViewModel>(), IsDefault:true)
                ]
            )
        );
    }
}
