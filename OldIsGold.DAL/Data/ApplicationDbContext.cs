using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OldIsGold.DAL.Models;

namespace OldIsGold.DAL.Data
{
    public class ApplicationDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<ItemImage> ItemImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Item entity
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.ItemId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Items)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Seller)
                    .WithMany(u => u.Items)
                    .HasForeignKey(e => e.SellerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ItemImage entity
            modelBuilder.Entity<ItemImage>(entity =>
            {
                entity.HasKey(e => e.ImageId);
                entity.Property(e => e.ImageData).IsRequired();
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(200);

                entity.HasOne(e => e.Item)
                    .WithMany(i => i.Images)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Item)
                    .WithMany(i => i.Orders)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Buyer)
                    .WithMany(u => u.OrdersAsBuyer)
                    .HasForeignKey(e => e.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Seller)
                    .WithMany(u => u.OrdersAsSeller)
                    .HasForeignKey(e => e.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Message entity
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.MessageId);
                entity.Property(e => e.Content).IsRequired();

                entity.HasOne(e => e.Sender)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Receiver)
                    .WithMany(u => u.ReceivedMessages)
                    .HasForeignKey(e => e.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Item)
                    .WithMany(i => i.Messages)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Report entity
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.ReportId);
                entity.Property(e => e.Reason).IsRequired().HasMaxLength(200);

                entity.HasOne(e => e.Item)
                    .WithMany(i => i.Reports)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Reporter)
                    .WithMany(u => u.Reports)
                    .HasForeignKey(e => e.ReporterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Wishlist entity
            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.HasKey(e => e.WishlistId);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.WishlistItems)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Item)
                    .WithMany(i => i.WishlistedBy)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure AdminLog entity
            modelBuilder.Entity<AdminLog>(entity =>
            {
                entity.HasKey(e => e.LogId);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Admin)
                    .WithMany(u => u.AdminLogs)
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Rating entity
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(e => e.RatingId);
                entity.Property(e => e.Score).IsRequired();

                entity.HasOne(e => e.RatedUser)
                    .WithMany(u => u.RatingsReceived)
                    .HasForeignKey(e => e.RatedUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RaterUser)
                    .WithMany(u => u.RatingsGiven)
                    .HasForeignKey(e => e.RaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.Ratings)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            });
        }
    }
}
