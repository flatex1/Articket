-- Создание таблицы клиентов (customers)
CREATE TABLE IF NOT EXISTS customers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    full_name TEXT NOT NULL,
    phone TEXT NOT NULL UNIQUE,
    email TEXT,
    birth_date DATE,
    document_type TEXT,
    document_number TEXT,
    verification_status BOOLEAN DEFAULT FALSE,
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Добавление индексов для быстрого поиска
CREATE INDEX IF NOT EXISTS idx_customers_phone ON customers(phone);
CREATE INDEX IF NOT EXISTS idx_customers_full_name ON customers(full_name);

-- Создание таблицы карт лояльности (loyalty_cards)
CREATE TABLE IF NOT EXISTS loyalty_cards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID NOT NULL REFERENCES customers(id),
    card_number TEXT UNIQUE,
    status TEXT NOT NULL DEFAULT 'active',
    discount_category_id UUID REFERENCES discount_categories(id),
    points INTEGER DEFAULT 0,
    level TEXT DEFAULT 'bronze',
    issue_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    expiry_date TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT loyalty_cards_status_check CHECK (status IN ('active', 'inactive', 'suspended', 'expired')),
    CONSTRAINT loyalty_cards_level_check CHECK (level IN ('bronze', 'silver', 'gold', 'platinum'))
);

-- Добавление индекса для быстрого поиска
CREATE INDEX IF NOT EXISTS idx_loyalty_cards_customer_id ON loyalty_cards(customer_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_cards_card_number ON loyalty_cards(card_number);

-- Обновление таблицы билетов для связи с клиентом
ALTER TABLE IF EXISTS tickets 
ADD COLUMN IF NOT EXISTS customer_id UUID REFERENCES customers(id),
ADD COLUMN IF NOT EXISTS loyalty_card_id UUID REFERENCES loyalty_cards(id);

-- Добавление индексов для быстрого поиска билетов по клиенту и карте лояльности
CREATE INDEX IF NOT EXISTS idx_tickets_customer_id ON tickets(customer_id);
CREATE INDEX IF NOT EXISTS idx_tickets_loyalty_card_id ON tickets(loyalty_card_id);

-- Создание функции-триггера для автоматического обновления updated_at
CREATE OR REPLACE FUNCTION trigger_set_timestamp()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = CURRENT_TIMESTAMP;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Создание триггеров для автоматического обновления updated_at
CREATE TRIGGER set_timestamp_customers
BEFORE UPDATE ON customers
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE TRIGGER set_timestamp_loyalty_cards
BEFORE UPDATE ON loyalty_cards
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

-- Добавление политик RLS (Row Level Security)
-- Примечание: политики отключены по умолчанию для удобства разработки
-- ALTER TABLE customers ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE loyalty_cards ENABLE ROW LEVEL SECURITY;

-- Создание политик для таблицы customers
-- CREATE POLICY customers_policy ON customers FOR ALL TO authenticated USING (true);

-- Создание политик для таблицы loyalty_cards
-- CREATE POLICY loyalty_cards_policy ON loyalty_cards FOR ALL TO authenticated USING (true); 