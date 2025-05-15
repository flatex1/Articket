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
    /// Класс для создания PDF-отчета о посещаемости спектаклей
    /// </summary>
    public class PerformanceReportDocument : IDocument
    {
        private readonly List<Schedule> _schedules;
        private readonly List<Ticket> _tickets;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly string _title;
        private readonly User _generatedBy;

        public PerformanceReportDocument(List<Schedule> schedules, List<Ticket> tickets, DateTime startDate, DateTime endDate, 
            User generatedBy, string title = null)
        {
            _schedules = schedules ?? throw new ArgumentNullException(nameof(schedules));
            _tickets = tickets ?? throw new ArgumentNullException(nameof(tickets));
            _startDate = startDate;
            _endDate = endDate;
            _generatedBy = generatedBy ?? throw new ArgumentNullException(nameof(generatedBy));
            _title = title ?? $"Отчет о посещаемости спектаклей за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
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

                    row.RelativeItem().AlignRight().Text("ОТЧЕТ О ПОСЕЩАЕМОСТИ")
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
                // Сводная информация о посещаемости
                column.Item().PaddingTop(10).Element(ComposeSummary);
                
                // Рейтинг посещаемости по спектаклям
                column.Item().PaddingTop(10).Element(ComposePerformanceAttendance);
                
                // Рейтинг посещаемости по дням недели
                column.Item().PaddingTop(10).Element(ComposeAttendanceByDayOfWeek);
                
                // Посещаемость по залам
                column.Item().PaddingTop(10).Element(ComposeAttendanceByHall);
            });
        }

        private void ComposeSummary(IContainer container)
        {
            var totalSchedules = _schedules.Count;
            var totalCapacity = _schedules.Sum(s => s.Hall?.Capacity ?? 0);
            var soldTickets = _tickets.Count(t => t.Status == "sold");
            var avgAttendancePercent = totalCapacity > 0 ? (double)soldTickets / totalCapacity * 100 : 0;
            var avgTicketsPerSchedule = totalSchedules > 0 ? (double)soldTickets / totalSchedules : 0;

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
                    
                    // Всего сеансов
                    table.Cell().Padding(5).Text("Всего сеансов:");
                    table.Cell().Padding(5).AlignRight().Text($"{totalSchedules}").Bold();
                    
                    // Общая вместимость
                    table.Cell().Padding(5).Text("Общая вместимость залов:");
                    table.Cell().Padding(5).AlignRight().Text($"{totalCapacity} мест").Bold();
                    
                    // Всего продано билетов
                    table.Cell().Padding(5).Text("Всего продано билетов:");
                    table.Cell().Padding(5).AlignRight().Text($"{soldTickets}").Bold();
                    
                    // Средняя заполняемость
                    table.Cell().Padding(5).Text("Средняя заполняемость:");
                    table.Cell().Padding(5).AlignRight().Text($"{avgAttendancePercent:N1}%").Bold();
                    
                    // Среднее кол-во билетов на сеанс
                    table.Cell().Padding(5).Text("Среднее количество билетов на сеанс:");
                    table.Cell().Padding(5).AlignRight().Text($"{avgTicketsPerSchedule:N1}").Bold();
                });
            });
        }

        private void ComposePerformanceAttendance(IContainer container)
        {
            // Группируем данные по спектаклям
            var performanceStats = _schedules
                .GroupBy(s => s.Performance?.Title)
                .Select(g => new 
                { 
                    Title = g.Key ?? "Неизвестный спектакль",
                    ScheduleCount = g.Count(),
                    TotalCapacity = g.Sum(s => s.Hall?.Capacity ?? 0),
                    TicketsSold = _tickets.Count(t => t.Status == "sold" && 
                                                    g.Any(s => s.Id == t.ScheduleId))
                })
                .Select(x => new
                {
                    x.Title,
                    x.ScheduleCount,
                    x.TotalCapacity,
                    x.TicketsSold,
                    AttendancePercent = x.TotalCapacity > 0 ? (double)x.TicketsSold / x.TotalCapacity * 100 : 0
                })
                .OrderByDescending(x => x.AttendancePercent)
                .ToList();

            if (!performanceStats.Any())
            {
                container.Text("Нет данных о посещаемости спектаклей").Italic();
                return;
            }

            var totalCapacity = performanceStats.Sum(x => x.TotalCapacity);
            var totalSold = performanceStats.Sum(x => x.TicketsSold);
            var avgAttendancePercent = totalCapacity > 0 ? (double)totalSold / totalCapacity * 100 : 0;

            container.Column(column =>
            {
                column.Item().Text("Рейтинг посещаемости по спектаклям")
                    .FontSize(14)
                    .Bold();

                column.Item().Table(table =>
                {
                    // Определяем колонки таблицы
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.5f);  // Название спектакля
                        columns.RelativeColumn(1);     // Количество сеансов
                        columns.RelativeColumn(1);     // Продано билетов
                        columns.RelativeColumn(1);     // Заполняемость, %
                    });

                    // Заголовок таблицы
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Название спектакля").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Кол-во сеансов").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Продано билетов").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Заполняемость, %").Bold();
                    });

                    // Данные таблицы
                    foreach (var item in performanceStats)
                    {
                        table.Cell().Padding(5).Text(item.Title);
                        table.Cell().Padding(5).Text(item.ScheduleCount.ToString());
                        table.Cell().Padding(5).Text(item.TicketsSold.ToString());
                        table.Cell().Padding(5).AlignRight().Text($"{item.AttendancePercent:N1}%");
                    }

                    // Итоговая строка
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("ИТОГО").Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(performanceStats.Sum(x => x.ScheduleCount).ToString()).Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(totalSold.ToString()).Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"{avgAttendancePercent:N1}%").Bold();
                });
            });
        }

        private void ComposeAttendanceByDayOfWeek(IContainer container)
        {
            // Группируем данные по дням недели
            var attendanceByDayOfWeek = _schedules
                .GroupBy(s => s.StartTime.DayOfWeek)
                .Select(g => new 
                { 
                    DayOfWeek = g.Key,
                    Schedules = g.Count(),
                    TotalCapacity = g.Sum(s => s.Hall?.Capacity ?? 0),
                    SoldTickets = _tickets.Count(t => t.Status == "sold" && 
                                                  g.Any(s => s.Id == t.ScheduleId))
                })
                .Select(x => new
                {
                    x.DayOfWeek,
                    DayName = GetDayOfWeekName(x.DayOfWeek),
                    x.Schedules,
                    x.TotalCapacity,
                    x.SoldTickets,
                    AttendancePercent = x.TotalCapacity > 0 ? (double)x.SoldTickets / x.TotalCapacity * 100 : 0
                })
                .OrderBy(x => x.DayOfWeek)
                .ToList();

            if (!attendanceByDayOfWeek.Any())
            {
                container.Text("Нет данных о посещаемости по дням недели").Italic();
                return;
            }

            container.Column(column =>
            {
                column.Item().Text("Посещаемость по дням недели")
                    .FontSize(14)
                    .Bold();

                column.Item().Table(table =>
                {
                    // Определяем колонки таблицы
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();      // День недели
                        columns.RelativeColumn();      // Количество сеансов
                        columns.RelativeColumn();      // Продано билетов
                        columns.RelativeColumn();      // Заполняемость, %
                    });

                    // Заголовок таблицы
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("День недели").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Кол-во сеансов").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Продано билетов").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Заполняемость, %").Bold();
                    });

                    // Данные таблицы
                    foreach (var item in attendanceByDayOfWeek)
                    {
                        table.Cell().Padding(5).Text(item.DayName);
                        table.Cell().Padding(5).Text(item.Schedules.ToString());
                        table.Cell().Padding(5).Text(item.SoldTickets.ToString());
                        table.Cell().Padding(5).AlignRight().Text($"{item.AttendancePercent:N1}%");
                    }
                });
            });
        }

        private void ComposeAttendanceByHall(IContainer container)
        {
            // Группируем данные по залам
            var attendanceByHall = _schedules
                .GroupBy(s => s.Hall?.Name)
                .Select(g => new 
                { 
                    HallName = g.Key ?? "Неизвестный зал",
                    Schedules = g.Count(),
                    TotalCapacity = g.Sum(s => s.Hall?.Capacity ?? 0),
                    SoldTickets = _tickets.Count(t => t.Status == "sold" && 
                                                   g.Any(s => s.Id == t.ScheduleId))
                })
                .Select(x => new
                {
                    x.HallName,
                    x.Schedules,
                    x.TotalCapacity,
                    x.SoldTickets,
                    AttendancePercent = x.TotalCapacity > 0 ? (double)x.SoldTickets / x.TotalCapacity * 100 : 0
                })
                .OrderByDescending(x => x.AttendancePercent)
                .ToList();

            if (!attendanceByHall.Any())
            {
                container.Text("Нет данных о посещаемости по залам").Italic();
                return;
            }

            container.Column(column =>
            {
                column.Item().Text("Посещаемость по залам")
                    .FontSize(14)
                    .Bold();

                column.Item().Table(table =>
                {
                    // Определяем колонки таблицы
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.5f);  // Название зала
                        columns.RelativeColumn();      // Количество сеансов
                        columns.RelativeColumn();      // Продано билетов
                        columns.RelativeColumn();      // Заполняемость, %
                    });

                    // Заголовок таблицы
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Зал").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Кол-во сеансов").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Продано билетов").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Заполняемость, %").Bold();
                    });

                    // Данные таблицы
                    foreach (var item in attendanceByHall)
                    {
                        table.Cell().Padding(5).Text(item.HallName);
                        table.Cell().Padding(5).Text(item.Schedules.ToString());
                        table.Cell().Padding(5).Text(item.SoldTickets.ToString());
                        table.Cell().Padding(5).AlignRight().Text($"{item.AttendancePercent:N1}%");
                    }
                });
            });
        }

        private string GetDayOfWeekName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Понедельник",
                DayOfWeek.Tuesday => "Вторник",
                DayOfWeek.Wednesday => "Среда",
                DayOfWeek.Thursday => "Четверг",
                DayOfWeek.Friday => "Пятница",
                DayOfWeek.Saturday => "Суббота",
                DayOfWeek.Sunday => "Воскресенье",
                _ => "Неизвестный день"
            };
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