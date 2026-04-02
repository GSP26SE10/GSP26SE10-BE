SET TIME ZONE 'Asia/Ho_Chi_Minh';

-- =========================
-- Base categories and data
-- =========================
INSERT INTO party_category (party_category_name, description, status, number_of_guests, image_url)
SELECT * FROM (
    VALUES
    ('Tiệc cưới', 'Tiệc cưới sang trọng', 'AVAILABLE', 200, '/images/wedding.jpg'),
    ('Tiệc sinh nhật', 'Tiệc sinh nhật ấm cúng', 'AVAILABLE', 50, '/images/birthday.jpg'),
    ('Tiệc doanh nghiệp', 'Tiệc doanh nghiệp chuyên nghiệp', 'AVAILABLE', 120, '/images/corporate.jpg')
) AS v(party_category_name, description, status, number_of_guests, image_url)
WHERE NOT EXISTS (
    SELECT 1 FROM party_category pc WHERE pc.party_category_name = v.party_category_name
);

INSERT INTO dish_category (dish_category_name, description)
SELECT * FROM (
    VALUES
    ('Main Course', 'Món chính'),
    ('Dessert', 'Món tráng miệng'),
    ('Beverage', 'Nước uống')
) AS v(dish_category_name, description)
WHERE NOT EXISTS (
    SELECT 1 FROM dish_category dc WHERE dc.dish_category_name = v.dish_category_name
);

INSERT INTO menu_category (menu_category_name, description, status)
SELECT * FROM (
    VALUES
    ('Buffet bò', 'Buffet các món bò cao cấp', 'AVAILABLE'),
    ('Buffet hải sản', 'Buffet hải sản tươi sống đa dạng', 'AVAILABLE'),
    ('Buffet gà', 'Buffet các món gà phù hợp gia đình', 'AVAILABLE'),
    ('Buffet chay', 'Buffet chay thanh đạm', 'AVAILABLE'),
    ('Buffet lẩu nướng', 'Buffet lẩu nướng tổng hợp', 'AVAILABLE')
) AS v(menu_category_name, description, status)
WHERE NOT EXISTS (
    SELECT 1 FROM menu_category mc WHERE mc.menu_category_name = v.menu_category_name
);

INSERT INTO ingredient (ingredient_name, description, img)
SELECT * FROM (
    VALUES
    ('Thịt bò', 'Thịt bò tươi cho món nướng, lẩu', '/images/ing_beef.jpg'),
    ('Tôm sú', 'Tôm sú tươi sống', '/images/ing_shrimp.jpg'),
    ('Mực tươi', 'Mực tươi cho món hấp, nướng', '/images/ing_squid.jpg'),
    ('Cá hồi', 'Phi lê cá hồi', '/images/ing_salmon.jpg'),
    ('Thịt gà', 'Thịt gà tươi', '/images/ing_chicken.jpg'),
    ('Nấm kim châm', 'Nấm kim châm tươi', '/images/ing_enoki.jpg'),
    ('Rau củ tổng hợp', 'Rau củ theo mùa', '/images/ing_veggie.jpg'),
    ('Đậu hũ non', 'Đậu hũ non cho món chay', '/images/ing_tofu.jpg')
) AS v(ingredient_name, description, img)
WHERE NOT EXISTS (
    SELECT 1 FROM ingredient i WHERE i.ingredient_name = v.ingredient_name
);

-- ============
-- Dishes
-- ============
INSERT INTO dish (dish_name, price, description, status, img, dish_category_id, note)
SELECT v.dish_name, v.price, v.description, 'AVAILABLE', v.img, dc.dish_category_id, v.note
FROM (
    VALUES
    ('Bò cuộn nấm kim châm',165000,'Thịt bò cuộn nấm kim châm nướng thơm, vị đậm đà.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Dùng ngon với sốt tiêu đen'),
    ('Bò nướng sa tế',175000,'Bò ướp sa tế cay nhẹ, nướng vừa chín tới.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Cấp độ cay vừa'),
    ('Bò lúc lắc khoai tây',185000,'Bò lúc lắc mềm mọng ăn kèm khoai tây chiên.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Món bán chạy'),
    ('Bò áp chảo sốt tiêu đen',198000,'Thịt bò áp chảo sốt tiêu đen thơm nồng.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Khuyên dùng chín vừa'),
    ('Sườn bò nướng mật ong',210000,'Sườn bò nướng mật ong vị ngọt mặn hài hòa.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Món chủ lực nhóm bò'),
    ('Lẩu bò nấm',249000,'Lẩu bò nấm nóng hổi với nước dùng thanh ngọt.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Khẩu phần 3-4 người'),
    ('Bò nhúng dấm',195000,'Bò thái lát nhúng dấm ăn kèm rau tươi.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Phong cách truyền thống'),
    ('Tôm sú nướng muối ớt',189000,'Tôm sú tươi nướng muối ớt đậm vị biển.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Cay nhẹ'),
    ('Mực hấp gừng hành',168000,'Mực hấp gừng hành giữ độ ngọt tự nhiên.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Mềm và thơm'),
    ('Cá hồi sốt bơ tỏi',238000,'Phi lê cá hồi áp chảo với sốt bơ tỏi.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Béo nhẹ, thơm tỏi'),
    ('Hàu nướng phô mai',179000,'Hàu nướng phủ phô mai béo thơm.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Phù hợp tiệc tối'),
    ('Ghẹ rang me',259000,'Ghẹ rang me vị chua ngọt hài hòa.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Đặc trưng hải sản'),
    ('Sò điệp nướng mỡ hành',229000,'Sò điệp nướng mỡ hành rắc đậu phộng.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Thơm béo'),
    ('Lẩu hải sản Tomyum',279000,'Lẩu hải sản vị Tomyum chua cay kiểu Thái.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Khẩu phần 4-5 người'),
    ('Gà nướng mật ong',159000,'Gà nướng mật ong da giòn, thịt mềm.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Hợp khẩu vị số đông'),
    ('Gà hấp lá chanh',149000,'Gà hấp lá chanh thơm nhẹ, dễ ăn.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Món truyền thống'),
    ('Gà chiên nước mắm',155000,'Gà chiên nước mắm đậm vị, vàng giòn.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Đưa cơm'),
    ('Lẩu gà lá é',189000,'Lẩu gà lá é thanh cay, mùi thơm đặc trưng.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Khẩu phần 3-4 người'),
    ('Nấm đùi gà nướng giấy bạc',119000,'Nấm đùi gà nướng giấy bạc thơm bơ tỏi.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Món chay phổ biến'),
    ('Đậu hũ non sốt nấm đông cô',109000,'Đậu hũ non sốt nấm đông cô thanh nhẹ.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Ít béo'),
    ('Rau củ xào ngũ sắc',99000,'Rau củ xào ngũ sắc giòn ngọt tự nhiên.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Bổ sung chất xơ'),
    ('Lẩu nấm chay',169000,'Lẩu nấm chay nước dùng thanh ngọt.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Phù hợp tiệc chay'),
    ('Chè khúc bạch',49000,'Chè khúc bạch thanh mát ăn kèm trái cây.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Dessert','Dùng lạnh'),
    ('Bánh flan caramel',39000,'Bánh flan mềm mịn, vị caramel nhẹ.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Dessert','Món tráng miệng quen thuộc'),
    ('Trái cây theo mùa',45000,'Đĩa trái cây tươi thay đổi theo mùa.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Dessert','Thanh mát sau bữa ăn'),
    ('Trà đào cam sả',42000,'Trà đào cam sả thơm mát, dễ uống.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Beverage','Món nước bán chạy'),
    ('Nước chanh dây',35000,'Nước chanh dây chua ngọt hài hòa.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Beverage','Giải khát tốt'),
    ('Nước suối tinh khiết',18000,'Nước suối đóng chai tiện lợi.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Beverage','Tính theo chai'),
    ('Bò Wagyu nướng đá',320000,'Thịt bò Wagyu nướng trên đá nóng, giữ trọn vị ngọt thịt.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Món cao cấp cho menu premium'),
    ('Sườn cừu nướng thảo mộc',285000,'Sườn cừu nướng cùng thảo mộc, thơm mềm đặc trưng.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Phù hợp tiệc cao cấp'),
    ('Tôm hùm bỏ lò phô mai',359000,'Tôm hùm bỏ lò phủ phô mai béo ngậy.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Món nổi bật nhóm hải sản premium'),
    ('Cua hoàng đế hấp',399000,'Cua hoàng đế hấp giữ vị ngọt tự nhiên.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Khẩu phần cao cấp'),
    ('Cá tầm nướng muối ớt',269000,'Cá tầm nướng muối ớt đậm đà, thịt chắc.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Hợp tiệc doanh nghiệp'),
    ('Cơm chiên hải sản',119000,'Cơm chiên hải sản tơi ngon, đậm vị.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Món tinh bột cân bằng thực đơn'),
    ('Mì xào bò rau củ',109000,'Mì xào bò cùng rau củ giòn ngọt.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Phù hợp menu gia đình'),
    ('Đậu hũ chiên sả ớt',89000,'Đậu hũ chiên giòn cùng sả ớt thơm cay nhẹ.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Main Course','Món chay phổ thông'),
    ('Chè hạt sen long nhãn',55000,'Chè hạt sen long nhãn thanh ngọt, dễ dùng sau bữa tiệc.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Dessert','Món tráng miệng truyền thống'),
    ('Trà vải',39000,'Trà vải mát lạnh, thơm nhẹ vị trái cây.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1773752703/dish/dish1.png','Beverage','Hợp khẩu vị số đông')
) AS v(dish_name, price, description, img, category_name, note)
JOIN dish_category dc ON dc.dish_category_name = v.category_name
WHERE NOT EXISTS (SELECT 1 FROM dish d WHERE d.dish_name = v.dish_name);

-- ============
-- Menus (2 cũ + 10 mới)
-- ============
INSERT INTO menu (menu_name, base_price, img_url, status, menu_category_id)
SELECT v.menu_name, v.base_price, v.img_url::jsonb, 'AVAILABLE', mc.menu_category_id
FROM (
    VALUES
    ('Combo Bò Úc',350000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet bò'),
    ('Combo Bò Đặc Biệt',520000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet bò'),
    ('Combo Hải Sản Tươi Sống',450000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet hải sản'),
    ('Combo Bò Nướng Tiêu Xanh',389000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet bò'),
    ('Combo Bò Gia Đình',359000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet bò'),
    ('Combo Bò Premium',420000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet bò'),
    ('Combo Hải Sản Biển Xanh',479000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet hải sản'),
    ('Combo Hải Sản Premium',629000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet hải sản'),
    ('Combo Gà Nướng Mật Ong',329000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet gà'),
    ('Combo Gà Lá Chanh',339000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet gà'),
    ('Combo Chay Thanh Đạm',299000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet chay'),
    ('Combo Chay Dinh Dưỡng',319000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet chay'),
    ('Combo Lẩu Nướng Tổng Hợp',459000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet lẩu nướng')
) AS v(menu_name, base_price, img_url, menu_category_name)
JOIN menu_category mc ON mc.menu_category_name = v.menu_category_name
WHERE NOT EXISTS (SELECT 1 FROM menu m WHERE m.menu_name = v.menu_name);

-- Menu-dish mapping (8 món/menu)
INSERT INTO menu_dish (menu_id, dish_id)
SELECT m.menu_id, d.dish_id
FROM (
    VALUES
    ('Combo Bò Úc','Bò cuộn nấm kim châm'),('Combo Bò Úc','Bò nướng sa tế'),('Combo Bò Úc','Bò áp chảo sốt tiêu đen'),('Combo Bò Úc','Bò lúc lắc khoai tây'),('Combo Bò Úc','Mì xào bò rau củ'),('Combo Bò Úc','Chè khúc bạch'),('Combo Bò Úc','Trà đào cam sả'),('Combo Bò Úc','Nước suối tinh khiết'),
    ('Combo Bò Đặc Biệt','Bò Wagyu nướng đá'),('Combo Bò Đặc Biệt','Sườn cừu nướng thảo mộc'),('Combo Bò Đặc Biệt','Sườn bò nướng mật ong'),('Combo Bò Đặc Biệt','Bò áp chảo sốt tiêu đen'),('Combo Bò Đặc Biệt','Lẩu bò nấm'),('Combo Bò Đặc Biệt','Chè hạt sen long nhãn'),('Combo Bò Đặc Biệt','Trà vải'),('Combo Bò Đặc Biệt','Nước chanh dây'),
    ('Combo Hải Sản Tươi Sống','Tôm sú nướng muối ớt'),('Combo Hải Sản Tươi Sống','Mực hấp gừng hành'),('Combo Hải Sản Tươi Sống','Hàu nướng phô mai'),('Combo Hải Sản Tươi Sống','Cơm chiên hải sản'),('Combo Hải Sản Tươi Sống','Sò điệp nướng mỡ hành'),('Combo Hải Sản Tươi Sống','Bánh flan caramel'),('Combo Hải Sản Tươi Sống','Nước chanh dây'),('Combo Hải Sản Tươi Sống','Nước suối tinh khiết'),
    ('Combo Bò Nướng Tiêu Xanh','Bò cuộn nấm kim châm'),('Combo Bò Nướng Tiêu Xanh','Bò nướng sa tế'),('Combo Bò Nướng Tiêu Xanh','Bò áp chảo sốt tiêu đen'),('Combo Bò Nướng Tiêu Xanh','Sườn bò nướng mật ong'),('Combo Bò Nướng Tiêu Xanh','Bò nhúng dấm'),('Combo Bò Nướng Tiêu Xanh','Chè khúc bạch'),('Combo Bò Nướng Tiêu Xanh','Trà đào cam sả'),('Combo Bò Nướng Tiêu Xanh','Nước suối tinh khiết'),
    ('Combo Bò Gia Đình','Bò cuộn nấm kim châm'),('Combo Bò Gia Đình','Bò lúc lắc khoai tây'),('Combo Bò Gia Đình','Gà nướng mật ong'),('Combo Bò Gia Đình','Mì xào bò rau củ'),('Combo Bò Gia Đình','Lẩu bò nấm'),('Combo Bò Gia Đình','Trái cây theo mùa'),('Combo Bò Gia Đình','Nước chanh dây'),('Combo Bò Gia Đình','Nước suối tinh khiết'),
    ('Combo Bò Premium','Bò Wagyu nướng đá'),('Combo Bò Premium','Sườn cừu nướng thảo mộc'),('Combo Bò Premium','Sườn bò nướng mật ong'),('Combo Bò Premium','Bò áp chảo sốt tiêu đen'),('Combo Bò Premium','Lẩu bò nấm'),('Combo Bò Premium','Chè hạt sen long nhãn'),('Combo Bò Premium','Trà vải'),('Combo Bò Premium','Nước chanh dây'),
    ('Combo Hải Sản Biển Xanh','Tôm sú nướng muối ớt'),('Combo Hải Sản Biển Xanh','Mực hấp gừng hành'),('Combo Hải Sản Biển Xanh','Hàu nướng phô mai'),('Combo Hải Sản Biển Xanh','Sò điệp nướng mỡ hành'),('Combo Hải Sản Biển Xanh','Cá tầm nướng muối ớt'),('Combo Hải Sản Biển Xanh','Chè khúc bạch'),('Combo Hải Sản Biển Xanh','Trà đào cam sả'),('Combo Hải Sản Biển Xanh','Nước suối tinh khiết'),
    ('Combo Hải Sản Premium','Tôm hùm bỏ lò phô mai'),('Combo Hải Sản Premium','Cua hoàng đế hấp'),('Combo Hải Sản Premium','Cá hồi sốt bơ tỏi'),('Combo Hải Sản Premium','Ghẹ rang me'),('Combo Hải Sản Premium','Lẩu hải sản Tomyum'),('Combo Hải Sản Premium','Chè hạt sen long nhãn'),('Combo Hải Sản Premium','Trà vải'),('Combo Hải Sản Premium','Nước chanh dây'),
    ('Combo Gà Nướng Mật Ong','Gà nướng mật ong'),('Combo Gà Nướng Mật Ong','Gà chiên nước mắm'),('Combo Gà Nướng Mật Ong','Gà hấp lá chanh'),('Combo Gà Nướng Mật Ong','Lẩu gà lá é'),('Combo Gà Nướng Mật Ong','Rau củ xào ngũ sắc'),('Combo Gà Nướng Mật Ong','Trái cây theo mùa'),('Combo Gà Nướng Mật Ong','Trà vải'),('Combo Gà Nướng Mật Ong','Nước suối tinh khiết'),
    ('Combo Gà Lá Chanh','Gà hấp lá chanh'),('Combo Gà Lá Chanh','Gà chiên nước mắm'),('Combo Gà Lá Chanh','Lẩu gà lá é'),('Combo Gà Lá Chanh','Rau củ xào ngũ sắc'),('Combo Gà Lá Chanh','Đậu hũ chiên sả ớt'),('Combo Gà Lá Chanh','Chè khúc bạch'),('Combo Gà Lá Chanh','Trà đào cam sả'),('Combo Gà Lá Chanh','Nước suối tinh khiết'),
    ('Combo Chay Thanh Đạm','Nấm đùi gà nướng giấy bạc'),('Combo Chay Thanh Đạm','Đậu hũ non sốt nấm đông cô'),('Combo Chay Thanh Đạm','Đậu hũ chiên sả ớt'),('Combo Chay Thanh Đạm','Rau củ xào ngũ sắc'),('Combo Chay Thanh Đạm','Lẩu nấm chay'),('Combo Chay Thanh Đạm','Trái cây theo mùa'),('Combo Chay Thanh Đạm','Nước chanh dây'),('Combo Chay Thanh Đạm','Nước suối tinh khiết'),
    ('Combo Chay Dinh Dưỡng','Nấm đùi gà nướng giấy bạc'),('Combo Chay Dinh Dưỡng','Đậu hũ non sốt nấm đông cô'),('Combo Chay Dinh Dưỡng','Đậu hũ chiên sả ớt'),('Combo Chay Dinh Dưỡng','Rau củ xào ngũ sắc'),('Combo Chay Dinh Dưỡng','Lẩu nấm chay'),('Combo Chay Dinh Dưỡng','Chè hạt sen long nhãn'),('Combo Chay Dinh Dưỡng','Trà vải'),('Combo Chay Dinh Dưỡng','Nước suối tinh khiết'),
    ('Combo Lẩu Nướng Tổng Hợp','Lẩu bò nấm'),('Combo Lẩu Nướng Tổng Hợp','Lẩu hải sản Tomyum'),('Combo Lẩu Nướng Tổng Hợp','Lẩu gà lá é'),('Combo Lẩu Nướng Tổng Hợp','Bò nướng sa tế'),('Combo Lẩu Nướng Tổng Hợp','Tôm sú nướng muối ớt'),('Combo Lẩu Nướng Tổng Hợp','Cơm chiên hải sản'),('Combo Lẩu Nướng Tổng Hợp','Trà đào cam sả'),('Combo Lẩu Nướng Tổng Hợp','Nước suối tinh khiết')
) AS x(menu_name, dish_name)
JOIN menu m ON m.menu_name = x.menu_name
JOIN dish d ON d.dish_name = x.dish_name
ON CONFLICT (menu_id, dish_id) DO NOTHING;

-- Link to party categories
INSERT INTO party_category_menu (party_category_id, menu_id)
SELECT pc.party_category_id, m.menu_id
FROM (
    VALUES
    ('Tiệc cưới','Combo Bò Úc'),
    ('Tiệc cưới','Combo Bò Đặc Biệt'),
    ('Tiệc cưới','Combo Hải Sản Premium'),
    ('Tiệc cưới','Combo Bò Premium'),
    ('Tiệc sinh nhật','Combo Bò Gia Đình'),
    ('Tiệc sinh nhật','Combo Gà Nướng Mật Ong'),
    ('Tiệc sinh nhật','Combo Chay Thanh Đạm'),
    ('Tiệc doanh nghiệp','Combo Hải Sản Tươi Sống'),
    ('Tiệc doanh nghiệp','Combo Hải Sản Biển Xanh'),
    ('Tiệc doanh nghiệp','Combo Gà Lá Chanh'),
    ('Tiệc doanh nghiệp','Combo Lẩu Nướng Tổng Hợp')
) AS x(party_category_name, menu_name)
JOIN party_category pc ON pc.party_category_name = x.party_category_name
JOIN menu m ON m.menu_name = x.menu_name
ON CONFLICT (party_category_id, menu_id) DO NOTHING;

-- Feedback menu (insert tường minh theo từng menu)
INSERT INTO feedback_menu (order_id, order_detail_id, menu_id, customer_id, rating, comment, img, status) VALUES
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Bò Úc' LIMIT 1), 4, 5, 'Thịt bò mềm, nêm nếm vừa, món nóng lên nhanh.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Bò Úc' LIMIT 1), 4, 5, 'Khẩu phần ổn, hợp tiệc gia đình, tráng miệng khá ngon.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Bò Đặc Biệt' LIMIT 1), 4, 5, 'Món premium rõ rệt, đặc biệt phần bò Wagyu rất chất lượng.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Bò Đặc Biệt' LIMIT 1), 4, 5, 'Đồ ăn ngon nhưng giá cao, phù hợp tiệc cần sự sang trọng.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Hải Sản Tươi Sống' LIMIT 1), 4, 5, 'Hải sản tươi, mực và tôm ổn định, lên món đúng giờ.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Hải Sản Tươi Sống' LIMIT 1), 4, 5, 'Chất lượng ổn nhưng phần nước chấm hôm đó hơi nhạt.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Bò Nướng Tiêu Xanh' LIMIT 1), 4, 5, 'Vị tiêu xanh đậm, các món nướng thơm và giữ nhiệt tốt.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Bò Nướng Tiêu Xanh' LIMIT 1), 4, 5, 'Menu cân bằng, có đủ món chính và món mát cuối bữa.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Bò Gia Đình' LIMIT 1), 4, 5, 'Người lớn tuổi ăn hợp, món không quá đậm vị.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Bò Gia Đình' LIMIT 1), 4, 5, 'Tầm giá tốt, khẩu phần đủ nhiều cho nhóm 6-8 người.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Bò Premium' LIMIT 1), 4, 5, 'Trình bày đẹp, nguyên liệu tốt, rất phù hợp tiệc cưới.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Bò Premium' LIMIT 1), 4, 5, 'Món ngon, chỉ tiếc thời gian ra món cuối hơi chậm.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Hải Sản Biển Xanh' LIMIT 1), 4, 5, 'Các món hải sản tươi, vị tổng thể hài hòa.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Hải Sản Biển Xanh' LIMIT 1), 4, 5, 'Sò điệp và hàu rất nổi bật, khách khen nhiều.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Hải Sản Premium' LIMIT 1), 4, 5, 'Menu premium đáng tiền, món chủ lực chất lượng cao.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Hải Sản Premium' LIMIT 1), 4, 5, 'Món ngon đồng đều, phù hợp tiếp khách doanh nghiệp.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Gà Nướng Mật Ong' LIMIT 1), 4, 5, 'Món gà thơm, trẻ em ăn được, tổng thể dễ dùng.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Gà Nướng Mật Ong' LIMIT 1), 4, 5, 'Hương vị ổn nhưng phần tráng miệng hơi đơn giản.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Gà Lá Chanh' LIMIT 1), 4, 5, 'Mùi lá chanh đặc trưng, các món gà làm rất tròn vị.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Gà Lá Chanh' LIMIT 1), 4, 5, 'Menu thanh vị, phù hợp tiệc thân mật và sinh nhật.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Chay Thanh Đạm' LIMIT 1), 4, 5, 'Món chay nhẹ bụng, nêm nếm vừa, dễ ăn cho nhiều người.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Chay Thanh Đạm' LIMIT 1), 4, 5, 'Rau củ tươi, món lên đẹp và không bị ngấy.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Chay Dinh Dưỡng' LIMIT 1), 4, 5, 'Thực đơn chay đa dạng, khẩu phần hợp lý theo giá.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Chay Dinh Dưỡng' LIMIT 1), 4, 5, 'Món chay trình bày tốt, khách lớn tuổi rất hài lòng.', NULL, 'ACTIVE'),

(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Lẩu Nướng Tổng Hợp' LIMIT 1), 4, 5, 'Kết hợp lẩu nướng hợp lý, đi nhóm đông rất phù hợp.', NULL, 'ACTIVE'),
(1, 1, (SELECT menu_id FROM menu WHERE menu_name = 'Combo Lẩu Nướng Tổng Hợp' LIMIT 1), 4, 5, 'Đồ ăn lên đều, không bị ngắt quãng trong suốt bữa tiệc.', NULL, 'ACTIVE');
