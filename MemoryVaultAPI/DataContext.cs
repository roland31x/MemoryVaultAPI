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
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CreateAccounts(modelBuilder);
            CreateMemories(modelBuilder);

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(e => new { e.MemoryID, e.LikerID });

                entity.HasOne(e => e.Liker)
                .WithMany(a => a.Likes)
                .HasForeignKey(e => e.LikerID)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Memory)
                .WithMany(m => m.Likes)
                .HasForeignKey(e => e.MemoryID)
                .OnDelete(DeleteBehavior.NoAction);
            });
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

                entity.Property(e => e.IsAdmin)
                .HasColumnType("bit")
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

                entity.OwnsMany(e => e.Images, img =>
                {
                    img.Property(i => i.bytes)
                    .HasColumnType("varbinary(max)")
                    .IsRequired();
                });

            });
        }
    }
}