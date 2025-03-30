using AfishaUno.Presentation.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AfishaUno.Presentation.Pages
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; }

        public LoginPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<LoginViewModel>();
        }
    }
} 