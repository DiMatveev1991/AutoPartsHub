using AutoPartsHub.Core;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Persistence;

/// <summary>
/// Представляет сеанс Entity Framework Core для базы данных AutoParts Hub.
/// </summary>
/// <param name="options">Настройки подключения и провайдера базы данных.</param>
public sealed class AutoPartsDbContext(DbContextOptions<AutoPartsDbContext> options)
    : DbContext(options)
{
    /// <summary>Получает набор пользователей.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Получает набор категорий.</summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>Получает набор товаров.</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>Получает набор правил совместимости товаров.</summary>
    public DbSet<ProductCompatibility> ProductCompatibilities => Set<ProductCompatibility>();

    /// <summary>Получает набор автомобилей пользователей.</summary>
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    /// <summary>Получает набор корзин.</summary>
    public DbSet<Cart> Carts => Set<Cart>();

    /// <summary>Получает набор позиций корзин.</summary>
    public DbSet<CartItem> CartItems => Set<CartItem>();

    /// <summary>Получает набор заказов.</summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>Получает набор позиций заказов.</summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>Получает набор товарных подписок.</summary>
    public DbSet<ProductSubscription> ProductSubscriptions => Set<ProductSubscription>();

    /// <summary>Получает набор уведомлений.</summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>
    /// Настраивает таблицы, ограничения, индексы и связи доменной модели.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUsers(modelBuilder);
        ConfigureCatalog(modelBuilder);
        ConfigureCommerce(modelBuilder);
        ConfigureNotifications(modelBuilder);
    }

    /// <summary>
    /// Настраивает пользователей и автомобили.
    /// </summary>
    private static void ConfigureUsers(ModelBuilder builder)
    {
        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(item => item.Role).HasConversion<int>();
            entity.HasIndex(item => item.TelegramChatId).IsUnique();
        });

        builder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicles");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Vin).HasMaxLength(17).IsRequired();
            entity.Property(item => item.Make).HasMaxLength(80).IsRequired();
            entity.Property(item => item.Model).HasMaxLength(80).IsRequired();
            entity.Property(item => item.Engine).HasMaxLength(80);
            entity.HasIndex(item => item.Vin).IsUnique();
            entity.HasIndex(item => item.UserId);
            entity.HasOne(item => item.User)
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Настраивает категории, товары и совместимость.
    /// </summary>
    private static void ConfigureCatalog(ModelBuilder builder)
    {
        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(120).IsRequired();
            entity.Property(item => item.Slug).HasMaxLength(120).IsRequired();
            entity.HasIndex(item => item.Slug).IsUnique();
        });

        builder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Article).HasMaxLength(80).IsRequired();
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(4000).IsRequired();
            entity.Property(item => item.Condition).HasConversion<int>();
            entity.Property(item => item.Price).HasPrecision(12, 2);
            entity.Property(item => item.ConcurrencyToken).IsConcurrencyToken();
            entity.HasIndex(item => item.Article).IsUnique();
            entity.HasIndex(item => new { item.IsActive, item.CategoryId });
            entity.HasOne(item => item.Category)
                .WithMany()
                .HasForeignKey(item => item.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(item => item.Compatibilities)
                .WithOne(item => item.Product)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(item => item.Compatibilities)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<ProductCompatibility>(entity =>
        {
            entity.ToTable("ProductCompatibilities");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Make).HasMaxLength(80).IsRequired();
            entity.Property(item => item.Model).HasMaxLength(80).IsRequired();
            entity.Property(item => item.Engine).HasMaxLength(80);
            entity.HasIndex(item => new { item.Make, item.Model, item.YearFrom, item.YearTo });
        });
    }

    /// <summary>
    /// Настраивает корзины, заказы и их позиции.
    /// </summary>
    private static void ConfigureCommerce(ModelBuilder builder)
    {
        builder.Entity<Cart>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.UserId).IsUnique();
            entity.HasOne(item => item.User)
                .WithOne()
                .HasForeignKey<Cart>(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(item => item.Items)
                .WithOne(item => item.Cart)
                .HasForeignKey(item => item.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(item => item.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(item => new { item.CartId, item.ProductId });
            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.OrderNumber).HasMaxLength(40).IsRequired();
            entity.Property(item => item.Status).HasConversion<int>();
            entity.Property(item => item.ContactName).HasMaxLength(120).IsRequired();
            entity.Property(item => item.Phone).HasMaxLength(30).IsRequired();
            entity.Property(item => item.DeliveryAddress).HasMaxLength(500).IsRequired();
            entity.Property(item => item.DeliveryMethod).HasConversion<int>();
            entity.Property(item => item.PaymentMethod).HasConversion<int>();
            entity.Property(item => item.Total).HasPrecision(12, 2);
            entity.HasIndex(item => item.OrderNumber).IsUnique();
            entity.HasIndex(item => new { item.UserId, item.CreatedAt });
            entity.HasOne(item => item.User)
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(item => item.Items)
                .WithOne(item => item.Order)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(item => item.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Article).HasMaxLength(80).IsRequired();
            entity.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.UnitPrice).HasPrecision(12, 2);
            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Настраивает подписки и уведомления.
    /// </summary>
    private static void ConfigureNotifications(ModelBuilder builder)
    {
        builder.Entity<ProductSubscription>(entity =>
        {
            entity.ToTable("ProductSubscriptions");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Type).HasConversion<int>();
            entity.Property(item => item.TargetPrice).HasPrecision(12, 2);
            entity.HasIndex(item => new { item.UserId, item.ProductId, item.Type, item.IsActive });
            entity.HasOne(item => item.User)
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Type).HasMaxLength(60).IsRequired();
            entity.Property(item => item.Text).HasMaxLength(1000).IsRequired();
            entity.Property(item => item.Status).HasConversion<int>();
            entity.Property(item => item.Error).HasMaxLength(1000);
            entity.HasIndex(item => new { item.Status, item.CreatedAt });
            entity.HasOne(item => item.User)
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
