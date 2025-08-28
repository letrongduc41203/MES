using Microsoft.EntityFrameworkCore;
using MES.Models;

namespace MES.Data
{
    public class MESDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderMachine> OrderMachines { get; set; }
        public DbSet<MachineMaintenance> MachineMaintenances { get; set; }
        public DbSet<OrderMaterial> OrderMaterials { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductMaterial> ProductMaterials { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=DucFendi;Database=MES_ProductionDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite keys
            modelBuilder.Entity<OrderMachine>()
                .HasKey(om => new { om.OrderId, om.MachineId });

            modelBuilder.Entity<OrderMaterial>()
                .HasKey(om => new { om.OrderId, om.MaterialId });

            modelBuilder.Entity<ProductMaterial>()
                .HasKey(pm => new { pm.ProductId, pm.MaterialId });

            // Configure relationships
            modelBuilder.Entity<OrderMachine>()
                .HasOne(om => om.Order)
                .WithMany(o => o.OrderMachines)
                .HasForeignKey(om => om.OrderId);

            modelBuilder.Entity<OrderMachine>()
                .HasOne(om => om.Machine)
                .WithMany(m => m.OrderMachines)
                .HasForeignKey(om => om.MachineId);

            modelBuilder.Entity<MachineMaintenance>()
                .HasOne(mm => mm.Machine)
                .WithMany(m => m.MaintenanceHistory)
                .HasForeignKey(mm => mm.MachineId);

            modelBuilder.Entity<OrderMaterial>()
                .HasOne(om => om.Order)
                .WithMany(o => o.OrderMaterials)
                .HasForeignKey(om => om.OrderId);

            modelBuilder.Entity<OrderMaterial>()
                .HasOne(om => om.Material)
                .WithMany(m => m.OrderMaterials)
                .HasForeignKey(om => om.MaterialId);

            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Product)
                .WithMany(p => p.ProductMaterials)
                .HasForeignKey(pm => pm.ProductId);

            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Material)
                .WithMany(m => m.ProductMaterials)
                .HasForeignKey(pm => pm.MaterialId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Product)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.ProductId);
        }
    }
}
