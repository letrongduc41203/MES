# Module Machines - Hệ thống MES

## Tổng quan
Module Machines cung cấp chức năng quản lý toàn diện cho máy móc trong hệ thống MES, bao gồm theo dõi trạng thái, lịch sử bảo trì, và kết nối với đơn hàng.

## Tính năng chính

### 1. Quản lý danh sách máy móc
- **CRUD Operations**: Thêm, sửa, xóa, tìm kiếm máy
- **Thông tin cơ bản**: ID, tên máy, loại máy, trạng thái
- **Trạng thái máy**: Idle, Running, Maintenance, Error

### 2. Kết nối với Orders
- **Gán đơn hàng**: Tự động gán đơn hàng cho máy khi chuyển trạng thái "In Progress"
- **Theo dõi thời gian**: Ghi nhận thời gian bắt đầu và kết thúc xử lý đơn hàng
- **Quan hệ nhiều-nhiều**: Bảng OrderMachines lưu trữ mối quan hệ

### 3. Quản lý trạng thái máy
- **Cập nhật realtime**: UI hiển thị trạng thái máy theo thời gian thực
- **Icon màu sắc**: 
  - 🟢 Running (Xanh lá)
  - 🔴 Error (Đỏ)
  - 🟡 Maintenance (Cam)
  - ⚪ Idle (Xám)
- **Kiểm tra ràng buộc**: Máy đang bảo trì không thể gán đơn hàng

### 4. Bảo trì & lịch sử
- **Lịch sử bảo trì**: Ghi nhận chi tiết các lần bảo trì
- **Cảnh báo tự động**: Thông báo khi máy quá 30 ngày chưa bảo trì
- **Thông tin bảo trì**: Loại bảo trì, chi phí, kỹ thuật viên

## Cấu trúc cơ sở dữ liệu

### Bảng Machines
```sql
CREATE TABLE Machines (
    MachineId INT IDENTITY(1,1) PRIMARY KEY,
    MachineName NVARCHAR(100) NOT NULL,
    MachineType NVARCHAR(50) NOT NULL,
    Status INT NOT NULL DEFAULT(0), -- 0:Idle, 1:Running, 2:Maintenance, 3:Error
    LastMaintenanceDate DATETIME NULL,
    CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
    UpdatedDate DATETIME NULL
);
```

### Bảng OrderMachines
```sql
CREATE TABLE OrderMachines (
    OrderId INT NOT NULL,
    MachineId INT NOT NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NULL,
    PRIMARY KEY (OrderId, MachineId)
);
```

### Bảng MachineMaintenance
```sql
CREATE TABLE MachineMaintenance (
    MaintenanceId INT IDENTITY(1,1) PRIMARY KEY,
    MachineId INT NOT NULL,
    MaintenanceDate DATETIME NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    MaintenanceType NVARCHAR(50) NOT NULL,
    Cost DECIMAL(18,2) NULL,
    Technician NVARCHAR(100) NULL
);
```

## Cài đặt và sử dụng

### 1. Chạy script SQL
```bash
# Mở SQL Server Management Studio
# Kết nối database MES_ProductionDB
# Chạy file Database/Machines_Schema.sql
```

### 2. Build và chạy ứng dụng
```bash
# Build project
dotnet build

# Chạy ứng dụng
dotnet run
```

### 3. Truy cập module Machines
- Mở ứng dụng MES
- Chọn menu "Machines" hoặc "Máy móc"
- Giao diện quản lý máy sẽ hiển thị

## API Endpoints

### Machines
- `GET /api/machines` - Lấy danh sách tất cả máy
- `GET /api/machines/{id}` - Lấy thông tin máy theo ID
- `POST /api/machines` - Tạo máy mới
- `PUT /api/machines/{id}` - Cập nhật thông tin máy
- `DELETE /api/machines/{id}` - Xóa máy

### Machine Status
- `PUT /api/machines/{id}/status` - Cập nhật trạng thái máy
- `GET /api/machines/status/{status}` - Lọc máy theo trạng thái

### Order Assignment
- `POST /api/machines/{id}/assign-order` - Gán đơn hàng cho máy
- `POST /api/machines/{id}/complete-order` - Hoàn thành đơn hàng

### Maintenance
- `POST /api/machines/{id}/maintenance` - Thêm bảo trì
- `GET /api/machines/{id}/maintenance-history` - Lịch sử bảo trì

## Giao diện người dùng

### Main View
- **DataGrid**: Hiển thị danh sách máy với các cột thông tin
- **Toolbar**: Các nút chức năng chính (Thêm, Sửa, Xóa, Bảo trì)
- **Search & Filter**: Tìm kiếm và lọc theo trạng thái
- **Status Indicators**: Icon màu sắc hiển thị trạng thái

### Detail Panel
- **Thông tin máy**: Tên, loại, trạng thái, ngày bảo trì cuối
- **Đơn hàng hiện tại**: Hiển thị đơn hàng đang chạy trên máy
- **Lịch sử bảo trì**: Danh sách các lần bảo trì

## Business Logic

### Quy tắc gán đơn hàng
1. Chỉ máy có trạng thái "Idle" mới được gán đơn hàng
2. Máy đang "Maintenance" hoặc "Error" không thể gán đơn hàng
3. Khi gán đơn hàng, máy chuyển sang trạng thái "Running"
4. Khi hoàn thành đơn hàng, máy trở về trạng thái "Idle"

### Quy tắc bảo trì
1. Máy cần bảo trì định kỳ mỗi 30 ngày
2. Cảnh báo khi máy quá 25 ngày chưa bảo trì
3. Máy đang bảo trì không thể thực hiện đơn hàng
4. Sau khi bảo trì, máy trở về trạng thái "Idle"

## Mô phỏng hoạt động

### Random thời gian xử lý
```csharp
// Trong MachineService
public async Task SimulateMachineOperation(int machineId)
{
    var random = new Random();
    var processingTime = random.Next(5, 30); // 5-30 phút
    
    // Cập nhật trạng thái máy
    await UpdateMachineStatusAsync(machineId, MachineStatus.Running);
    
    // Mô phỏng thời gian xử lý
    await Task.Delay(processingTime * 1000); // Convert to milliseconds
    
    // Hoàn thành và trở về Idle
    await UpdateMachineStatusAsync(machineId, MachineStatus.Idle);
}
```

## Monitoring và báo cáo

### Dashboard Metrics
- Tổng số máy
- Số máy đang hoạt động
- Số máy cần bảo trì
- Hiệu suất sử dụng máy

### Alerts
- Máy quá 30 ngày chưa bảo trì
- Máy gặp lỗi cần xử lý
- Máy hoạt động quá tải

## Troubleshooting

### Lỗi thường gặp
1. **Máy không thể gán đơn hàng**: Kiểm tra trạng thái máy
2. **Lỗi kết nối database**: Kiểm tra connection string
3. **UI không cập nhật**: Kiểm tra binding và INotifyPropertyChanged

### Debug
- Sử dụng SQL Server Profiler để theo dõi queries
- Kiểm tra logs trong Output window
- Sử dụng breakpoints trong Visual Studio

## Phát triển tiếp theo

### Tính năng nâng cao
- [ ] Real-time monitoring với SignalR
- [ ] Machine learning để dự đoán bảo trì
- [ ] Integration với IoT sensors
- [ ] Mobile app cho kỹ thuật viên

### Performance Optimization
- [ ] Caching với Redis
- [ ] Database indexing
- [ ] Async/await patterns
- [ ] Background services

## Liên hệ và hỗ trợ
- **Developer**: MES Development Team
- **Email**: support@mes.com
- **Documentation**: [Wiki Link]
- **Issue Tracker**: [GitHub Issues]

---
*Tài liệu này được cập nhật lần cuối: [Ngày hiện tại]* 