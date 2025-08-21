
IF DB_ID('StoreSale') IS NULL
BEGIN
    CREATE DATABASE StoreSale;
END
GO

USE StoreSale;
GO

-- 2. Xóa bảng nếu tồn tại
DROP TABLE IF EXISTS dbo.OrderDetails;
DROP TABLE IF EXISTS dbo.Orders;
DROP TABLE IF EXISTS dbo.Product;
DROP TABLE IF EXISTS dbo.Supplier;
DROP TABLE IF EXISTS dbo.Category;
DROP TABLE IF EXISTS dbo.PaymentMethod;
DROP TABLE IF EXISTS dbo.Employee;
DROP TABLE IF EXISTS dbo.Customer;
GO

-- 3. Tạo bảng Customer
CREATE TABLE Customer (
    CustomerID INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(200),
    CreatedAt DATETIME DEFAULT SYSUTCDATETIME()
);
GO

-- 4. Tạo bảng Employee
CREATE TABLE Employee (
    EmployeeID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeName NVARCHAR(100) NOT NULL,
    Position NVARCHAR(50),
    AuthorityLevel INT NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash VARBINARY(32) NOT NULL
);
GO

ALTER TABLE Employee
ADD Role NVARCHAR(20) NOT NULL DEFAULT 'SalesStaff';

-- Cập nhật role cho dữ liệu hiện có
UPDATE Employee
SET Role = CASE
    WHEN Position = 'Admin' THEN 'Admin'
    WHEN Position = 'Sales Staff' THEN 'SalesStaff'
    WHEN Position = 'Warehouse Staff' THEN 'WarehouseStaff'
END;

-- 5. Tạo bảng Category
CREATE TABLE Category (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL
);
GO

-- 6. Tạo bảng Supplier
CREATE TABLE Supplier (
    SupplierID INT PRIMARY KEY IDENTITY(1,1),
    SupplierName NVARCHAR(100) NOT NULL
);
GO

-- 7. Tạo bảng Product
CREATE TABLE Product (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) CHECK (Price > 0),
    CategoryID INT NULL,
    SupplierID INT NULL,
    StockQuantity INT DEFAULT 0,
    Description NVARCHAR(500) NULL,
    FOREIGN KEY (CategoryID) REFERENCES dbo.Category(CategoryID),
    FOREIGN KEY (SupplierID) REFERENCES dbo.Supplier(SupplierID)
);

-- 8. Tạo bảng PaymentMethod
CREATE TABLE PaymentMethod (
    PaymentMethodID INT PRIMARY KEY IDENTITY(1,1),
    MethodName NVARCHAR(50) NOT NULL
);
GO

-- 9. Tạo bảng Orders
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATE NOT NULL,
    CustomerID INT NOT NULL,
    EmployeeID INT NOT NULL,
    PaymentMethodID INT NOT NULL,
    TotalAmount DECIMAL(18,2) DEFAULT 0,
    OrderStatus NVARCHAR(50) NOT NULL DEFAULT 'Complete',
    FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID),
    FOREIGN KEY (EmployeeID) REFERENCES Employee(EmployeeID) ON DELETE CASCADE,
    FOREIGN KEY (PaymentMethodID) REFERENCES PaymentMethod(PaymentMethodID)
);
GO
ALTER TABLE Orders
ADD OrderStatus NVARCHAR(50) NOT NULL DEFAULT 'Complete';

-- 10. Tạo bảng OrderDetails
CREATE TABLE OrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) CHECK (UnitPrice >= 0),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
);
GO

-- 11. 

-- Customers
INSERT INTO Customer (CustomerName, Address, Phone, Email) VALUES
(N'Nguyễn Văn A', N'Hà Nội', '0901234567', 'a@gmail.com'),
(N'Trần Thị B', N'Hồ Chí Minh', '0902345678', 'b@gmail.com'),
(N'Lê Văn C', N'Đà Nẵng', '0903456789', 'c@gmail.com'),
(N'Phạm Thị D', N'Cần Thơ', '0904567890', 'd@gmail.com'),
(N'Hoàng Văn E', N'Hải Phòng', '0905678901', 'e@gmail.com');

-- Employees
INSERT INTO Employee (EmployeeName, Position, AuthorityLevel, Username, PasswordHash, Role)
VALUES
(N'Le Viet Cuong', 'Admin', 5, N'cuonglv', HASHBYTES('SHA2_256', N'3010'), 'Admin'),
(N'Nguyen Van Binh', 'Sales Staff', 2, N'nvb222', HASHBYTES('SHA2_256', N'333'), 'SalesStaff'),
(N'Pham Van An', 'Warehouse Staff', 2, N'pva1', HASHBYTES('SHA2_256', N'111'), 'WarehouseStaff');

ALTER TABLE Employee
ADD CONSTRAINT DF_Employee_AuthorityLevel DEFAULT 1 FOR AuthorityLevel;
-- Payment Methods
INSERT INTO PaymentMethod (MethodName) VALUES
('Cash'), ('Bank Transfer'), ('Credit Card');

-- Categories
INSERT INTO Category (CategoryName) VALUES
('Dairy'), ('Bakery'), ('Beverage'), ('Instant Food');


-- Suppliers
INSERT INTO Supplier (SupplierName) VALUES
('Viet Cuong'), ('Van Binh'), ('Van Anh');

-- Products	
INSERT INTO Product (ProductName, Price, CategoryID, SupplierID, StockQuantity) VALUES
('Milk', 25000, 1, 1, 50),
('Breads', 15000, 2, 2, 100),
('Soft drink', 12000, 3, 3, 200),
('Instant noodles', 5000, 4, 3, 300);

-- Orders
INSERT INTO Orders (OrderDate, CustomerID, EmployeeID, PaymentMethodID) VALUES
('2025-08-10', 1, 1, 1),
('2025-08-11', 2, 2, 2),
('2025-08-12', 3, 3, 3);

-- OrderDetails
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice) VALUES
(1, 1, 2, 25000),
(1, 2, 3, 15000),
(2, 3, 5, 12000),
(3, 4, 10, 5000);

-- 12. Trigger tính tổng tiền Orders
CREATE OR ALTER TRIGGER trg_UpdateTotalAmount
ON OrderDetails
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    UPDATE o
    SET o.TotalAmount = (
        SELECT SUM(Quantity * UnitPrice)
        FROM OrderDetails
        WHERE OrderID = o.OrderID
    )
    FROM Orders o
    WHERE o.OrderID IN (
        SELECT DISTINCT OrderID FROM inserted
        UNION
        SELECT DISTINCT OrderID FROM deleted
    );
END;
GO

-- 13. Cập nhật TotalAmount cho dữ liệu hiện có
UPDATE o
SET o.TotalAmount = (
    SELECT SUM(Quantity * UnitPrice)
    FROM OrderDetails
    WHERE OrderID = o.OrderID
)
FROM Orders o;
GO

-- 14. Kiểm tra dữ liệu
SELECT * FROM Customer;
SELECT * FROM Employee;
SELECT * FROM PaymentMethod;
SELECT * FROM Category;
SELECT * FROM Supplier;
SELECT * FROM Product;
SELECT * FROM Orders;
SELECT * FROM OrderDetails;
GO
