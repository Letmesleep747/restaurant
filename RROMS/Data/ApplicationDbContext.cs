using Microsoft.EntityFrameworkCore;
using RROMS.Models;

namespace RROMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<DiningArea> DiningAreas { get; set; }

        public DbSet<RestaurantTable> RestaurantTables { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<DiningArea>()
                .HasIndex(d => d.Name)
                .IsUnique();


            modelBuilder.Entity<RestaurantTable>()
                .HasIndex(t => t.TableNumber)
                .IsUnique();


            modelBuilder.Entity<RestaurantTable>()
                .HasOne(t => t.DiningArea)
                .WithMany(d => d.RestaurantTables)
                .HasForeignKey(t => t.DiningAreaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}