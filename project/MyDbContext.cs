using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project
{
    public class MyDbContext : DbContext
    {
        //public DbSet<Photo> Photos { get; set; }
        public DbSet<Collections> Collections { get; set; }
        public DbSet<Lists> Lists { get; set; }
        public DbSet<MyViewModels> myViewModels { get; set; }

        // Other DbSet properties and configuration

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configure the database provider and connection string for MySQL
            optionsBuilder.UseMySql("Server=mysql6013.site4now.net;Database=db_aa373f_root;Uid=aa373f_root;Pwd=rootroot1;Charset=utf8;",
                new MySqlServerVersion(new Version(5, 7, 36)));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure your model relationships and other configurations
        }
    }
}
