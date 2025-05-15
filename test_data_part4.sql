-- Заполнение таблицы seats (для двух залов)
-- Места для Главной сцены (8 рядов по 10 мест)
WITH 
row_numbers AS (SELECT generate_series(1, 8) AS row_number),
seat_numbers AS (SELECT generate_series(1, 10) AS seat_number)
INSERT INTO seats (id, hall_id, row_number, seat_number, category, created_at)
SELECT 
  gen_random_uuid(), 
  '11111111-dddd-1111-dddd-111111111111', 
  r.row_number, 
  s.seat_number, 
  CASE 
    WHEN r.row_number BETWEEN 1 AND 3 THEN 'VIP'
    WHEN r.row_number BETWEEN 4 AND 6 THEN 'Стандарт'
    ELSE 'Эконом'
  END,
  NOW()
FROM row_numbers r
CROSS JOIN seat_numbers s;

-- Места для Малой сцены (6 рядов по 8 мест)
WITH 
row_numbers AS (SELECT generate_series(1, 6) AS row_number),
seat_numbers AS (SELECT generate_series(1, 8) AS seat_number)
INSERT INTO seats (id, hall_id, row_number, seat_number, category, created_at)
SELECT 
  gen_random_uuid(), 
  '22222222-dddd-2222-dddd-222222222222', 
  r.row_number, 
  s.seat_number, 
  CASE 
    WHEN r.row_number BETWEEN 1 AND 2 THEN 'VIP'
    WHEN r.row_number BETWEEN 3 AND 4 THEN 'Стандарт'
    ELSE 'Эконом'
  END,
  NOW()
FROM row_numbers r
CROSS JOIN seat_numbers s;

-- Места для Экспериментальной сцены (5 рядов по 6 мест)
WITH 
row_numbers AS (SELECT generate_series(1, 5) AS row_number),
seat_numbers AS (SELECT generate_series(1, 6) AS seat_number)
INSERT INTO seats (id, hall_id, row_number, seat_number, category, created_at)
SELECT 
  gen_random_uuid(), 
  '33333333-dddd-3333-dddd-333333333333', 
  r.row_number, 
  s.seat_number, 
  CASE 
    WHEN r.row_number = 1 THEN 'VIP'
    WHEN r.row_number BETWEEN 2 AND 3 THEN 'Стандарт'
    ELSE 'Эконом'
  END,
  NOW()
FROM row_numbers r
CROSS JOIN seat_numbers s;

-- Заполнение таблицы tickets
-- Создадим 20 проданных билетов на разные спектакли

-- Билеты на "Ромео и Джульетта"
WITH seat_ids AS (
  SELECT id FROM seats 
  WHERE hall_id = '11111111-dddd-1111-dddd-111111111111'
  ORDER BY row_number, seat_number
  LIMIT 5
)
INSERT INTO tickets (id, schedule_id, seat_id, status, price, discount_category_id, reserved_until, qr_code, created_by, created_at, updated_at, customer_id, loyalty_card_id)
SELECT 
  gen_random_uuid(),
  '11111111-1111-bbbb-1111-111111111111',
  s.id,
  'sold',
  1500.00 * 
    CASE (ARRAY['11111111-cccc-1111-cccc-111111111111', '22222222-cccc-2222-cccc-222222222222', '33333333-cccc-3333-cccc-333333333333', 
               '44444444-cccc-4444-cccc-444444444444', '99999999-cccc-9999-cccc-999999999999'])[row_number] 
      WHEN '11111111-cccc-1111-cccc-111111111111' THEN 1.0
      WHEN '22222222-cccc-2222-cccc-222222222222' THEN 0.7
      WHEN '33333333-cccc-3333-cccc-333333333333' THEN 0.5
      WHEN '44444444-cccc-4444-cccc-444444444444' THEN 0.3
      WHEN '99999999-cccc-9999-cccc-999999999999' THEN 0.9
      ELSE 1.0
    END,
  (ARRAY['11111111-cccc-1111-cccc-111111111111', '22222222-cccc-2222-cccc-222222222222', '33333333-cccc-3333-cccc-333333333333', 
         '44444444-cccc-4444-cccc-444444444444', '99999999-cccc-9999-cccc-999999999999'])[row_number],
  NULL,
  concat('R&J-', row_number),
  '22222222-2222-2222-2222-222222222222',
  NOW() - INTERVAL '3 DAY',
  NOW() - INTERVAL '3 DAY',
  (ARRAY['11111111-aaaa-1111-aaaa-111111111111', '33333333-aaaa-3333-aaaa-333333333333', '55555555-aaaa-5555-aaaa-555555555555', 
         '77777777-aaaa-7777-aaaa-777777777777', '99999999-aaaa-9999-aaaa-999999999999'])[row_number],
  CASE row_number
    WHEN 1 THEN '11111111-1111-aaaa-1111-111111111111'
    WHEN 3 THEN '33333333-3333-aaaa-3333-333333333333'
    WHEN 5 THEN '55555555-5555-aaaa-5555-555555555555'
    ELSE NULL
  END
FROM seat_ids s
JOIN generate_series(1, 5) AS row_number ON true;

-- Билеты на "Вишневый сад"
WITH seat_ids AS (
  SELECT id FROM seats 
  WHERE hall_id = '22222222-dddd-2222-dddd-222222222222'
  ORDER BY row_number, seat_number
  LIMIT 5
)
INSERT INTO tickets (id, schedule_id, seat_id, status, price, discount_category_id, reserved_until, qr_code, created_by, created_at, updated_at, customer_id, loyalty_card_id)
SELECT 
  gen_random_uuid(),
  '22222222-2222-bbbb-2222-222222222222',
  s.id,
  'sold',
  1200.00 * 
    CASE (ARRAY['11111111-cccc-1111-cccc-111111111111', '33333333-cccc-3333-cccc-333333333333', 
               'eeeeeeee-cccc-eeee-cccc-eeeeeeeeeeee', 'ffffffff-cccc-ffff-cccc-ffffffffffff', '88888888-cccc-8888-cccc-888888888888'])[row_number] 
      WHEN '11111111-cccc-1111-cccc-111111111111' THEN 1.0
      WHEN '33333333-cccc-3333-cccc-333333333333' THEN 0.5
      WHEN 'eeeeeeee-cccc-eeee-cccc-eeeeeeeeeeee' THEN 0.9
      WHEN 'ffffffff-cccc-ffff-cccc-ffffffffffff' THEN 0.8
      WHEN '88888888-cccc-8888-cccc-888888888888' THEN 0.85
      ELSE 1.0
    END,
  (ARRAY['11111111-cccc-1111-cccc-111111111111', '33333333-cccc-3333-cccc-333333333333', 
         'eeeeeeee-cccc-eeee-cccc-eeeeeeeeeeee', 'ffffffff-cccc-ffff-cccc-ffffffffffff', '88888888-cccc-8888-cccc-888888888888'])[row_number],
  NULL,
  concat('VS-', row_number),
  '33333333-3333-3333-3333-333333333333',
  NOW() - INTERVAL '2 DAY',
  NOW() - INTERVAL '2 DAY',
  (ARRAY['22222222-aaaa-2222-aaaa-222222222222', '44444444-aaaa-4444-aaaa-444444444444', 'bbbbbbbb-aaaa-bbbb-aaaa-bbbbbbbbbbb1', 
         'dddddddd-aaaa-dddd-aaaa-ddddddddddd1', 'ffffffff-aaaa-ffff-aaaa-fffffffffff1'])[row_number],
  CASE row_number
    WHEN 3 THEN '66666666-6666-aaaa-6666-666666666666'
    WHEN 4 THEN '77777777-7777-aaaa-7777-777777777777'
    WHEN 5 THEN '88888888-8888-aaaa-8888-888888888888'
    ELSE NULL
  END
FROM seat_ids s
JOIN generate_series(1, 5) AS row_number ON true;

-- Билеты на "Гамлет"
WITH seat_ids AS (
  SELECT id FROM seats 
  WHERE hall_id = '11111111-dddd-1111-dddd-111111111111'
  ORDER BY row_number DESC, seat_number
  LIMIT 5
)
INSERT INTO tickets (id, schedule_id, seat_id, status, price, discount_category_id, reserved_until, qr_code, created_by, created_at, updated_at, customer_id, loyalty_card_id)
SELECT 
  gen_random_uuid(),
  '33333333-3333-bbbb-3333-333333333333',
  s.id,
  'sold',
  1800.00 * 
    CASE (ARRAY['11111111-cccc-1111-cccc-111111111111', '77777777-cccc-7777-cccc-777777777777', 
               'aaaaaaaa-cccc-aaaa-cccc-aaaaaaaaaaaa', 'bbbbbbbb-cccc-bbbb-cccc-bbbbbbbbbbbb', 'cccccccc-cccc-cccc-cccc-cccccccccccc'])[row_number] 
      WHEN '11111111-cccc-1111-cccc-111111111111' THEN 1.0
      WHEN '77777777-cccc-7777-cccc-777777777777' THEN 0.8
      WHEN 'aaaaaaaa-cccc-aaaa-cccc-aaaaaaaaaaaa' THEN 0.75
      WHEN 'bbbbbbbb-cccc-bbbb-cccc-bbbbbbbbbbbb' THEN 1.0
      WHEN 'cccccccc-cccc-cccc-cccc-cccccccccccc' THEN 0.2
      ELSE 1.0
    END,
  (ARRAY['11111111-cccc-1111-cccc-111111111111', '77777777-cccc-7777-cccc-777777777777', 
         'aaaaaaaa-cccc-aaaa-cccc-aaaaaaaaaaaa', 'bbbbbbbb-cccc-bbbb-cccc-bbbbbbbbbbbb', 'cccccccc-cccc-cccc-cccc-cccccccccccc'])[row_number],
  NULL,
  concat('HAMLET-', row_number),
  '44444444-4444-4444-4444-444444444444',
  NOW() - INTERVAL '4 DAY',
  NOW() - INTERVAL '4 DAY',
  (ARRAY['aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', '22222222-bbbb-2222-bbbb-222222222222', '33333333-bbbb-3333-bbbb-333333333333', 
         '44444444-bbbb-4444-bbbb-444444444444', '55555555-bbbb-5555-bbbb-555555555555'])[row_number],
  CASE row_number
    WHEN 2 THEN '99999999-9999-aaaa-9999-999999999999'
    WHEN 4 THEN 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1'
    ELSE NULL
  END
FROM seat_ids s
JOIN generate_series(1, 5) AS row_number ON true;

-- Билеты на "Горе от ума"
WITH seat_ids AS (
  SELECT id FROM seats 
  WHERE hall_id = '22222222-dddd-2222-dddd-222222222222'
  ORDER BY row_number DESC, seat_number DESC
  LIMIT 5
)
INSERT INTO tickets (id, schedule_id, seat_id, status, price, discount_category_id, reserved_until, qr_code, created_by, created_at, updated_at, customer_id, loyalty_card_id)
SELECT 
  gen_random_uuid(),
  '44444444-4444-bbbb-4444-444444444444',
  s.id,
  CASE row_number 
    WHEN 1 THEN 'sold'::ticket_status
    WHEN 2 THEN 'sold'::ticket_status
    WHEN 3 THEN 'sold'::ticket_status
    WHEN 4 THEN 'reserved'::ticket_status
    WHEN 5 THEN 'reserved'::ticket_status
    ELSE 'sold'::ticket_status
  END,
  1300.00 * 
    CASE (ARRAY['11111111-cccc-1111-cccc-111111111111', '22222222-cccc-2222-cccc-222222222222', 
               'dddddddd-cccc-dddd-cccc-dddddddddddd', '11111111-cccc-1111-cccc-111111111111', '11111111-cccc-1111-cccc-111111111111'])[row_number] 
      WHEN '11111111-cccc-1111-cccc-111111111111' THEN 1.0
      WHEN '22222222-cccc-2222-cccc-222222222222' THEN 0.7
      WHEN 'dddddddd-cccc-dddd-cccc-dddddddddddd' THEN 0.5
      ELSE 1.0
    END,
  (ARRAY['11111111-cccc-1111-cccc-111111111111', '22222222-cccc-2222-cccc-222222222222', 
         'dddddddd-cccc-dddd-cccc-dddddddddddd', '11111111-cccc-1111-cccc-111111111111', '11111111-cccc-1111-cccc-111111111111'])[row_number],
  CASE row_number 
    WHEN 4 THEN NOW() + INTERVAL '1 DAY'
    WHEN 5 THEN NOW() + INTERVAL '1 DAY'
    ELSE NULL
  END,
  concat('GOU-', row_number),
  '55555555-5555-5555-5555-555555555555',
  NOW() - INTERVAL '1 DAY',
  NOW() - INTERVAL '1 DAY',
  (ARRAY['11111111-aaaa-1111-aaaa-111111111111', '22222222-aaaa-2222-aaaa-222222222222', 
         '66666666-aaaa-6666-aaaa-666666666666', NULL, NULL])[row_number],
  CASE row_number
    WHEN 1 THEN '11111111-1111-aaaa-1111-111111111111'
    ELSE NULL
  END
FROM seat_ids s
JOIN generate_series(1, 5) AS row_number ON true; 