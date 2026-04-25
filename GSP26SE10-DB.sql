SET TIME ZONE 'Asia/Ho_Chi_Minh';

-- =========================================
-- DROP TABLES
-- =========================================

DROP TABLE IF EXISTS post_block CASCADE;
DROP TABLE IF EXISTS post CASCADE;
DROP TABLE IF EXISTS blog_category CASCADE;
DROP TABLE IF EXISTS user_device CASCADE;
DROP TABLE IF EXISTS notification CASCADE;
DROP TABLE IF EXISTS message CASCADE;
DROP TABLE IF EXISTS conversation CASCADE;
DROP TABLE IF EXISTS feedback_service CASCADE;
DROP TABLE IF EXISTS feedback_menu CASCADE;
DROP TABLE IF EXISTS payment CASCADE;
DROP TABLE IF EXISTS contact_request CASCADE;
DROP TABLE IF EXISTS service_extra_charge_catalog CASCADE;
DROP TABLE IF EXISTS order_detail_extra_charge CASCADE;
DROP TABLE IF EXISTS guest_discount_tier CASCADE;
DROP TABLE IF EXISTS order_detail_staff_task CASCADE;
DROP TABLE IF EXISTS task_template CASCADE;
DROP TABLE IF EXISTS staff_group_member CASCADE;
DROP TABLE IF EXISTS staff_group CASCADE;
DROP TABLE IF EXISTS order_detail_custom CASCADE;
DROP TABLE IF EXISTS order_service CASCADE;
DROP TABLE IF EXISTS order_detail CASCADE;
DROP TABLE IF EXISTS orders CASCADE;
DROP TABLE IF EXISTS service CASCADE;
DROP TABLE IF EXISTS party_category_menu CASCADE;
DROP TABLE IF EXISTS menu_dish CASCADE;
DROP TABLE IF EXISTS menu CASCADE;
DROP TABLE IF EXISTS menu_category CASCADE;
DROP TABLE IF EXISTS dish_detail CASCADE;
DROP TABLE IF EXISTS extra_charge_catalog CASCADE;
DROP TABLE IF EXISTS ingredient CASCADE;
DROP TABLE IF EXISTS dish CASCADE;
DROP TABLE IF EXISTS dish_category CASCADE;
DROP TABLE IF EXISTS party_category CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS role CASCADE;

-- =========================================
-- CREATE TABLES (theo thứ tự dependency)
-- =========================================

-- Bảng độc lập (không có FK)
CREATE TABLE role (
    role_id SERIAL PRIMARY KEY,
    role_name VARCHAR(50) UNIQUE NOT NULL
);

CREATE TABLE party_category (
    party_category_id SERIAL PRIMARY KEY,
    party_category_name VARCHAR(100) NOT NULL,
    description TEXT,
    status VARCHAR(50),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    number_of_guests INT,
    image_url VARCHAR(255),
    service_duration_minutes INT CHECK (service_duration_minutes > 0) -- số phút phục vụ chuẩn của loại tiệc
);

CREATE TABLE dish_category (
    dish_category_id SERIAL PRIMARY KEY,
    dish_category_name VARCHAR(100) NOT NULL,
    description TEXT
);

CREATE TABLE menu_category (
    menu_category_id SERIAL PRIMARY KEY,
    menu_category_name VARCHAR(100) NOT NULL,
    description TEXT,
    status VARCHAR(50),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE ingredient (
    ingredient_id SERIAL PRIMARY KEY,
    description TEXT,
    ingredient_name VARCHAR(100) NOT NULL,
    img VARCHAR(255)
);

-- Bảng phụ thuộc level 1
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    full_name VARCHAR(100),
    email VARCHAR(100) UNIQUE NOT NULL,
    phone VARCHAR(20),
    avatar VARCHAR(255),
    status VARCHAR(50),
    password_hash VARCHAR(255),
    user_name VARCHAR(100) UNIQUE,
    address TEXT,
    role_id INT REFERENCES role(role_id),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE user_device (
    user_device_id SERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    device_id VARCHAR(255) NOT NULL,
    expo_push_token VARCHAR(255) NOT NULL,
    platform VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, device_id),
    UNIQUE(expo_push_token)
);

CREATE TABLE dish (
    dish_id SERIAL PRIMARY KEY,
    note TEXT,
    dish_name VARCHAR(150) NOT NULL,
    price NUMERIC(12,2),
    description TEXT,
    status VARCHAR(50),
    img VARCHAR(255),
    dish_category_id INT REFERENCES dish_category(dish_category_id)
);

CREATE TABLE menu (
    menu_id SERIAL PRIMARY KEY,
    menu_name VARCHAR(150),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    base_price NUMERIC(12,2),
    ais_menu_summary TEXT,
    img_url JSONB, -- array: ["url1", "url2", ...]
    status VARCHAR(50),
    menu_category_id INT REFERENCES menu_category(menu_category_id)
);

CREATE TABLE service (
    service_id SERIAL PRIMARY KEY,
    service_name VARCHAR(100),
    description TEXT,
    base_price NUMERIC(12,2),
    ais_service_summary TEXT,
    status VARCHAR(50),
    img VARCHAR(255),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE extra_charge_catalog (
    extra_charge_catalog_id SERIAL PRIMARY KEY,
    charge_type VARCHAR(100),
    title VARCHAR(255),
    description TEXT,
    unit VARCHAR(50),
    unit_price NUMERIC(12,2),
    status VARCHAR(50),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE service_extra_charge_catalog (
    service_extra_charge_catalog_id SERIAL PRIMARY KEY,
    service_id INT NOT NULL REFERENCES service(service_id) ON DELETE CASCADE,
    extra_charge_catalog_id INT NOT NULL REFERENCES extra_charge_catalog(extra_charge_catalog_id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(service_id, extra_charge_catalog_id)
);

-- Bảng phụ thuộc level 2
CREATE TABLE dish_detail (
    dish_detail_id SERIAL PRIMARY KEY,
    dish_id INT REFERENCES dish(dish_id) ON DELETE CASCADE,
    ingredient_id INT REFERENCES ingredient(ingredient_id) ON DELETE CASCADE
);

CREATE TABLE menu_dish (
    menu_dish_id SERIAL PRIMARY KEY,
    menu_id INT REFERENCES menu(menu_id) ON DELETE CASCADE,
    dish_id INT REFERENCES dish(dish_id) ON DELETE CASCADE,
    UNIQUE(menu_id, dish_id)
);

CREATE TABLE party_category_menu (
    party_category_menu_id SERIAL PRIMARY KEY,
    party_category_id INT REFERENCES party_category(party_category_id) ON DELETE CASCADE,
    menu_id INT REFERENCES menu(menu_id) ON DELETE CASCADE,
    UNIQUE(party_category_id, menu_id)
);

-- Staff Group phải được tạo TRƯỚC order_detail
CREATE TABLE staff_group (
    staff_group_id SERIAL PRIMARY KEY,
    staff_group_name VARCHAR(100),
    status VARCHAR(50),
    leader_id INT REFERENCES users(user_id)
);

CREATE TABLE orders (
    order_id SERIAL PRIMARY KEY,
    customer_id INT REFERENCES users(user_id),
    status VARCHAR(50),
    total_price NUMERIC(12,2),
    deposit_amount NUMERIC(12,2),
    remaining_amount NUMERIC(12,2),
    note_order TEXT,
    mtd_zlp JSONB, -- metadata ZaloPay (app_trans_id, zp_trans_id, refund info...)
    reviewed_by INT REFERENCES users(user_id),
    reviewed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Bảng phụ thuộc level 3
CREATE TABLE order_detail (
    order_detail_id SERIAL PRIMARY KEY,
    order_id INT REFERENCES orders(order_id) ON DELETE CASCADE,
    address TEXT,
    number_of_guests INT,
    status VARCHAR(50),
    total_price NUMERIC(12,2),
    type VARCHAR(50), -- 'Order' or 'Custom Order'
    start_time TIMESTAMPTZ,
    end_time TIMESTAMPTZ,
    actual_end_time TIMESTAMPTZ, -- nullable: giờ kết thúc thực tế
    staff_group_id INT REFERENCES staff_group(staff_group_id),
    party_category_id INT REFERENCES party_category(party_category_id),
    menu_id INT REFERENCES menu(menu_id),
    note_order_detail TEXT,
    menu_snapshot JSONB,
    service_snapshot JSONB,
    custom_dish_snapshot JSONB,
    guest_discount_snapshot JSONB, -- snapshot discount theo mốc khách (rule/applied amount)
    extra_charge_snapshot JSONB -- snapshot phí phát sinh tại thời điểm chốt đơn/tiệc
);

CREATE TABLE staff_group_member (
    staff_group_member_id SERIAL PRIMARY KEY,
    staff_group_id INT REFERENCES staff_group(staff_group_id) ON DELETE CASCADE,
    staff_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    status VARCHAR(50),
    UNIQUE(staff_group_id, staff_id)
);

-- Bảng phụ thuộc level 4
CREATE TABLE order_detail_custom (
    order_detail_custom_id SERIAL PRIMARY KEY,
    order_detail_id INT REFERENCES order_detail(order_detail_id) ON DELETE CASCADE,
    dish_id INT REFERENCES dish(dish_id),
    total_amount NUMERIC(12,2)
);

CREATE TABLE order_service (
    order_service_id SERIAL PRIMARY KEY,
    order_detail_id INT REFERENCES order_detail(order_detail_id) ON DELETE CASCADE,
    service_id INT REFERENCES service(service_id),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    quantity INT
);

CREATE TABLE order_detail_extra_charge (
    order_detail_extra_charge_id SERIAL PRIMARY KEY,
    order_detail_id INT REFERENCES order_detail(order_detail_id) ON DELETE CASCADE,
    extra_charge_catalog_id INT REFERENCES extra_charge_catalog(extra_charge_catalog_id),
    charge_type VARCHAR(100),
    title VARCHAR(255),
    description TEXT,
    unit VARCHAR(50),
    unit_price NUMERIC(12,2),
    quantity INT,
    total_amount NUMERIC(12,2),
    status VARCHAR(50),
    create_by INT REFERENCES users(user_id),
    incurred_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    image JSONB, -- array: ["url1", "url2", ...]
    note TEXT
);

CREATE TABLE guest_discount_tier (
    guest_discount_tier_id SERIAL PRIMARY KEY,
    min_guest_count INT NOT NULL CHECK (min_guest_count > 0),
    discount_percent NUMERIC(5,2) NOT NULL CHECK (discount_percent >= 0 AND discount_percent <= 100),
    note TEXT,
    status VARCHAR(50) DEFAULT 'ACTIVE',
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(min_guest_count)
);

CREATE TABLE task_template (
    task_template_id SERIAL PRIMARY KEY,
    task_name VARCHAR(255) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE order_detail_staff_task (
    task_id SERIAL PRIMARY KEY,
    order_detail_id INT REFERENCES order_detail(order_detail_id) ON DELETE CASCADE,
    task_template_id INT NOT NULL REFERENCES task_template(task_template_id),
    staff_id INT REFERENCES users(user_id),
    task_name VARCHAR(255),
    task_status VARCHAR(50),
    start_time TIMESTAMPTZ,
    end_time TIMESTAMPTZ,
    note TEXT,
    img JSONB -- array: ["url1", "url2", ...]
);

CREATE TABLE payment (
    payment_id SERIAL PRIMARY KEY,
    order_id INT REFERENCES orders(order_id),
    amount NUMERIC(12,2),
    payment_type VARCHAR(50), -- 'Deposit' or 'Full'
    payment_method VARCHAR(50),
    payment_status VARCHAR(50),
    paid_at TIMESTAMPTZ
);

CREATE TABLE feedback_menu (
    feedback_menu_id SERIAL PRIMARY KEY,
    order_id INT REFERENCES orders(order_id),
    order_detail_id INT,
    menu_id INT REFERENCES menu(menu_id) ON DELETE CASCADE,
    customer_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    rating INT NOT NULL,
    comment TEXT,
    img JSONB, -- array: ["url1", "url2", ...]
    status VARCHAR(50),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE feedback_service (
    feedback_service_id SERIAL PRIMARY KEY,
    order_id INT REFERENCES orders(order_id),
    order_detail_id INT,
    service_id INT REFERENCES service(service_id) ON DELETE CASCADE,
    customer_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    rating INT NOT NULL,
    comment TEXT,
    img JSONB, -- array: ["url1", "url2", ...]
    status VARCHAR(50),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE contact_request (
    contact_request_id SERIAL PRIMARY KEY,
    customer_id INT REFERENCES users(user_id),
    full_name VARCHAR(100),
    email VARCHAR(100),
    phone VARCHAR(20),
    subject VARCHAR(255),
    content TEXT,
    status VARCHAR(50),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE conversation (
    conversation_id SERIAL PRIMARY KEY,
    customer_id INT REFERENCES users(user_id),
    owner_id INT REFERENCES users(user_id),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE message (
    message_id SERIAL PRIMARY KEY,
    conversation_id INT REFERENCES conversation(conversation_id) ON DELETE CASCADE,
    sender_id INT REFERENCES users(user_id),
    content TEXT,
    message_type VARCHAR(20) NOT NULL DEFAULT 'TEXT',
    menu_id INT REFERENCES menu(menu_id) ON DELETE SET NULL,
    sent_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE notification (
    notification_id SERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    content TEXT,
    type VARCHAR(50),
    is_read BOOLEAN DEFAULT FALSE,
    is_sent BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- =========================================
-- BLOG TABLES (độc lập, không quan hệ với các entity khác)
-- =========================================

CREATE TABLE blog_category (
    blog_category_id SERIAL PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL
);

CREATE TABLE post (
    post_id SERIAL PRIMARY KEY,
    blog_category_id INT NULL REFERENCES blog_category(blog_category_id),
    slug VARCHAR(255) UNIQUE NOT NULL,
    title VARCHAR(255) NOT NULL,
    excerpt TEXT,
    coverImage JSONB,
    status VARCHAR(50) NOT NULL,
    published_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE post_block (
    post_block_id SERIAL PRIMARY KEY,
    post_id INT NOT NULL REFERENCES post(post_id) ON DELETE CASCADE,
    type VARCHAR(50) NOT NULL,
    position INT NOT NULL,
    data JSONB,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- =========================================
-- SAMPLE DATA
-- =========================================

-- ROLE
INSERT INTO role (role_name) VALUES
('ADMIN'),
('GROUP_LEADER'),
('STAFF'),
('USER');

-- USERS
INSERT INTO users (full_name, email, password_hash, user_name, phone, avatar, address, status, role_id) VALUES
('Mai Hân', 'hanmi200485@gmail.com', '$2a$11$oU0cF5Hnquo1BclKPHCoLefeS4Iu0xKSHUhesEpVU.ig2pbQUybpy', 'admin', '0901234567', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1776500186/z7738524821400_2bfe558f39fd9982e1ea9271db390207_oxvz1e.jpg', '123 Admin St', 'ACTIVE', 1),
('Kiến Quốc', 'quocthkse183295@fpt.edu.vn', '$2a$11$oU0cF5Hnquo1BclKPHCoLefeS4Iu0xKSHUhesEpVU.ig2pbQUybpy', 'leader', '0901234568', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1776500291/0efe4c97-7932-496d-9c65-d2298a1ab25f.png', '456 Leader St', 'ACTIVE', 2),
('Quốc Huy', 'nguyenquochuy10987@gmail.com', '$2a$11$oU0cF5Hnquo1BclKPHCoLefeS4Iu0xKSHUhesEpVU.ig2pbQUybpy', 'staff', '0901234569', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1776500404/177c3c7b-fe4d-4d84-9b01-291e8fca5212.png', '789 Staff St', 'ACTIVE', 3),
('Thành Tài', 'phanvothanhtai1007@gmail.com', '$2a$11$oU0cF5Hnquo1BclKPHCoLefeS4Iu0xKSHUhesEpVU.ig2pbQUybpy', 'user', '0901234570', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1776500677/95f299a1-c88f-47d2-82f6-cd3be7ff7ea5.png', '321 User St', 'ACTIVE', 4);

-- PARTY CATEGORY
INSERT INTO party_category (party_category_name, description, status, number_of_guests, image_url, service_duration_minutes) VALUES
('Tiệc cưới', 'Tiệc cưới sang trọng và ấm cúng', 'AVAILABLE', 200, '/images/wedding.jpg', 300),
('Tiệc sinh nhật', 'Tiệc sinh nhật vui vẻ cho gia đình và bạn bè', 'AVAILABLE', 50, '/images/birthday.jpg', 180),
('Tiệc doanh nghiệp', 'Tiệc doanh nghiệp chuyên nghiệp', 'AVAILABLE', 100, '/images/corporate.jpg', 240),
('Tiệc hẹn hò', 'Party category for minimum 1 guest testing', 'AVAILABLE', 2, '/images/test-party-1.jpg', 120);

-- DISH CATEGORY
INSERT INTO dish_category (dish_category_name, description) VALUES
('Món chính', 'Món chính'),
('Món tráng miệng', 'Món tráng miệng'),
('Nước uống', 'Nước uống');

-- DISH
INSERT INTO dish (dish_name, price, description, status, img, dish_category_id, note) VALUES
('Gà nướng mật ong', 150000, 'Gà nướng mật ong mềm thơm', 'AVAILABLE', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775554489/dish/ganuongmatong.png', 1, 'Có thể chọn mức cay nhẹ'),
('Bít tết bò', 200000, 'Bò bít tết cao cấp', 'AVAILABLE', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775554579/dish/bobittet.png', 1, 'Chế biến theo yêu cầu độ chín'),
('Kem dừa', 50000, 'Kem dừa tươi mát', 'AVAILABLE', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775554663/dish/kemdua.png', 2, 'Có chứa sữa'),
('Nước cam tươi', 30000, 'Nước cam vắt tươi', 'AVAILABLE', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775554902/dish/nuocamtuoi.png', 3, 'Không thêm đường');

-- INGREDIENT
INSERT INTO ingredient (ingredient_name, description, img) VALUES
('Thịt gà', 'Thịt gà tươi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775555058/ingredient/thitga.png'),
('Thịt bò', 'Thịt bò cao cấp', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775555110/ingredient/thitbo.png'),
('Sữa tươi', 'Sữa tươi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775555227/ingredient/suatuoi.png'),
('Cam tươi', 'Cam tươi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775555272/ingredient/camtuoi.png');

-- DISH DETAIL
INSERT INTO dish_detail (dish_id, ingredient_id) VALUES
(1, 1),
(2, 2),
(3, 3),
(4, 4);

-- MENU CATEGORY
INSERT INTO menu_category (menu_category_name, description, status) VALUES
('Buffet bò', 'Buffet các món bò cao cấp', 'AVAILABLE'),
('Buffet hải sản', 'Buffet hải sản tươi sống đa dạng', 'AVAILABLE');

-- MENU (Buffet bò: 1, Buffet hải sản: 2) - 5 món mỗi category, img_url là array nhiều ảnh
INSERT INTO menu (menu_name, base_price, img_url, status, menu_category_id) VALUES
('Combo Bò Úc', 350000, '["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/bouc1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775136272/menu/bouc2.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775136496/bouc3.png"]'::jsonb, 'AVAILABLE', 1),
('Combo Bò Wagyu', 650000, '["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775136672/menu/bowangyu1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775136729/menu/bowangyu2.png"]'::jsonb, 'AVAILABLE', 1),
('Combo Bò Việt', 280000, '["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775136927/menu/boviet1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775136955/menu/boviet2.png"]'::jsonb, 'AVAILABLE', 1),
('Combo Bò Premium', 420000, '["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775137069/menu/bopremium1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775137109/menu/bopremium2.png"]'::jsonb, 'AVAILABLE', 1),
('Combo Bò Đặc Biệt', 520000, '["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775137222/menu/bodacbiet1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775137241/menu/bodacbiet2.png"]'::jsonb, 'AVAILABLE', 1),
('Combo Hải Sản Tươi Sống', 450000, '["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775137602/menu/haisantuoisong1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775137629/menu/haisantuoisong2.png"]'::jsonb, 'AVAILABLE', 2),
('Combo Hải Sản Cao Cấp', 550000, '["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775137757/menu/haisancaocap1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775137763/menu/haisancaocap2.png"]'::jsonb, 'AVAILABLE', 2),
('Combo Hải Sản Đặc Sản', 750000, '["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775137909/menu/haisandacsan1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775137937/menu/haisandacsan2.png"]'::jsonb, 'AVAILABLE', 2),
('Combo Hải Sản Premium', 620000, '["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138024/menu/haisanpremium1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138044/menu/haisanpremium2.png"]'::jsonb, 'AVAILABLE', 2),
('Combo Hải Sản Đại Dương', 850000, '["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138213/menu/haisandaiduong1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138315/menu/haisandaiduong2.png"]'::jsonb, 'AVAILABLE', 2);


-- MENU - DISH
INSERT INTO menu_dish (menu_id, dish_id) VALUES
(1, 1),
(1, 2),
(1, 3),
(1, 4),
(2, 1),
(2, 2),
(3, 2),
(3, 1),
(3, 4),
(4, 3),
(4, 4),
(5, 1),
(5, 3),
(5, 4),
(6, 1),
(6, 2),
(6, 3),
(6, 4),
(7, 1),
(7, 2),
(7, 4),
(8, 1),
(8, 2),
(8, 3),
(9, 3),
(9, 4),
(10, 1),
(10, 2),
(10, 3),
(10, 4);

-- PARTY CATEGORY - MENU
INSERT INTO party_category_menu (party_category_id, menu_id) VALUES
(1, 1),
(1, 4),
(1, 7),
(2, 2),
(2, 5),
(2, 8),
(3, 3),
(3, 6),
(3, 9),
(3, 10);

-- SERVICE
INSERT INTO service (service_name, description, base_price, status, img) VALUES
('Trang trí sân khấu', 'Thiết kế và trang trí sân khấu chuyên nghiệp', 1000, 'AVAILABLE', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1773421275/service/service1.png'),
('Hệ thống âm thanh ánh sáng', 'Cung cấp hệ thống âm thanh ánh sáng chất lượng cao', 1500000, 'AVAILABLE', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1773421379/service/service2.png'),
('Dịch vụ MC', 'MC chuyên nghiệp dẫn chương trình sự kiện', 1000000, 'AVAILABLE', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1773421422/service/service3.png');

-- EXTRA CHARGE CATALOG
INSERT INTO extra_charge_catalog (charge_type, title, description, unit, unit_price, status) VALUES
('DAMAGE', 'Bồi thường hư hỏng', 'Phụ phí bồi thường cho hư hỏng tài sản', 'món', 10000, 'ACTIVE'),
('LATE_OVERTIME', 'Quá giờ phục vụ', 'Phụ thu do phục vụ quá thời gian dự kiến', 'phút', 3000, 'ACTIVE'),
('EXTRA_SERVICE', 'Phát sinh thêm dịch vụ', 'Phụ thu cho các dịch vụ phát sinh ngoài gói', 'dịch vụ', 300000, 'ACTIVE'),
('EXTRA_EQUIPMENT', 'Phát sinh thêm thiết bị', 'Phụ thu cho thiết bị phát sinh thêm', 'món', 400000, 'ACTIVE'),
('CLEANING', 'Phí vệ sinh thêm', 'Phụ thu cho chi phí vệ sinh phát sinh', 'lần', 250000, 'ACTIVE'),
('TRANSPORT', 'Phí vận chuyển phát sinh', 'Phụ thu vận chuyển ngoài phạm vi tiêu chuẩn', 'chuyến', 800000, 'ACTIVE'),
('PENALTY', 'Phí phạt', 'Phụ phí phạt theo điều khoản hợp đồng', 'trường hợp', 500000, 'ACTIVE');

-- SERVICE - EXTRA CHARGE CATALOG (map phí phát sinh theo từng dịch vụ)
INSERT INTO service_extra_charge_catalog (service_id, extra_charge_catalog_id) VALUES
(1, 2), -- Trang trí sân khấu: overtime
(1, 4), -- Trang trí sân khấu: extra equipment
(1, 7), -- Trang trí sân khấu: penalty
(2, 2), -- Âm thanh ánh sáng: overtime
(2, 4), -- Âm thanh ánh sáng: extra equipment
(2, 5), -- Âm thanh ánh sáng: cleaning
(2, 7), -- Âm thanh ánh sáng: penalty
(3, 2), -- MC: overtime
(3, 3), -- MC: extra service
(3, 7); -- MC: penalty

-- GUEST DISCOUNT TIER (master data)
-- Cấu hình mốc giảm giá trước, sau đó áp dụng + snapshot vào order_detail khi tạo đơn
INSERT INTO guest_discount_tier (min_guest_count, discount_percent, note, status) VALUES
(50, 5.00, 'Đạt từ 50 khách giảm 5%', 'ACTIVE'),
(100, 10.00, 'Đạt từ 100 khách giảm 10%', 'ACTIVE'),
(150, 15.00, 'Đạt từ 150 khách giảm 15%', 'ACTIVE');



