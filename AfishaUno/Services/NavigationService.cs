using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AfishaUno.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Type> _pageTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, object> _navigationParameters = new Dictionary<string, object>();
        private readonly ILogger<NavigationService>? _logger;

        public Frame Frame { get; set; }

        public bool CanGoBack => Frame?.CanGoBack ?? false;

        public NavigationService(ILogger<NavigationService>? logger = null)
        {
            _logger = logger;
        }

        public void NavigateTo(string pageName, object? parameter = null)
        {
            if (string.IsNullOrEmpty(pageName))
            {
                _logger?.LogError("Попытка перейти на страницу с пустым именем");
                return;
            }

            if (Frame == null)
            {
                _logger?.LogError($"NavigationService.Frame не установлен. Навигация на {pageName} невозможна");
                return;
            }

            try
            {
                if (_pageTypes.TryGetValue(pageName, out var pageType))
                {
                    _logger?.LogInformation($"Навигация на страницу: {pageName} (тип: {pageType.Name})");
                    Frame.Navigate(pageType, parameter);
                    _logger?.LogInformation($"Навигация на страницу {pageName} успешно выполнена");
                }
                else
                {
                    _logger?.LogError($"Попытка перейти на неизвестную страницу: {pageName}. Доступные страницы: {string.Join(", ", _pageTypes.Keys)}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Ошибка при навигации на страницу {pageName}: {ex.Message}");
            }
        }

        public async Task NavigateToAsync(string pageName, object? parameter = null)
        {
            NavigateTo(pageName, parameter);
            await Task.CompletedTask;
        }

        public void Navigate(Type pageType, object? parameter = null)
        {
            if (pageType == null)
            {
                _logger?.LogError("Попытка перейти на страницу с типом null");
                return;
            }

            if (Frame == null)
            {
                _logger?.LogError("NavigationService.Frame не установлен. Навигация невозможна");
                return;
            }

            try
            {
                _logger?.LogInformation($"Навигация на страницу типа: {pageType.Name}");
                Frame.Navigate(pageType, parameter);
                _logger?.LogInformation($"Навигация на страницу типа {pageType.Name} успешно выполнена");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Ошибка при навигации на страницу типа {pageType.Name}: {ex.Message}");
            }
        }

        public void Configure(string pageName, Type pageType)
        {
            if (string.IsNullOrEmpty(pageName))
            {
                _logger?.LogError("Попытка сконфигурировать страницу с пустым именем");
                return;
            }

            if (pageType == null)
            {
                _logger?.LogError($"Попытка сконфигурировать страницу {pageName} с null типом");
                return;
            }

            _logger?.LogInformation($"Конфигурирование маршрута: {pageName} -> {pageType.Name}");

            // Если маршрут уже существует, обновляем его
            if (_pageTypes.ContainsKey(pageName))
            {
                _pageTypes[pageName] = pageType;
            }
            else
            {
                // Добавляем новый маршрут
                _pageTypes.Add(pageName, pageType);
            }
        }

        public void GoBack()
        {
            if (Frame != null && Frame.CanGoBack)
            {
                _logger?.LogInformation("Навигация назад");
                Frame.GoBack();
            }
        }

        public async Task GoBackAsync()
        {
            if (Frame != null && Frame.CanGoBack)
            {
                _logger?.LogInformation("Асинхронная навигация назад");
                Frame.GoBack();
            }

            // Возвращаем выполненную задачу для совместимости с async/await
            await Task.CompletedTask;
        }

        public void SetNavigationParameter(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger?.LogError("Попытка установить параметр с пустым ключом");
                return;
            }

            _navigationParameters[key] = value;
            _logger?.LogInformation($"Установлен параметр навигации: {key}");
        }

        public T GetNavigationParameter<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger?.LogError("Попытка получить параметр с пустым ключом");
                return default;
            }

            if (_navigationParameters.TryGetValue(key, out var value) && value is T typedValue)
            {
                _logger?.LogInformation($"Получен параметр навигации: {key}");
                return typedValue;
            }

            _logger?.LogWarning($"Параметр навигации не найден или имеет неверный тип: {key}");
            return default;
        }
    }
}
