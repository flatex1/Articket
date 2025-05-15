-- Заполнение таблицы halls
INSERT INTO halls (id, name, capacity, created_at, updated_at) VALUES
('11111111-dddd-1111-dddd-111111111111', 'Главная сцена', 800, NOW(), NOW()),
('22222222-dddd-2222-dddd-222222222222', 'Малая сцена', 300, NOW(), NOW()),
('33333333-dddd-3333-dddd-333333333333', 'Экспериментальная сцена', 150, NOW(), NOW()),
('44444444-dddd-4444-dddd-444444444444', 'Камерный зал', 100, NOW(), NOW()),
('55555555-dddd-5555-dddd-555555555555', 'Театральная гостиная', 60, NOW(), NOW()),
('66666666-dddd-6666-dddd-666666666666', 'Концертный зал', 500, NOW(), NOW()),
('77777777-dddd-7777-dddd-777777777777', 'Театр на крыше', 200, NOW(), NOW()),
('88888888-dddd-8888-dddd-888888888888', 'Подвальная сцена', 80, NOW(), NOW());

-- Заполнение таблицы performances
INSERT INTO performances (id, title, description, duration, poster_url, created_at, updated_at) VALUES
('11111111-eeee-1111-eeee-111111111111', 'Ромео и Джульетта', 'Бессмертная история любви Уильяма Шекспира', 180, '/posters/romeo_and_juliet.jpg', NOW(), NOW()),
('22222222-eeee-2222-eeee-222222222222', 'Вишневый сад', 'Пьеса А.П. Чехова в трех действиях', 210, '/posters/vishnevyi_sad.jpg', NOW(), NOW()),
('33333333-eeee-3333-eeee-333333333333', 'Гамлет', 'Трагедия Уильяма Шекспира', 195, '/posters/hamlet.jpg', NOW(), NOW()),
('44444444-eeee-4444-eeee-444444444444', 'Горе от ума', 'Комедия в стихах А.С. Грибоедова', 185, '/posters/gore_ot_uma.jpg', NOW(), NOW()),
('55555555-eeee-5555-eeee-555555555555', 'Три сестры', 'Драма А.П. Чехова в четырех действиях', 170, '/posters/tri_sestry.jpg', NOW(), NOW()),
('66666666-eeee-6666-eeee-666666666666', 'Мастер и Маргарита', 'По мотивам романа М.А. Булгакова', 240, '/posters/master_i_margarita.jpg', NOW(), NOW()),
('77777777-eeee-7777-eeee-777777777777', 'Ревизор', 'Комедия Н.В. Гоголя в пяти действиях', 160, '/posters/revizor.jpg', NOW(), NOW()),
('88888888-eeee-8888-eeee-888888888888', 'Чайка', 'Комедия А.П. Чехова в четырех действиях', 165, '/posters/chaika.jpg', NOW(), NOW()),
('99999999-eeee-9999-eeee-999999999999', 'Отелло', 'Трагедия Уильяма Шекспира', 190, '/posters/otello.jpg', NOW(), NOW()),
('aaaaaaaa-eeee-aaaa-eeee-aaaaaaaaaaaa', 'Евгений Онегин', 'По мотивам романа А.С. Пушкина', 175, '/posters/onegin.jpg', NOW(), NOW()),
('bbbbbbbb-eeee-bbbb-eeee-bbbbbbbbbbbb', 'Преступление и наказание', 'По роману Ф.М. Достоевского', 220, '/posters/prestuplenie.jpg', NOW(), NOW()),
('cccccccc-eeee-cccc-eeee-cccccccccccc', 'Идиот', 'По мотивам романа Ф.М. Достоевского', 205, '/posters/idiot.jpg', NOW(), NOW()),
('dddddddd-eeee-dddd-eeee-dddddddddddd', 'Анна Каренина', 'По роману Л.Н. Толстого', 230, '/posters/anna_karenina.jpg', NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Дядя Ваня', 'Сцены из деревенской жизни А.П. Чехова', 155, '/posters/dyadya_vanya.jpg', NOW(), NOW()),
('ffffffff-eeee-ffff-eeee-ffffffffffff', 'Женитьба', 'Совершенно невероятное событие Н.В. Гоголя', 150, '/posters/zheniitba.jpg', NOW(), NOW()),
('11111111-ffff-1111-ffff-111111111111', 'Гроза', 'Драма А.Н. Островского', 165, '/posters/groza.jpg', NOW(), NOW()),
('22222222-ffff-2222-ffff-222222222222', 'Бесприданница', 'Драма А.Н. Островского', 170, '/posters/bespridannitsa.jpg', NOW(), NOW()),
('33333333-ffff-3333-ffff-333333333333', 'На дне', 'Пьеса М. Горького', 175, '/posters/na_dne.jpg', NOW(), NOW()),
('44444444-ffff-4444-ffff-444444444444', 'Дон Кихот', 'По мотивам романа Сервантеса', 180, '/posters/don_kihot.jpg', NOW(), NOW()),
('55555555-ffff-5555-ffff-555555555555', 'Маленький принц', 'По произведению Антуана де Сент-Экзюпери', 120, '/posters/malenkii_princ.jpg', NOW(), NOW()); 