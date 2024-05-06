using Microsoft.EntityFrameworkCore;
using System.Net;
using MemoryVaultAPI.Models;

namespace MemoryVaultAPI
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Memory> Memories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CreateAccounts(modelBuilder);
            CreateMemories(modelBuilder);
        }
        private void CreateAccounts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.AccountID);

                entity.HasIndex(e => e.Username)
                .IsUnique();

                entity.Property(e => e.Username)
                .HasColumnType("nvarchar(16)")
                .IsRequired();

                entity.HasIndex(e => e.Email)
                .IsUnique();

                entity.Property(e => e.Email)
                .HasColumnType("nvarchar(50)")
                .IsRequired();

                entity.Property(e => e.Password)
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            });
        }

        private void CreateMemories(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Memory>(entity =>
            {
                entity.HasKey(e => e.MemoryID);

                entity.Property(e => e.Name)
                .HasColumnType("nvarchar(50)")
                .IsRequired();

                entity.Property(e => e.PostDate)
                .HasColumnType("datetime")
                .IsRequired();

                entity.Property(e => e.Description)
                .HasColumnType("nvarchar(500)")
                .IsRequired();

                entity.HasOne(e => e.Owner)
                .WithMany(m => m.Memories)
                .HasForeignKey(m => m.OwnerID)
                .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}