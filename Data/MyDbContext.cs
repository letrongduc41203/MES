using MES.Models;
using Microsoft.EntityFrameworkCore;

namespace MES.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderMaterial> OrderMaterials { get; set; }
        public DbSet<ProductMaterial> ProductMaterials { get; set; }
        public DbSet<OrderMachine> OrderMachines { get; set; }
        public DbSet<Machine> Machines { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite keys
            modelBuilder.Entity<OrderMaterial>()
                .HasKey(om => new { om.OrderId, om.MaterialId });
            modelBuilder.Entity<ProductMaterial>()
                .HasKey(pm => new { pm.ProductId, pm.MaterialId });
            modelBuilder.Entity<OrderMachine>()
                .HasKey(om => new { om.OrderId, om.MachineId });

            // Relationships for OrderMachine
            modelBuilder.Entity<OrderMachine>()
                .HasOne(om => om.Order)
                .WithMany(o => o.OrderMachines)
                .HasForeignKey(om => om.OrderId);

            modelBuilder.Entity<OrderMachine>()
                .HasOne(om => om.Machine)
                .WithMany(m => m.OrderMachines)
                .HasForeignKey(om => om.MachineId);


        }
    }
}
