using System;

namespace AfishaUno.Models
{
    /// <summary>
    /// Статический класс для передачи выбранного клиента между страницами
    /// используется как запасной вариант, если сервис навигации недоступен
    /// </summary>
    public static class CustomerSelectionManager
    {
        /// <summary>
        /// Выбранный клиент
        /// </summary>
        public static Customer SelectedCustomer { get; set; }
    }
} 