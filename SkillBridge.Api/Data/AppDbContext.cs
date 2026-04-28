using Microsoft.EntityFrameworkCore;
using SkillBridge.Api.Models;

namespace SkillBridge.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<MentorConnection> MentorConnections => Set<MentorConnection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Location).HasMaxLength(120);
            entity.Property(x => x.Bio).HasMaxLength(1000);
            entity.Property(x => x.Education).HasMaxLength(200);
            entity.Property(x => x.CareerGoal).HasMaxLength(200);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Role).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(100);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            entity.HasOne(x => x.Employer)
                .WithMany(x => x.PostedJobs)
                .HasForeignKey(x => x.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.IsActive, x.CreatedAt });
        });

        modelBuilder.Entity<MentorConnection>(entity =>
        {
            entity.ToTable("mentor_connections");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MenteeName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.MenteeEmail).HasMaxLength(160).IsRequired();
            entity.Property(x => x.MenteeGoal).HasMaxLength(300);
            entity.Property(x => x.Message).HasMaxLength(1200);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.RequestedAt).HasDefaultValueSql("NOW()");
            entity.HasOne(x => x.Mentor)
                .WithMany(x => x.MentorRequests)
                .HasForeignKey(x => x.MentorId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.MentorId, x.MenteeEmail, x.Status });
            entity.HasIndex(x => new { x.MenteeEmail, x.RequestedAt });
        });
    }
}
