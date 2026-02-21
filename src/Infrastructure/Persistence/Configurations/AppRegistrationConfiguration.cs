using Domain.AppRegistrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AppRegistrationConfiguration : IEntityTypeConfiguration<AppRegistration>
{
    public void Configure(EntityTypeBuilder<AppRegistration> builder)
    {
        builder.ToTable("AppRegistrations");

        builder.HasKey(a => a.Id);
        builder.HasIndex(a => a.ClientId).IsUnique();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.ClientId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ClientSecretHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(a => a.ClientSecretSalt)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.IsEnabled)
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(a => a.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(a => a.Scopes)
            .WithOne(s => s.AppRegistration!)
            .HasForeignKey(s => s.AppRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(a => a.Scopes).HasField("_scopes");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
