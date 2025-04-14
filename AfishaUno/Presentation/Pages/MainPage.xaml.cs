using AfishaUno.Models;
using AfishaUno.Presentation.ViewModels;
using AfishaUno.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AfishaUno.Presentation.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<MainViewModel>();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync();
        }

        private void Performance_Tapped(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Performance performance)
            {
                ViewModel.NavigateToPerformanceDetails(performance);
            }
        }
    }
} 