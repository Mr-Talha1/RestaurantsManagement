using BIPL_RAASTP2M.Models;
using Microsoft.EntityFrameworkCore;

namespace BIPL_RAASTP2M.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

       
        public DbSet<Logs> logs { get; set; }
        public DbSet<SystemUsers> SystemUsers { get; set; }
        public DbSet<Merchants> Merchants { get; set; }
        public DbSet<DiningTables> DiningTables { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<Customers> Customers { get; set; }
       
    }

   
}
