using MES.Data;
using MES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MES.Services
{

    public class OrderStatusCount
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Processing { get; set; }
        public int Completed { get; set; }
    }

    public class OrderService
    {
        public async Task<OrderStatusCount> GetOrderStatusCountsAsync()
        {
            var counts = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return new OrderStatusCount
            {
                Total = await _context.Orders.CountAsync(),
                Pending = counts.FirstOrDefault(x => x.Status == OrderStatus.Pending)?.Count ?? 0,
                Processing = counts.FirstOrDefault(x => x.Status == OrderStatus.Processing)?.Count ?? 0,
                Completed = counts.FirstOrDefault(x => x.Status == OrderStatus.Completed)?.Count ?? 0
            };
        }
        private readonly MyDbContext _context;

        public OrderService(MyDbContext context)
        {
            _context = context;
        }

        // Create order and assign to machine only if machine is valid and available.
        public async Task<Order> CreateOrderAsync(int productId, int quantity, int machineId, DateTime orderDate)
        {
            // Use a transaction so partial changes are not persisted if something fails
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate machine
                var machine = await _context.Machines.FindAsync(machineId);
                if (machine == null)
                    throw new InvalidOperationException($"Machine {machineId} not found.");

                if (machine.Status != MachineStatus.Available)
                    throw new InvalidOperationException($"Cannot assign order. Machine is in status '{machine.Status}'.");

                if (machine.CurrentOrderId != null)
                    throw new InvalidOperationException($"Cannot assign order. Machine is already running order ID {machine.CurrentOrderId}.");

                // 1. Create order
                var order = new Order
                {
                    ProductId = productId,
                    Quantity = quantity,
                    MachineId = machineId,
                    OrderDate = orderDate,
                    Status = OrderStatus.Pending
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // get OrderId

                // 2. Create OrderMachine (assignment)
                var orderMachine = new OrderMachine
                {
                    OrderId = order.OrderId,
                    MachineId = machineId,
                    StartTime = DateTime.Now
                };
                _context.OrderMachines.Add(orderMachine);

                // 3. Update machine state
                machine.Status = MachineStatus.Running;
                machine.CurrentOrderId = order.OrderId;
                machine.UpdatedDate = DateTime.Now;

                // 4. Create OrderMaterials from BOM
                var bomItems = await _context.ProductMaterials
                    .Where(pm => pm.ProductId == productId)
                    .ToListAsync();

                foreach (var pm in bomItems)
                {
                    var orderMaterial = new OrderMaterial
                    {
                        OrderId = order.OrderId,
                        MaterialId = pm.MaterialId,
                        QtyUsed = pm.QtyNeeded * quantity,
                        ProcessedQuantity = 0
                    };
                    _context.OrderMaterials.Add(orderMaterial);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return order;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders
                .Include(o => o.Machine)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) throw new InvalidOperationException($"Order {orderId} not found.");
            if (order.Status == newStatus) return;

            order.Status = newStatus;

            if (order.Machine != null)
            {
                if (newStatus == OrderStatus.Processing)
                {
                    order.Machine.Status = MachineStatus.Busy;
                    order.Machine.CurrentOrderId = order.OrderId;
                }
                else if (newStatus == OrderStatus.Completed)
                {
                    if (order.Machine.CurrentOrderId == order.OrderId)
                    {
                        order.Machine.Status = MachineStatus.Available;
                        order.Machine.CurrentOrderId = null;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task CompleteOrderAsync(int orderId)
        {
            // Update status first, which handles machine status changes
            await UpdateOrderStatusAsync(orderId, OrderStatus.Completed);

            var order = await _context.Orders
                .Include(o => o.OrderMachines)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order {orderId} không tồn tại");
            }

            // Set EndTime for the machine assignment
            var orderMachine = order.OrderMachines.FirstOrDefault(om => om.MachineId == order.MachineId);
            if (orderMachine != null)
            {
                orderMachine.EndTime = DateTime.Now;
            }

            if (order.Status != OrderStatus.Completed)
            {
                order.Status = OrderStatus.Completed;
            }

            // Đảm bảo có OrderMaterials; nếu chưa có thì tạo từ BOM
            var orderMaterials = await _context.OrderMaterials
                .Where(om => om.OrderId == orderId)
                .ToListAsync();

            if (orderMaterials.Count == 0)
            {
                var bomItems = await _context.ProductMaterials
                    .Where(pm => pm.ProductId == order.ProductId)
                    .ToListAsync();

                foreach (var pm in bomItems)
                {
                    _context.OrderMaterials.Add(new OrderMaterial
                    {
                        OrderId = orderId,
                        MaterialId = pm.MaterialId,
                        QtyUsed = pm.QtyNeeded * order.Quantity,
                        ProcessedQuantity = 0
                    });
                }
                await _context.SaveChangesAsync();

                orderMaterials = await _context.OrderMaterials
                    .Where(om => om.OrderId == orderId)
                    .ToListAsync();
            }

            // Trừ phần còn lại (idempotent)
            foreach (var om in orderMaterials)
            {
                int remaining = om.QtyUsed - om.ProcessedQuantity;
                if (remaining <= 0) continue;

                var material = await _context.Materials.FirstOrDefaultAsync(m => m.MaterialId == om.MaterialId);
                if (material != null)
                {
                    material.StockQuantity -= remaining;
                    material.LastUpdated = DateTime.Now;
                }
                om.ProcessedQuantity += remaining;
            }

            await _context.SaveChangesAsync();
        }
    }
}
