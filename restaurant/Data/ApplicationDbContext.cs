using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using restaurant.Models;
using restaurant.Models.Enums;
using restaurant.Models.Shared;

namespace restaurant.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // --- Customer module tables ---
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        // Shared with Admin Azure schema (mapped below)
        public DbSet<MenuItem> MenuItems { get; set; } = null!;
        public DbSet<DiningTable> DiningTables { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; } = null!;
        public DbSet<Promotion> Promotions { get; set; } = null!;
        public DbSet<OperatingHour> OperatingHours { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Map to Admin's existing Azure tables — do not create parallel copies
            builder.Entity<DiningTable>(e =>
            {
                e.ToTable("RestaurantTables");
                e.HasKey(t => t.Id);
                e.Property(t => t.Id).HasColumnName("RestaurantTableId");
            });

            builder.Entity<MenuItem>().ToTable("MenuItems");
            builder.Entity<OperatingHour>(e =>
            {
                e.ToTable("OperatingHours");
                e.HasKey(o => o.OperatingHourId);
                e.HasIndex(o => o.Day).IsUnique(); // one row per weekday, as in Admin
            });
            builder.Entity<Announcement>().ToTable("Announcements");
            builder.Entity<Promotion>(e =>
            {
                e.ToTable("Promotions");
                e.Property(p => p.DiscountValue).HasColumnType("decimal(10,2)");
            });

            builder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(8,2)");

            builder.Entity<Reservation>().Property(r => r.Status)
                .HasConversion<string>().HasMaxLength(20);
            builder.Entity<Order>().Property(o => o.Status)
                .HasConversion<string>().HasMaxLength(20);
            builder.Entity<Payment>().Property(p => p.Status)
                .HasConversion(new ValueConverter<PaymentStatus, string>(
                    v => v.ToString(),
                    v => ParsePaymentStatus(v))).HasMaxLength(20);
            builder.Entity<Payment>().Property(p => p.Method)
                .HasConversion(new ValueConverter<PaymentMethod, string>(
                    v => v == PaymentMethod.EWallet ? "E-Wallet" : v.ToString(),
                    v => ParsePaymentMethod(v))).HasMaxLength(20);
            builder.Entity<Notification>().Property(n => n.Type)
                .HasConversion<string>().HasMaxLength(20);

            // --- several relationships at once (SQL Server disallows multiple cascade paths to the same table). ---
            builder.Entity<Reservation>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Feedback>()
                .HasOne(f => f.Customer)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Order>()
                .HasOne(o => o.Reservation)
                .WithMany(r => r.Orders)
                .HasForeignKey(o => o.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Feedback>()
                .HasOne(f => f.Order)
                .WithMany()
                .HasForeignKey(f => f.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Feedback>()
                .HasOne(f => f.Reservation)
                .WithMany()
                .HasForeignKey(f => f.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed data is applied via SQL/customer_tables_and_seed.sql
            // so we do not conflict with Admin's MenuCategoryId schema.
        }

        // Shared Azure DB may contain Staff/Admin spellings that don't match
        // our enum names exactly — map them safely instead of crashing Orders.
        private static PaymentMethod ParsePaymentMethod(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return PaymentMethod.Cash;

            return value.Trim().ToLowerInvariant() switch
            {
                "cash" => PaymentMethod.Cash,
                "card" => PaymentMethod.Card,
                "online" => PaymentMethod.Online,
                "e-wallet" or "ewallet" or "e_wallet" => PaymentMethod.EWallet,
                // Older rows accidentally stored a status in the Method column
                "pending" or "cancelled" or "canceled" => PaymentMethod.Online,
                _ => Enum.TryParse<PaymentMethod>(value, true, out var parsed)
                    ? parsed
                    : PaymentMethod.Online
            };
        }

        private static PaymentStatus ParsePaymentStatus(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return PaymentStatus.Pending;

            return value.Trim().ToLowerInvariant() switch
            {
                "pending" => PaymentStatus.Pending,
                "paid" => PaymentStatus.Paid,
                "refunded" => PaymentStatus.Refunded,
                "failed" => PaymentStatus.Failed,
                "cancelled" or "canceled" => PaymentStatus.Cancelled,
                _ => Enum.TryParse<PaymentStatus>(value, true, out var parsed)
                    ? parsed
                    : PaymentStatus.Pending
            };
        }
    }
}