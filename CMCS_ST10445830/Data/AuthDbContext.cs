using Microsoft.EntityFrameworkCore;
using CMCS_ST10445830.Models;

namespace CMCS_ST10445830.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        // Database Tables
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimDocument> ClaimDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------------
            // Unique Username
            // -------------------------------
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // -------------------------------
            // One-to-one User <-> UserProfile
            // -------------------------------
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserProfile)
                .WithOne(up => up.User)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------------
            // Claim → Lecturer (User)
            // -------------------------------
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Lecturer)
                .WithMany()
                .HasForeignKey(c => c.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------------------
            // Claim → Coordinator
            // -------------------------------
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Coordinator)
                .WithMany()
                .HasForeignKey(c => c.CoordinatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------------------
            // Claim → Academic Manager
            // -------------------------------
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Manager)
                .WithMany()
                .HasForeignKey(c => c.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}