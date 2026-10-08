using KT10.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace KT10.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Для реальной СУБД индексы обеспечат уникальность. InMemory-провайдер их НЕ проверяет
        b.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();
        b.Entity<AppUser>().HasIndex(u => u.Username).IsUnique();
    }
}