using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using tnt_wpf_children.Models;

namespace tnt_wpf_children.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Relatives> Relatives { get; set; }
        public DbSet<Sessions> Sessions { get; set; }
        public DbSet<Admin> Admins { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Tạo folder riêng
            string appFolder = System.IO.Path.Combine(appDataPath, "ChildrenProtectionApp");

            // Nếu chưa có thì tạo, có rồi thì thôi
            if (!System.IO.Directory.Exists(appFolder))
                System.IO.Directory.CreateDirectory(appFolder);

            // Tên file DB
            string dbPath = System.IO.Path.Combine(appFolder, "children_protection.db");

            // Cho EF Core dùng file này
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Relatives>(e =>
            {
                e.HasKey(r => r.Id);
                e.Property(r => r.FullName).IsRequired().HasMaxLength(100);
                e.Property(r => r.PhoneNumber).IsRequired().HasMaxLength(10);
                e.Property(r => r.Face).IsRequired();
                e.Property(r => r.CreatedAt).IsRequired();
                e.Property(r => r.UpdatedAt);
                e.HasIndex(r => r.PhoneNumber).IsUnique();
                e.Property(r => r.Note).HasMaxLength(255);
                e.HasMany(r => r.Sessions).WithOne(s => s.Relative).HasForeignKey(s => s.RelativeId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(r => r.PhoneNumber).IsUnique();
                e.Property(s => s.Status).IsRequired();
            });
            
            modelBuilder.Entity<Admin>(e =>
            {
                e.HasKey(a => a.Username);
                e.Property(a => a.Username).IsRequired().HasMaxLength(20);
                e.Property(a => a.PasswordHash).IsRequired().HasMaxLength(20);
            });
            
            modelBuilder.Entity<Sessions>(e =>
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.CheckinTime).IsRequired();
                e.Property(s => s.CheckoutTime);
                e.Property(s => s.Status).IsRequired();                
                e.HasOne(s => s.Relative)
                    .WithMany(r => r.Sessions)
                    .HasForeignKey(s => s.RelativeId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(s => s.NumberOfChildren).IsRequired();
                e.Property(r => r.Note).HasMaxLength(255);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
