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


        public DbSet<DiningArea> DiningAreas { get; set; }

        public DbSet<RestaurantTable> RestaurantTables { get; set; }

        public DbSet<OperatingHour> OperatingHours { get; set; }

        public DbSet<MenuCategory> MenuCategories { get; set; }

        public DbSet<MenuItem> MenuItems { get; set; }

        public DbSet<Promotion> Promotions { get; set; }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Dining Area name must be unique
            modelBuilder.Entity<DiningArea>()
                .HasIndex(d => d.Name)
                .IsUnique();


            // Restaurant Table number must be unique
            modelBuilder.Entity<RestaurantTable>()
                .HasIndex(t => t.TableNumber)
                .IsUnique();


            // Dining Area 1 -> many Restaurant Tables
            modelBuilder.Entity<RestaurantTable>()
                .HasOne(t => t.DiningArea)
                .WithMany(d => d.RestaurantTables)
                .HasForeignKey(t => t.DiningAreaId)
                .OnDelete(DeleteBehavior.Restrict);


            // One Operating Hour record per day
            modelBuilder.Entity<OperatingHour>()
                .HasIndex(o => o.Day)
                .IsUnique();


            modelBuilder.Entity<MenuCategory>()
                .HasIndex(c => c.Name)
                .IsUnique();


            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.MenuCategory)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(m => m.MenuCategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasColumnType("decimal(10,2)");
        }
    }
}