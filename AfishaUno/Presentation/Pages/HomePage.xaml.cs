using Microsoft.UI.Xaml.Controls;
using AfishaUno.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AfishaUno.Presentation.Pages
{
    public sealed partial class HomePage : Page
    {
        public HomeViewModel ViewModel { get; }

        public HomePage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<HomeViewModel>();
            
            // ВАЖНО: не устанавливаем Frame для NavigationService здесь
            // Он уже установлен в MainPage.xaml.cs как ContentFrame
            // Frame для NavigationService должен быть установлен только один раз
        }

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.UpdateUserInfo();
        }
    }
} 