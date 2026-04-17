using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WpfCRUD.Models;

namespace WpfCRUD
{
    class DatabaseContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Manufacture> Manufactures { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductDescription> ProductDescriptions { get; set; }
        public DbSet<ProductName> ProductNames { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<UnitName> UnitNames { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=database.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, Name = "Гость"},
                new UserRole { Id = 2, Name = "Менеджер" },
                new UserRole { Id = 3, Name = "Владелец" },
                new UserRole { Id = 4, Name = "Администратор"}
            );

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Email = "admin", Password = "admin", UserRoleId = 4}
            );

            // Test Data
            modelBuilder.Entity<User>().HasData(
                new User { Id = 2, FullName = "Иванов Иван Иванович" , Email = "ivan@yandex.ru", Password = "ivanpass", UserRoleId = 2 },
                new User { Id = 3, FullName = "Иванов Иван Иванович", Email = "ivan@yandex.ru", Password = "ivanpass", UserRoleId = 3 }
            );
        }
    }
}
