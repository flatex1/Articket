using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AfishaUno.Models;

namespace AfishaUno.Services.Documents
{
    /// <summary>
    /// Класс для создания PDF-отчета о продажах билетов
    /// </summary>
    public class SalesReportDocument : IDocument
    {
        private readonly List<Ticket> _tickets;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly string _title;
        private readonly User _generatedBy;

        public SalesReportDocument(List<Ticket> tickets, DateTime startDate, DateTime endDate, User generatedBy, string title = null)
        {
            _tickets = tickets ?? throw new ArgumentNullException(nameof(tickets));
            _startDate = startDate;
            _endDate = endDate;
            _generatedBy = generatedBy ?? throw new ArgumentNullException(nameof(generatedBy));
            _title = title ?? $"Отчет о продажах билетов за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                // Заголовок отчета
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("ARTICKET")
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.Blue.Medium);

                    row.RelativeItem().AlignRight().Text("ОТЧЕТ О ПРОДАЖАХ")
                        .FontSize(16)
                        .Bold();
                });

                // Период отчета
                column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Row(row =>
                {
                    row.RelativeItem().Text(_title)
                        .FontSize(14);

                    row.RelativeItem().AlignRight().Text($"Сформирован: {DateTime.Now:dd.MM.yyyy HH:mm}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                // Сводная информация
                column.Item().PaddingTop(10).Element(ComposeSummary);
                
                // Таблица с детальной информацией о продажах
                column.Item().PaddingTop(10).Element(ComposeTicketsTable);
                
                // График продаж по дням (текстовое представление)
                column.Item().PaddingTop(10).Element(ComposeSalesByDate);
                
                // Топ спектаклей
                column.Item().PaddingTop(10).Element(ComposeTopPerformances);
            });
        }

        private void ComposeSummary(IContainer container)
        {
            var totalRevenue = _tickets.Where(t => t.Status == "sold").Sum(t => t.Price);
            var soldTickets = _tickets.Count(t => t.Status == "sold");
            var reservedTickets = _tickets.Count(t => t.Status == "reserved");
            var cancelledTickets = _tickets.Count(t => t.Status == "cancelled");
            var avgTicketPrice = soldTickets > 0 ? totalRevenue / soldTickets : 0;

            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
            {
                column.Item().Text("Сводная информация")
                    .FontSize(14)
                    .Bold();

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);  // Метка
                        columns.RelativeColumn(2);  // Значение
                    });
                    
                    // Всего продано билетов
                    table.Cell().Padding(5).Text("Всего продано билетов:");
                    table.Cell().Padding(5).AlignRight().Text($"{soldTickets}").Bold();
                    
                    // Всего забронировано билетов
                    table.Cell().Padding(5).Text("Всего забронировано билетов:");
                    table.Cell().Padding(5).AlignRight().Text($"{reservedTickets}").Bold();
                    
                    // Всего отмененных билетов
                    table.Cell().Padding(5).Text("Всего отмененных билетов:");
                    table.Cell().Padding(5).AlignRight().Text($"{cancelledTickets}").Bold();
                    
                    // Общая выручка
                    table.Cell().Padding(5).Text("Общая выручка:");
                    table.Cell().Padding(5).AlignRight().Text($"{totalRevenue:N2} руб.").Bold();
                    
                    // Средняя цена билета
                    table.Cell().Padding(5).Text("Средняя цена билета:");
                    table.Cell().Padding(5).AlignRight().Text($"{avgTicketPrice:N2} руб.").Bold();
                });
            });
        }

        private void ComposeTicketsTable(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("Детализация продаж")
                    .FontSize(14)
                    .Bold();

                column.Item().Table(table =>
                {
                    // Определяем колонки таблицы
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.5f);  // Название спектакля
                        columns.RelativeColumn(1.5f);  // Дата и время
                        columns.RelativeColumn();      // Зал
                        columns.RelativeColumn();      // Статус
                        columns.RelativeColumn();      // Цена
                    });

                    // Заголовок таблицы
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Спектакль").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Дата и время").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Зал").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Статус").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Цена").Bold().AlignRight();
                    });

                    // Данные таблицы (ограничимся первыми 50 билетами)
                    var ticketsToShow = _tickets.Take(50).ToList();
                    foreach (var ticket in ticketsToShow)
                    {
                        // Название спектакля
                        table.Cell().Padding(5).Text(ticket.Schedule?.Performance?.Title ?? "Н/Д");
                        
                        // Дата и время
                        table.Cell().Padding(5).Text(ticket.Schedule?.StartTime.ToString("dd.MM.yyyy HH:mm") ?? "Н/Д");
                        
                        // Зал
                        table.Cell().Padding(5).Text(ticket.Schedule?.Hall?.Name ?? "Н/Д");
                        
                        // Статус
                        string statusText = ticket.Status == "sold" ? "Продан" : 
                                            ticket.Status == "reserved" ? "Забронирован" : "Отменен";
                        table.Cell().Padding(5).Text(statusText);
                        
                        // Цена
                        table.Cell().Padding(5).AlignRight().Text($"{ticket.Price:N2} руб.");
                    }

                    // Если билетов больше чем мы отображаем
                    if (_tickets.Count > ticketsToShow.Count)
                    {
                        // Используем корректный способ добавления текста для последней строки
                        table.Cell().Padding(5).Text($"... и еще {_tickets.Count - ticketsToShow.Count} записей")
                            .FontColor(Colors.Grey.Medium).Italic();
                        
                        // Пустые ячейки для остальных колонок в этой строке
                        table.Cell().Padding(5);
                        table.Cell().Padding(5);
                        table.Cell().Padding(5);
                        table.Cell().Padding(5);
                    }
                });
            });
        }

        private void ComposeSalesByDate(IContainer container)
        {
            // Группируем билеты по дате и считаем
            var salesByDate = _tickets
                .Where(t => t.Status == "sold")
                .GroupBy(t => new 
                { 
                    Date = t.CreatedAt.Date,
                    PerformanceTitle = t.Schedule?.Performance?.Title ?? "Неизвестный спектакль"
                })
                .Select(g => new 
                { 
                    Date = g.Key.Date.ToString("dd.MM.yyyy"),
                    PerformanceTitle = g.Key.PerformanceTitle,
                    Count = g.Count(), 
                    Revenue = g.Sum(t => t.Price) 
                })
                .OrderBy(x => x.Date)
                .ToList();

            if (!salesByDate.Any())
            {
                container.Text("Нет данных о продажах по дням").Italic();
                return;
            }

            container.Column(column =>
            {
                column.Item().Text("Продажи по дням")
                    .FontSize(14)
                    .Bold();

                column.Item().Table(table =>
                {
                    // Определяем колонки таблицы
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.5f);  // Название спектакля
                        columns.RelativeColumn(1);     // Дата
                        columns.RelativeColumn(1);     // Количество билетов
                        columns.RelativeColumn(1);     // Сумма продаж
                    });

                    // Заголовок таблицы
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Название спектакля").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Дата").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Кол-во билетов").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Сумма").Bold();
                    });

                    // Детализация по дням
                    foreach (var item in salesByDate)
                    {
                        table.Cell().Padding(5).Text(item.PerformanceTitle);
                        table.Cell().Padding(5).Text(item.Date);
                        table.Cell().Padding(5).Text(item.Count.ToString());
                        table.Cell().Padding(5).AlignRight().Text($"{item.Revenue:N2} руб.");
                    }

                    // Итоговая строка
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("ИТОГО").Bold();
                    
                    // Объединяем дату с названием спектакля (для итоговой строки не нужны)
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5);
                    
                    // Суммарное количество билетов
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(salesByDate.Sum(x => x.Count).ToString()).Bold();
                    
                    // Суммарная выручка
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"{salesByDate.Sum(x => x.Revenue):N2} руб.").Bold();
                });
            });
        }

        private void ComposeTopPerformances(IContainer container)
        {
            // Группируем билеты по спектаклям
            var topPerformances = _tickets
                .Where(t => t.Status == "sold")
                .GroupBy(t => t.Schedule?.Performance?.Title)
                .Select(g => new { 
                    Title = g.Key ?? "Неизвестный спектакль", 
                    Count = g.Count(), 
                    Revenue = g.Sum(t => t.Price) 
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            if (!topPerformances.Any())
            {
                container.Text("Нет данных о продажах по спектаклям").Italic();
                return;
            }

            container.Column(column =>
            {
                column.Item().Text("Топ спектаклей по выручке")
                    .FontSize(14)
                    .Bold();

                column.Item().Table(table =>
                {
                    // Определяем колонки таблицы
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);    // Номер
                        columns.RelativeColumn(3);     // Название спектакля
                        columns.RelativeColumn();      // Количество билетов
                        columns.RelativeColumn();      // Выручка
                    });

                    // Заголовок таблицы
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("#").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Название").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Билетов").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Сумма").Bold();
                    });

                    // Данные таблицы
                    for (int i = 0; i < topPerformances.Count; i++)
                    {
                        var item = topPerformances[i];
                        
                        table.Cell().Padding(5).Text($"{i + 1}");
                        table.Cell().Padding(5).Text(item.Title);
                        table.Cell().Padding(5).Text(item.Count.ToString());
                        table.Cell().Padding(5).AlignRight().Text($"{item.Revenue:N2} руб.");
                    }
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                // Линия
                column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5);
                
                // Нижний колонтитул
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span("Документ сформирован: ")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Medium);
                        
                        text.Span($"{DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Medium);
                    });

                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Сформировал: ")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Medium);
                        
                        text.Span($"{_generatedBy.FullName}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Medium);
                    });
                });
            });
        }
    }
} 