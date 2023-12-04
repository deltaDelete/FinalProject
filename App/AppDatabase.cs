using App.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace App;

public class AppDatabase : DbContext
{
    private static readonly string ConnectionString =
        "server=localhost;user=dev;password=devPassword;database=project3";
    // "server=10.10.1.24;user=user_01;password=user01pro;database=pro1_2";

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderedProduct> OrdersProducts { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;

    /// <inheritdoc cref="DbContext"/>
    public AppDatabase()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
            ConnectionString,
            ServerVersion.AutoDetect(ConnectionString)
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderedProduct>()
            .HasOne(x => x.Order)
            .WithMany(x => x.Products);
        modelBuilder.Entity<OrderedProduct>()
            .HasOne(x => x.Product)
            .WithMany(x => x.Orders);

        modelBuilder.Entity<OrderStatus>()
            .HasData(
                new OrderStatus { Id = 1, Name = "Оформлен" },
                new OrderStatus { Id = 2, Name = "Ожидает оплату" },
                new OrderStatus { Id = 3, Name = "Оплачен" },
                new OrderStatus { Id = 4, Name = "Выполнен" }
            );

        modelBuilder.Entity<OrderStatus>()
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
    }
}