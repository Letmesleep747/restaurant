using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RROMS.Models;

namespace RROMS.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<RestaurantTable> RestaurantTables { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName)
                    .HasColumnName("FullName")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.IsActive)
                    .HasColumnName("IsActive")
                    .IsRequired();

                entity.Property(u => u.CreatedAt)
                    .HasColumnName("CreatedAt")
                    .IsRequired();

                entity.Property(u => u.Address)
                    .HasColumnName("Address");
            });

            // Reservations
            builder.Entity<Reservation>(entity =>
            {
                entity.ToTable("Reservations");

                entity.HasKey(r => r.Id);

                entity.HasOne(r => r.Customer)
                    .WithMany()
                    .HasForeignKey(r => r.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.RestaurantTable)
                    .WithMany()
                    .HasForeignKey(r => r.TableId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(r => r.Status)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(r => r.SpecialRequests)
                    .HasMaxLength(500);
            });

            // RestaurantTables
            builder.Entity<RestaurantTable>(entity =>
            {
                entity.ToTable("RestaurantTables");

                entity.HasKey(t => t.RestaurantTableId);

                entity.Property(t => t.TableNumber)
                    .HasMaxLength(20)
                    .IsRequired();
            });

            // Orders
            builder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                entity.HasKey(o => o.Id);

                entity.Property(o => o.TotalAmount)
                    .HasColumnType("decimal(10,2)");

                entity.HasOne(o => o.Customer)
                    .WithMany()
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Reservation)
                    .WithMany()
                    .HasForeignKey(o => o.ReservationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderItems
            builder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");

                entity.HasKey(oi => oi.Id);

                entity.Property(oi => oi.UnitPrice)
                    .HasColumnType("decimal(10,2)");

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.MenuItem)
                    .WithMany()
                    .HasForeignKey(oi => oi.MenuItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // MenuItems
            builder.Entity<MenuItem>(entity =>
            {
                entity.ToTable("MenuItems");

                entity.HasKey(m => m.Id);

                entity.Property(m => m.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(m => m.Price)
                    .HasColumnType("decimal(10,2)");
            });

            // Payments
            builder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Amount)
                    .HasColumnType("decimal(10,2)");

                entity.HasOne(p => p.Order)
                    .WithMany(o => o.Payments)
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}