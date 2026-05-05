using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Para> Paras { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=raspisanie.db");
}