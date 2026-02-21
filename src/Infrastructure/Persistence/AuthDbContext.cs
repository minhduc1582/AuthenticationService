using Domain.AppRegistrations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class AuthDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private static readonly Guid AdminRoleId = Guid.Parse("a7e4f34b-6480-4d7c-a1e7-0e9aeb317833");
    private static readonly Guid UserRoleId = Guid.Parse("43a43fd4-96b8-4eb6-bc0f-2f0bb542ee24");
    private const string AdminRoleConcurrencyStamp = "f28e3d57-0c8d-47bc-9f21-6e2236f2e3de";
    private const string UserRoleConcurrencyStamp = "0c93f97a-6f1c-43cb-94da-763265cf7a24";

    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppRegistration> AppRegistrations => Set<AppRegistration>();
    public DbSet<Scope> Scopes => Set<Scope>();
    public DbSet<AppRegistrationScope> AppRegistrationScopes => Set<AppRegistrationScope>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDeleteRules();
        ApplyAuditStamps();

        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplySoftDeleteRules();
        ApplyAuditStamps();

        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasQueryFilter(u => !u.IsDeleted);
            entity.Property(u => u.IsDeleted)
                  .HasDefaultValue(false);
            entity.Property(u => u.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(u => u.UpdatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.HasData(
                new ApplicationRole("Admin")
                {
                    Id = AdminRoleId,
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = AdminRoleConcurrencyStamp,
                    Description = "Administrators with full access."
                },
                new ApplicationRole("User")
                {
                    Id = UserRoleId,
                    NormalizedName = "USER",
                    ConcurrencyStamp = UserRoleConcurrencyStamp,
                    Description = "Regular user role."
                });
        });

        builder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }

    private void ApplySoftDeleteRules()
    {
        foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
            }
        }
    }

    private void ApplyAuditStamps()
    {
        foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
        {
            if (entry.State == EntityState.Added)
            {
                var now = DateTimeOffset.UtcNow;
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}
