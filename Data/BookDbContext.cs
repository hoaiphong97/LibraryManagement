using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LibraryManagement.Data
{
    public class BookDbContext : DbContext
    {
        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<PreOrder> PreOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình relationship cho Category (self-referencing)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Series → Category (optional)
            modelBuilder.Entity<Series>()
                .HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // PreOrder → Series (optional)
            modelBuilder.Entity<PreOrder>()
                .HasOne(p => p.Series)
                .WithMany()
                .HasForeignKey(p => p.SeriesId)
                .OnDelete(DeleteBehavior.SetNull);

            // PreOrder → Book (optional, sau khi lên kệ)
            modelBuilder.Entity<PreOrder>()
                .HasOne(p => p.Book)
                .WithMany()
                .HasForeignKey(p => p.BookId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed data cho Categories (phân cấp)
            modelBuilder.Entity<Category>().HasData(
                // Cấp 1
                new Category { Id = 1, Name = "DAM MY", ParentId = 1 },
                new Category { Id = 2, Name = "NGON TINH", ParentId = 2 },
                new Category { Id = 3, Name = "VAN HOC VIET NAM", ParentId = 3 },
                new Category { Id = 4, Name = "VAN HOC CHAU A", ParentId = 4 },
                new Category { Id = 5, Name = "VAN HOC NUOC NGOAI", ParentId = 5 },
                new Category { Id = 6, Name = "SACH NGOAI VAN", ParentId = 6 },
                new Category { Id = 7, Name = "TRUYEN TRANH", ParentId = 7 }
            );
        }
    }
}
