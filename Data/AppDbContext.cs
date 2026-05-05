using Microsoft.EntityFrameworkCore;
using raspisanie.Models;

namespace raspisanie.Data;

// Контекст базы данных — SQLite файл raspisanie.db
public class AppDbContext : DbContext
{
    public DbSet<Para> Paras { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=raspisanie.db");
}
