﻿using Ecommerce_APIs.Models.Entites;
using Ecommerce_APIs.Models.Entites.Ecommerce_APIs.Models.Entities;
using Ecommerce_APIs.Models.Entities;
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

        public DbSet<Users> userss { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Self-referencing relationship: ParentCategory ↔ SubCategories
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete loops

            // Product ↔ Category relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.CategoryBy)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Category → Users (CreatedBy)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Category → Users (UpdatedBy)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.UpdatedBy)
                .WithMany()
                .HasForeignKey(c => c.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // For ading the images to the Product
            modelBuilder.Entity<ProductImage>()
                 .HasOne(pi => pi.Product)
                 .WithMany(p => p.ProductImages)
                 .HasForeignKey(pi => pi.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);

            // for adding oder items to the order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()         
                .HasForeignKey(o => o.UserId);


            modelBuilder.Entity<OrderItem>()
                  .HasOne(oi => oi.Product)
                  .WithMany()
                  .HasForeignKey(oi => oi.ProductId);

            // For Order tracking
            modelBuilder.Entity<OrderTracking>()
                 .HasOne(ot => ot.User)
                 .WithMany()
                 .HasForeignKey(ot => ot.UserId)
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

        }
    }

    }
