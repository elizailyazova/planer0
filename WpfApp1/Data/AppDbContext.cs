using WpfApp1.Models;

namespace WpfApp1.Data;

using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<TaskModel> Tasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseMySql("server=localhost;database=planer;user=root;password=1234",
            new MySqlServerVersion(new Version(9, 1, 0)));
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskModel>()
            .Property(t => t.Id)
            .ValueGeneratedOnAdd();
    }
    
}


