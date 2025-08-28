-- Script tạo bảng cho module Machines
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio

USE MES_ProductionDB;
GO

-- Tạo bảng Machines
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Machines]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Machines](
        [MachineId] [int] IDENTITY(1,1) NOT NULL,
        [MachineName] [nvarchar](100) NOT NULL,
        [MachineType] [nvarchar](50) NOT NULL,
        [Status] [int] NOT NULL DEFAULT(0), -- 0: Idle, 1: Running, 2: Maintenance, 3: Error
        [LastMaintenanceDate] [datetime] NULL,
        [CreatedDate] [datetime] NOT NULL DEFAULT(GETDATE()),
        [UpdatedDate] [datetime] NULL,
        CONSTRAINT [PK_Machines] PRIMARY KEY CLUSTERED ([MachineId] ASC)
    );
END
GO

-- Tạo bảng OrderMachines (quan hệ nhiều-nhiều giữa Orders và Machines)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderMachines]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrderMachines](
        [OrderId] [int] NOT NULL,
        [MachineId] [int] NOT NULL,
        [StartTime] [datetime] NOT NULL,
        [EndTime] [datetime] NULL,
        CONSTRAINT [PK_OrderMachines] PRIMARY KEY CLUSTERED ([OrderId] ASC, [MachineId] ASC)
    );
END
GO

-- Tạo bảng MachineMaintenance
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MachineMaintenance]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MachineMaintenance](
        [MaintenanceId] [int] IDENTITY(1,1) NOT NULL,
        [MachineId] [int] NOT NULL,
        [MaintenanceDate] [datetime] NOT NULL,
        [Description] [nvarchar](500) NOT NULL,
        [MaintenanceType] [nvarchar](50) NOT NULL,
        [Cost] [decimal](18,2) NULL,
        [Technician] [nvarchar](100) NULL,
        CONSTRAINT [PK_MachineMaintenance] PRIMARY KEY CLUSTERED ([MaintenanceId] ASC)
    );
END
GO

-- Tạo bảng Orders nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Orders](
        [OrderId] [int] IDENTITY(1,1) NOT NULL,
        [ProductId] [int] NOT NULL,
        [Quantity] [int] NOT NULL,
        [OrderDate] [datetime] NOT NULL DEFAULT(GETDATE()),
        [Status] [int] NOT NULL DEFAULT(0), -- 0: Pending, 1: Processing, 2: Completed
        CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([OrderId] ASC)
    );
END
GO

-- Tạo bảng Products nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Products](
        [ProductId] [int] IDENTITY(1,1) NOT NULL,
        [ProductName] [nvarchar](100) NOT NULL,
        [ProductCode] [nvarchar](50) NOT NULL,
        [Unit] [nvarchar](20) NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ProductId] ASC)
    );
END
GO

-- Tạo bảng Materials nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Materials]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Materials](
        [MaterialId] [int] IDENTITY(1,1) NOT NULL,
        [MaterialName] [nvarchar](100) NOT NULL,
        [Unit] [nvarchar](20) NOT NULL,
        [StockQuantity] [float] NOT NULL DEFAULT(0),
        [LastUpdated] [datetime] NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT [PK_Materials] PRIMARY KEY CLUSTERED ([MaterialId] ASC)
    );
END
GO

-- Tạo bảng ProductMaterials nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductMaterials]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProductMaterials](
        [ProductId] [int] NOT NULL,
        [MaterialId] [int] NOT NULL,
        [QtyNeeded] [int] NOT NULL,
        CONSTRAINT [PK_ProductMaterials] PRIMARY KEY CLUSTERED ([ProductId] ASC, [MaterialId] ASC)
    );
END
GO

-- Tạo bảng OrderMaterials nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderMaterials]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrderMaterials](
        [OrderId] [int] NOT NULL,
        [MaterialId] [int] NOT NULL,
        [QtyUsed] [int] NOT NULL,
        [ProcessedQuantity] [int] NOT NULL DEFAULT(0),
        CONSTRAINT [PK_OrderMaterials] PRIMARY KEY CLUSTERED ([OrderId] ASC, [MaterialId] ASC)
    );
END
GO

-- Tạo bảng Users nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users](
        [UserId] [int] IDENTITY(1,1) NOT NULL,
        [Username] [nvarchar](50) NOT NULL,
        [PasswordHash] [nvarchar](255) NOT NULL,
        [Role] [nvarchar](20) NOT NULL,
        [FullName] [nvarchar](100) NOT NULL,
        [CreatedAt] [datetime] NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC)
    );
END
GO

-- Tạo Foreign Keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_OrderMachines_Orders]') AND parent_object_id = OBJECT_ID(N'[dbo].[OrderMachines]'))
BEGIN
    ALTER TABLE [dbo].[OrderMachines]  WITH CHECK ADD  CONSTRAINT [FK_OrderMachines_Orders] FOREIGN KEY([OrderId])
    REFERENCES [dbo].[Orders] ([OrderId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_OrderMachines_Machines]') AND parent_object_id = OBJECT_ID(N'[dbo].[OrderMachines]'))
BEGIN
    ALTER TABLE [dbo].[OrderMachines]  WITH CHECK ADD  CONSTRAINT [FK_OrderMachines_Machines] FOREIGN KEY([MachineId])
    REFERENCES [dbo].[Machines] ([MachineId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MachineMaintenance_Machines]') AND parent_object_id = OBJECT_ID(N'[dbo].[MachineMaintenance]'))
BEGIN
    ALTER TABLE [dbo].[MachineMaintenance]  WITH CHECK ADD  CONSTRAINT [FK_MachineMaintenance_Machines] FOREIGN KEY([MachineId])
    REFERENCES [dbo].[Machines] ([MachineId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Orders_Products]') AND parent_object_id = OBJECT_ID(N'[dbo].[Orders]'))
BEGIN
    ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Products] FOREIGN KEY([ProductId])
    REFERENCES [dbo].[Products] ([ProductId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_OrderMaterials_Orders]') AND parent_object_id = OBJECT_ID(N'[dbo].[OrderMaterials]'))
BEGIN
    ALTER TABLE [dbo].[OrderMaterials]  WITH CHECK ADD  CONSTRAINT [FK_OrderMaterials_Orders] FOREIGN KEY([OrderId])
    REFERENCES [dbo].[Orders] ([OrderId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_OrderMaterials_Materials]') AND parent_object_id = OBJECT_ID(N'[dbo].[OrderMaterials]'))
BEGIN
    ALTER TABLE [dbo].[OrderMaterials]  WITH CHECK ADD  CONSTRAINT [FK_OrderMaterials_Materials] FOREIGN KEY([MaterialId])
    REFERENCES [dbo].[Materials] ([MaterialId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ProductMaterials_Products]') AND parent_object_id = OBJECT_ID(N'[dbo].[ProductMaterials]'))
BEGIN
    ALTER TABLE [dbo].[ProductMaterials]  WITH CHECK ADD  CONSTRAINT [FK_ProductMaterials_Products] FOREIGN KEY([ProductId])
    REFERENCES [dbo].[Products] ([ProductId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ProductMaterials_Materials]') AND parent_object_id = OBJECT_ID(N'[dbo].[ProductMaterials]'))
BEGIN
    ALTER TABLE [dbo].[ProductMaterials]  WITH CHECK ADD  CONSTRAINT [FK_ProductMaterials_Materials] FOREIGN KEY([MaterialId])
    REFERENCES [dbo].[Materials] ([MaterialId]);
END
GO

-- Tạo Indexes để tối ưu hiệu suất
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Machines]') AND name = N'IX_Machines_Status')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Machines_Status] ON [dbo].[Machines]([Status] ASC);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Machines]') AND name = N'IX_Machines_LastMaintenanceDate')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Machines_LastMaintenanceDate] ON [dbo].[Machines]([LastMaintenanceDate] ASC);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OrderMachines]') AND name = N'IX_OrderMachines_MachineId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_OrderMachines_MachineId] ON [dbo].[OrderMachines]([MachineId] ASC);
END
GO

-- Thêm dữ liệu mẫu cho bảng Machines
IF NOT EXISTS (SELECT * FROM [dbo].[Machines] WHERE [MachineName] = 'Máy CNC-001')
BEGIN
    INSERT INTO [dbo].[Machines] ([MachineName], [MachineType], [Status], [LastMaintenanceDate])
    VALUES 
        ('Máy CNC-001', 'CNC Router', 0, DATEADD(day, -15, GETDATE())),
        ('Máy CNC-002', 'CNC Router', 0, DATEADD(day, -25, GETDATE())),
        ('Máy Laser-001', 'Laser Cutter', 0, DATEADD(day, -10, GETDATE())),
        ('Máy 3D-001', '3D Printer', 0, DATEADD(day, -5, GETDATE())),
        ('Máy Cắt-001', 'Plasma Cutter', 0, DATEADD(day, -20, GETDATE())),
        ('Máy Hàn-001', 'Welding Machine', 0, DATEADD(day, -30, GETDATE())),
        ('Máy Mài-001', 'Grinding Machine', 0, DATEADD(day, -12, GETDATE())),
        ('Máy Khoan-001', 'Drilling Machine', 0, DATEADD(day, -18, GETDATE()));
END
GO

-- Thêm dữ liệu mẫu cho bảng MachineMaintenance
IF NOT EXISTS (SELECT * FROM [dbo].[MachineMaintenance] WHERE [MachineId] = 1 AND [MaintenanceDate] = DATEADD(day, -15, GETDATE()))
BEGIN
    INSERT INTO [dbo].[MachineMaintenance] ([MachineId], [MaintenanceDate], [Description], [MaintenanceType], [Cost], [Technician])
    VALUES 
        (1, DATEADD(day, -15, GETDATE()), 'Bảo trì định kỳ, thay dầu, kiểm tra hệ thống', 'Preventive', 500000, 'Nguyễn Văn A'),
        (2, DATEADD(day, -25, GETDATE()), 'Sửa chữa motor, thay phụ tùng', 'Corrective', 1200000, 'Trần Văn B'),
        (3, DATEADD(day, -10, GETDATE()), 'Bảo trì định kỳ, vệ sinh ống kính', 'Preventive', 300000, 'Lê Văn C'),
        (4, DATEADD(day, -5, GETDATE()), 'Kiểm tra nhiệt độ, vệ sinh bàn in', 'Preventive', 200000, 'Phạm Văn D'),
        (5, DATEADD(day, -20, GETDATE()), 'Thay điện cực, kiểm tra áp suất khí', 'Preventive', 400000, 'Hoàng Văn E'),
        (6, DATEADD(day, -30, GETDATE()), 'Bảo trì định kỳ, kiểm tra hệ thống làm mát', 'Preventive', 600000, 'Vũ Văn F'),
        (7, DATEADD(day, -12, GETDATE()), 'Thay đá mài, kiểm tra độ rung', 'Preventive', 250000, 'Đặng Văn G'),
        (8, DATEADD(day, -18, GETDATE()), 'Bảo trì định kỳ, kiểm tra độ chính xác', 'Preventive', 350000, 'Bùi Văn H');
END
GO

-- Thêm dữ liệu mẫu cho bảng Products
IF NOT EXISTS (SELECT * FROM [dbo].[Products] WHERE [ProductCode] = 'PROD001')
BEGIN
    INSERT INTO [dbo].[Products] ([ProductName], [ProductCode], [Unit])
    VALUES 
        (N'Sản phẩm A', 'PROD001', 'Cái'),
        (N'Sản phẩm B', 'PROD002', 'Bộ'),
        (N'Sản phẩm C', 'PROD003', 'Kg'),
        (N'Sản phẩm D', 'PROD004', 'Mét');
END
GO

-- Thêm dữ liệu mẫu cho bảng Materials
IF NOT EXISTS (SELECT * FROM [dbo].[Materials] WHERE [MaterialName] = 'Vật liệu A')
BEGIN
    INSERT INTO [dbo].[Materials] ([MaterialName], [Unit], [StockQuantity])
    VALUES 
        ('Vật liệu A', 'Kg', 100.5),
        ('Vật liệu B', 'Mét', 50.0),
        ('Vật liệu C', 'Cái', 200),
        ('Vật liệu D', 'Bộ', 25);
END
GO

-- Thêm dữ liệu mẫu cho bảng Orders
IF NOT EXISTS (SELECT * FROM [dbo].[Orders] WHERE [ProductId] = 1)
BEGIN
    INSERT INTO [dbo].[Orders] ([ProductId], [Quantity], [Status])
    VALUES 
        (1, 10, 0),
        (2, 5, 1),
        (3, 20, 0),
        (4, 8, 2);
END
GO

-- Thêm dữ liệu mẫu cho bảng OrderMachines
IF NOT EXISTS (SELECT * FROM [dbo].[OrderMachines] WHERE [OrderId] = 2 AND [MachineId] = 1)
BEGIN
    INSERT INTO [dbo].[OrderMachines] ([OrderId], [MachineId], [StartTime])
    VALUES 
        (2, 1, DATEADD(hour, -2, GETDATE())), -- Order 2 đang chạy trên Machine 1
        (3, 2, DATEADD(hour, -1, GETDATE())); -- Order 3 đang chạy trên Machine 2
END
GO

-- Cập nhật trạng thái máy 1 và 2 thành Running
UPDATE [dbo].[Machines] SET [Status] = 1 WHERE [MachineId] IN (1, 2);
GO

-- Thêm dữ liệu mẫu cho bảng ProductMaterials
IF NOT EXISTS (SELECT * FROM [dbo].[ProductMaterials] WHERE [ProductId] = 1 AND [MaterialId] = 1)
BEGIN
    INSERT INTO [dbo].[ProductMaterials] ([ProductId], [MaterialId], [QtyNeeded])
    VALUES 
        (1, 1, 2), -- Product 1 cần 2 Material 1
        (1, 2, 1), -- Product 1 cần 1 Material 2
        (2, 1, 3), -- Product 2 cần 3 Material 1
        (2, 2, 2), -- Product 2 cần 2 Material 2
        (3, 3, 5), -- Product 3 cần 5 Material 3
        (4, 1, 1), -- Product 4 cần 1 Material 1
        (4, 4, 3); -- Product 4 cần 3 Material 4
END
GO

-- Thêm dữ liệu mẫu cho bảng OrderMaterials
IF NOT EXISTS (SELECT * FROM [dbo].[OrderMaterials] WHERE [OrderId] = 1 AND [MaterialId] = 1)
BEGIN
    INSERT INTO [dbo].[OrderMaterials] ([OrderId], [MaterialId], [QtyUsed], [ProcessedQuantity])
    VALUES 
        (1, 1, 2, 0), -- Order 1 sử dụng 2 Material 1, chưa xử lý
        (1, 2, 1, 0), -- Order 1 sử dụng 1 Material 2, chưa xử lý
        (2, 1, 3, 2), -- Order 2 sử dụng 3 Material 1, đã xử lý 2
        (2, 2, 1, 0), -- Order 2 sử dụng 1 Material 2, chưa xử lý
        (3, 3, 5, 0), -- Order 3 sử dụng 5 Material 3, chưa xử lý
        (4, 1, 1, 0); -- Order 4 sử dụng 1 Material 1, chưa xử lý
END
GO

-- Thêm dữ liệu mẫu cho bảng Users
IF NOT EXISTS (SELECT * FROM [dbo].[Users] WHERE [Username] = 'admin')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [PasswordHash], [Role], [FullName])
    VALUES 
        ('admin', 'admin123', 'Administrator', 'Quản trị viên hệ thống'),
        ('operator', 'operator123', 'Operator', 'Nhân viên vận hành'),
        ('technician', 'tech123', 'Technician', 'Kỹ thuật viên'),
        ('supervisor', 'supervisor123', 'Supervisor', 'Giám sát viên');
END
GO

-- Tạo View để hiển thị thông tin máy và trạng thái
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_MachineStatus]'))
    DROP VIEW [dbo].[vw_MachineStatus];
GO

CREATE VIEW [dbo].[vw_MachineStatus] AS
SELECT 
    m.MachineId,
    m.MachineName,
    m.MachineType,
    CASE m.Status
        WHEN 0 THEN 'Idle'
        WHEN 1 THEN 'Running'
        WHEN 2 THEN 'Maintenance'
        WHEN 3 THEN 'Error'
        ELSE 'Unknown'
    END AS StatusText,
    m.Status,
    m.LastMaintenanceDate,
    m.CreatedDate,
    m.UpdatedDate,
    DATEDIFF(day, ISNULL(m.LastMaintenanceDate, m.CreatedDate), GETDATE()) AS DaysSinceLastMaintenance,
    CASE 
        WHEN DATEDIFF(day, ISNULL(m.LastMaintenanceDate, m.CreatedDate), GETDATE()) > 30 THEN 'Cần bảo trì'
        WHEN DATEDIFF(day, ISNULL(m.LastMaintenanceDate, m.CreatedDate), GETDATE()) > 25 THEN 'Sắp đến hạn bảo trì'
        ELSE 'Trong hạn bảo trì'
    END AS MaintenanceStatus
FROM [dbo].[Machines] m;
GO

-- Tạo Stored Procedure để cập nhật trạng thái máy
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpdateMachineStatus]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpdateMachineStatus];
GO

CREATE PROCEDURE [dbo].[sp_UpdateMachineStatus]
    @MachineId INT,
    @NewStatus INT,
    @Result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE [dbo].[Machines] 
        SET [Status] = @NewStatus, 
            [UpdatedDate] = GETDATE()
        WHERE [MachineId] = @MachineId;
        
        IF @@ROWCOUNT > 0
            SET @Result = 1;
        ELSE
            SET @Result = 0;
    END TRY
    BEGIN CATCH
        SET @Result = 0;
        THROW;
    END CATCH
END
GO

-- Tạo Stored Procedure để gán đơn hàng cho máy
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_AssignOrderToMachine]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_AssignOrderToMachine];
GO

CREATE PROCEDURE [dbo].[sp_AssignOrderToMachine]
    @OrderId INT,
    @MachineId INT,
    @Result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Kiểm tra máy có sẵn sàng không
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Machines] WHERE [MachineId] = @MachineId AND [Status] = 0)
        BEGIN
            SET @Result = 0;
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Thêm quan hệ Order-Machine
        INSERT INTO [dbo].[OrderMachines] ([OrderId], [MachineId], [StartTime])
        VALUES (@OrderId, @MachineId, GETDATE());
        
        -- Cập nhật trạng thái máy thành Running
        UPDATE [dbo].[Machines] 
        SET [Status] = 1, 
            [UpdatedDate] = GETDATE()
        WHERE [MachineId] = @MachineId;
        
        COMMIT TRANSACTION;
        SET @Result = 1;
    END TRY
    BEGIN CATCH
        SET @Result = 0;
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Đã tạo xong schema và dữ liệu mẫu cho module Machines!';
GO 