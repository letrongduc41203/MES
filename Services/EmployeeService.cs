using MES.Data;
using MES.Models;
using Microsoft.EntityFrameworkCore;

namespace MES.Services
{
    public class EmployeeService
    {
        private readonly MyDbContext _context;

        public EmployeeService(MyDbContext context)
        {
            _context = context;
        }

        // 1. Thêm nhân viên
        public async Task<Employee> AddEmployeeAsync(string fullName)
        {
            var emp = new Employee
            {
                FullName = fullName.Trim(),
                Status = "Active",
                CreatedDate = DateTime.Now
            };
            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();
            return emp;
        }

        // 2. Cập nhật nhân viên
        public async Task UpdateEmployeeAsync(int employeeId, string fullName, string status)
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
            if (emp == null) throw new InvalidOperationException($"Employee {employeeId} not found");
            emp.FullName = fullName.Trim();
            emp.Status = status;
            emp.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        // 3. Xóa mềm nhân viên (nếu đã liên kết)
        public async Task<bool> DeleteEmployeeAsync(int employeeId)
        {
            var hasLinks = await _context.OrderEmployees.AnyAsync(x => x.EmployeeId == employeeId)
                           || await _context.MachineMaintenances.AnyAsync(mm => mm.Technician == employeeId.ToString());

            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
            if (emp == null) return false;

            if (hasLinks)
            {
                // Soft delete: chuyển Inactive
                emp.Status = "Inactive";
                emp.UpdatedDate = DateTime.Now;
            }
            else
            {
                _context.Employees.Remove(emp);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees.OrderBy(e => e.FullName).ToListAsync();
        }

        public async Task<List<Employee>> GetActiveAsync()
        {
            return await _context.Employees.Where(e => e.Status == "Active").OrderBy(e => e.FullName).ToListAsync();
        }

        // 4. Phân công vào Orders (nhiều NV)
        public async Task AssignEmployeesToOrderAsync(int orderId, IEnumerable<int> employeeIds, string role)
        {
            var activeIds = await _context.Employees
                .Where(e => employeeIds.Contains(e.EmployeeId) && e.Status == "Active")
                .Select(e => e.EmployeeId)
                .ToListAsync();

            foreach (var empId in activeIds.Distinct())
            {
                var exists = await _context.OrderEmployees.AnyAsync(oe => oe.OrderId == orderId && oe.EmployeeId == empId);
                if (exists) continue;
                _context.OrderEmployees.Add(new OrderEmployee
                {
                    OrderId = orderId,
                    EmployeeId = empId,
                    AssignedDate = DateTime.Now,
                    Role = role
                });
            }
            await _context.SaveChangesAsync();
        }

        // 5. Ghi nhận bảo trì bởi Technician (lưu EmployeeId dạng chuỗi vào Technician)
        public async Task<bool> AddMaintenanceAsync(int machineId, string description, int technicianEmployeeId)
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == technicianEmployeeId && e.Status == "Active");
            if (emp == null) return false;

            var record = new MachineMaintenance
            {
                MachineId = machineId,
                MaintenanceDate = DateTime.Now,
                Description = description,
                Technician = technicianEmployeeId.ToString()
            };
            _context.MachineMaintenances.Add(record);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
