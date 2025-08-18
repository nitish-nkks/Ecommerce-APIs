using Ecommerce_APIs.Models.Entites;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_APIs.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderTracking> OrderTrackings { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<StaticPage> StaticPages { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<FlashSale> FlashSales { get; set; }
        public DbSet<AllowedState> AllowedStates { get; set; }
        public DbSet<AllowedCity> AllowedCities { get; set; }
        public DbSet<InternalUser> InternalUsers { get; set; }

        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

        public DbSet<OrderReturnRequest> OrderReturnRequests { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Self-referencing relationship: ParentCategory ↔ SubCategories
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product ↔ Category relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Category → Customer (CreatedBy)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Category → InternalUser (UpdatedBy)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.UpdatedBy)
                .WithMany()
                .HasForeignKey(c => c.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);


            // for adding oder items to the order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                  .HasOne(oi => oi.Product)
                  .WithMany()
                  .HasForeignKey(oi => oi.ProductId);

            // For Order tracking
            modelBuilder.Entity<OrderTracking>()
                 .HasOne(ot => ot.Customer)
                 .WithMany()
                 .HasForeignKey(ot => ot.CustomerId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderTracking>()
                .HasOne(ot => ot.Order)
                .WithMany()
                .HasForeignKey(ot => ot.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // BlogPost ↔ Category relationship
            modelBuilder.Entity<BlogPost>()
                .Property(b => b.Status)
                .HasDefaultValue("draft");
            // FlashSale ↔ Product relationship
            modelBuilder.Entity<FlashSale>()
                .HasOne(fs => fs.Product)
                .WithMany()
                .HasForeignKey(fs => fs.ProductId);


            modelBuilder.Entity<Customer>()
                .Ignore(c => c.CartItems);


        }
    }

    }
