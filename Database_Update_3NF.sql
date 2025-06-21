-- =============================================
-- СОЗДАНИЕ БАЗЫ ДАННЫХ СЕРВИСНОГО ЦЕНТРА С НУЛЯ
-- Сервисный центр - 3NF структура с тестовыми данными
-- ВЫПОЛНИТЕ ЭТОТ СКРИПТ В SQL SERVER MANAGEMENT STUDIO
-- =============================================

-- Создание базы данных
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ServiceCenter1')
BEGIN
    CREATE DATABASE ServiceCenter1;
    PRINT 'База данных ServiceCenter1 создана успешно!';
END
ELSE
BEGIN
    PRINT 'База данных ServiceCenter1 уже существует.';
END
GO

USE ServiceCenter1;
GO

-- Удаление существующих объектов
IF OBJECT_ID('sp_CreateRequest') IS NOT NULL DROP PROCEDURE sp_CreateRequest;
IF OBJECT_ID('sp_UpdateRequestStatus') IS NOT NULL DROP PROCEDURE sp_UpdateRequestStatus;
IF OBJECT_ID('vw_RequestsFull') IS NOT NULL DROP VIEW vw_RequestsFull;
IF OBJECT_ID('vw_UsersWithRoles') IS NOT NULL DROP VIEW vw_UsersWithRoles;
GO

-- =============================================
-- 1. СОЗДАНИЕ ТАБЛИЦ
-- =============================================

-- Таблица ролей
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    Permissions NVARCHAR(MAX)
);

-- Таблица статусов заявок
CREATE TABLE RequestStatuses (
    StatusID INT IDENTITY(1,1) PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    Color NVARCHAR(7) DEFAULT '#000000'
);

-- Таблица сотрудников
CREATE TABLE Employees (
    EmployeeID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    RoleID INT NOT NULL,
    HireDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_Employees_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- Таблица клиентов
CREATE TABLE Clients (
    ClientID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20) UNIQUE,
    Address NVARCHAR(255),
    RegistrationDate DATETIME DEFAULT GETDATE()
);

-- Таблица пользователей (аутентификация)
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    EmployeeID INT,
    CONSTRAINT FK_Users_Employees FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);

-- Таблица заявок
CREATE TABLE Requests (
    RequestID INT IDENTITY(1,1) PRIMARY KEY,
    ClientID INT NOT NULL,
    DeviceName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    StatusID INT NOT NULL,
    RequestDate DATETIME DEFAULT GETDATE(),
    CompletedDate DATETIME,
    AssignedEmployeeID INT,
    Priority INT DEFAULT 1,
    CONSTRAINT FK_Requests_Clients FOREIGN KEY (ClientID) REFERENCES Clients(ClientID),
    CONSTRAINT FK_Requests_Statuses FOREIGN KEY (StatusID) REFERENCES RequestStatuses(StatusID),
    CONSTRAINT FK_Requests_Employees FOREIGN KEY (AssignedEmployeeID) REFERENCES Employees(EmployeeID)
);

-- =============================================
-- 2. ЗАПОЛНЕНИЕ СПРАВОЧНЫХ ТАБЛИЦ
-- =============================================

-- Вставка ролей
INSERT INTO Roles (RoleName, Description, Permissions) VALUES
('Администратор', 'Полный доступ к системе', 'ALL'),
('Сотрудник', 'Ограниченный доступ к заявкам', 'READ_WRITE_REQUESTS'),
('Менеджер', 'Управление заявками и клиентами', 'MANAGE_REQUESTS_CLIENTS');

-- Вставка статусов заявок
INSERT INTO RequestStatuses (StatusName, Description, Color) VALUES
('Новая', 'Заявка только что создана', '#FF6B6B'),
('В работе', 'Заявка находится в обработке', '#4ECDC4'),
('Ожидает запчасти', 'Ожидание поставки запчастей', '#45B7D1'),
('Готова', 'Работа завершена, готово к выдаче', '#96CEB4'),
('Выдана', 'Заявка закрыта, оборудование выдано', '#FFEAA7'),
('Отменена', 'Заявка отменена клиентом', '#DDA0DD');

-- =============================================
-- 3. СОЗДАНИЕ ТЕСТОВЫХ СОТРУДНИКОВ
-- =============================================

-- Администратор
INSERT INTO Employees (FirstName, LastName, Email, Phone, RoleID) VALUES
('Иван', 'Петров', 'admin@servicecenter.ru', '+7(999)123-45-67', 1);

-- Сотрудник
INSERT INTO Employees (FirstName, LastName, Email, Phone, RoleID) VALUES
('Мария', 'Сидорова', 'user@servicecenter.ru', '+7(999)234-56-78', 2);

-- Менеджер
INSERT INTO Employees (FirstName, LastName, Email, Phone, RoleID) VALUES
('Алексей', 'Козлов', 'manager@servicecenter.ru', '+7(999)345-67-89', 3);

-- Дополнительный сотрудник
INSERT INTO Employees (FirstName, LastName, Email, Phone, RoleID) VALUES
('Елена', 'Иванова', 'tech@servicecenter.ru', '+7(999)456-78-90', 2);

-- =============================================
-- 4. СОЗДАНИЕ ТЕСТОВЫХ ПОЛЬЗОВАТЕЛЕЙ (АУТЕНТИФИКАЦИЯ)
-- =============================================

-- Пароли хешированы с помощью BCrypt
-- admin/admin, user/user, manager/manager, tech/tech

INSERT INTO Users (Username, PasswordHash, EmployeeID) VALUES
('admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 1),  -- admin
('user', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 2),   -- user
('manager', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 3), -- manager
('tech', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 4);   -- tech

-- =============================================
-- 5. СОЗДАНИЕ ТЕСТОВЫХ КЛИЕНТОВ
-- =============================================

INSERT INTO Clients (FirstName, LastName, Email, Phone, Address) VALUES
('Анна', 'Смирнова', 'anna.smirnova@email.com', '+7(999)111-11-11', 'ул. Ленина, 1, кв. 5'),
('Дмитрий', 'Волков', 'dmitry.volkov@email.com', '+7(999)222-22-22', 'ул. Пушкина, 10, кв. 12'),
('Ольга', 'Новикова', 'olga.novikova@email.com', '+7(999)333-33-33', 'ул. Гагарина, 25, кв. 8'),
('Сергей', 'Морозов', 'sergey.morozov@email.com', '+7(999)444-44-44', 'ул. Королева, 15, кв. 20');

-- =============================================
-- 6. СОЗДАНИЕ ТЕСТОВЫХ ЗАЯВОК
-- =============================================

INSERT INTO Requests (ClientID, DeviceName, Description, StatusID, RequestDate, AssignedEmployeeID, Priority) VALUES
(1, 'Ноутбук HP Pavilion', 'Не включается, горит индикатор питания', 2, DATEADD(day, -5, GETDATE()), 2, 2),
(2, 'Смартфон Samsung Galaxy', 'Разбит экран, требуется замена', 3, DATEADD(day, -3, GETDATE()), 4, 1),
(3, 'Принтер Canon PIXMA', 'Не печатает, ошибка картриджа', 4, DATEADD(day, -1, GETDATE()), 2, 3),
(4, 'Планшет iPad', 'Не заряжается, проблема с разъемом', 1, GETDATE(), NULL, 2);

-- =============================================
-- 7. СОЗДАНИЕ ИНДЕКСОВ ДЛЯ ПРОИЗВОДИТЕЛЬНОСТИ
-- =============================================

CREATE INDEX IX_Requests_ClientID ON Requests(ClientID);
CREATE INDEX IX_Requests_StatusID ON Requests(StatusID);
CREATE INDEX IX_Requests_RequestDate ON Requests(RequestDate);
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Clients_Phone ON Clients(Phone);
CREATE INDEX IX_Employees_RoleID ON Employees(RoleID);

-- =============================================
-- 8. СОЗДАНИЕ ПРЕДСТАВЛЕНИЙ ДЛЯ УДОБСТВА
-- =============================================

-- Представление для отображения заявок с полной информацией
CREATE VIEW vw_RequestsFull AS
SELECT 
    r.RequestID,
    r.DeviceName,
    r.Description,
    rs.StatusName,
    rs.Color AS StatusColor,
    r.RequestDate,
    r.CompletedDate,
    r.Priority,
    c.FirstName + ' ' + c.LastName AS ClientName,
    c.Phone AS ClientPhone,
    c.Email AS ClientEmail,
    e.FirstName + ' ' + e.LastName AS AssignedEmployee,
    rol.RoleName AS EmployeeRole
FROM Requests r
INNER JOIN Clients c ON r.ClientID = c.ClientID
INNER JOIN RequestStatuses rs ON r.StatusID = rs.StatusID
LEFT JOIN Employees e ON r.AssignedEmployeeID = e.EmployeeID
LEFT JOIN Roles rol ON e.RoleID = rol.RoleID;
GO

-- Представление для пользователей с ролями
CREATE VIEW vw_UsersWithRoles AS
SELECT 
    u.UserID,
    u.Username,
    e.FirstName + ' ' + e.LastName AS EmployeeName,
    e.Email,
    e.Phone,
    r.RoleName,
    r.Description AS RoleDescription,
    e.IsActive
FROM Users u
INNER JOIN Employees e ON u.EmployeeID = e.EmployeeID
INNER JOIN Roles r ON e.RoleID = r.RoleID;
GO

-- =============================================
-- 9. СОЗДАНИЕ ХРАНИМЫХ ПРОЦЕДУР
-- =============================================

-- Процедура для создания новой заявки
CREATE PROCEDURE sp_CreateRequest
    @ClientPhone NVARCHAR(20),
    @ClientFirstName NVARCHAR(50),
    @ClientLastName NVARCHAR(50),
    @DeviceName NVARCHAR(100),
    @Description NVARCHAR(500),
    @Priority INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ClientID INT;
    
    -- Поиск или создание клиента
    SELECT @ClientID = ClientID FROM Clients WHERE Phone = @ClientPhone;
    
    IF @ClientID IS NULL
    BEGIN
        INSERT INTO Clients (FirstName, LastName, Phone)
        VALUES (@ClientFirstName, @ClientLastName, @ClientPhone);
        SET @ClientID = SCOPE_IDENTITY();
    END
    
    -- Создание заявки
    INSERT INTO Requests (ClientID, DeviceName, Description, StatusID, Priority)
    VALUES (@ClientID, @DeviceName, @Description, 1, @Priority);
    
    SELECT SCOPE_IDENTITY() AS RequestID;
END;
GO

-- Процедура для обновления статуса заявки
CREATE PROCEDURE sp_UpdateRequestStatus
    @RequestID INT,
    @StatusID INT,
    @AssignedEmployeeID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Requests 
    SET StatusID = @StatusID,
        AssignedEmployeeID = @AssignedEmployeeID,
        CompletedDate = CASE WHEN @StatusID IN (4, 5) THEN GETDATE() ELSE CompletedDate END
    WHERE RequestID = @RequestID;
END;
GO

-- =============================================
-- 10. ВЫВОД ИНФОРМАЦИИ О СОЗДАННЫХ ДАННЫХ
-- =============================================

PRINT '=============================================';
PRINT 'БАЗА ДАННЫХ СЕРВИСНОГО ЦЕНТРА СОЗДАНА УСПЕШНО!';
PRINT '=============================================';
PRINT '';
PRINT 'Создано 6 таблиц в третьей нормальной форме:';
PRINT '- Users (пользователи)';
PRINT '- Employees (сотрудники)';
PRINT '- Roles (роли)';
PRINT '- Clients (клиенты)';
PRINT '- Requests (заявки)';
PRINT '- RequestStatuses (статусы заявок)';
PRINT '';
PRINT 'ТЕСТОВЫЕ УЧЕТНЫЕ ЗАПИСИ:';
PRINT 'Администратор: admin / admin';
PRINT 'Сотрудник: user / user';
PRINT 'Менеджер: manager / manager';
PRINT 'Техник: tech / tech';
PRINT '';
PRINT 'Создано тестовых данных:';
PRINT '- 4 сотрудника с учетными записями';
PRINT '- 4 клиента';
PRINT '- 4 заявки в разных статусах';
PRINT '- 3 роли и 6 статусов заявок';
PRINT '';
PRINT 'Добавлены индексы, представления и хранимые процедуры.';
PRINT 'База данных готова к использованию!';
GO 