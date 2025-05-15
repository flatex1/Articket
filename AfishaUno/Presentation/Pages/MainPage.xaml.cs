using AfishaUno.Presentation.ViewModels;
using Microsoft.Extensions.Logging;

namespace AfishaUno.Presentation.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }
        private readonly ILogger<MainPage> _logger;

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<MainViewModel>();
            _logger = App.Current.Services.GetService<ILogger<MainPage>>();
            
            var navigationService = App.Current.Services.GetService<Services.INavigationService>();
            navigationService.Frame = ContentFrame;
            _logger.LogInformation("NavigationService.Frame установлен на ContentFrame в MainPage");
            
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync();
            var navigationService = App.Current.Services.GetService<Services.INavigationService>();
            if (ContentFrame.Content == null)
            {
                _logger.LogInformation("ContentFrame.Content == null, навигация на LoginPage");
                navigationService.NavigateTo("LoginPage");
            }
        }
    }
}