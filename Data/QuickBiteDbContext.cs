using Microsoft.EntityFrameworkCore;
using QuickBiteAPI.Models;

namespace QuickBiteAPI.Data
{
    public class QuickBiteDbContext : DbContext
    {
        public QuickBiteDbContext(DbContextOptions<QuickBiteDbContext> options) : base(options)
        {
        }

        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure MenuItem entity
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DietaryTag).HasMaxLength(100);
            });

            // Seed some initial data
            modelBuilder.Entity<MenuItem>().HasData(
                new MenuItem
                {
                    Id = 1,
                    Name = "Margherita Pizza",
                    Description = "Classic pizza with tomato sauce, mozzarella, and fresh basil",
                    Price = 12.99m,
                    Category = "Pizza",
                    DietaryTag = "Vegetarian"
                },
                new MenuItem
                {
                    Id = 2,
                    Name = "Chicken Burger",
                    Description = "Juicy grilled chicken breast with lettuce, tomato, and mayo",
                    Price = 9.99m,
                    Category = "Burger",
                    DietaryTag = "Non-Vegetarian"
                },
                new MenuItem
                {
                    Id = 3,
                    Name = "Caesar Salad",
                    Description = "Fresh romaine lettuce with parmesan cheese and caesar dressing",
                    Price = 8.99m,
                    Category = "Salad",
                    DietaryTag = "Vegetarian"
                }
            );
        }
    }
}
