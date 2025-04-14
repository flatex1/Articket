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
using Microsoft.Extensions.DependencyInjection;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AfishaUno.Presentation.Pages
{
	/// <summary>
	/// Страница для добавления нового спектакля
	/// </summary>
	public sealed partial class AddPerformancePage : Page
	{
		public AddPerformanceViewModel ViewModel { get; }

		public AddPerformancePage()
		{
			this.InitializeComponent();
			ViewModel = App.Current.Services.GetService<AddPerformanceViewModel>();
			this.DataContext = ViewModel;
		}
	}
}
