using Microsoft.EntityFrameworkCore;
using Student.Core.Entities;

namespace Student.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Core.Entities.Student> Students { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Core.Entities.Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Course).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Age).IsRequired();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<Core.Entities.Student>().HasData(
                new Core.Entities.Student
                {
                    Id = 1,
                    Name = "Aarav Sharma",
                    Email = "aarav.sharma@example.com",
                    Age = 21,
                    Course = "Computer Science",
                    CreatedDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
                },
                new Core.Entities.Student
                {
                    Id = 2,
                    Name = "Priya Patel",
                    Email = "priya.patel@example.com",
                    Age = 22,
                    Course = "Information Technology",
                    CreatedDate = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new Core.Entities.Student
                {
                    Id = 3,
                    Name = "Rohan Verma",
                    Email = "rohan.verma@example.com",
                    Age = 20,
                    Course = "Electronics Engineering",
                    CreatedDate = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc)
                },
                new Core.Entities.Student
                {
                    Id = 4,
                    Name = "Ananya Iyer",
                    Email = "ananya.iyer@example.com",
                    Age = 23,
                    Course = "Data Science",
                    CreatedDate = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc)
                },
                new Core.Entities.Student
                {
                    Id = 5,
                    Name = "Vikramaditya Deshmukh",
                    Email = "vikram.deshmukh@example.com",
                    Age = 21,
                    Course = "Artificial Intelligence",
                    CreatedDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Core.Entities.Student
                {
                    Id = 6,
                    Name = "Sneha Kulkarni",
                    Email = "sneha.kulkarni@example.com",
                    Age = 22,
                    Course = "Computer Science",
                    CreatedDate = new DateTime(2026, 4, 18, 0, 0, 0, DateTimeKind.Utc)
                },
                new Core.Entities.Student
                {
                    Id = 7,
                    Name = "Aditya Reddi",
                    Email = "aditya.reddi@example.com",
                    Age = 24,
                    Course = "Cyber Security",
                    CreatedDate = new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                new Core.Entities.Student
                {
                    Id = 8,
                    Name = "Kavya Nair",
                    Email = "kavya.nair@example.com",
                    Age = 20,
                    Course = "Information Technology",
                    CreatedDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc)
                },
                new Core.Entities.Student
                {
                    Id = 9,
                    Name = "Siddharth Malhotra",
                    Email = "siddharth.m@example.com",
                    Age = 22,
                    Course = "Cloud Computing",
                    CreatedDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new Core.Entities.Student
                {
                    Id = 10,
                    Name = "Meera Joshi",
                    Email = "meera.joshi@example.com",
                    Age = 21,
                    Course = "Data Science",
                    CreatedDate = new DateTime(2026, 6, 25, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@zestindia.com",
                    PasswordHash = adminPasswordHash,
                    Role = "Admin",
                    CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
