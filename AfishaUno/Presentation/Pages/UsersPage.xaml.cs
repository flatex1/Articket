using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using AfishaUno.Presentation.ViewModels;
using AfishaUno.Services;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AfishaUno.Presentation.Pages
{
    public sealed partial class UsersPage : Page
    {
        public UsersViewModel ViewModel { get; }
        private bool _dialogIsOpen = false;

        public UsersPage()
        {
            this.InitializeComponent();
            ViewModel = (Application.Current as App)?.Services.GetService(typeof(UsersViewModel)) as UsersViewModel
                ?? new UsersViewModel((Application.Current as App)?.Services.GetService(typeof(ISupabaseService)) as ISupabaseService);
            this.DataContext = ViewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsDialogOpen))
            {
                if (ViewModel.IsDialogOpen && !_dialogIsOpen)
                {
                    _dialogIsOpen = true;
                    await UserDialog.ShowAsync();
                    _dialogIsOpen = false;
                    ViewModel.IsDialogOpen = false; // Сбросим флаг после закрытия
                }
            }
        }
    }
} 