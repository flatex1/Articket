using Microsoft.UI.Xaml.Controls;
using System;

namespace AfishaUno.Services
{
    public interface INavigationService
    {
        void Initialize(Frame frame);
        void Configure(string key, Type pageType);
        void NavigateTo(string pageKey);
        void NavigateTo(string pageKey, object parameter);
        void GoBack();
    }
} 