using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Data;

/// <summary>
/// Khởi tạo DbContext cho hệ thống Quản lý sân bóng đá, tích hợp IdentityDbContext để quản lý người dùng và phân quyền.
/// </summary>
public partial class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Tập hợp các thực thể đặt sân.
    /// </summary>
    public virtual DbSet<Booking> Bookings { get; set; }

    /// <summary>
    /// Tập hợp các thực thể khách hàng.
    /// </summary>
    public virtual DbSet<Customer> Customers { get; set; }

    /// <summary>
    /// Tập hợp các thực thể sân bóng.
    /// </summary>
    public virtual DbSet<Pitch> Pitches { get; set; }

    /// <summary>
    /// Tập hợp các thực thể khung giờ.
    /// </summary>
    public virtual DbSet<TimeSlot> TimeSlots { get; set; }

    /// <summary>
    /// Tập hợp các thực thể ngôn ngữ hệ thống.
    /// </summary>
    public virtual DbSet<Language> Languages { get; set; }

    /// <summary>
    /// Tập hợp các bài viết.
    /// </summary>
    public virtual DbSet<Article> Articles { get; set; }

    /// <summary>
    /// Tập hợp các danh mục bài viết.
    /// </summary>
    public virtual DbSet<ArticleCategory> ArticleCategories { get; set; }

    /// <summary>
    /// Tập hợp các bình luận bài viết.
    /// </summary>
    public virtual DbSet<ArticleComment> ArticleComments { get; set; }

    /// <summary>Khụng giờ mặc định</summary>
    public virtual DbSet<DefaultTimeSlot> DefaultTimeSlots { get; set; }

    /// <summary>Giá dịch vụ</summary>
    public virtual DbSet<ServicePrice> ServicePrices { get; set; }

    /// <summary>Danh mục sản phẩm</summary>
    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    /// <summary>Đơn vị tính</summary>
    public virtual DbSet<Unit> Units { get; set; }

    /// <summary>Khuyến mại và mã giảm giá</summary>
    public virtual DbSet<Promotion> Promotions { get; set; }

    /// <summary>Sản phẩm trong Shop</summary>
    public virtual DbSet<Product> Products { get; set; }

    /// <summary>Đơn hàng bán lẻ</summary>
    public virtual DbSet<Order> Orders { get; set; }

    /// <summary>Chi tiết đơn hàng</summary>
    public virtual DbSet<OrderItem> OrderItems { get; set; }

    /// <summary>Phiếu nhập hàng</summary>
    public virtual DbSet<StockImport> StockImports { get; set; }

    /// <summary>Chi tiết phiếu nhập hàng</summary>
    public virtual DbSet<StockImportItem> StockImportItems { get; set; }

    /// <summary>Giao dịch sổ quỹ</summary>
    public virtual DbSet<CashTransaction> CashTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bookings__3214EC07796A9E90");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Bookings_Customers");

            entity.HasOne(d => d.Pitch).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PitchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Pitches");

            entity.HasOne(d => d.TimeSlot).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TimeSlotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_TimeSlots");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC0788E3795B");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne<Microsoft.AspNetCore.Identity.IdentityUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .HasConstraintName("FK_Customers_AspNetUsers")
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Pitch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pitches__3214EC07DC791CA5");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PricePerHour).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TimeSlot__3214EC071DF07EBE");

            entity.Property(e => e.EndTime).HasPrecision(0);
            entity.Property(e => e.PriceOverride).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StartTime).HasPrecision(0);

            entity.HasOne(d => d.Pitch).WithMany(p => p.TimeSlots)
                .HasForeignKey(d => d.PitchId)
                .HasConstraintName("FK_TimeSlots_Pitches");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AlternateName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SortOrder).HasDefaultValue(1);
            entity.Property(e => e.Status).HasDefaultValue(1);
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(500);
            entity.Property(e => e.Summary).HasMaxLength(500);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.AuthorId).HasMaxLength(450);
            entity.Property(e => e.AuthorName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue(0);
            entity.Property(e => e.ViewCount).HasDefaultValue(0);

            entity.HasOne(d => d.Category)
                .WithMany(p => p.Articles)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Articles_ArticleCategories");
        });

        modelBuilder.Entity<ArticleCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ArticleCategories_Parent");
        });

        modelBuilder.Entity<ArticleComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AuthorName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AuthorEmail).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue(0);

            entity.HasOne(d => d.Article)
                .WithMany(p => p.Comments)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ArticleComments_Articles");

            entity.HasOne(d => d.Parent)
                .WithMany()
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ArticleComments_Parent");
        });

        modelBuilder.Entity<DefaultTimeSlot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PriceOverride).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.DayType).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
        });

        modelBuilder.Entity<ServicePrice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServiceName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PriceUnit).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(200);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.ServiceType).HasDefaultValue(5);
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconClass).HasMaxLength(200);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ProductCategories_Parent");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Symbol).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SKU).HasMaxLength(50);
            entity.HasIndex(e => e.SKU).IsUnique().HasFilter("[SKU] IS NOT NULL");
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SellingPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");

            entity.HasOne(d => d.Category)
                .WithMany()
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Products_ProductCategories");

            entity.HasOne(d => d.Unit)
                .WithMany()
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Products_Units");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderCode).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.OrderCode).IsUnique();
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FinalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentMethod).HasDefaultValue(0);
            entity.Property(e => e.PaymentStatus).HasDefaultValue(0);
            entity.Property(e => e.OrderStatus).HasDefaultValue(1);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");

            entity.HasOne(d => d.Customer)
                .WithMany()
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Orders_Customers");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OrderItems_Orders");

            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_OrderItems_Products");
        });

        modelBuilder.Entity<StockImport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImportCode).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.ImportCode).IsUnique();
            entity.Property(e => e.SupplierName).HasMaxLength(150);
            entity.Property(e => e.ImportDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
        });

        modelBuilder.Entity<StockImportItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.StockImport)
                .WithMany(p => p.ImportItems)
                .HasForeignKey(d => d.StockImportId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_StockImportItems_StockImports");

            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_StockImportItems_Products");
        });

        modelBuilder.Entity<CashTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionCode).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.TransactionCode).IsUnique();
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PayerPayee).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
