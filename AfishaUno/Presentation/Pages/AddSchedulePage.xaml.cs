using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using AfishaUno.Presentation.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AfishaUno.Presentation.Pages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class AddSchedulePage : Page
	{
        public AddScheduleViewModel ViewModel { get; }

        public AddSchedulePage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<AddScheduleViewModel>();
            Loaded += AddSchedulePage_Loaded;
        }

        private async void AddSchedulePage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync();
        }
    }
}
