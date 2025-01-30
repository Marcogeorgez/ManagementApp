using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LuminaryVisuals.Data
{

    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<UserProjectPin> UserProjectPins{ get; set; }

        public DbSet<PayoneerSettings> PayoneerSettings { get; set; }
        public DbSet<Entities.ColumnPreset> ColumnPresets { get; set; }
        public DbSet<MigratedUser> MigratedUsers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Revision> Revisions { get; set; }
        public DbSet<Archive> Archives { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatReadStatus> ChatReadStatus { get; set; }

        public DbSet<UserNote> UserNote { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<CalculationParameter> CalculationParameter { get; set; }
        public DbSet<CalculationOption> CalculationOption { get; set; }
        public DbSet<ClientEditingGuidelines> ClientEditingGuidelines { get; set; }
        public DbSet<EditorLoggingHours> EditorLoggingHours { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DataProtectionKey>(entity =>
            {
                entity.ToTable("DataProtectionKeys");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FriendlyName).HasMaxLength(100);
                entity.Property(e => e.Xml).HasColumnType("text");
            });

            // Configure PostgreSQL specific settings
            builder.UseIdentityColumns();

            // Configure ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users"); // Match your schema table name
                entity.Property(e => e.Id).HasMaxLength(255);
                entity.HasIndex(e => e.Id).IsUnique();
            });
            builder.Entity<PayoneerSettings>()
                .HasOne(ps => ps.User)
                .WithOne(u => u.PayoneerSettings)
                .HasForeignKey<PayoneerSettings>(ps => ps.UserId);
            builder.Entity<Entities.ColumnPreset>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Preferences).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId);
            });
            // Configure composite primary key
            builder.Entity<UserProjectPin>()
                    .HasKey(up => new { up.UserId, up.ProjectId });
            builder.Entity<UserProjectPin>()
                .HasOne(up => up.User)
                .WithMany(u => u.PinnedProjects)
                .HasForeignKey(up => up.UserId);
             builder.Entity<UserProjectPin>()
                .HasOne(up => up.Project)
                .WithMany(u => u.PinnedByUsers)
                .HasForeignKey(up => up.ProjectId);
            // Configure Project
            builder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
                entity.Property(e => e.ProjectName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Description);
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

                entity.OwnsOne(p => p.CalculationDetails);
                entity.OwnsOne(p => p.PrimaryEditorDetails);
                entity.OwnsOne(p => p.SecondaryEditorDetails);
                entity.OwnsOne(p => p.ProjectSpecifications);


                    entity.HasOne(p => p.Chat)
                          .WithOne(c => c.Project)
                          .HasForeignKey<Chat>(c => c.ProjectId)
                            .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Archive)
                      .WithOne(a => a.Project)
                      .HasForeignKey<Archive>(a => a.ProjectId)
                        .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<MigratedUser>()
                .HasIndex(g => g.GoogleProviderKey)
                .IsUnique();

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

                // Foreign Key - Project: Each Chat belongs to a specific project
                entity.HasOne(c => c.Project)
                      .WithOne(p => p.Chat)  // Project has one Chat (one-to-one relationship)
                      .HasForeignKey<Chat>(c => c.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });
            // Configure Chat Messages
            builder.Entity<Message>(entity =>
            {
                entity.ToTable("Messages");

                // Foreign Key - Chat: A message belongs to a chat
                entity.HasOne(m => m.Chat)
                      .WithMany(c => c.Messages)  // A chat can have many messages
                      .HasForeignKey(m => m.ChatId)  // Foreign key to Chat
                      .OnDelete(DeleteBehavior.Cascade);

                // Foreign Key - User: A message is sent by a user
                entity.HasOne(m => m.User)
                      .WithMany()  // A user can send many messages
                      .HasForeignKey(m => m.UserId)  // Foreign key to User
                      .OnDelete(DeleteBehavior.Cascade);  // Prevent deletion of User from deleting their messages

                // Required properties
                entity.Property(m => m.Content)
                      .IsRequired();  // Content is required

                entity.Property(m => m.Timestamp)
                      .IsRequired();  

                entity.Property(m => m.IsApproved)
                      .HasDefaultValue(false);  

                entity.Property(m => m.IsDeleted)
                      .HasDefaultValue(false);
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
            // ChatReadStatus entity configuration
            builder.Entity<ChatReadStatus>(entity =>
            {
                // Primary Key
                entity.HasKey(crs => crs.Id);

                // Foreign Key - Message (changed from Chat to Message)
                entity.HasOne(crs => crs.Message)
                      .WithMany(m => m.ChatReadStatuses)  // Each message can have many read statuses
                      .HasForeignKey(crs => crs.MessageId)
                      .OnDelete(DeleteBehavior.Cascade); // Cascade delete when a Message is deleted

                // Foreign Key - User
                entity.HasOne(crs => crs.User)
                      .WithMany()  // Each user can have many read statuses
                      .HasForeignKey(crs => crs.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes for better performance on queries (unique read status per user and message)
                entity.HasIndex(crs => new { crs.MessageId, crs.UserId })
                      .IsUnique(); 

                // Required properties
                entity.Property(crs => crs.IsRead)
                      .IsRequired();

                entity.Property(crs => crs.ReadTimestamp)
                      .IsRequired();
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
