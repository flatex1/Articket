using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using AfishaUno.Presentation.ViewModels;

namespace AfishaUno.Presentation.Pages
{
    public sealed partial class SchedulePage : Page
    {
        public ScheduleViewModel ViewModel { get; }

        public SchedulePage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<ScheduleViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.LoadSchedulesAsync();
        }
    }
} 