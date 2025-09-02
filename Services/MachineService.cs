using MES.Data;
using MES.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MES.Services
{
    public class MachineService
    {
        private readonly MESDbContext _context;

        public MachineService(MESDbContext context)
        {
            _context = context;
        }

        // CRUD Operations
        public async Task<List<Machine>> GetAllMachinesAsync()
        {
            return await _context.Machines
                .Include(m => m.OrderMachines)
                    .ThenInclude(om => om.Order)
                        .ThenInclude(o => o.Product) // Include Product data
                .Include(m => m.MaintenanceHistory)
                .OrderBy(m => m.MachineName)
                .ToListAsync();
        }

        public async Task<Machine?> GetMachineByIdAsync(int machineId)
        {
            return await _context.Machines
                .Include(m => m.OrderMachines)
                    .ThenInclude(om => om.Order)
                        .ThenInclude(o => o.Product) // Include Product data
                .Include(m => m.MaintenanceHistory)
                .FirstOrDefaultAsync(m => m.MachineId == machineId);
        }

        public async Task<Machine> CreateMachineAsync(Machine machine)
        {
            machine.CreatedDate = DateTime.Now;
            _context.Machines.Add(machine);
            await _context.SaveChangesAsync();
            return machine;
        }

        public async Task<bool> UpdateMachineAsync(Machine machine)
        {
            var existingMachine = await _context.Machines.FindAsync(machine.MachineId);
            if (existingMachine == null)
                return false;

            existingMachine.MachineName = machine.MachineName;
            existingMachine.MachineType = machine.MachineType;
            existingMachine.Status = machine.Status;
            existingMachine.LastMaintenanceDate = machine.LastMaintenanceDate;
            existingMachine.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMachineAsync(int machineId)
        {
            var machine = await _context.Machines.FindAsync(machineId);
            if (machine == null)
                return false;

            // Check if machine is currently running
            if (machine.Status == MachineStatus.Running)
                return false;

            _context.Machines.Remove(machine);
            await _context.SaveChangesAsync();
            return true;
        }

        // Machine Status Management
        public async Task<bool> UpdateMachineStatusAsync(int machineId, MachineStatus newStatus)
        {
            var machine = await _context.Machines.FindAsync(machineId);
            if (machine == null)
                return false;

            machine.Status = newStatus;
            machine.UpdatedDate = DateTime.Now;

            // If setting to Maintenance, update LastMaintenanceDate
            if (newStatus == MachineStatus.Maintenance)
            {
                machine.LastMaintenanceDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Order Assignment
        public async Task<(bool Success, string ErrorMessage)> AssignOrderToMachineAsync(int orderId, int machineId)
        {
            var machine = await _context.Machines.FindAsync(machineId);
            if (machine == null)
            {
                return (false, "Máy không tồn tại.");
            }

            // 1. Check if machine is available
            if (machine.Status != MachineStatus.Available)
            {
                return (false, $"Không thể gán đơn hàng. Máy đang ở trạng thái '{machine.Status}'.");
            }

            // 2. Check if machine is already running another order
            if (machine.CurrentOrderId != null)
            {
                return (false, $"Không thể gán đơn hàng. Máy đang chạy đơn hàng ID {machine.CurrentOrderId}.");
            }

            var order = await _context.Set<Order>().FindAsync(orderId);
            if (order == null)
            {
                return (false, "Đơn hàng không tồn tại.");
            }

            // All checks passed, proceed with assignment
            var orderMachine = new OrderMachine
            {
                OrderId = orderId,
                MachineId = machineId,
                StartTime = DateTime.Now
            };
            _context.OrderMachines.Add(orderMachine);

            // Update machine status and current order
            machine.Status = MachineStatus.Running;
            machine.CurrentOrderId = orderId;
            machine.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<bool> CompleteOrderOnMachineAsync(int orderId, int machineId)
        {
            var orderMachine = await _context.OrderMachines
                .FirstOrDefaultAsync(om => om.OrderId == orderId && om.MachineId == machineId);

            if (orderMachine == null)
                return false;

            orderMachine.EndTime = DateTime.Now;

            // Update machine status back to Idle
            var machine = await _context.Machines.FindAsync(machineId);
            if (machine != null)
            {
                machine.Status = MachineStatus.Idle;
                machine.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Maintenance Management
        public async Task<bool> StartMaintenanceAsync(int machineId, string description, string technician)
        {
            var machine = await _context.Machines.FindAsync(machineId);
            if (machine == null || machine.Status == MachineStatus.Running)
            {
                return false; // Cannot start maintenance if machine is running or not found
            }

            // 1. Update machine status
            machine.Status = MachineStatus.Maintenance;
            machine.UpdatedDate = DateTime.Now;

            // 2. Create new maintenance record
            var maintenanceRecord = new MachineMaintenance
            {
                MachineId = machineId,
                MaintenanceDate = DateTime.Now,
                Description = description,
                Technician = technician
            };
            _context.MachineMaintenances.Add(maintenanceRecord);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteMaintenanceAsync(int machineId)
        {
            var machine = await _context.Machines.FindAsync(machineId);
            if (machine == null || machine.Status != MachineStatus.Maintenance)
            {
                return false; // Can only complete maintenance if it's in progress
            }

            // 1. Update machine status back to Available
            machine.Status = MachineStatus.Available;
            machine.LastMaintenanceDate = DateTime.Now;
            machine.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Machine>> GetMachinesNeedingMaintenanceAsync(int daysThreshold = 30)
        {
            var thresholdDate = DateTime.Now.AddDays(-daysThreshold);
            return await _context.Machines
                .Where(m => m.LastMaintenanceDate == null || m.LastMaintenanceDate < thresholdDate)
                .ToListAsync();
        }

        // Search and Filter
        public async Task<List<Machine>> SearchMachinesAsync(string searchTerm)
        {
            return await _context.Machines
                .Where(m => m.MachineName.Contains(searchTerm) || m.MachineType.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<List<Machine>> GetMachinesByStatusAsync(MachineStatus status)
        {
            return await _context.Machines
                .Where(m => m.Status == status)
                .ToListAsync();
        }

        // Get machine statistics
        public async Task<object> GetMachineStatisticsAsync()
        {
            var totalMachines = await _context.Machines.CountAsync();
            var idleMachines = await _context.Machines.CountAsync(m => m.Status == MachineStatus.Idle);
            var runningMachines = await _context.Machines.CountAsync(m => m.Status == MachineStatus.Running);
            var maintenanceMachines = await _context.Machines.CountAsync(m => m.Status == MachineStatus.Maintenance);
            var errorMachines = await _context.Machines.CountAsync(m => m.Status == MachineStatus.Error);

            return new
            {
                Total = totalMachines,
                Idle = idleMachines,
                Running = runningMachines,
                Maintenance = maintenanceMachines,
                Error = errorMachines
            };
        }
    }
} 