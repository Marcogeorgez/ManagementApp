using Luminary.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Luminary.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<VideoEditor> VideoEditors { get; set; }
    public DbSet<VideoStatus> VideoStatuses { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Archive> Archives { get; set; }
    public DbSet<Chat> Chats { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure PostgreSQL specific settings
        builder.UseIdentityColumns();

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users"); // Match your schema table name
            entity.Property(e => e.GoogleId).HasMaxLength(255);
            entity.HasIndex(e => e.GoogleId).IsUnique();

            // Relationship with Role
            entity.HasOne(u => u.Role)
                  .WithMany()
                  .HasForeignKey(u => u.RoleId);
        });

        // Configure Role
        builder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
        });

        // Seed default roles
        builder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "Editor" },
            new Role { RoleId = 3, RoleName = "Client" },
            new Role { RoleId = 3, RoleName = "Guest" } // Default role
        );
    }
}