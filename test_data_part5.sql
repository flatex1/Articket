-- Заполнение таблицы reports
INSERT INTO reports (id, title, report_type, parameters, data, created_by, created_at) VALUES
('11111111-1111-dddd-1111-111111111111', 'Отчет о продажах за сентябрь 2023', 'sales', 
 '{"start_date": "2023-09-01", "end_date": "2023-09-30"}', 
 '{"total_sales": 125, "total_revenue": 187500.00, "avg_ticket_price": 1500.00, "most_popular_performance": "Ромео и Джульетта"}',
 '11111111-1111-1111-1111-111111111111', NOW() - INTERVAL '4 DAY'),
 
('22222222-2222-dddd-2222-222222222222', 'Посещаемость спектаклей за 3 квартал 2023', 'attendance', 
 '{"start_date": "2023-07-01", "end_date": "2023-09-30", "group_by": "performance"}', 
 '{"total_attendance": 2350, "top_performances": [{"title": "Мастер и Маргарита", "attendance": 420}, {"title": "Ромео и Джульетта", "attendance": 380}]}',
 '11111111-1111-1111-1111-111111111111', NOW() - INTERVAL '7 DAY'),
 
('33333333-3333-dddd-3333-333333333333', 'Доходность по категориям билетов за август 2023', 'revenue', 
 '{"start_date": "2023-08-01", "end_date": "2023-08-31", "group_by": "discount_category"}', 
 '{"total_revenue": 165000.00, "categories": [{"name": "Без скидки", "revenue": 120000.00}, {"name": "Студент", "revenue": 20000.00}]}',
 '66666666-6666-6666-6666-666666666666', NOW() - INTERVAL '14 DAY'),
 
('44444444-4444-dddd-4444-444444444444', 'Загруженность залов за 3 квартал 2023', 'occupancy', 
 '{"start_date": "2023-07-01", "end_date": "2023-09-30", "group_by": "hall"}', 
 '{"average_occupancy": 78, "halls": [{"name": "Главная сцена", "occupancy": 85}, {"name": "Малая сцена", "occupancy": 72}]}',
 '99999999-9999-9999-9999-999999999999', NOW() - INTERVAL '10 DAY'),
 
('55555555-5555-dddd-5555-555555555555', 'Анализ популярности спектаклей за 2023 год', 'performance_popularity', 
 '{"start_date": "2023-01-01", "end_date": "2023-09-30"}', 
 '{"top_performances": [{"title": "Мастер и Маргарита", "tickets_sold": 1850}, {"title": "Евгений Онегин", "tickets_sold": 1680}]}',
 '11111111-1111-1111-1111-111111111111', NOW() - INTERVAL '21 DAY'),
 
('66666666-6666-dddd-6666-666666666666', 'Отчет по клиентам с картами лояльности', 'loyalty', 
 '{"min_points": 100}', 
 '{"total_clients": 35, "total_active_cards": 42, "avg_points": 245, "total_point_value": 73500}',
 '66666666-6666-6666-6666-666666666666', NOW() - INTERVAL '5 DAY'),
 
('77777777-7777-dddd-7777-777777777777', 'Наполняемость зала на премьерных показах', 'premier_attendance', 
 '{"start_date": "2023-01-01", "end_date": "2023-09-30"}', 
 '{"average_occupancy": 92, "premiers": [{"title": "Маленький принц", "date": "2023-07-15", "occupancy": 98}, {"title": "Идиот", "date": "2023-03-10", "occupancy": 95}]}',
 '99999999-9999-9999-9999-999999999999', NOW() - INTERVAL '12 DAY'),
 
('88888888-8888-dddd-8888-888888888888', 'Проданные билеты по дням недели', 'sales_by_weekday', 
 '{"start_date": "2023-08-01", "end_date": "2023-08-31"}', 
 '{"weekdays": [{"day": "Saturday", "tickets": 320}, {"day": "Friday", "tickets": 280}, {"day": "Sunday", "tickets": 210}]}',
 '11111111-1111-1111-1111-111111111111', NOW() - INTERVAL '8 DAY'),
 
('99999999-9999-dddd-9999-999999999999', 'Анализ применения скидок за 3 квартал 2023', 'discount_usage', 
 '{"start_date": "2023-07-01", "end_date": "2023-09-30"}', 
 '{"total_tickets": 2350, "with_discount": 820, "discount_categories": [{"name": "Студент", "count": 350}, {"name": "Пенсионер", "count": 210}]}',
 '66666666-6666-6666-6666-666666666666', NOW() - INTERVAL '15 DAY'),
 
('aaaaaaaa-aaaa-dddd-aaaa-aaaaaaaaaaa1', 'Отчет по сотрудникам: продажи за сентябрь 2023', 'employee_performance', 
 '{"start_date": "2023-09-01", "end_date": "2023-09-30"}', 
 '{"total_employees": 8, "top_performers": [{"name": "Иванова Мария Петровна", "tickets": 45}, {"name": "Смирнов Виктор Андреевич", "tickets": 38}]}',
 '11111111-1111-1111-1111-111111111111', NOW() - INTERVAL '3 DAY'); 