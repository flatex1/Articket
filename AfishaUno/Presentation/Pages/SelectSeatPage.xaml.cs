using AfishaUno.Presentation.ViewModels;

namespace AfishaUno.Presentation.Pages
{
    /// <summary>
    /// Страница выбора места в зале
    /// </summary>
    public sealed partial class SelectSeatPage : Page
    {
        public SelectSeatViewModel ViewModel { get; }

        public SelectSeatPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService(typeof(SelectSeatViewModel)) as SelectSeatViewModel;
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
