using AuthApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<TaskJob> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(e =>
            {
                e.ToTable("Account");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("Id");
                e.Property(x => x.Username).HasColumnName("Username").HasMaxLength(50).IsRequired();
                e.Property(x => x.Fullname).HasColumnName("Fullname").HasMaxLength(100).IsRequired();
                e.Property(x => x.Password).HasColumnName("PasswordHash").HasMaxLength(255).IsRequired();
                e.Property(x => x.Email).HasColumnName("Email").HasMaxLength(100).IsRequired();
                e.Property(x => x.PhoneNumber).HasColumnName("PhoneNumber").HasMaxLength(20); 
                e.Property(x => x.Role).HasColumnName("Role").HasMaxLength(20).IsRequired();
                e.Property(x => x.CreatedAt).HasColumnName("CreatedAt").ValueGeneratedOnAdd(); 
            });

            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable("categories");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            });

            modelBuilder.Entity<TaskJob>(e =>
            {
                e.ToTable("tasks");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
                e.Property(x => x.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
                e.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();
                e.Property(x => x.AccountId).HasColumnName("account_id").IsRequired();

                e.HasOne(x => x.Category)
                    .WithMany(c => c.Tasks)
                    .HasForeignKey(x => x.CategoryId);
                e.HasOne(x => x.Account)
                    .WithMany(a => a.Tasks)
                    .HasForeignKey(x => x.AccountId);
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
