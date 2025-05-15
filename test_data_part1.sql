-- Очистка таблиц перед вставкой данных (в обратном порядке относительно зависимостей)
TRUNCATE TABLE reports, tickets, seats, schedule, loyalty_cards, performances, halls, discount_categories, customers, users CASCADE;

-- Заполнение таблицы users
INSERT INTO users (id, email, full_name, role, created_at, updated_at) VALUES
('11111111-1111-1111-1111-111111111111', 'admin@articket.ru', 'Администратор Системы', 'admin', NOW(), NOW()),
('22222222-2222-2222-2222-222222222222', 'ivanova@articket.ru', 'Иванова Мария Петровна', 'cashier', NOW(), NOW()),
('33333333-3333-3333-3333-333333333333', 'petrov@articket.ru', 'Петров Иван Сергеевич', 'cashier', NOW(), NOW()),
('44444444-4444-4444-4444-444444444444', 'sidorov@articket.ru', 'Сидоров Алексей Иванович', 'cashier', NOW(), NOW()),
('55555555-5555-5555-5555-555555555555', 'kuznetsova@articket.ru', 'Кузнецова Елена Александровна', 'cashier', NOW(), NOW()),
('66666666-6666-6666-6666-666666666666', 'novikov@articket.ru', 'Новиков Дмитрий Валерьевич', 'admin', NOW(), NOW()),
('77777777-7777-7777-7777-777777777777', 'sokolova@articket.ru', 'Соколова Татьяна Николаевна', 'cashier', NOW(), NOW()),
('88888888-8888-8888-8888-888888888888', 'morozov@articket.ru', 'Морозов Сергей Владимирович', 'cashier', NOW(), NOW()),
('99999999-9999-9999-9999-999999999999', 'volkova@articket.ru', 'Волкова Наталья Михайловна', 'manager', NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'kozlov@articket.ru', 'Козлов Андрей Петрович', 'cashier', NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'pavlova@articket.ru', 'Павлова Ольга Игоревна', 'cashier', NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-cccccccccccc', 'smirnov@articket.ru', 'Смирнов Виктор Андреевич', 'cashier', NOW(), NOW()),
('dddddddd-dddd-dddd-dddd-dddddddddddd', 'mikhailova@articket.ru', 'Михайлова Юлия Сергеевна', 'manager', NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'fedorov@articket.ru', 'Федоров Максим Викторович', 'cashier', NOW(), NOW()),
('ffffffff-ffff-ffff-ffff-ffffffffffff', 'vasileva@articket.ru', 'Васильева Екатерина Дмитриевна', 'cashier', NOW(), NOW());

-- Заполнение таблицы customers
INSERT INTO customers (id, full_name, phone, email, birth_date, document_type, document_number, verification_status, notes, created_at, updated_at) VALUES
('11111111-aaaa-1111-aaaa-111111111111', 'Андреев Павел Николаевич', '+7 (901) 123-45-67', 'andreev@example.com', '1985-03-15', 'passport_rf', '4510 123456', true, 'Постоянный клиент', NOW(), NOW()),
('22222222-aaaa-2222-aaaa-222222222222', 'Борисова Анна Сергеевна', '+7 (902) 234-56-78', 'borisova@example.com', '1990-07-21', 'passport_rf', '4511 234567', false, NULL, NOW(), NOW()),
('33333333-aaaa-3333-aaaa-333333333333', 'Васильев Дмитрий Александрович', '+7 (903) 345-67-89', 'vasiliev@example.com', '1978-11-03', 'passport_rf', '4512 345678', true, 'Предпочитает классику', NOW(), NOW()),
('44444444-aaaa-4444-aaaa-444444444444', 'Григорьева Елена Ивановна', '+7 (904) 456-78-90', 'grigorieva@example.com', '1982-05-10', 'passport_rf', '4513 456789', false, NULL, NOW(), NOW()),
('55555555-aaaa-5555-aaaa-555555555555', 'Дмитриев Сергей Петрович', '+7 (905) 567-89-01', 'dmitriev@example.com', '1975-09-27', 'passport_rf', '4514 567890', true, 'VIP-клиент', NOW(), NOW()),
('66666666-aaaa-6666-aaaa-666666666666', 'Егорова Ольга Дмитриевна', '+7 (906) 678-90-12', 'egorova@example.com', '1993-02-14', 'student_id', 'СТ-123456', false, 'Студентка театрального', NOW(), NOW()),
('77777777-aaaa-7777-aaaa-777777777777', 'Жуков Алексей Владимирович', '+7 (907) 789-01-23', 'zhukov@example.com', '1980-01-19', 'passport_rf', '4515 678901', true, NULL, NOW(), NOW()),
('88888888-aaaa-8888-aaaa-888888888888', 'Зайцева Ирина Алексеевна', '+7 (908) 890-12-34', 'zaitseva@example.com', '1987-06-30', 'passport_rf', '4516 789012', false, NULL, NOW(), NOW()),
('99999999-aaaa-9999-aaaa-999999999999', 'Иванов Константин Михайлович', '+7 (909) 901-23-45', 'ivanov@example.com', '1976-12-05', 'passport_rf', '4517 890123', true, 'Театральный критик', NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Кузнецова Мария Владимировна', '+7 (910) 012-34-56', 'kuznetsova@example.com', '1992-04-23', 'passport_rf', '4518 901234', false, NULL, NOW(), NOW()),
('bbbbbbbb-aaaa-bbbb-aaaa-bbbbbbbbbbb1', 'Лебедев Николай Сергеевич', '+7 (911) 123-45-67', 'lebedev@example.com', '1983-08-11', 'passport_rf', '4519 012345', true, NULL, NOW(), NOW()),
('cccccccc-aaaa-cccc-aaaa-ccccccccccc1', 'Максимова Татьяна Андреевна', '+7 (912) 234-56-78', 'maksimova@example.com', '1991-10-29', 'foreign_passport', '71 1234567', false, 'Иностранная гражданка', NOW(), NOW()),
('dddddddd-aaaa-dddd-aaaa-ddddddddddd1', 'Новиков Артем Дмитриевич', '+7 (913) 345-67-89', 'novikov@example.com', '1979-07-17', 'passport_rf', '4520 123456', true, NULL, NOW(), NOW()),
('eeeeeeee-aaaa-eeee-aaaa-eeeeeeeeeee1', 'Орлова Екатерина Сергеевна', '+7 (914) 456-78-90', 'orlova@example.com', '1994-01-25', 'student_id', 'СТ-234567', false, 'Студентка', NOW(), NOW()),
('ffffffff-aaaa-ffff-aaaa-fffffffffff1', 'Павлов Игорь Александрович', '+7 (915) 567-89-01', 'pavlov@example.com', '1973-11-08', 'passport_rf', '4521 234567', true, 'Бизнесмен', NOW(), NOW()),
('11111111-bbbb-1111-bbbb-111111111111', 'Романова Светлана Петровна', '+7 (916) 678-90-12', 'romanova@example.com', '1988-03-20', 'passport_rf', '4522 345678', false, NULL, NOW(), NOW()),
('22222222-bbbb-2222-bbbb-222222222222', 'Соколов Денис Игоревич', '+7 (917) 789-01-23', 'sokolov@example.com', '1981-05-16', 'passport_rf', '4523 456789', true, 'Коллекционер программок', NOW(), NOW()),
('33333333-bbbb-3333-bbbb-333333333333', 'Тихонова Наталья Владимировна', '+7 (918) 890-12-34', 'tikhonova@example.com', '1986-09-22', 'passport_rf', '4524 567890', false, NULL, NOW(), NOW()),
('44444444-bbbb-4444-bbbb-444444444444', 'Ушаков Виктор Дмитриевич', '+7 (919) 901-23-45', 'ushakov@example.com', '1977-02-13', 'passport_rf', '4525 678901', true, NULL, NOW(), NOW()),
('55555555-bbbb-5555-bbbb-555555555555', 'Федорова Юлия Сергеевна', '+7 (920) 012-34-56', 'fedorova@example.com', '1990-12-01', 'passport_rf', '4526 789012', false, NULL, NOW(), NOW());

-- Заполнение таблицы discount_categories
INSERT INTO discount_categories (id, name, discount_percent, requires_verification, description, created_at, updated_at) VALUES
('11111111-cccc-1111-cccc-111111111111', 'Без скидки', 0, false, 'Стандартный билет без скидки', NOW(), NOW()),
('22222222-cccc-2222-cccc-222222222222', 'Пенсионер', 30, true, 'Скидка для пенсионеров при предъявлении пенсионного удостоверения', NOW(), NOW()),
('33333333-cccc-3333-cccc-333333333333', 'Студент', 50, true, 'Скидка для студентов дневной формы обучения', NOW(), NOW()),
('44444444-cccc-4444-cccc-444444444444', 'Школьник', 70, true, 'Скидка для школьников', NOW(), NOW()),
('55555555-cccc-5555-cccc-555555555555', 'Многодетная семья', 50, true, 'Скидка для членов многодетных семей', NOW(), NOW()),
('66666666-cccc-6666-cccc-666666666666', 'Ветеран', 100, true, 'Бесплатный билет для ветеранов ВОВ', NOW(), NOW()),
('77777777-cccc-7777-cccc-777777777777', 'Именинник', 20, true, 'Скидка в день рождения (±3 дня)', NOW(), NOW()),
('88888888-cccc-8888-cccc-888888888888', 'Корпоративный клиент', 15, false, 'Скидка для сотрудников компаний-партнеров', NOW(), NOW()),
('99999999-cccc-9999-cccc-999999999999', 'VIP-клиент', 10, false, 'Скидка для постоянных VIP-клиентов', NOW(), NOW()),
('aaaaaaaa-cccc-aaaa-cccc-aaaaaaaaaaaa', 'Групповая скидка', 25, false, 'Скидка при покупке от 10 билетов', NOW(), NOW()),
('bbbbbbbb-cccc-bbbb-cccc-bbbbbbbbbbbb', 'Премьерный показ', 0, false, 'Специальные цены на премьеры (без скидки)', NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Инвалид', 80, true, 'Скидка для инвалидов при предъявлении документов', NOW(), NOW()),
('dddddddd-cccc-dddd-cccc-dddddddddddd', 'Театральный работник', 50, true, 'Скидка для сотрудников театров', NOW(), NOW()),
('eeeeeeee-cccc-eeee-cccc-eeeeeeeeeeee', 'Серебряная карта', 10, false, 'Скидка для держателей серебряной карты лояльности', NOW(), NOW()),
('ffffffff-cccc-ffff-cccc-ffffffffffff', 'Золотая карта', 20, false, 'Скидка для держателей золотой карты лояльности', NOW(), NOW()); 