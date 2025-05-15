using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

namespace AfishaUno.Presentation.Views
{
    /// <summary>
    /// Вспомогательный класс для работы с диалоговыми окнами
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Показывает информационное диалоговое окно
        /// </summary>
        /// <param name="xamlRoot">XamlRoot текущей страницы</param>
        /// <param name="title">Заголовок диалога</param>
        /// <param name="message">Сообщение</param>
        /// <param name="closeButtonText">Текст кнопки закрытия</param>
        /// <param name="logger">Логгер для записи информации</param>
        /// <returns>Результат диалога</returns>
        public static async Task<ContentDialogResult> ShowInformationAsync(
            XamlRoot xamlRoot,
            string title,
            string message,
            string closeButtonText = "ОК",
            ILogger logger = null)
        {
            try
            {
                if (xamlRoot == null)
                {
                    logger?.LogWarning("DialogHelper: Невозможно показать диалог - XamlRoot равен null");
                    return ContentDialogResult.None;
                }

                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = closeButtonText,
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = xamlRoot
                };

                logger?.LogInformation($"DialogHelper: Отображение информационного диалога '{title}'");
                return await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                logger?.LogError($"DialogHelper: Ошибка при отображении диалога: {ex.Message}");
                return ContentDialogResult.None;
            }
        }

        /// <summary>
        /// Показывает диалог успешного завершения операции
        /// </summary>
        /// <param name="xamlRoot">XamlRoot текущей страницы</param>
        /// <param name="title">Заголовок диалога</param>
        /// <param name="message">Сообщение</param>
        /// <param name="closeButtonText">Текст кнопки закрытия</param>
        /// <param name="logger">Логгер для записи информации</param>
        /// <returns>Результат диалога</returns>
        public static async Task<ContentDialogResult> ShowSuccessAsync(
            XamlRoot xamlRoot,
            string title,
            string message,
            string closeButtonText = "ОК",
            ILogger logger = null)
        {
            try
            {
                if (xamlRoot == null)
                {
                    logger?.LogError("DialogHelper: Невозможно показать диалог успеха - XamlRoot равен null");
                    return ContentDialogResult.None;
                }
                
                logger?.LogInformation("DialogHelper: Создание диалога успеха '{Title}'", title);
                
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = closeButtonText,
                    DefaultButton = ContentDialogButton.Close,
                    Background = new SolidColorBrush(Microsoft.UI.Colors.LightGreen),
                    XamlRoot = xamlRoot
                };

                try
                {
                    logger?.LogInformation("DialogHelper: Отображение диалога успеха '{Title}'", title);
                    return await dialog.ShowAsync();
                }
                catch (Exception dialogEx)
                {
                    logger?.LogError(dialogEx, "DialogHelper: Ошибка при показе диалога: {Error}", dialogEx.Message);
                    
                    dialog.Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray);
                    try 
                    {
                        logger?.LogInformation("DialogHelper: Повторная попытка показа диалога без стилизации");
                        return await dialog.ShowAsync();
                    }
                    catch
                    {
                        logger?.LogError("DialogHelper: Все попытки отобразить диалог завершились неудачно");
                        return ContentDialogResult.None;
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "DialogHelper: Критическая ошибка при отображении диалога успеха: {Error}", ex.Message);
                
                if (ex.InnerException != null)
                {
                    logger?.LogError(ex.InnerException, "DialogHelper: Внутренняя ошибка: {Error}", ex.InnerException.Message);
                }
                
                return ContentDialogResult.None;
            }
        }

        /// <summary>
        /// Показывает диалог с ошибкой
        /// </summary>
        /// <param name="xamlRoot">XamlRoot текущей страницы</param>
        /// <param name="title">Заголовок диалога</param>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="closeButtonText">Текст кнопки закрытия</param>
        /// <param name="logger">Логгер для записи информации</param>
        /// <returns>Результат диалога</returns>
        public static async Task<ContentDialogResult> ShowErrorAsync(
            XamlRoot xamlRoot,
            string title,
            string message,
            string closeButtonText = "ОК",
            ILogger logger = null)
        {
            try
            {
                if (xamlRoot == null)
                {
                    logger?.LogWarning("DialogHelper: Невозможно показать диалог ошибки - XamlRoot равен null");
                    return ContentDialogResult.None;
                }

                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = closeButtonText,
                    DefaultButton = ContentDialogButton.Close,
                    Background = new SolidColorBrush(Microsoft.UI.Colors.MistyRose),
                    XamlRoot = xamlRoot
                };

                logger?.LogInformation($"DialogHelper: Отображение диалога ошибки '{title}'");
                return await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                logger?.LogError($"DialogHelper: Ошибка при отображении диалога ошибки: {ex.Message}");
                return ContentDialogResult.None;
            }
        }
    }
} 