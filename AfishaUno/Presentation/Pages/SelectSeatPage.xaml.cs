using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using AfishaUno.Presentation.ViewModels;
using AfishaUno.Services;
using System.ComponentModel;

namespace AfishaUno.Presentation.Pages
{
    /// <summary>
    /// Страница выбора места в зале
    /// </summary>
    public sealed partial class SelectSeatPage : Page
    {
        public SelectSeatViewModel ViewModel { get; }
        private bool _refundDialogIsOpen = false;

        public SelectSeatPage()
        {
            this.InitializeComponent();
            var services = (Application.Current as App)?.Services;
            ViewModel = services?.GetService(typeof(SelectSeatViewModel)) as SelectSeatViewModel
                ?? new SelectSeatViewModel(
                    services?.GetService(typeof(ISupabaseService)) as ISupabaseService,
                    services?.GetService(typeof(INavigationService)) as INavigationService,
                    services?.GetService(typeof(IAuthorizationService)) as IAuthorizationService
                );
            this.DataContext = ViewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.SelectedTicket))
            {
                if (ViewModel.SelectedTicket != null && !_refundDialogIsOpen)
                {
                    _refundDialogIsOpen = true;
                    await RefundDialog.ShowAsync();
                    _refundDialogIsOpen = false;
                    ViewModel.SelectedTicket = null; // Сбросить после закрытия
                }
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Guid scheduleId)
            {
                await ViewModel.InitializeAsync(scheduleId);
            }
        }
    }
}
