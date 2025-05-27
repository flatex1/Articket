using System;
using System.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AfishaUno.Models;
using QRCoder;

namespace AfishaUno.Services.Documents
{
    /// <summary>
    /// Класс для создания PDF-документа билета
    /// </summary>
    public class TicketDocument : IDocument
    {
        private readonly Ticket _ticket;
        private readonly Schedule _schedule;
        private readonly Seat _seat;
        private readonly Customer _customer;

        // Размеры билета установлены на стандартный формат 
        // (можно изменить если нужны другие размеры)
        private const float TicketWidth = 600f;
        private const float TicketHeight = 290f;

        public TicketDocument(Ticket ticket, Schedule schedule, Seat seat, Customer customer = null)
        {
            _ticket = ticket ?? throw new ArgumentNullException(nameof(ticket));
            _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            _seat = seat ?? throw new ArgumentNullException(nameof(seat));
            _customer = customer; // Клиент может быть null для анонимного билета
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(TicketWidth, TicketHeight);
                    page.Margin(10);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content()
                        .Column(column =>
                        {
                            // Заголовок билета
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("БИЛЕТ")
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Blue.Medium);
                                
                                row.RelativeItem().AlignRight().Text($"#{_ticket.Id.ToString().Substring(0, 8).ToUpper()}")
                                    .FontSize(14)
                                    .FontColor(Colors.Grey.Medium);
                            });

                            // Название театра
                            column.Item().PaddingVertical(5).Row(row =>
                            {
                                row.RelativeItem().Height(30).Image(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "logo.png"));
                            });

                            // Информация о спектакле
                            column.Item().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(10).Column(ticketInfo =>
                            {
                                // Название спектакля
                                ticketInfo.Item().Text(_schedule.Performance.Title)
                                    .FontSize(16)
                                    .Bold();

                                // Дата и время спектакля
                                ticketInfo.Item().Text(text =>
                                {
                                    text.Span("Дата и время: ").Bold();
                                    text.Span($"{_schedule.StartTime:dd.MM.yyyy HH:mm}");
                                });

                                // Зал
                                ticketInfo.Item().Text(text =>
                                {
                                    text.Span("Зал: ").Bold();
                                    text.Span(_schedule.Hall.Name);
                                });

                                // Место
                                ticketInfo.Item().Text(text =>
                                {
                                    text.Span("Место: ").Bold();
                                    text.Span($"Ряд {_seat.RowNumber}, Место {_seat.SeatNumber}, {_seat.Category}");
                                });

                                // Стоимость билета
                                ticketInfo.Item().Text(text =>
                                {
                                    text.Span("Стоимость: ").Bold();
                                    text.Span($"{_ticket.Price:N2} руб.");
                                });

                                // Информация о категории скидки (если есть)
                                if (_ticket.DiscountCategoryId.HasValue && _ticket.DiscountCategory != null)
                                {
                                    ticketInfo.Item().Text(text =>
                                    {
                                        text.Span("Категория: ").Bold();
                                        text.Span(_ticket.DiscountCategory.Name);
                                    });
                                }
                            });

                            // Информация о покупателе
                            if (_customer != null)
                            {
                                column.Item().PaddingTop(5).Border(1).BorderColor(Colors.Grey.Lighten3).Padding(10).Column(customerInfo =>
                                {
                                    customerInfo.Item().Text("Информация о покупателе")
                                        .FontSize(12)
                                        .Bold();

                                    customerInfo.Item().Text(text =>
                                    {
                                        text.Span("ФИО: ").Bold();
                                        text.Span(_customer.FullName);
                                    });

                                    customerInfo.Item().Text(text =>
                                    {
                                        text.Span("Телефон: ").Bold();
                                        text.Span(_customer.Phone);
                                    });

                                    if (!string.IsNullOrEmpty(_customer.Email))
                                    {
                                        customerInfo.Item().Text(text =>
                                        {
                                            text.Span("Email: ").Bold();
                                            text.Span(_customer.Email);
                                        });
                                    }
                                });
                            }

                            // Нижняя секция с QR-кодом и дополнительной информацией
                            column.Item().PaddingTop(5).Row(footerRow =>
                            {
                                // QR-код билета в левой части
                                footerRow.RelativeItem(2).Element(ComposeQrCode);

                                // Информация справа от QR-кода
                                footerRow.RelativeItem(3).Column(infoColumn =>
                                {
                                    // Первый блок текста - информация о билете
                                    infoColumn.Item().PaddingBottom(2)
                                        .Text(text =>
                                        {
                                            text.DefaultTextStyle(x => x.FontSize(8));
                                            text.Line("Билет действителен для однократного прохода");
                                            text.Line("Сохраняйте билет до конца мероприятия");
                                            text.Line($"Дата продажи: {DateTime.Now:dd.MM.yyyy HH:mm}");
                                        });

                                    // Второй блок текста - инструкция
                                    infoColumn.Item().PaddingTop(5)
                                        .Text(text =>
                                        {
                                            text.DefaultTextStyle(x => x.FontSize(9).Bold());
                                            text.Span("Для прохода в зал предъявите этот билет контролёру");
                                        });

                                    // Веб-сайт театра
                                    infoColumn.Item().PaddingTop(5).AlignLeft()
                                        .Text("www.articket.ru")
                                        .FontSize(10)
                                        .FontColor(Colors.Blue.Medium);
                                });
                            });
                        });

                    // Дополнительная информация в подвале билета (мелким шрифтом)
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Articket © 2025 - Все права защищены").FontSize(8);
                        });
                });
        }

        // Метод для создания QR-кода
        private void ComposeQrCode(IContainer container)
        {
            // Создаем QR-код с информацией о билете
            string qrCodeText = _ticket.QrCode ?? $"TICKET-{_ticket.Id}-{_schedule.Id}-{_seat.Id}";
            
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrData = qrGenerator.CreateQrCode(qrCodeText, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrData))
                {
                    // Получаем Bitmap из QR-кода с меньшим размером модуля
                    Bitmap qrCodeImage = qrCode.GetGraphic(10);
                    
                    // Сохраняем изображение в поток
                    using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                    {
                        qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        stream.Position = 0;
                        
                        byte[] imageData = stream.ToArray();
                        
                        // Используем фиксированный размер для QR-кода и убираем ограничение по высоте
                        container
                            .Width(90)
                            .AlignCenter()
                            .AlignMiddle()
                            .Image(imageData, ImageScaling.FitWidth);
                    }
                    
                    // Освобождаем ресурсы Bitmap
                    qrCodeImage.Dispose();
                }
            }
        }
    }
} 