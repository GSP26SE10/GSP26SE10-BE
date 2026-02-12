SET TIME ZONE 'Asia/Ho_Chi_Minh';

-- =========================================
-- DROP TABLES
-- =========================================

DROP TABLE IF EXISTS message CASCADE;
DROP TABLE IF EXISTS conversation CASCADE;
DROP TABLE IF EXISTS feedback CASCADE;
DROP TABLE IF EXISTS payment CASCADE;
DROP TABLE IF EXISTS order_detail_staff_task CASCADE;
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
DROP TABLE IF EXISTS dish_detail CASCADE;
DROP TABLE IF EXISTS ingredient CASCADE;
DROP TABLE IF EXISTS dish CASCADE;
DROP TABLE IF EXISTS dish_category CASCADE;
DROP TABLE IF EXISTS party_category CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS role CASCADE;

-- =========================================
-- CREATE TABLES
-- =========================================

CREATE TABLE role (
    role_id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL
);

CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    full_name VARCHAR(100),
    email VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255),
    role_id INT REFERENCES role(role_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE party_category (
    party_category_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE dish_category (
    dish_category_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE dish (
    dish_id SERIAL PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    price NUMERIC(12,2),
    dish_category_id INT REFERENCES dish_category(dish_category_id)
);

CREATE TABLE ingredient (
    ingredient_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE dish_detail (
    dish_id INT REFERENCES dish(dish_id) ON DELETE CASCADE,
    ingredient_id INT REFERENCES ingredient(ingredient_id) ON DELETE CASCADE,
    PRIMARY KEY (dish_id, ingredient_id)
);

CREATE TABLE menu (
    menu_id SERIAL PRIMARY KEY,
    name VARCHAR(150),
    price NUMERIC(12,2)
);

CREATE TABLE menu_dish (
    menu_id INT REFERENCES menu(menu_id) ON DELETE CASCADE,
    dish_id INT REFERENCES dish(dish_id) ON DELETE CASCADE,
    PRIMARY KEY (menu_id, dish_id)
);

CREATE TABLE party_category_menu (
    party_category_id INT REFERENCES party_category(party_category_id) ON DELETE CASCADE,
    menu_id INT REFERENCES menu(menu_id) ON DELETE CASCADE,
    PRIMARY KEY (party_category_id, menu_id)
);

CREATE TABLE service (
    service_id SERIAL PRIMARY KEY,
    name VARCHAR(100),
    price NUMERIC(12,2)
);

CREATE TABLE orders (
    order_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id),
    party_category_id INT REFERENCES party_category(party_category_id),
    status VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE order_detail (
    order_detail_id SERIAL PRIMARY KEY,
    order_id INT REFERENCES orders(order_id) ON DELETE CASCADE,
    address TEXT,
    number_of_guests INT,
    start_time TIMESTAMP,
    end_time TIMESTAMP,
    status VARCHAR(50)
);

CREATE TABLE order_detail_custom (
    custom_id SERIAL PRIMARY KEY,
    order_detail_id INT REFERENCES order_detail(order_detail_id) ON DELETE CASCADE,
    custom_name VARCHAR(150),
    custom_price NUMERIC(12,2)
);

CREATE TABLE order_service (
    order_id INT REFERENCES orders(order_id) ON DELETE CASCADE,
    service_id INT REFERENCES service(service_id),
    PRIMARY KEY (order_id, service_id)
);

CREATE TABLE staff_group (
    staff_group_id SERIAL PRIMARY KEY,
    name VARCHAR(100)
);

CREATE TABLE staff_group_member (
    staff_group_id INT REFERENCES staff_group(staff_group_id) ON DELETE CASCADE,
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    PRIMARY KEY (staff_group_id, user_id)
);

CREATE TABLE order_detail_staff_task (
    task_id SERIAL PRIMARY KEY,
    order_detail_id INT REFERENCES order_detail(order_detail_id) ON DELETE CASCADE,
    staff_group_id INT REFERENCES staff_group(staff_group_id),
    description TEXT
);

CREATE TABLE payment (
    payment_id SERIAL PRIMARY KEY,
    order_id INT REFERENCES orders(order_id),
    amount NUMERIC(12,2),
    status VARCHAR(50),
    paid_at TIMESTAMP
);

CREATE TABLE feedback (
    feedback_id SERIAL PRIMARY KEY,
    order_id INT REFERENCES orders(order_id),
    rating INT,
    comment TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE conversation (
    conversation_id SERIAL PRIMARY KEY,
    customer_id INT REFERENCES users(user_id),
    owner_id INT REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE message (
    message_id SERIAL PRIMARY KEY,
    conversation_id INT REFERENCES conversation(conversation_id) ON DELETE CASCADE,
    sender_id INT REFERENCES users(user_id),
    content TEXT,
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =========================================
-- SAMPLE DATA
-- =========================================

-- ROLE
INSERT INTO role (name) VALUES
('ADMIN'),
('GROUP_LEADER'),
('STAFF'),
('USER');

-- USERS
INSERT INTO users (full_name, email, password, role_id) VALUES
('System Administrator', 'admin@buffet.vn', '123', 1),
('Team Leader Nguyen', 'leader@buffet.vn', '123', 2),
('Staff Member A', 'staff@buffet.vn', '123', 3),
('Nguyen Van A', 'user@buffet.vn', '123', 4);

-- PARTY CATEGORY
INSERT INTO party_category (name) VALUES
('Wedding Party'),
('Birthday Party'),
('Corporate Event');

-- DISH CATEGORY
INSERT INTO dish_category (name) VALUES
('Main Course'),
('Dessert'),
('Beverage');

-- DISH
INSERT INTO dish (name, price, dish_category_id) VALUES
('Honey Grilled Chicken', 150000, 1),
('Beef Steak', 200000, 1),
('Coconut Ice Cream', 50000, 2),
('Fresh Orange Juice', 30000, 3);

-- INGREDIENT
INSERT INTO ingredient (name) VALUES
('Chicken'),
('Beef'),
('Milk'),
('Orange');

-- DISH DETAIL
INSERT INTO dish_detail VALUES
(1,1),
(2,2),
(3,3),
(4,4);

-- MENU
INSERT INTO menu (name, price) VALUES
('Standard Menu', 300000),
('Premium Menu', 500000);

-- MENU - DISH
INSERT INTO menu_dish VALUES
(1,1),
(1,3),
(1,4),
(2,1),
(2,2),
(2,3),
(2,4);

-- PARTY CATEGORY - MENU
INSERT INTO party_category_menu VALUES
(1,1),
(1,2),
(2,1),
(3,2);

-- SERVICE
INSERT INTO service (name, price) VALUES
('Stage Decoration', 2000000),
('Sound & Lighting System', 1500000),
('MC Service', 1000000);

-- ORDER
INSERT INTO orders (user_id, party_category_id, status)
VALUES (4,1,'PENDING');

-- ORDER DETAIL
INSERT INTO order_detail 
(order_id, address, number_of_guests, start_time, end_time, status)
VALUES 
(1,
'123 Nguyen Trai, District 1, Ho Chi Minh City',
150,
NOW(),
NOW() + INTERVAL '5 hours',
'PENDING');

-- ORDER SERVICE
INSERT INTO order_service VALUES (1,1);
INSERT INTO order_service VALUES (1,2);

-- STAFF GROUP
INSERT INTO staff_group (name) VALUES ('Service Team A');

-- STAFF GROUP MEMBER
INSERT INTO staff_group_member VALUES (1,2); -- Group Leader
INSERT INTO staff_group_member VALUES (1,3); -- Staff

-- STAFF TASK
INSERT INTO order_detail_staff_task 
(order_detail_id, staff_group_id, description)
VALUES 
(1,1,'Setup tables and serve guests');

-- PAYMENT
INSERT INTO payment (order_id, amount, status)
VALUES (1, 7500000, 'UNPAID');

-- FEEDBACK
INSERT INTO feedback (order_id, rating, comment)
VALUES (1,5,'Excellent and professional service!');

-- CONVERSATION
INSERT INTO conversation (customer_id, owner_id)
VALUES (4,2);

-- MESSAGE
INSERT INTO message (conversation_id, sender_id, content)
VALUES 
(1,4,'I would like to change the event time.'),
(1,2,'Sure, please provide the new schedule.');
