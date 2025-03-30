using AfishaUno.Presentation.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AfishaUno.Presentation.Pages
{
    public sealed partial class RegisterPage : Page
    {
        public RegisterViewModel ViewModel { get; }

        public RegisterPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<RegisterViewModel>();
        }
    }
} 