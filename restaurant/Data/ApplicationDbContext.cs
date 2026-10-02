using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
        // --- Extra Tables and MenuItems ---
        public DbSet<Table> Tables { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        // ---  shared tables for Admin module context (if admin module have then delete in customer module context) ---
        public DbSet<MenuItem> MenuItems { get; set; } = null!;
        public DbSet<DiningTable> DiningTables { get; set; } = null!;
        // ------------------------------------------------------------
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // add relationships, constraints, indexes here
            builder.Entity<Reservation>().Property(r => r.Status)
                .HasConversion<string>().HasMaxLength(20);
            builder.Entity<Order>().Property(o => o.Status)
                .HasConversion<string>().HasMaxLength(20);
            builder.Entity<Payment>().Property(p => p.Status)
                .HasConversion<string>().HasMaxLength(20);
            builder.Entity<Payment>().Property(p => p.Method)
                .HasConversion<string>().HasMaxLength(20);
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

            builder.Entity<DiningTable>().HasData(
                new DiningTable { Id = 1, TableNumber = "T1", Capacity = 2, IsActive = true },
                new DiningTable { Id = 1, TableNumber = "T2", Capacity = 4, IsActive = true },
                new DiningTable { Id = 1, TableNumber = "T3", Capacity = 6, IsActive = true }
                );

            builder.Entity<MenuItem>().HasData(
                new MenuItem { Id = 1, Name = "Margherita Pizza", Description = "Tomato, mozzarella, basil.", Price = 9.50m, Category = "Main Course", IsAvailable = true },
                new MenuItem { Id = 2, Name = "Caesar Salad", Description = "Crisp romaine lettuce with Caesar dressing, croutons, and Parmesan cheese.", Price = 6.00m, Category = "Starter", IsAvailable = true },
                new MenuItem { Id = 3, Name = "Sparkling Water", Description = "Chilled sparkling water with a hint of lemon.", Price = 2.00m, Category = "Beverage", IsAvailable = true }
                );
        }
    }
}