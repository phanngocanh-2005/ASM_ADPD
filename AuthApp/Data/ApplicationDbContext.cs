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
        public DbSet<AcademicProgram> AcademicPrograms { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<CourseAssignment> CourseAssignments { get; set; }
        public DbSet<AcademicRecord> AcademicRecords { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

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
                e.Property(x => x.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
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
                e.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("Pending");
                e.Property(x => x.DueDate).HasColumnName("due_date");
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");

                e.HasOne(x => x.Category)
                    .WithMany(c => c.Tasks)
                    .HasForeignKey(x => x.CategoryId);
                e.HasOne(x => x.Account)
                    .WithMany(a => a.Tasks)
                    .HasForeignKey(x => x.AccountId);
            });

            modelBuilder.Entity<AcademicProgram>(e =>
            {
                e.HasIndex(x => x.ProgramCode).IsUnique();
                e.Property(x => x.Status).HasDefaultValue("Active");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Student>(e =>
            {
                e.HasIndex(x => x.StudentCode).IsUnique();
                e.Property(x => x.Status).HasDefaultValue("Active");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(x => x.GPA).HasPrecision(5, 2);

                e.HasOne(x => x.Account)
                    .WithMany()
                    .HasForeignKey(x => x.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.AcademicProgram)
                    .WithMany(p => p.Students)
                    .HasForeignKey(x => x.AcademicProgramId)
                    .IsRequired(false);
            });

            modelBuilder.Entity<Teacher>(e =>
            {
                e.HasIndex(x => x.AccountId).IsUnique();
                e.HasIndex(x => x.TeacherCode).IsUnique();
                e.Property(x => x.Status).HasDefaultValue("Active");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasOne(x => x.Account)
                    .WithMany()
                    .HasForeignKey(x => x.AccountId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.AcademicProgram)
                    .WithMany()
                    .HasForeignKey(x => x.AcademicProgramId)
                    .IsRequired(false);
            });

            modelBuilder.Entity<Course>(e =>
            {
                e.HasIndex(x => x.CourseCode).IsUnique();
                e.Property(x => x.Status).HasDefaultValue("Active");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasOne(x => x.AcademicProgram)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(x => x.AcademicProgramId);
            });

            modelBuilder.Entity<Enrollment>(e =>
            {
                e.HasIndex(x => new { x.StudentId, x.CourseId, x.Status }).IsUnique();
                e.Property(x => x.Status).HasDefaultValue("Enrolled");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasOne(x => x.Student)
                    .WithMany(s => s.Enrollments)
                    .HasForeignKey(x => x.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Course)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CourseAssignment>(e =>
            {
                e.Property(x => x.Status).HasDefaultValue("Active");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasOne(x => x.Teacher)
                    .WithMany(t => t.CourseAssignments)
                    .HasForeignKey(x => x.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Course)
                    .WithMany(c => c.CourseAssignments)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AcademicRecord>(e =>
            {
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasOne(x => x.Student)
                    .WithMany(s => s.AcademicRecords)
                    .HasForeignKey(x => x.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Course)
                    .WithMany(c => c.AcademicRecords)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Enrollment)
                    .WithMany(e => e.AcademicRecords)
                    .HasForeignKey(x => x.EnrollmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(x => x.Teacher)
                    .WithMany(t => t.AcademicRecords)
                    .HasForeignKey(x => x.GradedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Schedule>(e =>
            {
                e.Property(x => x.Status).HasDefaultValue("Active");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasOne(x => x.Course)
                    .WithMany(c => c.Schedules)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
