using LuminaryVisuals.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LuminaryVisuals.Data
{

    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Revision> Revisions { get; set; }
        public DbSet<Archive> Archives { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<UserNote> UserNote { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<CalculationParameter> CalculationParameter { get; set; }
        public DbSet<CalculationOption> CalculationOption { get; set; }
        public DbSet<ClientEditingGuidelines> ClientEditingGuidelines { get; set; }
        public DbSet<EditorLoggingHours> EditorLoggingHours { get; set; }


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
                entity.Property(e => e.WorkingMonth);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.ShootDate).HasColumnType("DATE");
                entity.Property(e => e.DueDate).HasColumnType("DATE");
                entity.Property(e => e.WorkingMonth).HasColumnType("DATE");

                entity
                    .HasOne(p => p.PrimaryEditor)
                    .WithMany()
                    .HasForeignKey(p => p.PrimaryEditorId)
                    .IsRequired(false);
                entity
                    .HasOne(p => p.SecondaryEditor)
                    .WithMany()
                    .HasForeignKey(p => p.SecondaryEditorId)
                    .IsRequired(false);

                entity.OwnsOne(p => p.PrimaryEditorDetails);
                entity.OwnsOne(p => p.SecondaryEditorDetails);
                entity.OwnsOne(p => p.ProjectSpecifications);


                entity.HasMany(p => p.Chats)
                      .WithOne(c => c.Project)
                      .HasForeignKey(c => c.ProjectId)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Archive)
                      .WithOne(a => a.Project)
                      .HasForeignKey<Archive>(a => a.ProjectId)
                        .OnDelete(DeleteBehavior.Cascade);
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
            builder.Entity<EditorLoggingHours>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.ProjectId, e.Date })
                .IsUnique();

                entity.Property(e => e.EditorWorkingHours)
                .HasPrecision(5, 2);

                entity.Property(e => e.Date)
                .HasColumnType("date");
            });
                

            builder.Entity<ClientEditingGuidelines>(entity =>
            {
                entity.ToTable("ClientEditingGuidelines");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.HasOne(e => e.User) 
                    .WithMany() 
                    .HasForeignKey(e => e.UserId) 
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // Seed Parameters
            builder.Entity<CalculationParameter>().HasData(
                new CalculationParameter { Id = 1, Name = "HighlightsDifficulty", Description = "Highlights Difficulty ", ParameterType = "Option", DefaultValue = 1.0m },
                new CalculationParameter { Id = 2, Name = "PrePartsPercentage", Description = "Pre-Parts Percentage", ParameterType = "Option", DefaultValue = 1.0m },
                new CalculationParameter { Id = 3, Name = "Resolution", Description = "Resolution of video", ParameterType = "Option", DefaultValue = 1.0m },
                new CalculationParameter { Id = 4, Name = "FootageQuality", Description = "Quality of Footage", ParameterType = "Option", DefaultValue = 1.0m },

                new CalculationParameter { Id = 5, Name = "CameraMulti", Description = "Multiplier for camera when there exist more than 2", ParameterType = "Decimal", DefaultValue = 0.3m },
                new CalculationParameter { Id = 6, Name = "SizeMulti", Description = "Multiplier for raw footage size when bigger than 300gb", ParameterType = "Decimal", DefaultValue = 0.4m }
            );

            // Seed Parameter Options
            builder.Entity<CalculationOption>().HasData(
                // Options for Highlights-Difficulty
                new CalculationOption { Id = 1, CalculationParameterId = 1, OptionName = "Straight Forward Linear, little mixing", Multiplier = 0.9m },
                new CalculationOption { Id = 2, CalculationParameterId = 1, OptionName = "Hybrid Mostly Linear", Multiplier = 1m },
                new CalculationOption { Id = 3, CalculationParameterId = 1, OptionName = "Movie with heavy SFX + VFX", Multiplier = 1.2m },

                // Options for Pre-Parts Percentage
                new CalculationOption { Id = 4, CalculationParameterId = 2, OptionName = "0%", Multiplier = 1m },
                new CalculationOption { Id = 5, CalculationParameterId = 2, OptionName = "30%", Multiplier = .95m },
                new CalculationOption { Id = 6, CalculationParameterId = 2, OptionName = "60%", Multiplier = .85m },
                new CalculationOption { Id = 7, CalculationParameterId = 2, OptionName = "100%", Multiplier = 0.7m },

                // Options for Resolution
                new CalculationOption { Id = 8, CalculationParameterId = 3, OptionName = "1080p", Multiplier = 1m },
                new CalculationOption { Id = 9, CalculationParameterId = 3, OptionName = "Mixed", Multiplier = 1.05m },
                new CalculationOption { Id = 10, CalculationParameterId = 3, OptionName = "4k", Multiplier = 1.1m },

                // Options for Footage Quality
                new CalculationOption { Id = 11, CalculationParameterId = 4, OptionName = "Needs work", Multiplier = 1.15m },
                new CalculationOption { Id = 12, CalculationParameterId = 4, OptionName = "Mostly good", Multiplier = 1.05m },
                new CalculationOption { Id = 13, CalculationParameterId = 4, OptionName = "Great", Multiplier = 1m }, 
                new CalculationOption { Id = 14, CalculationParameterId = 4, OptionName = "Excellent", Multiplier = 0.9m }

            );
        }
    }

}
