using System;
using AfishaUno.Presentation.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AfishaUno.Presentation.Pages
{
    /// <summary>
    /// Страница для генерации и управления отчетами системы
    /// </summary>
    public sealed partial class ReportsPage : Page
    {
        public ReportsViewModel ViewModel { get; }

        public ReportsPage()
        {
            ViewModel = App.Current.Services.GetService(typeof(ReportsViewModel)) as ReportsViewModel;
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Загружаем список сохраненных отчетов при переходе на страницу
            ViewModel.LoadSavedReportsAsync().ConfigureAwait(false);
        }
    }
} 