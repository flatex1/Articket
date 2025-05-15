-- Скрипт для выполнения всех тестовых данных
BEGIN;

-- Очистка таблиц перед вставкой (порядок важен для соблюдения ограничений внешних ключей)
TRUNCATE TABLE reports, tickets, seats, schedule, loyalty_cards, performances, halls, discount_categories, customers, users CASCADE;

-- Вставка данных
\i test_data_part1.sql
\i test_data_part2.sql
\i test_data_part3.sql
\i test_data_part4.sql
\i test_data_part5.sql

-- Установка последовательности для автоматической генерации номеров карт лояльности
CREATE SEQUENCE IF NOT EXISTS loyalty_card_seq START 16;

-- Функция для автоматического создания номера карты лояльности
CREATE OR REPLACE FUNCTION generate_loyalty_card_number()
RETURNS TRIGGER AS $$
BEGIN
  IF NEW.card_number IS NULL THEN
    NEW.card_number := 'LC-' || LPAD(nextval('loyalty_card_seq')::text, 4, '0') || '-' || to_char(current_date, 'YYYY');
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Триггер для автоматического создания номера карты лояльности
DROP TRIGGER IF EXISTS generate_loyalty_card_number_trigger ON loyalty_cards;
CREATE TRIGGER generate_loyalty_card_number_trigger
BEFORE INSERT ON loyalty_cards
FOR EACH ROW
WHEN (NEW.card_number IS NULL)
EXECUTE FUNCTION generate_loyalty_card_number();

COMMIT;

-- Информация о завершении импорта
SELECT 'Тестовые данные успешно импортированы.' AS result;
SELECT COUNT(*) AS users_count FROM users;
SELECT COUNT(*) AS customers_count FROM customers;
SELECT COUNT(*) AS discount_categories_count FROM discount_categories;
SELECT COUNT(*) AS halls_count FROM halls;
SELECT COUNT(*) AS performances_count FROM performances;
SELECT COUNT(*) AS loyalty_cards_count FROM loyalty_cards;
SELECT COUNT(*) AS schedule_count FROM schedule;
SELECT COUNT(*) AS seats_count FROM seats;
SELECT COUNT(*) AS tickets_count FROM tickets;
SELECT COUNT(*) AS reports_count FROM reports; 