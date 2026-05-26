using Microsoft.EntityFrameworkCore;
using raspisanie.Models;

namespace raspisanie.Data;

public class AppDbContext : DbContext
{
    public DbSet<Para> Paras => Set<Para>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=raspisanie.db");
    }
}
