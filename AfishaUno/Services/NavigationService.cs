using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace AfishaUno.Services
{
    public class NavigationService : INavigationService
    {
        private Frame _frame;
        private readonly Dictionary<string, Type> _pages = new Dictionary<string, Type>();

        public void Initialize(Frame frame)
        {
            _frame = frame;
        }

        public void Configure(string key, Type pageType)
        {
            _pages[key] = pageType;
        }

        public void NavigateTo(string pageKey)
        {
            if (!_pages.ContainsKey(pageKey))
            {
                throw new ArgumentException($"Страница {pageKey} не зарегистрирована");
            }

            _frame.Navigate(_pages[pageKey]);
        }

        public void NavigateTo(string pageKey, object parameter)
        {
            if (!_pages.ContainsKey(pageKey))
            {
                throw new ArgumentException($"Страница {pageKey} не зарегистрирована");
            }

            _frame.Navigate(_pages[pageKey], parameter);
        }

        public void GoBack()
        {
            if (_frame.CanGoBack)
            {
                _frame.GoBack();
            }
        }
    }
} 