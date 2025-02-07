using FoodY.Models;
using Microsoft.EntityFrameworkCore;

using FoodY.ViewModel;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;





namespace FoodY.Data
{
    public class ApplicationDbContext : IdentityDbContext

    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;
        public DbSet<Contact> ContactUs { get; set; } = null!;

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartDetail> CartDetails { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure Cart entity
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(c => c.Id);  // Define primary key

                // Configure relationship: One Cart has many CartDetails
                entity.HasMany(c => c.CartDetails)
                      .WithOne(cd => cd.Cart)
                      .HasForeignKey(cd => cd.CartId)
                      .OnDelete(DeleteBehavior.Cascade);  // Cascade delete if Cart is deleted

                // Optional IdentityUser relationship (not mapped)
                entity.Ignore(c => c.UserId);  // Ignore the IdentityUser navigation property

                // Field configurations
                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(100);  // Set maximum length for Name

                entity.Property(c => c.PaymentType)
                      .IsRequired()
                      .HasMaxLength(50);  // Set maximum length for PaymentType

                entity.Property(c => c.CreatedDate)
                      .HasDefaultValueSql("GETUTCDATE()");  // Default value for CreatedDate

                entity.Property(c => c.Status)
                      .HasMaxLength(50);  // Optional, set maximum length for Status

                entity.Property(c => c.UserId)
                      .HasMaxLength(450);  // Typically, UserId has max length of 450
            });

            // Configure CartDetail entity
            modelBuilder.Entity<CartDetail>(entity =>
            {
                entity.HasKey(cd => cd.Id);  // Define primary key

                // Configure relationship: CartDetail belongs to one Cart
                entity.HasOne(cd => cd.Cart)
                      .WithMany(c => c.CartDetails)
                      .HasForeignKey(cd => cd.CartId)
                      .OnDelete(DeleteBehavior.Cascade);  // Cascade delete if Cart is deleted

                // Configure relationship: CartDetail belongs to one Product
                entity.HasOne(cd => cd.Product)
                      .WithMany()
                      .HasForeignKey(cd => cd.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);  // Cascade delete if Product is deleted

                // Optional IdentityUser relationship (not mapped)
                entity.Ignore(cd => cd.UserId);  // Ignore the IdentityUser navigation property

                // Field configurations
                entity.Property(cd => cd.Quantity)
                      .IsRequired()
                      .HasMaxLength(10);  // Set maximum length for Quantity

                entity.Property(cd => cd.Price)
                      .HasColumnType("decimal(18,2)");  // Define precision for Price

                entity.Property(cd => cd.CreatedDate)
                      .HasDefaultValueSql("GETUTCDATE()");  // Default value for CreatedDate

                entity.Property(cd => cd.UserId)
                      .HasMaxLength(450);  // Set maximum length for UserId
            });


            // Configure Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.Property(i => i.Name).IsRequired();
                entity.HasOne(i => i.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(i => i.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ProductImage
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImages");
                entity.Property(ii => ii.FileName).IsRequired();
                entity.HasOne(ii => ii.Product)
                    .WithMany(i => i.Images)
                    .HasForeignKey(ii => ii.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.Property(c => c.Name).IsRequired();
            });
            //configure contact
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.ToTable("ContactUs");
                entity.Property(c => c.Name).IsRequired();
            }
            );



        }

    }
}