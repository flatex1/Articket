using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace AfishaUno.Services
{
    public interface INavigationService
    {
        // Свойство Frame для доступа к навигационному фрейму
        Frame Frame { get; set; }
        
        // Проверка возможности возврата
        bool CanGoBack { get; }
        
        // Навигация на страницу по имени
        void NavigateTo(string pageName, object? parameter = null);
        
        // Асинхронная навигация на страницу по имени
        Task NavigateToAsync(string pageName, object? parameter = null);
        
        // Навигация по типу страницы
        void Navigate(Type pageType, object? parameter = null);
        
        // Конфигурация маршрутов
        void Configure(string pageName, Type pageType);
        
        // Возврат на предыдущую страницу
        void GoBack();
        
        // Асинхронный возврат на предыдущую страницу
        Task GoBackAsync();
        
        // Установка параметра для передачи между страницами
        void SetNavigationParameter(string key, object value);
        
        // Получение параметра по ключу
        T GetNavigationParameter<T>(string key);
    }
} 