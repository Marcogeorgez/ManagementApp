using LuminaryVisuals.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Project = LuminaryVisuals.Data.Entities.Project;

namespace LuminaryVisuals.Data
{

    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<VideoEditor> VideoEditors { get; set; }
        public DbSet<VideoStatus> VideoStatuses { get; set; }
        public DbSet<ClientPayment> Payments { get; set; }
        public DbSet<Archive> Archives { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<UserNote> UserNote { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure PostgreSQL specific settings
            builder.UseIdentityColumns();

            // Configure ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users"); // Match your schema table name
                entity.Property(e => e.Id).HasMaxLength(255);
                entity.HasIndex(e => e.Id).IsUnique();
            });

            // Configure Project
            builder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
                entity.Property(e => e.ProjectName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.ShootDate).IsRequired();
                entity.Property(e => e.DueDate).IsRequired();
                entity.Property(e => e.ProgressBar).IsRequired();
                entity.Property(e => e.WorkingMonth).IsRequired();
                entity.Property(e => e.Status).IsRequired();

                entity.HasMany(p => p.VideoEditors)
                      .WithOne(ve => ve.Project)
                      .HasForeignKey(ve => ve.ProjectId);
                entity.HasMany(p => p.VideoStatuses)
                      .WithOne(vs => vs.Project)
                      .HasForeignKey(vs => vs.ProjectId);
                entity.HasMany(p => p.EditorPayments)
                      .WithOne(ep => ep.Project)
                      .HasForeignKey(ep => ep.ProjectId);
                entity.HasOne(p => p.ClientPayment)
                      .WithOne(cp => cp.Project)
                      .HasForeignKey<ClientPayment>(cp => cp.ProjectId);
                entity.HasMany(p => p.Chats)
                      .WithOne(c => c.Project)
                      .HasForeignKey(c => c.ProjectId);
                entity.HasOne(p => p.Archive)
                      .WithOne(a => a.Project)
                      .HasForeignKey<Archive>(a => a.ProjectId);
            });

            // Configure VideoEditor
            builder.Entity<VideoEditor>(entity =>
            {
                entity.ToTable("VideoEditors");
                entity.HasOne(ve => ve.User)
                      .WithMany(u => u.VideoEditors)
                      .HasForeignKey(ve => ve.UserId);
            });

            // Configure VideoStatus
            builder.Entity<VideoStatus>(entity =>
            {
                entity.ToTable("VideoStatuses");
                entity.HasOne(vs => vs.User)
                      .WithMany(u => u.VideoStatuses)
                      .HasForeignKey(vs => vs.UserId);
            });

            // Configure Payment
            builder.Entity<ClientPayment>(entity =>
            {
                entity.ToTable("ClientPayment");
                entity.HasOne(cp => cp.Project)
                      .WithOne(p => p.ClientPayment)
                      .HasForeignKey<ClientPayment>(cp => cp.ProjectId);
            });

            builder.Entity<EditorPayments>(entity =>
            {
                entity.ToTable("EditorPayments");

                entity.HasOne(ep => ep.User)
                      .WithMany(u => u.EditorPayments)
                      .HasForeignKey(ep => ep.UserId);

                entity.HasOne(ep => ep.Project)
                      .WithMany(p => p.EditorPayments)
                      .HasForeignKey(ep => ep.ProjectId);

                entity.HasIndex(ep => new { ep.UserId, ep.ProjectId, ep.PaymentMonth, ep.PaymentYear })
                      .IsUnique();
            });


            // Configure Archive
            builder.Entity<Archive>(entity =>
            {
                entity.ToTable("Archives");
                entity.HasKey(a => a.ArchiveId);
                entity.HasOne(a => a.Project) // Set up the relationship from Archive to Project
                      .WithOne(p => p.Archive) // Navigation property in Project
                      .HasForeignKey<Archive>(a => a.ProjectId);
            });

            // Configure Chat
            builder.Entity<Chat>(entity =>
            {
                entity.ToTable("Chats");
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Chats)
                      .HasForeignKey(c => c.UserId);
                entity.Property(c => c.IsApproved)
                      .HasDefaultValue(false);
                entity.Property(c => c.IsEditorMessage)
                      .IsRequired();
            });

            // Configure User Notes 
            builder.Entity<UserNote>(entity =>
            {
                entity.HasOne(un => un.TargetUser)
                      .WithMany()
                      .HasForeignKey(un => un.TargetUserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(un => un.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(un => un.CreatedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);

            });

        }
    }

}
