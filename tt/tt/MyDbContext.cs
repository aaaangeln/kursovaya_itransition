using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using tt.Models;

namespace tt
{
    public class MyDbContext : DbContext
    {
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Collection> Collection { get; set; }

        // Other DbSet properties and configuration

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configure the database provider and connection string for MySQL
            optionsBuilder.UseMySql("Server=mysql6013.site4now.net;Database=db_aa373f_root;Uid=aa373f_root;Pwd=rootroot1;",
                new MySqlServerVersion(new Version(5, 7, 36)));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure your model relationships and other configurations
        }
    }
}
