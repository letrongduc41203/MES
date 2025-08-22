using Microsoft.EntityFrameworkCore;
using MES.Models;

namespace MES.Data
{
    public class MESDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=DucFendi;Database=MES_ProductionDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}
