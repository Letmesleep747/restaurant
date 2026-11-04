-- Extra menu items + images for Customer browsing (safe / idempotent).
-- Does not delete existing data.

USE [RROMS];
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MenuCategories WHERE Name = N'Dessert')
BEGIN
    INSERT INTO dbo.MenuCategories (Name, Description, IsActive, CreatedAt)
    VALUES (N'Dessert', N'Sweet finishes', 1, SYSUTCDATETIME());
END
GO

DECLARE @starter INT = (SELECT TOP 1 Id FROM dbo.MenuCategories WHERE Name = N'Starter');
DECLARE @main INT = (SELECT TOP 1 Id FROM dbo.MenuCategories WHERE Name = N'Main Course');
DECLARE @bev INT = (SELECT TOP 1 Id FROM dbo.MenuCategories WHERE Name = N'Beverage');
DECLARE @dessert INT = (SELECT TOP 1 Id FROM dbo.MenuCategories WHERE Name = N'Dessert');

UPDATE dbo.MenuItems SET ImageUrl = N'https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?auto=format&fit=crop&w=900&q=80' WHERE Name = N'Margherita Pizza';
UPDATE dbo.MenuItems SET ImageUrl = N'https://images.unsplash.com/photo-1546793665-c74683f339c1?auto=format&fit=crop&w=900&q=80' WHERE Name = N'Caesar Salad';
UPDATE dbo.MenuItems SET ImageUrl = N'https://images.unsplash.com/photo-1523362628745-0c100150b504?auto=format&fit=crop&w=900&q=80' WHERE Name = N'Sparkling Water';
UPDATE dbo.MenuItems SET ImageUrl = N'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?auto=format&fit=crop&w=900&q=80' WHERE Name = N'Grilled Chicken';
UPDATE dbo.MenuItems SET ImageUrl = N'https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=900&q=80' WHERE Name = N'Iced Lemon Tea';

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Tomato Bruschetta')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@starter, N'Tomato Bruschetta', N'Toasted bread with tomatoes, basil, and olive oil.', 5.50, 1, SYSUTCDATETIME(), N'Starter', N'https://images.unsplash.com/photo-1572695157366-5e585ab2b69f?auto=format&fit=crop&w=900&q=80');

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Soup of the Day')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@starter, N'Soup of the Day', N'Chef''s daily soup served with warm bread.', 4.80, 1, SYSUTCDATETIME(), N'Starter', N'https://images.unsplash.com/photo-1547592166-23ac45744acd?auto=format&fit=crop&w=900&q=80');

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Garlic Prawns')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@starter, N'Garlic Prawns', N'Sautéed prawns in garlic butter and herbs.', 8.90, 1, SYSUTCDATETIME(), N'Starter', N'https://images.unsplash.com/photo-1565680018434-b513d5e5fd47?auto=format&fit=crop&w=900&q=80');

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Beef Lasagna')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@main, N'Beef Lasagna', N'Layered pasta with rich beef ragu and cheese.', 13.90, 1, SYSUTCDATETIME(), N'Main Course', N'https://images.unsplash.com/photo-1574894709920-11b28e7367e3?auto=format&fit=crop&w=900&q=80');

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Salmon Fillet')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@main, N'Salmon Fillet', N'Pan-seared salmon with lemon butter sauce.', 16.50, 1, SYSUTCDATETIME(), N'Main Course', N'https://images.unsplash.com/photo-1467003909585-2f8a72700288?auto=format&fit=crop&w=900&q=80');

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Classic Burger')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@main, N'Classic Burger', N'Beef patty, lettuce, tomato, and house sauce.', 11.90, 1, SYSUTCDATETIME(), N'Main Course', N'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=900&q=80');

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Vegetable Pasta')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@main, N'Vegetable Pasta', N'Tossed pasta with seasonal vegetables and pesto.', 10.50, 1, SYSUTCDATETIME(), N'Main Course', N'https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?auto=format&fit=crop&w=900&q=80');

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Fresh Orange Juice')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@bev, N'Fresh Orange Juice', N'Freshly squeezed orange juice.', 4.00, 1, SYSUTCDATETIME(), N'Beverage', N'https://images.unsplash.com/photo-1600271886742-f049cd451bba?auto=format&fit=crop&w=900&q=80');

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Cappuccino')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@bev, N'Cappuccino', N'Espresso with steamed milk and foam.', 3.80, 1, SYSUTCDATETIME(), N'Beverage', N'https://images.unsplash.com/photo-1572442388796-11668a67e53d?auto=format&fit=crop&w=900&q=80');

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Mango Smoothie')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@bev, N'Mango Smoothie', N'Creamy mango smoothie with yogurt.', 5.20, 1, SYSUTCDATETIME(), N'Beverage', N'https://images.unsplash.com/photo-1525385133512-2f3bdd039054?auto=format&fit=crop&w=900&q=80');

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Chocolate Brownie')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@dessert, N'Chocolate Brownie', N'Warm brownie served with vanilla ice cream.', 6.50, 1, SYSUTCDATETIME(), N'Dessert', N'https://images.unsplash.com/photo-1606313564200-e75d5e30476c?auto=format&fit=crop&w=900&q=80');

IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name = N'Cheesecake')
INSERT INTO dbo.MenuItems (MenuCategoryId, Name, Description, Price, IsAvailable, CreatedAt, Category, ImageUrl)
VALUES (@dessert, N'Cheesecake', N'Creamy baked cheesecake with berry topping.', 6.90, 1, SYSUTCDATETIME(), N'Dessert', N'https://images.unsplash.com/photo-1533134242443-d4fd215305ad?auto=format&fit=crop&w=900&q=80');

PRINT 'Menu seed with images completed.';
GO
