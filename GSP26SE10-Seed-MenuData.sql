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
    ('Món chính', 'Món chính'),
    ('Món tráng miệng', 'Món tráng miệng'),
    ('Nước uống', 'Nước uống')
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
    ('Thịt bò', 'Thịt bò cao cấp', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775555110/ingredient/thitbo.png'),
    ('Tôm sú', 'Tôm sú tươi sống', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556177/ingredient/tomsu.png'),
    ('Mực tươi', 'Mực tươi cho món hấp, nướng', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556228/ingredient/muctuoi.png'),
    ('Cá hồi', 'Phi lê cá hồi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556282/ingredient/cahoi.png'),
    ('Thịt gà', 'Thịt gà tươi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775555058/ingredient/thitga.png'),
    ('Nấm kim châm', 'Nấm kim châm tươi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556324/ingredient/namkimcham.png'),
    ('Rau củ tổng hợp', 'Rau củ theo mùa', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775557074/ingredient/raucutonghop.png'),
    ('Đậu hũ non', 'Đậu hũ non cho món chay', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556402/ingredient/dauhunon.png'),
    ('Cua hoàng đế', 'Cua hoàng đế tươi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556450/ingredient/cuahoangde.png'),
    ('Cá tầm', 'Cá tầm tươi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556505/ingredient/catam.png'),
    ('Hạt sen', 'Hạt sen dùng cho món chè', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556590/ingredient/hatsen.png'),
    ('Vải tươi', 'Trái vải tươi cho thức uống/tráng miệng', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556669/ingredient/vaituoi.png'),
    ('Thịt cừu', 'Thịt cừu cho món nướng', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556713/ingredient/thitcuu.png'),
    ('Hàu tươi', 'Hàu tươi dùng cho món nướng', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556751/ingredient/hautuoi.png'),
    ('Sò điệp', 'Sò điệp tươi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556811/ingredient/sodiep.png'),
    ('Tôm hùm', 'Tôm hùm tươi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556847/ingredient/tomhum.png'),
    ('Ghẹ tươi', 'Ghẹ tươi cho món rang me', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556884/ingredient/ghetuoi.png'),
    ('Sữa tươi', 'Sữa tươi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775555227/ingredient/suatuoi.png'),
    ('Chanh dây', 'Chanh dây tươi cho thức uống', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556926/ingredient/chanhday.png'),
    ('Đào', 'Đào ngâm/đào tươi cho thức uống', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775556968/ingredient/dao.png'),
    ('Cam tươi', 'Cam tươi', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775555272/ingredient/camtuoi.png'),
    ('Nước suối', 'Nước uống đóng chai', 'https://res.cloudinary.com/dl0dri4pf/image/upload/v1775557027/ingredient/nuocsuoi.png')
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
    ('Bò cuộn nấm kim châm',165000,'Thịt bò cuộn nấm kim châm nướng thơm, vị đậm đà.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775557271/dish/bocuonnamkimcham.png','Món chính','Dùng ngon với sốt tiêu đen'),
    ('Bò nướng sa tế',175000,'Bò ướp sa tế cay nhẹ, nướng vừa chín tới.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775557434/dish/bonuongsate.png','Món chính','Cấp độ cay vừa'),
    ('Bò lúc lắc khoai tây',185000,'Bò lúc lắc mềm mọng ăn kèm khoai tây chiên.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775557484/dish/boluclackhoaitay.png','Món chính','Món bán chạy'),
    ('Bò áp chảo sốt tiêu đen',198000,'Thịt bò áp chảo sốt tiêu đen thơm nồng.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775557548/dish/boapchaosottieuden.png','Món chính','Khuyên dùng chín vừa'),
    ('Sườn bò nướng mật ong',210000,'Sườn bò nướng mật ong vị ngọt mặn hài hòa.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775557640/dish/suonnuongmatong.png','Món chính','Món chủ lực nhóm bò'),
    ('Lẩu bò nấm',249000,'Lẩu bò nấm nóng hổi với nước dùng thanh ngọt.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775557713/dish/laubonam.png','Món chính','Khẩu phần 3-4 người'),
    ('Bò nhúng dấm',195000,'Bò thái lát nhúng dấm ăn kèm rau tươi.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775557803/dish/bonhungdam.pngg','Món chính','Phong cách truyền thống'),
    ('Tôm sú nướng muối ớt',189000,'Tôm sú tươi nướng muối ớt đậm vị biển.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775557885/dish/tomsunuongmuoiot.png','Món chính','Cay nhẹ'),
    ('Mực hấp gừng hành',168000,'Mực hấp gừng hành giữ độ ngọt tự nhiên.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775557945/dish/muchapgunghanh.png','Món chính','Mềm và thơm'),
    ('Cá hồi sốt bơ tỏi',238000,'Phi lê cá hồi áp chảo với sốt bơ tỏi.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558003/dish/cahoisotbotoi.png','Món chính','Béo nhẹ, thơm tỏi'),
    ('Hàu nướng phô mai',179000,'Hàu nướng phủ phô mai béo thơm.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558058/dish/haunuongphomai.png','Món chính','Phù hợp tiệc tối'),
    ('Ghẹ rang me',259000,'Ghẹ rang me vị chua ngọt hài hòa.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558103/dish/gherangme.png','Món chính','Đặc trưng hải sản'),
    ('Sò điệp nướng mỡ hành',229000,'Sò điệp nướng mỡ hành rắc đậu phộng.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558149/dish/sodiepnuongmohanh.png','Món chính','Thơm béo'),
    ('Lẩu hải sản Tomyum',279000,'Lẩu hải sản vị Tomyum chua cay kiểu Thái.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558228/dish/lauhaisantomyum.png','Món chính','Khẩu phần 4-5 người'),
    ('Gà nướng mật ong',150000,'Gà nướng mật ong mềm thơm','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775554489/dish/ganuongmatong.png','Món chính','Có thể chọn mức cay nhẹ'),
    ('Gà hấp lá chanh',149000,'Gà hấp lá chanh thơm nhẹ, dễ ăn.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558298/dish/gahaplachanh.png','Món chính','Món truyền thống'),
    ('Gà chiên nước mắm',155000,'Gà chiên nước mắm đậm vị, vàng giòn.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558338/dish/gachiennuocmam.png','Món chính','Đưa cơm'),
    ('Lẩu gà lá é',189000,'Lẩu gà lá é thanh cay, mùi thơm đặc trưng.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558391/dish/laugalae.png','Món chính','Khẩu phần 3-4 người'),
    ('Nấm đùi gà nướng giấy bạc',119000,'Nấm đùi gà nướng giấy bạc thơm bơ tỏi.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558539/dish/namduiganuongiaybac.png','Món chính','Món chay phổ biến'),
    ('Đậu hũ non sốt nấm đông cô',109000,'Đậu hũ non sốt nấm đông cô thanh nhẹ.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558676/dish/dauhunonsotnamdongco.png','Món chính','Ít béo'),
    ('Rau củ xào ngũ sắc',99000,'Rau củ xào ngũ sắc giòn ngọt tự nhiên.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558724/dish/raucuxaongusac.png','Món chính','Bổ sung chất xơ'),
    ('Lẩu nấm chay',169000,'Lẩu nấm chay nước dùng thanh ngọt.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558775/dish/launamchay.png','Món chính','Phù hợp tiệc chay'),
    ('Chè khúc bạch',49000,'Chè khúc bạch thanh mát ăn kèm trái cây.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558822/dish/chekhucbach.png','Món tráng miệng','Dùng lạnh'),
    ('Bánh flan caramel',39000,'Bánh flan mềm mịn, vị caramel nhẹ.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558870/dish/banhflancaramel.png','Món tráng miệng','Món tráng miệng quen thuộc'),
    ('Trái cây theo mùa',45000,'Đĩa trái cây tươi thay đổi theo mùa.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558926/dish/traicaytheomua.png','Món tráng miệng','Thanh mát sau bữa ăn'),
    ('Trà đào cam sả',42000,'Trà đào cam sả thơm mát, dễ uống.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775558982/dish/tradaocamsa.png','Nước uống','Món nước bán chạy'),
    ('Nước chanh dây',35000,'Nước chanh dây chua ngọt hài hòa.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559034/dish/nuocchanhday.png','Nước uống','Giải khát tốt'),
    ('Nước suối tinh khiết',18000,'Nước suối đóng chai tiện lợi.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559150/dish/nuocsuoitinhkhiet.png','Nước uống','Tính theo chai'),
    ('Bò Wagyu nướng đá',320000,'Thịt bò Wagyu nướng trên đá nóng, giữ trọn vị ngọt thịt.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559097/dish/bowagyunuonda.png','Món chính','Món cao cấp cho menu premium'),
    ('Sườn cừu nướng thảo mộc',285000,'Sườn cừu nướng cùng thảo mộc, thơm mềm đặc trưng.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559195/dish/suoncuunuongthaomoc.png','Món chính','Phù hợp tiệc cao cấp'),
    ('Tôm hùm bỏ lò phô mai',359000,'Tôm hùm bỏ lò phủ phô mai béo ngậy.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559258/dish/tomhumlophomai.png','Món chính','Món nổi bật nhóm hải sản premium'),
    ('Cua hoàng đế hấp',399000,'Cua hoàng đế hấp giữ vị ngọt tự nhiên.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559323/dish/cuahoandehap.png','Món chính','Khẩu phần cao cấp'),
    ('Cá tầm nướng muối ớt',269000,'Cá tầm nướng muối ớt đậm đà, thịt chắc.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559377/dish/catamnuongmuoiot.png','Món chính','Hợp tiệc doanh nghiệp'),
    ('Cơm chiên hải sản',119000,'Cơm chiên hải sản tơi ngon, đậm vị.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559425/dish/comchienhaisan.png','Món chính','Món tinh bột cân bằng thực đơn'),
    ('Mì xào bò rau củ',109000,'Mì xào bò cùng rau củ giòn ngọt.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559469/dish/mixaoboraucu.png','Món chính','Phù hợp menu gia đình'),
    ('Đậu hũ chiên sả ớt',89000,'Đậu hũ chiên giòn cùng sả ớt thơm cay nhẹ.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559518/dish/dauhuchienxaot.png','Món chính','Món chay phổ thông'),
    ('Chè hạt sen long nhãn',55000,'Chè hạt sen long nhãn thanh ngọt, dễ dùng sau bữa tiệc.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559575/dish/chehatsenlongnhan.png','Món tráng miệng','Món tráng miệng truyền thống'),
    ('Trà vải',39000,'Trà vải mát lạnh, thơm nhẹ vị trái cây.','https://res.cloudinary.com/dl0dri4pf/image/upload/v1775559625/dish/travai.png','Nước uống','Hợp khẩu vị số đông')
) AS v(dish_name, price, description, img, category_name, note)
JOIN dish_category dc ON dc.dish_category_name = v.category_name
WHERE NOT EXISTS (SELECT 1 FROM dish d WHERE d.dish_name = v.dish_name);

-- Dish detail (mapping món -> nhiều nguyên liệu, idempotent)
INSERT INTO dish_detail (dish_id, ingredient_id)
SELECT d.dish_id, i.ingredient_id
FROM (
    VALUES
    ('Bò cuộn nấm kim châm', 'Thịt bò'),
    ('Bò cuộn nấm kim châm', 'Nấm kim châm'),
    ('Bò nướng sa tế', 'Thịt bò'),
    ('Bò nướng sa tế', 'Rau củ tổng hợp'),
    ('Bò lúc lắc khoai tây', 'Thịt bò'),
    ('Bò lúc lắc khoai tây', 'Rau củ tổng hợp'),
    ('Bò áp chảo sốt tiêu đen', 'Thịt bò'),
    ('Sườn bò nướng mật ong', 'Thịt bò'),
    ('Sườn bò nướng mật ong', 'Rau củ tổng hợp'),
    ('Lẩu bò nấm', 'Thịt bò'),
    ('Lẩu bò nấm', 'Nấm kim châm'),
    ('Lẩu bò nấm', 'Rau củ tổng hợp'),
    ('Bò nhúng dấm', 'Thịt bò'),
    ('Bò nhúng dấm', 'Rau củ tổng hợp'),
    ('Tôm sú nướng muối ớt', 'Tôm sú'),
    ('Tôm sú nướng muối ớt', 'Rau củ tổng hợp'),
    ('Mực hấp gừng hành', 'Mực tươi'),
    ('Mực hấp gừng hành', 'Rau củ tổng hợp'),
    ('Cá hồi sốt bơ tỏi', 'Cá hồi'),
    ('Cá hồi sốt bơ tỏi', 'Rau củ tổng hợp'),
    ('Hàu nướng phô mai', 'Hàu tươi'),
    ('Hàu nướng phô mai', 'Rau củ tổng hợp'),
    ('Ghẹ rang me', 'Ghẹ tươi'),
    ('Ghẹ rang me', 'Rau củ tổng hợp'),
    ('Sò điệp nướng mỡ hành', 'Sò điệp'),
    ('Sò điệp nướng mỡ hành', 'Rau củ tổng hợp'),
    ('Lẩu hải sản Tomyum', 'Tôm sú'),
    ('Lẩu hải sản Tomyum', 'Mực tươi'),
    ('Lẩu hải sản Tomyum', 'Rau củ tổng hợp'),
    ('Gà nướng mật ong', 'Thịt gà'),
    ('Gà nướng mật ong', 'Rau củ tổng hợp'),
    ('Gà hấp lá chanh', 'Thịt gà'),
    ('Gà hấp lá chanh', 'Rau củ tổng hợp'),
    ('Gà chiên nước mắm', 'Thịt gà'),
    ('Gà chiên nước mắm', 'Rau củ tổng hợp'),
    ('Lẩu gà lá é', 'Thịt gà'),
    ('Lẩu gà lá é', 'Rau củ tổng hợp'),
    ('Nấm đùi gà nướng giấy bạc', 'Nấm kim châm'),
    ('Nấm đùi gà nướng giấy bạc', 'Rau củ tổng hợp'),
    ('Đậu hũ non sốt nấm đông cô', 'Đậu hũ non'),
    ('Đậu hũ non sốt nấm đông cô', 'Nấm kim châm'),
    ('Rau củ xào ngũ sắc', 'Rau củ tổng hợp'),
    ('Lẩu nấm chay', 'Nấm kim châm'),
    ('Lẩu nấm chay', 'Đậu hũ non'),
    ('Lẩu nấm chay', 'Rau củ tổng hợp'),
    ('Chè khúc bạch', 'Rau củ tổng hợp'),
    ('Bánh flan caramel', 'Sữa tươi'),
    ('Trái cây theo mùa', 'Rau củ tổng hợp'),
    ('Trà đào cam sả', 'Đào'),
    ('Trà đào cam sả', 'Cam'),
    ('Nước chanh dây', 'Chanh dây'),
    ('Nước suối tinh khiết', 'Nước suối'),
    ('Bò Wagyu nướng đá', 'Thịt bò'),
    ('Bò Wagyu nướng đá', 'Rau củ tổng hợp'),
    ('Sườn cừu nướng thảo mộc', 'Thịt cừu'),
    ('Sườn cừu nướng thảo mộc', 'Rau củ tổng hợp'),
    ('Tôm hùm bỏ lò phô mai', 'Tôm hùm'),
    ('Tôm hùm bỏ lò phô mai', 'Rau củ tổng hợp'),
    ('Cua hoàng đế hấp', 'Cua hoàng đế'),
    ('Cua hoàng đế hấp', 'Rau củ tổng hợp'),
    ('Cá tầm nướng muối ớt', 'Cá tầm'),
    ('Cá tầm nướng muối ớt', 'Rau củ tổng hợp'),
    ('Cơm chiên hải sản', 'Tôm sú'),
    ('Cơm chiên hải sản', 'Rau củ tổng hợp'),
    ('Mì xào bò rau củ', 'Thịt bò'),
    ('Mì xào bò rau củ', 'Rau củ tổng hợp'),
    ('Đậu hũ chiên sả ớt', 'Đậu hũ non'),
    ('Đậu hũ chiên sả ớt', 'Rau củ tổng hợp'),
    ('Chè hạt sen long nhãn', 'Hạt sen'),
    ('Trà vải', 'Vải tươi')
) AS x(dish_name, ingredient_name)
JOIN dish d ON d.dish_name = x.dish_name
JOIN ingredient i ON i.ingredient_name = x.ingredient_name
WHERE NOT EXISTS (
    SELECT 1
    FROM dish_detail dd
    WHERE dd.dish_id = d.dish_id
      AND dd.ingredient_id = i.ingredient_id
);

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
    ('Combo Bò Nướng Tiêu Xanh',389000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775139380/menu/botieuxanh1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775139389/menu/botieuxanh2.png"]','Buffet bò'),
    ('Combo Bò Gia Đình',359000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775139869/menu/bogiadinh.png"]','Buffet bò'),
    ('Combo Bò Premium',420000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet bò'),
    ('Combo Hải Sản Premium',629000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1772536026/menu/menu1.png"]','Buffet hải sản'),
    ('Combo Gà Nướng Mật Ong',329000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138483/menu/ganuongmatong1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138522/menu/ganuongmatong2.png"]','Buffet gà'),
    ('Combo Gà Lá Chanh',339000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138634/menu/galachanh1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138645/menu/galachanh2.png"]','Buffet gà'),
    ('Combo Chay Thanh Đạm',299000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138741/menu/chaythanhdam1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138780/menu/chaythanhdam2.png"]','Buffet chay'),
    ('Combo Chay Dinh Dưỡng',319000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138904/menu/chaydinhduong1.png","https://res.cloudinary.com/dl0dri4pf/image/upload/v1775138922/menu/chaydinhduong2.png"]','Buffet chay'),
    ('Combo Lẩu Nướng Tổng Hợp',459000,'["https://res.cloudinary.com/dl0dri4pf/image/upload/v1775139011/menu/tonghop.png"]','Buffet lẩu nướng')
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
    ('Combo Bò Việt','Bò cuộn nấm kim châm'),('Combo Bò Việt','Bò nướng sa tế'),('Combo Bò Việt','Bò lúc lắc khoai tây'),('Combo Bò Việt','Mì xào bò rau củ'),('Combo Bò Việt','Rau củ xào ngũ sắc'),('Combo Bò Việt','Chè khúc bạch'),('Combo Bò Việt','Trà đào cam sả'),('Combo Bò Việt','Nước suối tinh khiết'),
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

-- feedback_menu: không seed (cần order_id / order_detail_id thật; để trống cho dữ liệu runtime)
